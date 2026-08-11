using System.Text.Json.Serialization;

namespace InkjetOperator.Models;

/// <summary>
/// Standard backend response wrapper — matches ResponseManager { statusCode, data }.
/// </summary>
public class ApiResponse<T>
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

public class PaginatedResult<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

// --- Job models ---

public class PrintJob
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("barcode_raw")]
    public string BarcodeRaw { get; set; } = "";

    [JsonPropertyName("lot_number")]
    public string? LotNumber { get; set; }

    [JsonPropertyName("pattern_no_erp")]
    public string? PatternNoErp { get; set; }

    [JsonPropertyName("order_no")]
    public string? OrderNo { get; set; }

    [JsonPropertyName("customer_name")]
    public string? CustomerName { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("qty")]
    public int? Qty { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Waiting";

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("created_by")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("attempt")]
    public int Attempt { get; set; }

    [JsonPropertyName("warning")]
    public string? Warning { get; set; }

    [JsonPropertyName("st_status")]
    public string? StStatus { get; set; }
}

// --- Request DTOs ---

public class CreateJobRequest
{
    [JsonPropertyName("barcode_raw")]
    public string BarcodeRaw { get; set; } = "";

    [JsonPropertyName("created_by")]
    public string CreatedBy { get; set; } = "operator";

    [JsonPropertyName("order_no")]
    public string? OrderNo { get; set; }

    [JsonPropertyName("customer_name")]
    public string? CustomerName { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("qty")]
    public int? Qty { get; set; }

    [JsonPropertyName("st_status")]
    public string StStatus { get; set; } = "0";
}

public class CreatePatternRequest
{
    [JsonPropertyName("barcode")]
    public string Barcode { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("job_id")]
    public int JobId { get; set; }

    [JsonPropertyName("inkjet_configs")]
    public List<InkjetConfigDto> InkjetConfigs { get; set; } = new();

    [JsonPropertyName("conveyor_speeds")]
    public ConveyorSpeedDto? ConveyorSpeeds { get; set; }

    [JsonPropertyName("servo_configs")]
    public List<ServoConfigDto> ServoConfigs { get; set; } = new();
}

public class CreateUvJobRequest
{
    [JsonPropertyName("print_jobs_id")]
    public int PrintJobsId { get; set; }

    [JsonPropertyName("items")]
    public List<UvJobItem> Items { get; set; } = new();
}

public class UvJobItem
{
    [JsonPropertyName("machine")]
    public string Machine { get; set; } = "";

    [JsonPropertyName("table_name")]
    public string? TableName { get; set; }

    [JsonPropertyName("program_name")]
    public string? ProgramName { get; set; }

    [JsonPropertyName("lot")]
    public string? Lot { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("text1")]
    public string? Text1 { get; set; }

    [JsonPropertyName("text2")]
    public string? Text2 { get; set; }

    [JsonPropertyName("text3")]
    public string? Text3 { get; set; }

    [JsonPropertyName("text4")]
    public string? Text4 { get; set; }

    [JsonPropertyName("text5")]
    public string? Text5 { get; set; }
}

// plan_routing ที่อ่านมาจาก source DB ตอน register — เก็บค่าดิบ ไม่ตีความ
public class CreatePlanRoutingRequest
{
    [JsonPropertyName("print_jobs_id")]
    public int PrintJobsId { get; set; }

    [JsonPropertyName("lot_no")]
    public string? LotNo { get; set; }

    [JsonPropertyName("erp_mfg")]
    public string? ErpMfg { get; set; }

    [JsonPropertyName("marking_method")]
    public string? MarkingMethod { get; set; }

    [JsonPropertyName("process_sequence")]
    public string? ProcessSequence { get; set; }
}

// --- Pattern models (from GET /job/getResolved) ---

public class ResolvedJobResponse
{
    [JsonPropertyName("job")]
    public PrintJob Job { get; set; } = new();

    [JsonPropertyName("pattern")]
    public PatternDetail Pattern { get; set; } = new();

    [JsonPropertyName("plan_routing")]
    public PlanRoutingDto? PlanRouting { get; set; }
}

public class PlanRoutingDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("print_jobs_id")]
    public int? PrintJobsId { get; set; }

    [JsonPropertyName("lot_no")]
    public string? LotNo { get; set; }

    [JsonPropertyName("erp_mfg")]
    public string? ErpMfg { get; set; }

    [JsonPropertyName("marking_method")]
    public string? MarkingMethod { get; set; }

    [JsonPropertyName("process_sequence")]
    public string? ProcessSequence { get; set; }
}

public class PatternDetail
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("barcode")]
    public string Barcode { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("inkjet_configs")]
    public List<InkjetConfigDto> InkjetConfigs { get; set; } = new();

    [JsonPropertyName("conveyor_speeds")]
    public ConveyorSpeedDto? ConveyorSpeeds { get; set; }

    [JsonPropertyName("servo_configs")]
    public List<ServoConfigDto> ServoConfigs { get; set; } = new();
}

public class InkjetConfigDto
{
    [JsonPropertyName("ordinal")]
    public int Ordinal { get; set; }

    [JsonPropertyName("program_number")]
    public int? ProgramNumber { get; set; }

    [JsonPropertyName("program_name")]
    public string? ProgramName { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("trigger_delay")]
    public int? TriggerDelay { get; set; }

    [JsonPropertyName("pos_act")]
    public int? PosAct { get; set; }

    [JsonPropertyName("delay")]
    public int? Delay { get; set; }

    [JsonPropertyName("direction")]
    public int? Direction { get; set; }

    [JsonPropertyName("steel_type")]
    public string? SteelType { get; set; }

    [JsonPropertyName("suspended")]
    public bool Suspended { get; set; }

    [JsonPropertyName("text_blocks")]
    public List<TextBlockDto> TextBlocks { get; set; } = new();
}

public class TextBlockDto
{
    [JsonPropertyName("block_number")]
    public int BlockNumber { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("x")]
    public int? X { get; set; }

    [JsonPropertyName("y")]
    public int? Y { get; set; }

    [JsonPropertyName("size")]
    public int? Size { get; set; }

    [JsonPropertyName("scale")]
    public int? Scale { get; set; }
}

public class ConveyorSpeedDto
{
    [JsonPropertyName("speed1")]
    public int? Speed1 { get; set; }

    [JsonPropertyName("speed2")]
    public int? Speed2 { get; set; }

    [JsonPropertyName("speed3")]
    public int? Speed3 { get; set; }
}

public class ServoConfigDto
{
    [JsonPropertyName("ordinal")]
    public int Ordinal { get; set; }

    [JsonPropertyName("position")]
    public int? Position { get; set; }

    [JsonPropertyName("post_act")]
    public double? PostAct { get; set; }

    [JsonPropertyName("delay")]
    public double? Delay { get; set; }

    [JsonPropertyName("trigger")]
    public int? Trigger { get; set; }
}

// --- Command result (POST /job/postResults) ---

public class JobResultsPayload
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("commands")]
    public List<CommandResult> Commands { get; set; } = new();
}

public class CommandResult
{
    [JsonPropertyName("ordinal")]
    public int? Ordinal { get; set; }

    [JsonPropertyName("command")]
    public string Command { get; set; } = "";

    [JsonPropertyName("payload")]
    public Dictionary<string, object>? Payload { get; set; }

    [JsonPropertyName("response")]
    public string? Response { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("sent_at")]
    public string? SentAt { get; set; }
}
