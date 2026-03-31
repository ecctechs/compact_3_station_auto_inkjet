namespace InkjetOperator.Models
{
    public class CreateJobRequest
    {
        public string BarcodeRaw { get; set; }
        public string OrderNo { get; set; }
        public string CustomerName { get; set; }
        public string Type { get; set; }
        public int Qty { get; set; }
        public string CreatedBy { get; set; }
    }
}