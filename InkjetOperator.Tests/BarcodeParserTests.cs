using InkjetOperator.Services;

namespace InkjetOperator.Tests
{
    public class BarcodeParserTests
    {
        [Theory]
        [InlineData("ABC-001-230601", "ABC-001", "230601")]
        [InlineData("PATTERN-A-SUB-LOT99", "PATTERN-A-SUB", "LOT99")]
        [InlineData("SIMPLE-12345", "SIMPLE", "12345")]
        [InlineData("A-B-C-D", "A-B-C", "D")]
        public void GetPatternNo_And_GetLotNo_SplitCorrectly(
            string barcode, string expectedPattern, string expectedLot)
        {
            // Act
            string pattern = BarcodeParser.GetPatternNo(barcode);
            string lot = BarcodeParser.GetLotNo(barcode);

            // Assert
            Assert.Equal(expectedPattern, pattern);
            Assert.Equal(expectedLot, lot);
        }

        [Fact]
        public void GetPatternNo_NoDash_ReturnsFullBarcode()
        {
            string result = BarcodeParser.GetPatternNo("ABC001");
            Assert.Equal("ABC001", result);
        }

        [Fact]
        public void GetLotNo_NoDash_ReturnsEmpty()
        {
            string result = BarcodeParser.GetLotNo("ABC001");
            Assert.Equal("", result);
        }

        [Fact]
        public void GetPatternNo_StartsWithDash_ReturnsFullBarcode()
        {
            Assert.Equal("-ABC", BarcodeParser.GetPatternNo("-ABC"));
        }

        [Fact]
        public void GetLotNo_EndsWithDash_ReturnsEmpty()
        {
            Assert.Equal("", BarcodeParser.GetLotNo("ABC-"));
        }

        [Fact]
        public void GetPatternNo_OnlyDash_ReturnsFullBarcode()
        {
            Assert.Equal("-", BarcodeParser.GetPatternNo("-"));
        }

        [Fact]
        public void GetLotNo_OnlyDash_ReturnsEmpty()
        {
            Assert.Equal("", BarcodeParser.GetLotNo("-"));
        }

        [Fact]
        public void GetPatternNo_DashAtStart_ReturnsEmpty()
        {
            // "-LOT" → lastDashIndex=0 → <=0 → return full barcode
            Assert.Equal("-LOT", BarcodeParser.GetPatternNo("-LOT"));
        }

        [Fact]
        public void GetLotNo_DashAtStart_ReturnsAfterDash()
        {
            // "-LOT" → lastDashIndex=0 → not <0, not >= length-1 → "LOT"
            Assert.Equal("LOT", BarcodeParser.GetLotNo("-LOT"));
        }
    }
}
