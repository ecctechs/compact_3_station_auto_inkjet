using System.Text.Json.Serialization;

namespace InkjetOperator.Models
{
    public class UVinkjet
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("print_jobs_id")]
        public int PrintJobsId { get; set; } // เพิ่มเพื่อเชื่อมโยงกับ Job

        [JsonPropertyName("inkjet_name")]
        public string InkjetName { get; set; } = "";

        [JsonPropertyName("lot")]
        public string Lot { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("program_name")]
        public string ProgramName { get; set; } = "";

        [JsonPropertyName("status")]
        public string Status { get; set; } = ""; // เพิ่มเพื่อระบุสถานะ เช่น "printing"

        [JsonPropertyName("station")]
        public string Station { get; set; } = ""; // เพิ่มเพื่อระบุสถานีเครื่องจักร

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
