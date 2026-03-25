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

    [JsonPropertyName("pattern_id")]
    public int? PatternId { get; set; }

    [JsonPropertyName("lot_number")]
    public string? LotNumber { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "pending";

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("created_by")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("attempt")]
    public int Attempt { get; set; }

    [JsonPropertyName("warning")]
    public string? Warning { get; set; }
}

// --- Pattern models (from GET /job/getResolved) ---

public class ResolvedJobResponse
{
    [JsonPropertyName("job")]
    public PrintJob Job { get; set; } = new();

    [JsonPropertyName("pattern")]
    public PatternDetail Pattern { get; set; } = new();
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
    public int? PostAct { get; set; }

    [JsonPropertyName("delay")]
    public int? Delay { get; set; }

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

// เพิ่ม DTO สำหรับสร้าง job
public class CreateJobRequest
{
    [JsonPropertyName("barcode_raw")]
    public string BarcodeRaw { get; set; }

    [JsonPropertyName("created_by")]
    public string CreatedBy { get; set; }

    [JsonPropertyName("order_no")]
    public string OrderNo { get; set; }

    [JsonPropertyName("customer_name")]
    public string CustomerName { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("qty")]
    public int Qty { get; set; }
}