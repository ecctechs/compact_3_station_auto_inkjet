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
        // CREATE JOB
        // =========================
        public async Task<bool> CreateJobAsync(CreateJobRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/job/create", request, JsonOptions);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CreateJob error: " + ex);
                return false;
            }
        }

        // =========================
        // GET PENDING JOBS
        // =========================
        public async Task<List<PrintJob>> GetPendingJobsAsync()
        {
            try
            {
                var response = await _http.GetAsync("/job/getAll?status=pending");
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

                // ตรวจสอบสถานะ (ถ้าไม่ใช่ 2xx จะโยน Exception ไปที่ catch)
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                // Debug ดู Error ที่ตอบกลับมาจาก Backend (เช่น Barcode already exists)
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
    }
}