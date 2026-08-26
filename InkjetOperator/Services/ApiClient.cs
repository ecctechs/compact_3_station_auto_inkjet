using System.Net.Http.Json;
using System.Text.Json;
using InkjetOperator.Models;

namespace InkjetOperator.Services;

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

    public async Task<bool> PingAsync()
    {
        try
        {
            var response = await _http.GetAsync("/system/ping");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<(PrintJob? job, string? error)> CreateJobAsync(CreateJobRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/job/create", request, JsonOptions);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return (null, $"[{(int)response.StatusCode}] {body}");
            var wrapper = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PrintJob>>(body, JsonOptions);
            return (wrapper?.Data, null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(PatternDetail? pattern, string? error)> CreatePatternAsync(CreatePatternRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/pattern/create", request, JsonOptions);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return (null, $"[{(int)response.StatusCode}] {body}");
            var wrapper = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PatternDetail>>(body, JsonOptions);
            return (wrapper?.Data, null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool ok, string? error)> CreateUvJobDataAsync(CreateUvJobRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/uv-job/create", request, JsonOptions);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return (false, $"[{(int)response.StatusCode}] {body}");
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>บันทึกระยะแคลมป์ของงาน — job เดิมเรียกซ้ำจะทับแถวเดิม ไม่สร้างซ้ำ</summary>
    public async Task<(bool ok, string? error)> CreateIaiAsync(IaiCreateRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/iai/create", request, JsonOptions);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return (false, $"[{(int)response.StatusCode}] {body}");
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>อ่านระยะแคลมป์ของงานจาก backend</summary>
    public async Task<(IaiClampSettingDto? result, string? error)> GetIaiByJobAsync(int jobId)
    {
        try
        {
            var response = await _http.GetAsync($"/iai/getByJob/{jobId}");
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return (null, $"[{(int)response.StatusCode}] {body}");

            var wrapper = JsonSerializer.Deserialize<ApiResponse<IaiClampSettingDto>>(body, JsonOptions);
            return (wrapper?.Data, null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool ok, string? error)> CreatePlanRoutingAsync(CreatePlanRoutingRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/plan-routing/create", request, JsonOptions);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return (false, $"[{(int)response.StatusCode}] {body}");
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<bool> DeleteJobAsync(int jobId)
    {
        try
        {
            var response = await _http.DeleteAsync($"/job/remove/{jobId}");
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("DeleteJob error: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// <paramref name="fromUtc"/> / <paramref name="toUtc"/> ใช้กรอง created_at ที่ฝั่ง backend
    /// ต้องกรองที่นั่น ไม่ใช่กรองในหน้าจอ เพราะ endpoint คืนมาแค่ limit แถวล่าสุด
    /// งานเก่ากว่านั้นจะไม่ถูกส่งมาให้กรองตั้งแต่แรก
    /// </summary>
    public async Task<(List<PrintJob> jobs, string? error)> GetAllJobsAsync(
        int limit = 100, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        try
        {
            var url = $"/job/getAll?page=1&limit={limit}";
            if (fromUtc.HasValue) url += $"&from={Uri.EscapeDataString(fromUtc.Value.ToString("o"))}";
            if (toUtc.HasValue) url += $"&to={Uri.EscapeDataString(toUtc.Value.ToString("o"))}";

            var response = await _http.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return (new(), $"[{(int)response.StatusCode}] {body}");
            var wrapper = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PaginatedResult<PrintJob>>>(body, JsonOptions);
            var jobs = wrapper?.Data?.Data ?? new();
            return (jobs, null);
        }
        catch (Exception ex)
        {
            return (new(), ex.Message);
        }
    }

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

    public async Task<bool> SaveSendStepAsync(int jobId, string stepName)
    {
        try
        {
            var payload = new
            {
                command = stepName,
                success = true,
                sent_at = DateTime.UtcNow.ToString("o"),
            };
            var response = await _http.PostAsJsonAsync($"/job/addCommand/{jobId}", payload, JsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<(List<PrintJob> jobs, string? error)> GetJobsByMarkingMethodAsync(string markingMethod, int limit = 100)
    {
        try
        {
            var response = await _http.GetAsync($"/job/getByMarkingMethod/{markingMethod}?limit={limit}");
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return (new(), $"[{(int)response.StatusCode}] {body}");
            var wrapper = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<List<PrintJob>>>(body, JsonOptions);
            return (wrapper?.Data ?? new(), null);
        }
        catch (Exception ex)
        {
            return (new(), ex.Message);
        }
    }

    public async Task<(bool ok, string? error)> SendToSt1Async(int jobId)
    {
        try
        {
            var response = await _http.PatchAsync($"/job/{jobId}/send-to-st1", null);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return (false, $"[{(int)response.StatusCode}] {body}");
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool ok, string? error)> UpdateJobStatusAsync(int jobId, string status)
    {
        try
        {
            var payload = new { status };
            var response = await _http.PatchAsync($"/job/{jobId}/status",
                JsonContent.Create(payload, options: JsonOptions));
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return (false, $"[{(int)response.StatusCode}] {body}");
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

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
