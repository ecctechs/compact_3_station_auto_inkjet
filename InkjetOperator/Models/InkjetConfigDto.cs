using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InkjetOperator.Models
{
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
}
