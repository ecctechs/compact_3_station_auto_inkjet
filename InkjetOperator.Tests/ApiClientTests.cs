using System.Net;
using System.Text;
using InkjetOperator.Models;
using InkjetOperator.Services;
using Moq;
using Moq.Protected;

namespace InkjetOperator.Tests
{
    public class ApiClientTests
    {
        private const string BaseUrl = "http://test.local:3000";

        // ── Helpers ─────────────────────────────────────────

        /// <summary>สร้าง handler ที่ตอบ status + json ที่กำหนด และเก็บ request ล่าสุดไว้ตรวจ</summary>
        private static (Mock<HttpMessageHandler> handler, List<HttpRequestMessage> requests, List<string> bodies)
            MockHandler(HttpStatusCode status, string json = "{}")
        {
            var requests = new List<HttpRequestMessage>();
            var bodies = new List<string>();
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(async (HttpRequestMessage req, CancellationToken _) =>
                {
                    requests.Add(req);
                    // อ่าน body ตอนนี้เลย เพราะ request อาจถูก dispose หลังจบ call
                    bodies.Add(req.Content != null ? await req.Content.ReadAsStringAsync() : "");
                    return new HttpResponseMessage(status)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                });
            return (handler, requests, bodies);
        }

        /// <summary>สร้าง handler ที่โยน exception (จำลอง timeout / network ล่ม)</summary>
        private static Mock<HttpMessageHandler> ThrowingHandler(Exception ex)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(ex);
            return handler;
        }

        private static ApiClient MakeClient(Mock<HttpMessageHandler> handler)
            => new ApiClient(BaseUrl, new HttpClient(handler.Object));

        private static CreateJobRequest SampleJobRequest() => new()
        {
            BarcodeRaw = "C240801-027",
            OrderNo = "ORD-001",
            CustomerName = "ACME",
            Type = "UV",
            Qty = 10,
            CreatedBy = "tester",
            st_status = "1",
        };

        // ── CreateJobAsync ──────────────────────────────────

        [Fact]
        public async Task CreateJobAsync_Success_ReturnsTrue()
        {
            var (handler, _, _) = MockHandler(HttpStatusCode.OK);
            var api = MakeClient(handler);

            Assert.True(await api.CreateJobAsync(SampleJobRequest()));
        }

        [Fact]
        public async Task CreateJobAsync_PostsToCorrectEndpoint()
        {
            var (handler, requests, _) = MockHandler(HttpStatusCode.OK);
            var api = MakeClient(handler);

            await api.CreateJobAsync(SampleJobRequest());

            var req = Assert.Single(requests);
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.Equal($"{BaseUrl}/job/create", req.RequestUri!.ToString());
        }

        [Fact]
        public async Task CreateJobAsync_SerializesBodyAsSnakeCase()
        {
            var (handler, _, bodies) = MockHandler(HttpStatusCode.OK);
            var api = MakeClient(handler);

            await api.CreateJobAsync(SampleJobRequest());

            string body = Assert.Single(bodies);
            // JsonOptions ใช้ SnakeCaseLower — ล็อก wire format ไว้กัน refactor เผลอเปลี่ยน
            Assert.Contains("\"barcode_raw\":\"C240801-027\"", body);
            Assert.Contains("\"order_no\":\"ORD-001\"", body);
            Assert.Contains("\"qty\":10", body);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.NotFound)]
        public async Task CreateJobAsync_ServerError_ReturnsFalse(HttpStatusCode status)
        {
            var (handler, _, _) = MockHandler(status);
            var api = MakeClient(handler);

            Assert.False(await api.CreateJobAsync(SampleJobRequest()));
        }

        [Fact]
        public async Task CreateJobAsync_Timeout_ReturnsFalse()
        {
            var handler = ThrowingHandler(new TaskCanceledException("timeout"));
            var api = MakeClient(handler);

            Assert.False(await api.CreateJobAsync(SampleJobRequest()));
        }

        [Fact]
        public async Task CreateJobAsync_NetworkError_ReturnsFalse()
        {
            var handler = ThrowingHandler(new HttpRequestException("connection refused"));
            var api = MakeClient(handler);

            Assert.False(await api.CreateJobAsync(SampleJobRequest()));
        }

        // ── GetPatternByBarcodeAsync ────────────────────────

        [Fact]
        public async Task GetPatternByBarcode_Found_ReturnsPatternDetail()
        {
            string json = """
                {
                  "statusCode": 200,
                  "data": {
                    "id": 5,
                    "barcode": "CCCC-01",
                    "description": "test pattern",
                    "inkjet_configs": [],
                    "servo_configs": []
                  }
                }
                """;
            var (handler, _, _) = MockHandler(HttpStatusCode.OK, json);
            var api = MakeClient(handler);

            var result = await api.GetPatternByBarcodeAsync("CCCC-01");

            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal("CCCC-01", result.Barcode);
            Assert.Equal("test pattern", result.Description);
        }

        [Fact]
        public async Task GetPatternByBarcode_CallsCorrectEndpoint()
        {
            var (handler, requests, _) = MockHandler(HttpStatusCode.OK, """{"statusCode":200,"data":null}""");
            var api = MakeClient(handler);

            await api.GetPatternByBarcodeAsync("ABC-001");

            var req = Assert.Single(requests);
            Assert.Equal(HttpMethod.Get, req.Method);
            Assert.Equal($"{BaseUrl}/pattern/lookup/ABC-001", req.RequestUri!.ToString());
        }

        [Fact]
        public async Task GetPatternByBarcode_SpecialChars_UrlEncoded()
        {
            var (handler, requests, _) = MockHandler(HttpStatusCode.OK, """{"statusCode":200,"data":null}""");
            var api = MakeClient(handler);

            await api.GetPatternByBarcodeAsync("AB/C 1");

            var req = Assert.Single(requests);
            // ใช้ AbsoluteUri เพราะ ToString() จะถอด %20 กลับเป็นช่องว่าง
            Assert.Equal($"{BaseUrl}/pattern/lookup/AB%2FC%201", req.RequestUri!.AbsoluteUri);
        }

        [Fact]
        public async Task GetPatternByBarcode_NotFound_ReturnsNull()
        {
            var (handler, _, _) = MockHandler(HttpStatusCode.NotFound);
            var api = MakeClient(handler);

            Assert.Null(await api.GetPatternByBarcodeAsync("UNKNOWN"));
        }

        [Fact]
        public async Task GetPatternByBarcode_ServerError_ReturnsNull()
        {
            var (handler, _, _) = MockHandler(HttpStatusCode.InternalServerError);
            var api = MakeClient(handler);

            Assert.Null(await api.GetPatternByBarcodeAsync("CCCC-01"));
        }

        [Fact]
        public async Task GetPatternByBarcode_NullData_ReturnsNull()
        {
            var (handler, _, _) = MockHandler(HttpStatusCode.OK, """{"statusCode":200,"data":null}""");
            var api = MakeClient(handler);

            Assert.Null(await api.GetPatternByBarcodeAsync("CCCC-01"));
        }

        [Fact]
        public async Task GetPatternByBarcode_Timeout_ReturnsNull()
        {
            var handler = ThrowingHandler(new TaskCanceledException("timeout"));
            var api = MakeClient(handler);

            Assert.Null(await api.GetPatternByBarcodeAsync("CCCC-01"));
        }

        [Fact]
        public async Task GetPatternByBarcode_MalformedJson_ReturnsNull()
        {
            var (handler, _, _) = MockHandler(HttpStatusCode.OK, "not-json-at-all");
            var api = MakeClient(handler);

            Assert.Null(await api.GetPatternByBarcodeAsync("CCCC-01"));
        }

        // ── PingAsync (ผ่าน injected client) ────────────────

        [Fact]
        public async Task Ping_Success_ReturnsTrue()
        {
            var (handler, _, _) = MockHandler(HttpStatusCode.OK);
            var api = MakeClient(handler);

            Assert.True(await api.PingAsync());
        }

        [Fact]
        public async Task Ping_CallsJobGetAllWaiting()
        {
            var (handler, requests, _) = MockHandler(HttpStatusCode.OK);
            var api = MakeClient(handler);

            await api.PingAsync();

            var req = Assert.Single(requests);
            Assert.Equal($"{BaseUrl}/job/getAll?status=Waiting", req.RequestUri!.ToString());
        }

        [Theory]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.NotFound)]
        public async Task Ping_ServerError_ReturnsFalse(HttpStatusCode status)
        {
            var (handler, _, _) = MockHandler(status);
            var api = MakeClient(handler);

            Assert.False(await api.PingAsync());
        }

        [Fact]
        public async Task Ping_Timeout_ReturnsFalse()
        {
            var handler = ThrowingHandler(new TaskCanceledException("timeout"));
            var api = MakeClient(handler);

            Assert.False(await api.PingAsync());
        }

        [Fact]
        public async Task Ping_NetworkError_ReturnsFalse()
        {
            var handler = ThrowingHandler(new HttpRequestException("no route to host"));
            var api = MakeClient(handler);

            Assert.False(await api.PingAsync());
        }

        // ── BaseUrl normalization ───────────────────────────

        [Fact]
        public async Task Constructor_TrailingSlash_Trimmed()
        {
            var (handler, requests, _) = MockHandler(HttpStatusCode.OK);
            var api = new ApiClient(BaseUrl + "/", new HttpClient(handler.Object));

            await api.PingAsync();

            var req = Assert.Single(requests);
            Assert.StartsWith($"{BaseUrl}/", req.RequestUri!.ToString());
        }
    }
}
