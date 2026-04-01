using System.Text.Json.Serialization;

namespace InkjetOperator.Models
{
    public class UVinkjet
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("inkjet_name")]
        public string InkjetName { get; set; } = "";

        [JsonPropertyName("lot")]
        public string lot { get; set; } = "";

        [JsonPropertyName("name")]
        public string name { get; set; } = "";

        [JsonPropertyName("program_name")]
        public string program_name { get; set; } = "";

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
