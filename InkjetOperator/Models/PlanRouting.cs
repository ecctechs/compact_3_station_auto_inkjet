using System.Text.Json.Serialization;

namespace InkjetOperator.Models
{
    /// <summary>
    /// ข้อมูล plan_routing (อ่านจาก source DB ตอน scan) — ใช้ตัดสินใจส่งงานให้ถูกเครื่อง
    ///
    /// marking_method = เลข 2 หลัก [Shim][Plate] แต่ละหลัก: 0=ไม่ทำ, 1=UV Print, 2=Inkjet
    ///   หลักที่ 1 (Shim)  → 1=UV2/MK067,  2=Inkjet2/MK059  (ordinal 2)
    ///   หลักที่ 2 (Plate) → 1=UV1/MK063,  2=Inkjet1/MK058  (ordinal 1)
    /// </summary>
    public class PlanRouting
    {
        [JsonPropertyName("lot_no")]
        public string LotNo { get; set; } = "";

        [JsonPropertyName("erp_mfg")]
        public string? ErpMfg { get; set; }

        [JsonPropertyName("marking_method")]
        public string? MarkingMethod { get; set; }

        [JsonPropertyName("process_sequence")]
        public string? ProcessSequence { get; set; }

        // ── decoder ─────────────────────────────────────────────
        // normalize marking เป็น 2 หลักก่อน (pad ซ้ายด้วย 0) กันเคสมาเป็น int เช่น "2" → "02"

        /// <summary>marking_method ที่ normalize เป็น 2 หลักแล้ว (ค่าอื่น/ว่าง → "00")</summary>
        [JsonIgnore]
        public string NormalizedMarking
        {
            get
            {
                string m = (MarkingMethod ?? "").Trim();
                if (m.Length == 0) return "00";
                if (m.Length == 1) return "0" + m;          // "2" → "02"
                if (m.Length > 2) return m.Substring(m.Length - 2); // เอา 2 หลักท้าย
                return m;
            }
        }

        /// <summary>หลักที่ 1 = Shim</summary>
        [JsonIgnore]
        public char ShimDigit => NormalizedMarking[0];

        /// <summary>หลักที่ 2 = Plate</summary>
        [JsonIgnore]
        public char PlateDigit => NormalizedMarking[1];

        /// <summary>Plate → UV1/MK063 (marking[1] == '1')</summary>
        [JsonIgnore]
        public bool SendUv1 => PlateDigit == '1';

        /// <summary>Shim → UV2/MK067 (marking[0] == '1')</summary>
        [JsonIgnore]
        public bool SendUv2 => ShimDigit == '1';

        /// <summary>Plate → Inkjet1/MK058 (marking[1] == '2')</summary>
        [JsonIgnore]
        public bool SendInkjet1 => PlateDigit == '2';

        /// <summary>Shim → Inkjet2/MK059 (marking[0] == '2')</summary>
        [JsonIgnore]
        public bool SendInkjet2 => ShimDigit == '2';

        /// <summary>งานนี้ต้องใช้ inkjet ไหม (หลักใดหลักหนึ่ง = 2)</summary>
        [JsonIgnore]
        public bool NeedInkjet => SendInkjet1 || SendInkjet2;

        /// <summary>งานนี้ต้องใช้ UV ไหม</summary>
        [JsonIgnore]
        public bool NeedUv => SendUv1 || SendUv2;

        /// <summary>มี marking อย่างน้อย 1 สถานีไหม (ไม่ใช่ "00")</summary>
        [JsonIgnore]
        public bool HasAnyMarking => NormalizedMarking != "00";
    }
}
