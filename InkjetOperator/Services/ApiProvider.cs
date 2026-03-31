using System.Diagnostics;
using InkjetOperator.Services;

namespace InkjetOperator
{
    public static class ApiProvider
    {
        public static ApiClient Instance { get; private set; }

        public static void Init()
        {
            Instance = new ApiClient(AppConfig.ApiUrl);
        }
    }
}