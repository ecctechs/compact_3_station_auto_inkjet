using InkjetOperator.Models;

namespace InkjetOperator.Tests
{
    /// <summary>
    /// Unit tests ของ decoder marking_method ใน PlanRouting
    /// marking = [Shim][Plate] : 0=ไม่ทำ, 1=UV, 2=Inkjet
    ///   SendUv1 = Plate=='1' (MK063), SendUv2 = Shim=='1' (MK067)
    ///   SendInkjet1 = Plate=='2' (MK058), SendInkjet2 = Shim=='2' (MK059)
    /// </summary>
    public class PlanRoutingTests
    {
        // marking, SendUv1(Plate), SendUv2(Shim), SendInkjet1(Plate), SendInkjet2(Shim), NeedInkjet, HasAnyMarking
        [Theory]
        [InlineData("00", false, false, false, false, false, false)]
        [InlineData("01", true, false, false, false, false, true)]   // Plate=UV
        [InlineData("02", false, false, true, false, true, true)]    // Plate=Inkjet
        [InlineData("10", false, true, false, false, false, true)]   // Shim=UV
        [InlineData("20", false, false, false, true, true, true)]    // Shim=Inkjet
        [InlineData("11", true, true, false, false, false, true)]    // ทั้งคู่ UV
        [InlineData("12", false, true, true, false, true, true)]     // Shim=UV, Plate=Inkjet
        [InlineData("21", true, false, false, true, true, true)]     // Shim=Inkjet, Plate=UV
        [InlineData("22", false, false, true, true, true, true)]     // ทั้งคู่ Inkjet
        public void Decode_MarkingMethod_MapsToCorrectMachines(
            string marking, bool uv1, bool uv2, bool ink1, bool ink2, bool needInkjet, bool hasAny)
        {
            var plan = new PlanRouting { MarkingMethod = marking };

            Assert.Equal(uv1, plan.SendUv1);
            Assert.Equal(uv2, plan.SendUv2);
            Assert.Equal(ink1, plan.SendInkjet1);
            Assert.Equal(ink2, plan.SendInkjet2);
            Assert.Equal(needInkjet, plan.NeedInkjet);
            Assert.Equal(hasAny, plan.HasAnyMarking);
        }

        [Fact]
        public void Decode_Null_TreatedAs00_NoCrash()
        {
            var plan = new PlanRouting { MarkingMethod = null };

            Assert.Equal("00", plan.NormalizedMarking);
            Assert.False(plan.SendUv1);
            Assert.False(plan.SendUv2);
            Assert.False(plan.SendInkjet1);
            Assert.False(plan.SendInkjet2);
            Assert.False(plan.HasAnyMarking);
        }

        [Theory]
        [InlineData("2", "02")]      // 1 หลัก → pad ซ้าย
        [InlineData("1", "01")]
        [InlineData(" 12 ", "12")]   // มี whitespace → trim
        [InlineData("", "00")]       // ว่าง → 00
        [InlineData("012", "12")]    // เกิน 2 หลัก → เอา 2 หลักท้าย
        public void Normalize_PadsAndTrimsTo2Digits(string input, string expected)
        {
            var plan = new PlanRouting { MarkingMethod = input };
            Assert.Equal(expected, plan.NormalizedMarking);
        }

        [Fact]
        public void SingldDigit2_RoutesToInkjet1()
        {
            // "2" → "02" → Plate=Inkjet1
            var plan = new PlanRouting { MarkingMethod = "2" };
            Assert.True(plan.SendInkjet1);
            Assert.True(plan.NeedInkjet);
            Assert.True(plan.HasAnyMarking);
        }
    }
}
