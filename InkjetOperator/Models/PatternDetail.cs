using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InkjetOperator.Models
{
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
}
