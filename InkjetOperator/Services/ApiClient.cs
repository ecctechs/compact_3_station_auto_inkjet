using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using InkjetOperator.Models;

namespace InkjetOperator.Services
{
    /// <summary>
    /// HTTP client for the InkjetBackend REST API
    /// </summary>
    public class ApiClient
    {
        private static readonly HttpClient _http = new HttpClient();

        private readonly string _baseUrl;

        /// <summary>รายละเอียด error ล่าสุดจาก backend (ใช้ debug / แสดงสาเหตุจริง)</summary>
        public string LastError { get; private set; } = "";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
        };

        public ApiClient(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');

            _http.BaseAddress = new Uri(_baseUrl);
            _http.Timeout = TimeSpan.FromSeconds(10);
        }

        // =========================
        // HEALTH CHECK (Ping Backend)
        // =========================
        /// <summary>เช็คว่า Backend เชื่อมต่อได้ไหม (GET /job/getAll?status=pending ตอบกลับได้)</summary>
        public async Task<bool> PingAsync()
        {
            try
            {
                // อ่าน IP ล่าสุดจาก config ทุกครั้ง (ไม่ใช้ _baseUrl ที่ cache ไว้)
                string freshUrl = AppConfig.ApiUrl;

                using var client = new HttpClient
                {
                    BaseAddress = new Uri(freshUrl),
                    Timeout = TimeSpan.FromSeconds(5)
                };

                var response = await client.GetAsync("/job/getAll?status=Waiting");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // =========================
        // CREATE JOB
        // =========================
        public async Task<PrintJob?> CreateJobAsync(CreateJobRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/job/create", request, JsonOptions);
                response.EnsureSuccessStatusCode();

                // backend คืน job ที่สร้าง (มี id) → ใช้ link uv_job_data ต่อ
                var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PrintJob>>(JsonOptions);
                return wrapper?.Data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CreateJob error: " + ex);
                return null;
            }
        }

        // =========================
        // GET PENDING JOBS
        // =========================
        public async Task<List<PrintJob>> GetPendingJobsAsync()
        {
            try
            {
                var response = await _http.GetAsync("/job/getAll");
                response.EnsureSuccessStatusCode();

                var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResult<PrintJob>>>(JsonOptions);

                return wrapper?.Data?.Data ?? new List<PrintJob>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetPendingJobs error: " + ex);
                return new List<PrintJob>();
            }
        }

        // =========================
        // GET JOB BY ID
        // =========================
        public async Task<PrintJob?> GetJobByIdAsync(int jobId)
        {
            try
            {
                var response = await _http.GetAsync($"/job/getById/{jobId}");
                response.EnsureSuccessStatusCode();

                var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PrintJob>>(JsonOptions);

                return wrapper?.Data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetJobById error: " + ex);
                return null;
            }
        }

        // =========================
        // GET RESOLVED JOB
        // =========================
        public async Task<ResolvedJobResponse?> GetResolvedJobAsync(int jobId)
        {
            try
            {
                var response = await _http.GetAsync($"/job/getResolved/{jobId}");
                response.EnsureSuccessStatusCode();

                var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<ResolvedJobResponse>>(JsonOptions);

                return wrapper?.Data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetResolvedJob error: " + ex);
                return null;
            }
        }

        // =========================
        // EXECUTE JOB
        // =========================
        public async Task<bool> ExecuteJobAsync(int jobId)
        {
            try
            {
                var response = await _http.PostAsync($"/job/execute/{jobId}", null);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ExecuteJob error: " + ex);
                return false;
            }
        }

        // =========================
        // POST RESULTS
        // =========================
        public async Task<bool> PostResultsAsync(int jobId, JobResultsPayload results)
        {
            try
            {
                var response = await _http.PostAsJsonAsync($"/job/postResults/{jobId}", results, JsonOptions);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("PostResults error: " + ex);
                return false;
            }
        }

        // =========================
        // RETRY JOB
        // =========================
        public async Task<bool> RetryJobAsync(int jobId)
        {
            try
            {
                var response = await _http.PostAsync($"/job/retry/{jobId}", null);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RetryJob error: " + ex);
                return false;
            }
        }

        public async Task<bool> CreatePatternAsync(PatternDetail request)
        {
            try
            {
                // ส่งไปยัง Endpoint /pattern/create ตามที่ตั้งไว้ใน Backend
                var response = await _http.PostAsJsonAsync("/pattern/create", request, JsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    // เก็บสาเหตุจริงจาก backend (เช่น "Barcode already exists" หรือ validation)
                    string body = await response.Content.ReadAsStringAsync();
                    LastError = $"[HTTP {(int)response.StatusCode}] {body}";
                    Debug.WriteLine("CreatePattern failed: " + LastError);
                    return false;
                }

                LastError = "";
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Debug.WriteLine("CreatePattern error: " + ex.Message);
                return false;
            }
        }

        public async Task<PatternDetail?> GetPatternByBarcodeAsync(string barcode)
        {
            try
            {
                // encode barcode เพื่อรองรับอักขระพิเศษเช่น / หรือ -
                string encodedBarcode = Uri.EscapeDataString(barcode);
                var response = await _http.GetAsync($"/pattern/lookup/{encodedBarcode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // แกะเอาเฉพาะกิ่ง data ตาม SuccessResponse ของ Backend
                    var result = JsonSerializer.Deserialize<ApiResponse<PatternDetail>>(content, JsonOptions);
                    return result?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lookup Pattern error: {ex.Message}");
                return null;
            }
        }

        // =========================
        // CREATE UV INKJET
        // =========================
        public async Task<bool> CreateUvInkjetAsync(UVinkjet request)
        {
            try
            {
                // ส่งไปยัง Endpoint /uv-inkjet/create ตามโครงสร้างเดิมของโปรเจกต์
                var response = await _http.PostAsJsonAsync("/uv-inkjet/create", request, JsonOptions);

                // ตรวจสอบ Success StatusCode (2xx)
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                // บันทึก Error หากการส่งข้อมูลล้มเหลว
                Debug.WriteLine("CreateUvInkjet error: " + ex.Message);
                return false;
            }
        }

        // =========================
        // GET ALL UV INKJET RECORDS
        // =========================
        public async Task<List<UVinkjet>> GetAllUvInkjetAsync()
        {
            try
            {
                var response = await _http.GetAsync("/uv-inkjet/getAll");
                response.EnsureSuccessStatusCode();

                // แก้ไข: เปลี่ยนจาก List<UVinkjet> เป็น PaginatedResult<UVinkjet>
                var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResult<UVinkjet>>>(JsonOptions);

                // คืนค่า List ที่อยู่ข้างใน PaginatedResult อีกที
                return wrapper?.Data?.Data ?? new List<UVinkjet>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetAllUvInkjet error: " + ex.Message);
                return new List<UVinkjet>();
            }
        }

        // =========================
        // UV JOB DATA (preview detail) — capture ตอน register / poll ตอน preview
        // =========================

        /// <summary>ส่ง UV detail (UV1/UV2) ของงานหนึ่งไปเก็บ backend (upsert ตาม print_jobs_id)</summary>
        public async Task<bool> CreateUvJobDataAsync(int jobId, List<UvJobData> items)
        {
            try
            {
                var payload = new { print_jobs_id = jobId, items };
                var response = await _http.PostAsJsonAsync("/uv-job/create", payload, JsonOptions);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateUvJobData error (job: {jobId}): " + ex.Message);
                return false;
            }
        }

        /// <summary>poll UV detail ของงานที่เลือก → เอาไปโชว์ preview แท็บ UV1/UV2</summary>
        public async Task<List<UvJobData>> GetUvJobDataByJobAsync(int jobId)
        {
            try
            {
                var response = await _http.GetAsync($"/uv-job/getByJob/{jobId}");
                response.EnsureSuccessStatusCode();

                var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<List<UvJobData>>>(JsonOptions);
                return wrapper?.Data ?? new List<UvJobData>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetUvJobDataByJob error (job: {jobId}): " + ex.Message);
                return new List<UvJobData>();
            }
        }

        // =========================
        // UPDATE UV INKJET BY ID
        // =========================
        public async Task<bool> UpdateUvInkjetAsync(int id, object updateData)
        {
            try
            {
                // ส่งข้อมูลไปยัง /uv-inkjet/update/:id ด้วย Method PUT
                var response = await _http.PutAsJsonAsync($"/uv-inkjet/update/{id}", updateData, JsonOptions);

                // ตรวจสอบสถานะการทำงาน (2xx Success)
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateUvInkjet error (ID: {id}): " + ex.Message);
                return false;
            }
        }


        // =========================
        // GET ALL PLC SETTINGS
        // =========================
        public async Task<List<PlcRegisterMap>> GetAllPlcSettingsAsync()
        {
            try
            {
                // ไม่ส่ง page/limit — Backend จะคืนทุกแถวเรียงตาม sort_order
                var response = await _http.GetAsync("/plc-setting/getAll");
                response.EnsureSuccessStatusCode();

                var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResult<PlcRegisterMap>>>(JsonOptions);

                return wrapper?.Data?.Data ?? new List<PlcRegisterMap>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetAllPlcSettings error: " + ex.Message);
                return new List<PlcRegisterMap>();
            }
        }

        // =========================
        // BULK SAVE PLC SETTINGS
        // =========================
        /// <summary>แทนที่ตาราง plc_register_map ทั้งชุดใน transaction เดียว</summary>
        public async Task<bool> BulkSavePlcSettingsAsync(List<PlcRegisterMap> rows)
        {
            try
            {
                var request = new PlcBulkSaveRequest { Rows = rows };
                var response = await _http.PostAsJsonAsync("/plc-setting/bulkSave", request, JsonOptions);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("BulkSavePlcSettings error: " + ex.Message);
                return false;
            }
        }

        // =========================
        // UPDATE JOB
        // =========================
        public async Task<bool> UpdateJobAsync(int jobId, object updateData)
        {
            try
            {
                // ส่งข้อมูลไปยัง /job/update/:id
                var response = await _http.PostAsJsonAsync($"/job/update/{jobId}", updateData, JsonOptions);

                // ตรวจสอบ Success StatusCode (2xx)
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateJob error (ID: {jobId}): " + ex.Message);
                return false;
            }
        }
    }
}