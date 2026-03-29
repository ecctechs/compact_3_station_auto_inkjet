using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers; // เพิ่มบรรทัดนี้ด้านบนสุด
using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// HTTP client for the InkjetBackend REST API.
/// Polls for pending jobs, fetches resolved patterns, posts results.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
    };

    public ApiClient(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _http = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(10),
        };
    }

    public async Task<bool> CreateLastSentJobAsync(object jobData)
    {
        try
        {
            // ใช้ PostAsJsonAsync และส่ง JsonOptions (SnakeCase) เข้าไปด้วย
            var response = await _http.PostAsJsonAsync("/job/lastSent/create", jobData, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"CreateLastSentJob failed: {response.StatusCode} - {errorBody}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("CreateLastSentJob error: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// GET /job/lastSent/getAll?st_status=sent&page=1&limit=10
    /// ดึงรายการประวัติการส่งงานล่าสุดพร้อมระบบแบ่งหน้า
    /// </summary>
    public async Task<List<PrintJob>> GetLastSentJobsAsync(string stStatus = "sent", int page = 1, int limit = 10)
    {
        try
        {
            // สร้าง Query String สำหรับ Filter และ Pagination
            var url = $"/job/lastSent/getAll?st_status={stStatus}&page={page}&limit={limit}";

            var response = await _http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetLastSentJobs failed: {response.StatusCode} - {errorBody}");
                return new List<PrintJob>();
            }

            // ใช้ ReadFromJsonAsync พร้อม JsonOptions (SnakeCase) เหมือน GetPendingJobsAsync
            var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResult<PrintJob>>>(JsonOptions);

            return wrapper?.Data?.Data ?? new List<PrintJob>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("GetLastSentJobs error: " + ex.Message);
            return new List<PrintJob>();
        }
    }

    /// <summary>GET /job/getAll?status=pending — polled every 5s by frmMain timer.</summary>
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
            Console.WriteLine("GetPendingJobs error: " + ex.Message);
            return new List<PrintJob>();
        }
    }

    /// <summary>GET /job/getById/:id</summary>
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
            Console.WriteLine("GetJobById error: " + ex.Message);
            return null;
        }
    }

    /// <summary>GET /job/getResolved/:id — returns job + pattern with resolved templates.</summary>
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
            Console.WriteLine("GetResolvedJob error: " + ex.Message);
            return null;
        }
    }

    /// <summary>POST /job/execute/:id — marks job as executing.</summary>
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
            Console.WriteLine("ExecuteJob error: " + ex.Message);
            return false;
        }
    }

    /// <summary>POST /job/postResults/:id — posts command results back to backend.</summary>
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
            Console.WriteLine("PostResults error: " + ex.Message);
            return false;
        }
    }

    /// <summary>POST /job/retry/:id — resets failed job to pending.</summary>
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
            Console.WriteLine("RetryJob error: " + ex.Message);
            return false;
        }
    }

    /// <summary>POST /job/create — create new job (with improved error logging).</summary>
    public async Task<PrintJob?> CreateJobAsync(CreateJobRequest newJob)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/job/create", newJob, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"CreateJob failed: {(int)response.StatusCode} {response.ReasonPhrase} - Body: {body}");
                return null;
            }

            var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PrintJob>>(JsonOptions);
            return wrapper?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine("CreateJob error: " + ex.Message);
            return null;
        }
    }
}
