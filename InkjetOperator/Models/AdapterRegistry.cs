using InkjetOperator.Adapters;

namespace InkjetOperator
{
    public static class AdapterRegistry
    {
        public static IInkjetAdapter? MK058 { get; set; }
        public static IInkjetAdapter? MK059 { get; set; }
        public static IInkjetAdapter? MK060 { get; set; }
        public static IInkjetAdapter? MK061 { get; set; }

        public static IInkjetAdapter?[] AllMk =>
            new[] { MK058, MK059, MK060, MK061 };
    }
}