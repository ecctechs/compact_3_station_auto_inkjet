using System;
using System.Text.Json.Serialization;

namespace InkjetOperator.Models
{
    /// <summary>รายละเอียดเครื่อง UV ต่อ 1 งาน (UV1/UV2) — query จาก print_data ตอน register แล้วเก็บ backend</summary>
    public class UvJobData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("print_jobs_id")]
        public int PrintJobsId { get; set; }

        [JsonPropertyName("machine")]
        public string Machine { get; set; } = "";      // UV1 / UV2

        [JsonPropertyName("table_name")]
        public string TableName { get; set; } = "";     // MK063 / MK067

        [JsonPropertyName("program_name")]
        public string ProgramName { get; set; } = "";

        [JsonPropertyName("lot")]
        public string Lot { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

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

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
