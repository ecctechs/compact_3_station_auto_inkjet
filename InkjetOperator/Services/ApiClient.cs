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

    private static readonly SocketsHttpHandler SharedHandler = new()
    {
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        MaxConnectionsPerServer = 8,
    };

    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(3);

    public ApiClient(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _http = new HttpClient(SharedHandler, disposeHandler: false)
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = RequestTimeout,
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

    public async Task<(bool ok, string? error)> UpdateUvTextsAsync(
        int rowId, IReadOnlyDictionary<string, string?> texts)
    {
        try
        {
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(texts, JsonOptions),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _http.PatchAsync($"/uv-job/{rowId}", content);
            var body = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode
                ? (true, null)
                : (false, $"[{(int)response.StatusCode}] {body}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(ResetRuntimeResult? result, string? error)> ResetRuntimeAsync()
    {
        try
        {
            var response = await _http.PostAsync("/system/resetRuntime", null);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return (null, $"[{(int)response.StatusCode}] {body}");

            var wrapper = System.Text.Json.JsonSerializer
                .Deserialize<ApiResponse<ResetRuntimeResult>>(body, JsonOptions);
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

    public async Task<(bool ok, string? error)> UpdatePatternAsync(int patternId, PatternDetail pattern)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                $"/pattern/update/{patternId}", pattern, JsonOptions);
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

    public async Task<bool> SaveSendStepAsync(int jobId, string stepName, object? detail = null)
    {
        try
        {
            var body = new
            {
                command = stepName,
                success = true,
                sent_at = DateTime.UtcNow.ToString("o"),
                payload = detail,
            };
            var response = await _http.PostAsJsonAsync($"/job/addCommand/{jobId}", body, JsonOptions);
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

    public async Task<(bool ok, string? error)> ClaimRemoteStartAsync(
        int jobId, string? program, string? step = null)
    {
        try
        {
            var payload = new
            {
                remote_start = "2",
                remote_program = program,
                remote_step = step,
                remote_error = (string?)null,
            };
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _http.PatchAsync($"/job/{jobId}/remote-start", content);
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

    public async Task<(List<MachineQueueRow> rows, string? error)> GetMachineQueueAsync(
        string? machine = null)
    {
        try
        {
            var url = "/machine-queue/getAll";
            if (!string.IsNullOrWhiteSpace(machine)) url += $"?machine={Uri.EscapeDataString(machine)}";

            var response = await _http.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return (new(), $"[{(int)response.StatusCode}] {body}");

            var wrapper = System.Text.Json.JsonSerializer
                .Deserialize<ApiResponse<List<MachineQueueRow>>>(body, JsonOptions);
            return (wrapper?.Data ?? new(), null);
        }
        catch (Exception ex)
        {
            return (new(), ex.Message);
        }
    }

    public async Task<(bool ok, string? error)> EnqueueMachinesAsync(
        int jobId, List<MachineQueueItem> items)
    {
        try
        {
            var payload = new { print_jobs_id = jobId, items };
            var response = await _http.PostAsJsonAsync("/machine-queue/enqueue", payload, JsonOptions);
            var body = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode
                ? (true, null)
                : (false, $"[{(int)response.StatusCode}] {body}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(MachineClaimResult? result, string? error)> ClaimMachineAsync(
        string machine, int? jobId = null)
    {
        try
        {
            var payload = new Dictionary<string, object> { ["machine"] = machine };
            if (jobId != null) payload["print_jobs_id"] = jobId.Value;

            var response = await _http.PostAsJsonAsync(
                "/machine-queue/claim", payload, JsonOptions);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return (null, $"[{(int)response.StatusCode}] {body}");

            var wrapper = System.Text.Json.JsonSerializer
                .Deserialize<ApiResponse<MachineClaimResult>>(body, JsonOptions);
            return (wrapper?.Data, null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(ReleaseResult? result, string? error)> ReleaseMachineAsync(
        string machine, int? expectedHolderId, bool holdForNextRound = false)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                "/machine-queue/release",
                new { machine, expected_holder_id = expectedHolderId, hold_for_next_round = holdForNextRound },
                JsonOptions);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return (null, $"[{(int)response.StatusCode}] {body}");

            var wrapper = System.Text.Json.JsonSerializer
                .Deserialize<ApiResponse<ReleaseResult>>(body, JsonOptions);

            return (wrapper?.Data ?? new ReleaseResult(), null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public Task<(bool ok, string? error)> BeginQueueSendAsync(int rowId, string token) =>
        QueueSendRequestAsync(rowId, "begin-send", new { token });

    public Task<(bool ok, string? error)> FinishQueueSendAsync(
        int rowId, string token, string outcome, object? detail = null, string? error = null) =>
        QueueSendRequestAsync(rowId, "finish-send", new { token, outcome, detail, error = error ?? "" });

    private async Task<(bool ok, string? error)> QueueSendRequestAsync(int rowId, string action, object payload)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync($"/machine-queue/{rowId}/{action}", payload, JsonOptions);
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode ? (true, null) : (false, $"[{(int)response.StatusCode}] {body}");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool ok, string? error)> UpdateMachineQueueAsync(
        int rowId, string? state = null, string? programName = null, bool? sent = null)
    {
        try
        {
            var payload = new Dictionary<string, object>();
            if (state != null) payload["state"] = state;
            if (programName != null) payload["program_name"] = programName;
            if (sent != null) payload["sent"] = sent.Value;

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload, JsonOptions),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _http.PatchAsync($"/machine-queue/{rowId}", content);
            var body = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode
                ? (true, null)
                : (false, $"[{(int)response.StatusCode}] {body}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool ok, string? error)> ClearMachineQueueAsync(int jobId, bool onlyUnsent = false)
    {
        try
        {
            var response = await _http.DeleteAsync($"/machine-queue/job/{jobId}" + (onlyUnsent ? "?only_unsent=true" : ""));
            var body = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode
                ? (true, null)
                : (false, $"[{(int)response.StatusCode}] {body}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool ok, string? error)> SetRemoteStartAsync(
        int jobId, bool requested, string? program = null, string? failure = null, string? step = null)
    {
        try
        {
            var payload = new
            {
                remote_start = requested ? "1" : "0",
                remote_program = program,
                remote_step = step,
                remote_error = failure,
            };
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _http.PatchAsync($"/job/{jobId}/remote-start", content);
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
