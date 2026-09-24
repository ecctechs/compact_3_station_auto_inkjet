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

    /// <summary>
    /// สายเชื่อมต่อใช้ร่วมกันทุก <see cref="ApiClient"/> ในโปรแกรม
    ///
    /// <para>
    /// เดิม <c>HttpClient</c> แต่ละตัวถือสายของตัวเอง และหน้า Scan Barcode สร้าง
    /// <c>ApiClient</c> ใหม่ทุกครั้งที่กด OK โดยไม่ได้ปิดตัวเก่า สายที่เปิดค้างไว้
    /// จึงค้างอยู่อย่างนั้นจนกว่าตัวเก็บขยะจะมาเก็บ วัดจริงแล้วกด 60 ครั้ง
    /// เหลือสายค้าง 60 เส้น สแกนทั้งกะหลายร้อยงานก็ค้างหลายร้อยเส้น
    /// สุดท้ายพอร์ตของ Windows หมดแล้วเปิดสายใหม่ไม่ได้
    /// </para>
    /// <para>
    /// ย้ายสายมาไว้ที่เดียวแล้วให้ทุกตัวใช้ร่วมกัน ตัว <c>HttpClient</c> ที่ถูกทิ้ง
    /// จึงไม่ได้พาสายไปด้วย สายที่ใช้เสร็จกลับเข้ากองกลางให้คนถัดไปใช้ต่อ
    /// (<c>disposeHandler: false</c> คือบอกว่าอย่าไปปิดกองกลางตอนตัวเองถูกทิ้ง)
    /// </para>
    /// <para>
    /// ปล่อยสายที่ไม่มีใครใช้ทิ้งหลังว่าง 1 นาที และรื้อสายที่ใช้มานาน 5 นาที
    /// เพื่อไม่ให้ค้างกับปลายทางเดิมตอนมีคนไปเปลี่ยน PC_IP ที่หน้า Setting
    /// </para>
    /// </summary>
    private static readonly SocketsHttpHandler SharedHandler = new()
    {
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        MaxConnectionsPerServer = 8,
    };

    public ApiClient(string baseUrl) // เตรียมตัวส่งคำขอไป Backend
    {
        _baseUrl = baseUrl.TrimEnd('/'); // ตัดเครื่องหมายท้าย URL ก่อนใช้งาน
        _http = new HttpClient(SharedHandler, disposeHandler: false) // ใช้ชุดการเชื่อมต่อร่วมกับตัวเรียกอื่น
        {
            BaseAddress = new Uri(_baseUrl), // กำหนด IP และพอร์ตปลายทาง
            Timeout = TimeSpan.FromSeconds(10), // รอคำตอบแต่ละครั้งไม่เกิน 10 วินาที
        };
    }

    public async Task<bool> PingAsync() // ตรวจ Backend ก่อนสร้างงาน
    {
        try // เริ่มตรวจว่า Backend พร้อมรับงาน และดักข้อผิดพลาดไว้
        {
            var response = await _http.GetAsync("/system/ping"); // ถามว่า Backend พร้อมหรือไม่
            return response.IsSuccessStatusCode; // ตอบรหัสสำเร็จถือว่าเชื่อมต่อได้
        }
        catch { return false; } // เรียกไม่ได้ให้คืนว่าไม่พร้อม
    }

    public async Task<(PrintJob? job, string? error)> CreateJobAsync(CreateJobRequest request) // Flow 8: ส่งหัวงานไปสร้าง Job
    {
        try // เริ่มส่งข้อมูลไปสร้าง Job และดักข้อผิดพลาดไว้
        {
            var response = await _http.PostAsJsonAsync("/job/create", request, JsonOptions); // ส่ง JSON ไป JobController.create()
            var body = await response.Content.ReadAsStringAsync(); // อ่านคำตอบจาก Backend
            if (!response.IsSuccessStatusCode) // ถ้า Backend ตอบว่าไม่สำเร็จ
                return (null, $"[{(int)response.StatusCode}] {body}"); // คืนรหัสและข้อความผิดพลาด
            var wrapper = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PrintJob>>(body, JsonOptions); // แปลงคำตอบเป็น Job รวม ID ที่สร้างใหม่
            return (wrapper?.Data, null); // คืนข้อมูลที่ Backend ส่งกลับ
        }
        catch (Exception ex) // จัดการปัญหาระหว่างส่งข้อมูลไปสร้าง Job
        {
            return (null, ex.Message); // คืนสาเหตุให้หน้าจอแสดง
        }
    }

    public async Task<(PatternDetail? pattern, string? error)> CreatePatternAsync(CreatePatternRequest request) // Flow 9: ส่ง Pattern ของ Job
    {
        try // เริ่มส่ง Pattern ไปบันทึก และดักข้อผิดพลาดไว้
        {
            var response = await _http.PostAsJsonAsync("/pattern/create", request, JsonOptions); // ส่ง JSON ไป PatternController.create()
            var body = await response.Content.ReadAsStringAsync(); // อ่านคำตอบจาก Backend
            if (!response.IsSuccessStatusCode) // ถ้า Backend ตอบว่าไม่สำเร็จ
                return (null, $"[{(int)response.StatusCode}] {body}"); // คืนรหัสและข้อความผิดพลาด
            var wrapper = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PatternDetail>>(body, JsonOptions); // แปลงคำตอบเป็น Pattern ที่บันทึกแล้ว
            return (wrapper?.Data, null); // คืนข้อมูลที่ Backend ส่งกลับ
        }
        catch (Exception ex) // จัดการปัญหาระหว่างส่ง Pattern ไปบันทึก
        {
            return (null, ex.Message); // คืนสาเหตุให้หน้าจอแสดง
        }
    }

    /// <summary>
    /// แก้ข้อความของแถว UV แถวเดียว — ส่งเฉพาะช่องที่เปลี่ยน
    ///
    /// <para>
    /// ใช้ตอนคนหน้างานพิมพ์ทับข้อความในตาราง UV ที่หน้า Order Detail
    /// ไม่แตะ machine หรือ program_name ของแถวนั้น
    /// </para>
    /// </summary>
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

    /// <summary>
    /// ล้างร่องรอยการเดินงานทั้งหมด ให้ทุกใบกลับไปเป็นรอเริ่ม — เครื่องมือทดสอบ
    ///
    /// <para>
    /// ลบคิวเครื่อง ลบประวัติคำสั่ง และตั้งสถานะทุกงานเป็นรอเริ่ม ข้อมูลของงานเอง
    /// (pattern, ข้อความ UV, แผน, ค่าแคลมป์) ไม่ถูกแตะ
    /// </para>
    /// </summary>
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

    public async Task<(bool ok, string? error)> CreateUvJobDataAsync(CreateUvJobRequest request) // Flow 10: ส่งข้อมูล UV ของ Job
    {
        try // เริ่มส่งข้อมูล UV ไปบันทึก และดักข้อผิดพลาดไว้
        {
            var response = await _http.PostAsJsonAsync("/uv-job/create", request, JsonOptions); // ส่ง JSON ไป UvJobController.create()
            var body = await response.Content.ReadAsStringAsync(); // อ่านคำตอบจาก Backend
            if (!response.IsSuccessStatusCode) // ถ้า Backend ตอบว่าไม่สำเร็จ
                return (false, $"[{(int)response.StatusCode}] {body}"); // คืนว่าไม่สำเร็จพร้อมสาเหตุ
            return (true, null); // บันทึกสำเร็จ ไม่มีข้อความผิดพลาด
        }
        catch (Exception ex) // จัดการปัญหาระหว่างส่งข้อมูล UV ไปบันทึก
        {
            return (false, ex.Message); // คืนว่าไม่สำเร็จพร้อมข้อความ
        }
    }

    /// <summary>
    /// บันทึก pattern ของงานทับของเดิม
    ///
    /// <para>
    /// ใช้ตอนพนักงานแก้ค่าที่หน้า Order Detail เช่นสลับเครื่อง (SWAP) หรือสลับ
    /// ทิศทางพิมพ์ (ABC) ค่าต้องลงฐานข้อมูลจริง เพราะคนกดส่งงานคือหน้า Order List
    /// ซึ่งอ่าน pattern ใหม่จาก backend ทุกครั้ง ไม่ได้ใช้ค่าที่ค้างอยู่ในจอ
    /// </para>
    /// <para>
    /// ฝั่ง backend ลบ inkjet_configs / text_blocks / servo_configs ของ pattern นี้
    /// ทิ้งแล้วสร้างใหม่จากที่ส่งไป จึงต้องส่ง <see cref="PatternDetail"/> ไปทั้งก้อน
    /// ส่งไปแค่บางส่วนแล้วส่วนที่ไม่ได้ส่งจะหายไปด้วย
    /// </para>
    /// </summary>
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

    /// <summary>บันทึกระยะแคลมป์ของงาน — job เดิมเรียกซ้ำจะทับแถวเดิม ไม่สร้างซ้ำ</summary>
    public async Task<(bool ok, string? error)> CreateIaiAsync(IaiCreateRequest request) // Flow 12: ส่งค่าแคลมป์ไปเก็บ
    {
        try // เริ่มส่งค่าแคลมป์ไปบันทึก และดักข้อผิดพลาดไว้
        {
            var response = await _http.PostAsJsonAsync("/iai/create", request, JsonOptions); // ส่ง JSON ไป IaiController.create()
            var body = await response.Content.ReadAsStringAsync(); // อ่านคำตอบจาก Backend
            if (!response.IsSuccessStatusCode) // ถ้า Backend ตอบว่าไม่สำเร็จ
                return (false, $"[{(int)response.StatusCode}] {body}"); // คืนว่าไม่สำเร็จพร้อมสาเหตุ
            return (true, null); // บันทึกสำเร็จ ไม่มีข้อความผิดพลาด
        }
        catch (Exception ex) // จัดการปัญหาระหว่างส่งค่าแคลมป์ไปบันทึก
        {
            return (false, ex.Message); // คืนว่าไม่สำเร็จพร้อมข้อความ
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

    public async Task<(bool ok, string? error)> CreatePlanRoutingAsync(CreatePlanRoutingRequest request) // Flow 11: ส่งแผนงานของ Job
    {
        try // เริ่มส่งแผนงานไปบันทึก และดักข้อผิดพลาดไว้
        {
            var response = await _http.PostAsJsonAsync("/plan-routing/create", request, JsonOptions); // ส่ง JSON ไป PlanRoutingController.create()
            var body = await response.Content.ReadAsStringAsync(); // อ่านคำตอบจาก Backend
            if (!response.IsSuccessStatusCode) // ถ้า Backend ตอบว่าไม่สำเร็จ
                return (false, $"[{(int)response.StatusCode}] {body}"); // คืนว่าไม่สำเร็จพร้อมสาเหตุ
            return (true, null); // บันทึกสำเร็จ ไม่มีข้อความผิดพลาด
        }
        catch (Exception ex) // จัดการปัญหาระหว่างส่งแผนงานไปบันทึก
        {
            return (false, ex.Message); // คืนว่าไม่สำเร็จพร้อมข้อความ
        }
    }

    public async Task<bool> DeleteJobAsync(int jobId) // ลบ Job เมื่อสร้าง Pattern ไม่สำเร็จ
    {
        try // เริ่มขอลบ Job ที่ไม่มี Pattern และดักข้อผิดพลาดไว้
        {
            var response = await _http.DeleteAsync($"/job/remove/{jobId}"); // ขอลบ Job ตาม ID
            response.EnsureSuccessStatusCode(); // ถ้ารหัสตอบไม่สำเร็จให้เข้า catch
            return true; // Backend ยืนยันว่าลบ Job แล้ว
        }
        catch (Exception ex) // จัดการปัญหาระหว่างขอลบ Job ที่ไม่มี Pattern
        {
            Console.WriteLine("DeleteJob error: " + ex.Message); // เขียนสาเหตุลบไม่ได้ลง Console
            return false; // ลบ Job ไม่สำเร็จ แจ้งผลให้ผู้เรียกทราบ
        }
    }

    /// <summary>
    /// <paramref name="fromUtc"/> / <paramref name="toUtc"/> ใช้กรอง created_at ที่ฝั่ง backend
    /// ต้องกรองที่นั่น ไม่ใช่กรองในหน้าจอ เพราะ endpoint คืนมาแค่ limit แถวล่าสุด
    /// งานเก่ากว่านั้นจะไม่ถูกส่งมาให้กรองตั้งแต่แรก
    /// </summary>
    public async Task<(List<PrintJob> jobs, string? error)> GetAllJobsAsync( // Flow 14: อ่านรายการงานจาก Backend
        int limit = 100, DateTime? fromUtc = null, DateTime? toUtc = null) // กำหนดจำนวนแถวและช่วงเวลาที่ต้องการ
    {
        try // เริ่มอ่านรายการงานจาก Backend และดักข้อผิดพลาดไว้
        {
            var url = $"/job/getAll?page=1&limit={limit}"; // ขอรายการงานหน้าแรกตามจำนวนที่กำหนด
            if (fromUtc.HasValue) url += $"&from={Uri.EscapeDataString(fromUtc.Value.ToString("o"))}"; // แนบเวลาเริ่มถ้ามีตัวกรอง
            if (toUtc.HasValue) url += $"&to={Uri.EscapeDataString(toUtc.Value.ToString("o"))}"; // แนบเวลาสิ้นสุดถ้ามีตัวกรอง

            var response = await _http.GetAsync(url); // อ่านรายการจาก JobController.getAll()
            var body = await response.Content.ReadAsStringAsync(); // อ่านคำตอบจาก Backend
            if (!response.IsSuccessStatusCode) // ถ้า Backend ตอบว่าไม่สำเร็จ
                return (new(), $"[{(int)response.StatusCode}] {body}"); // อ่านไม่ได้ คืนรายการว่างพร้อมสาเหตุ
            var wrapper = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PaginatedResult<PrintJob>>>(body, JsonOptions); // แปลง JSON เป็นชุดรายการงาน
            var jobs = wrapper?.Data?.Data ?? new(); // ดึงรายการออกมา ถ้าไม่มีใช้รายการว่าง
            return (jobs, null); // ส่งรายการไปแสดงที่ Order List
        }
        catch (Exception ex) // จัดการปัญหาระหว่างอ่านรายการงานจาก Backend
        {
            return (new(), ex.Message); // คืนรายการว่างพร้อมข้อผิดพลาด
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

    /// <summary>
    /// บันทึกว่า step นี้ส่งสำเร็จแล้ว
    /// <paramref name="detail"/> เก็บลงคอลัมน์ payload (JSONB) ไว้ย้อนดูภายหลัง
    /// เช่นรุ่นย่อย .uvdx ที่เลือกจริง — ไม่ส่งมาก็เก็บเป็น null เหมือนเดิม
    /// </summary>
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

    /// <summary>
    /// ตั้ง/ล้างคำขอให้ ST1 ส่งคำสั่งแทน — ST3 ตั้งพร้อมชื่อโปรแกรมที่เลือกไว้แล้ว
    /// ST1 ล้างทิ้ง (<paramref name="requested"/> = false) ทุกครั้งที่ลงมือส่งเสร็จ
    /// ไม่ว่าจะสำเร็จหรือไม่ เพื่อไม่ให้รอบ poll ถัดไปหยิบไปส่งซ้ำ
    /// <para>
    /// <paramref name="failure"/> คือสาเหตุที่ส่งไม่สำเร็จ ฝากไว้ให้ ST3 อ่านไปแสดง
    /// ที่จอตัวเอง — ส่ง null มาเมื่อไหร่คือล้างของเดิมทิ้ง
    /// </para>
    /// </summary>
    /// <summary>
    /// ST1 จองคำขอไว้แล้วว่ากำลังส่งเข้าเครื่องอยู่ (remote_start = "2")
    ///
    /// ต้องแยกจากสถานะ "รอคนหยิบ" เพราะ ST3 ใช้ตัดสินว่าจะตีงานกลับเป็น Waiting
    /// ได้ไหม — ใบที่ไม่มีใครหยิบเลยตีกลับได้ ใบที่กำลังส่งอยู่ห้ามแตะ
    /// </summary>
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

    // ── คิวของเครื่อง ──────────────────────────────────────
    //
    // กติกาอยู่ที่ backend ทั้งหมด (ตาราง machine_queue) ตรงนี้เป็นแค่ทางผ่าน
    // "เครื่องว่าง" คือเครื่องที่ไม่มีแถว active ไม่ได้ดูจากสถานะของงาน

    /// <summary>คิวที่ยังไม่ปล่อยเครื่องทั้งหมด — ระบุเครื่องเพื่อดูเฉพาะเครื่องนั้น</summary>
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

    /// <summary>จองเครื่องให้งานหนึ่ง — เรียกตอนกดเริ่มงาน จองครบทุกเครื่องที่แผนต้องใช้</summary>
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

    /// <summary>
    /// ขอหยิบงานถัดไปของเครื่องนั้นมาถือเครื่อง — คืน Claimed เป็น null เมื่อไม่ว่างหรือคิวว่าง
    /// </summary>
    public async Task<(MachineClaimResult? result, string? error)> ClaimMachineAsync(
        string machine, int? jobId = null)
    {
        try
        {
            // ใส่เฉพาะช่องที่มีค่าจริง — ฝั่ง backend ตรวจด้วย zod ซึ่ง optional หมายถึง
            // "ไม่ส่งมาก็ได้" ไม่ได้แปลว่า "ส่ง null มาได้" ส่ง null ไปคือถูกตีกลับ 400
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

    /// <summary>ปล่อยเครื่อง — คนกดปุ่มหน้างานแล้ว แปลว่าพิมพ์ชิ้นเดิมเสร็จ</summary>
    public async Task<(ReleaseResult? result, string? error)> ReleaseMachineAsync(
        string machine, bool holdForNextRound = false)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                "/machine-queue/release",
                new { machine, hold_for_next_round = holdForNextRound },
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

    /// <summary>แก้แถวในคิว — ใช้ตอนเลือกรุ่นย่อย UV เสร็จ หรือคืนแถวให้ลองส่งใหม่</summary>
    public async Task<(bool ok, string? error)> UpdateMachineQueueAsync(
        int rowId, string? state = null, string? programName = null, bool? sent = null)
    {
        try
        {
            // ส่งเฉพาะช่องที่ตั้งใจจะแก้จริง ๆ
            //
            // เดิมส่งทั้งสามช่องเสมอ ช่องที่ไม่ได้ตั้งค่าจึงกลายเป็น null แล้วโดน zod
            // ตีกลับ 400 ทั้งคำขอ ผลคือการบอกว่า "ส่งเข้าเครื่องแล้ว" ไม่เคยถึง backend
            // แถวยังเป็น "ถึงคิวแล้วแต่ยังไม่ได้ส่ง" อยู่ รอบ poll จึงส่งซ้ำทุก 5 วินาที
            //
            // อีกด้านหนึ่ง ถ้า state ผ่านการตรวจ ช่อง program_name ที่ติด null ไปด้วย
            // จะไปล้างรุ่นย่อยของโปรแกรม UV ที่เลือกไว้ทิ้งโดยไม่มีใครสั่ง
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

    /// <summary>ล้างคิวของงานหนึ่งทิ้ง — ยกเลิกงาน จบงาน หรือสั่งพิมพ์ใหม่</summary>
    public async Task<(bool ok, string? error)> ClearMachineQueueAsync(int jobId)
    {
        try
        {
            var response = await _http.DeleteAsync($"/machine-queue/job/{jobId}");
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
