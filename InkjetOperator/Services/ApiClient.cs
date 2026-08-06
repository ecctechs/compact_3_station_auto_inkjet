using System.Net.Http.Json;
using System.Text.Json;
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

    /// <summary>GET /plc-setting/getAll — all register-map rows ordered by sort_order.</summary>
    public async Task<List<PlcRegisterMap>> GetAllPlcSettingsAsync()
    {
        try
        {
            var response = await _http.GetAsync("/plc-setting/getAll");
            response.EnsureSuccessStatusCode();

            var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<List<PlcRegisterMap>>>(JsonOptions);
            return wrapper?.Data ?? new List<PlcRegisterMap>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("GetAllPlcSettings error: " + ex.Message);
            return new List<PlcRegisterMap>();
        }
    }

    /// <summary>POST /plc-setting/bulkSave — replaces the whole table with these rows.</summary>
    public async Task<bool> BulkSavePlcSettingsAsync(List<PlcRegisterMap> rows)
    {
        try
        {
            var payload = new PlcBulkSaveRequest { Rows = rows };
            var response = await _http.PostAsJsonAsync("/plc-setting/bulkSave", payload, JsonOptions);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("BulkSavePlcSettings error: " + ex.Message);
            return false;
        }
    }
}
