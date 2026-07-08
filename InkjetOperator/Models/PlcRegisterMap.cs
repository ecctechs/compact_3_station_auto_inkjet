using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InkjetOperator.Models
{
    public class PlcRegisterMap
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("address_start")]
        public int AddressStart { get; set; }

        [JsonPropertyName("address_stop")]
        public int AddressStop { get; set; }

        [JsonPropertyName("plc_start")]
        public string PlcStart { get; set; } = "";

        [JsonPropertyName("plc_stop")]
        public string PlcStop { get; set; } = "";

        [JsonPropertyName("list_name")]
        public string ListName { get; set; } = "";

        [JsonPropertyName("data_type")]
        public string DataType { get; set; } = "Int";

        [JsonPropertyName("bit")]
        public int Bit { get; set; } = 32;

        [JsonPropertyName("sort_order")]
        public int SortOrder { get; set; }
    }

    /// <summary>Body ของ POST /plc-setting/bulkSave</summary>
    public class PlcBulkSaveRequest
    {
        [JsonPropertyName("rows")]
        public List<PlcRegisterMap> Rows { get; set; } = new();
    }
}
