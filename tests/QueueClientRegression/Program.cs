using InkjetOperator.Models;
using InkjetOperator.Services;

await ParallelSendRegression.RunAsync();

var uri = new Uri(Environment.GetEnvironmentVariable("QUEUE_TEST_API_URL") ?? "http://invalid");
if (!uri.IsLoopback || uri.Port == 3000) throw new Exception("Use only the isolated test API");
int jobId = int.Parse(Environment.GetEnvironmentVariable("QUEUE_TEST_JOB_ID")!);
var api = new ApiClient(uri.ToString());
void Check(bool ok, string message) { if (!ok) throw new Exception(message); }

Check(MarkingMethodService.Resolve("11").Steps.SequenceEqual(new[] { "UV1", "UV2" }), "11 flow changed");
Check(MarkingMethodService.Resolve("12").Steps.SequenceEqual(new[] { "MK", "UV2" }), "12 flow changed");
Check(MarkingMethodService.Resolve("32").Steps.SequenceEqual(new[] { "MK", "UV2" }), "32 flow changed");
Check(MarkingMethodService.Resolve("22").Steps.SequenceEqual(new[] { "MK", "MK" }), "22 flow changed");

Check((await api.EnqueueMachinesAsync(jobId, [new() { Machine = "MK" }])).ok, "enqueue failed");
// ส่ง null จริงเมื่อเครื่องว่าง ไม่ให้ serializer ตัดจน Backend ปฏิเสธ
var (release, releaseError) = await api.ReleaseMachineAsync("MK", null);
Check(release?.Next != null, "release idle machine: " + releaseError);
int id = release!.Next!.Id;
var token = Guid.NewGuid().ToString();
Check((await api.BeginQueueSendAsync(id, token)).ok, "begin failed");
Check(!(await api.BeginQueueSendAsync(id, token)).ok, "duplicate begin accepted");
var (rows, error) = await api.GetMachineQueueAsync();
Check(error == null && rows.Single().NeedsSendReview, "dispatch state not read");
Check(JobStageService.Describe(jobId, "02", rows) == "กำลังส่ง / รอตรวจสอบผล", "review not visible");
Check((await api.ReleaseMachineAsync("MK", id)).result == null, "uncertain send released");
Check(!(await api.ClearMachineQueueAsync(jobId, onlyUnsent: true)).ok, "unsafe cleanup accepted");
Check((await api.FinishQueueSendAsync(id, token, "sent", new { program = "test" })).ok, "finish failed");
Check((await api.FinishQueueSendAsync(id, token, "sent", new { program = "test" })).ok, "finish retry failed");
(rows, error) = await api.GetMachineQueueAsync();
Check(error == null && rows.Single().SentAt != null && !rows.Single().NeedsSendReview, "success not read");
Check((await api.ReleaseMachineAsync("MK", id)).result?.Released?.Id == id, "release failed");
Check((await api.ReleaseMachineAsync("MK", id)).result == null, "stale release accepted");
Console.WriteLine("PASS: real C# API client, queue lifecycle and marking 11/12/32/22");
