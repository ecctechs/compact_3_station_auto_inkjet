namespace InkjetOperator.Services
{
    public static class BarcodeParser
    {
        public static string GetPatternNo(string barcode)
        {
            int lastDashIndex = barcode.LastIndexOf('-');
            if (lastDashIndex <= 0)
                return barcode;

            return barcode.Substring(0, lastDashIndex);
        }

        public static string GetLotNo(string barcode)
        {
            int lastDashIndex = barcode.LastIndexOf('-');
            if (lastDashIndex < 0 || lastDashIndex >= barcode.Length - 1)
                return "";

            return barcode.Substring(lastDashIndex + 1);
        }
    }
}
