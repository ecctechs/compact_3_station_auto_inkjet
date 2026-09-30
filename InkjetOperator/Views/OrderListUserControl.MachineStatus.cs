using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

public partial class OrderListUserControl
{
    private List<OrderRow> _displayRows = [];
    private readonly Dictionary<int, (MachineQueueRow Row, string Text, AntdUI.TTypeMini Type)> _machineStatus = [];
    private readonly HashSet<int> _activeStatusQueues = [];

    private void ReconcileMachineStatus()
    {
        foreach (int id in _machineStatus.Keys.Where(id => !_activeStatusQueues.Contains(id)).ToArray())
            _machineStatus.Remove(id);
    }

    private void SetMachineStatus(MachineQueueRow row, string text, AntdUI.TTypeMini type)
    {
        _machineStatus[row.Id] = (row, text, type);
        UpdateMachineStatusCells();
    }

    private void UpdateMachineStatusCells()
    {
        if (IsDisposed) return;
        foreach (var row in _displayRows)
        {
            var job = _allJobs.FirstOrDefault(j => j.Id == row.Id);
            if (job != null) row.MachineStatus = BuildMachineStatus(job);
        }
    }

    private AntdUI.CellTag[] BuildMachineStatus(PrintJob job) // สร้างป้ายสถานะเครื่องของ Job นี้
    {
        var tags = new List<AntdUI.CellTag>(); // เตรียมรวมป้ายแต่ละเครื่อง
        var steps = MarkingMethodService.Resolve(job.PlanRouting?.MarkingMethod).Steps; // อ่านเครื่องที่งานใช้จากรหัส Marking
        foreach (var machine in steps.Distinct(StringComparer.OrdinalIgnoreCase)) // สร้างป้ายแยกเครื่องโดยไม่วนชื่อซ้ำ
        {
            var rows = _queueRows.Where(r => r.PrintJobsId == job.Id && r.Machine == machine) // เลือกคิวของ Job และเครื่องนี้
                .Concat(_machineStatus.Values.Where(s => s.Row.PrintJobsId == job.Id && s.Row.Machine == machine).Select(s => s.Row)) // รวมคิวที่กำลังมีสถานะส่งบนจอ
                .GroupBy(r => r.Id).Select(g => g.First()).OrderBy(r => r.Round).ToList(); // ตัดคิวซ้ำแล้วเรียงตามรอบ
            if (rows.Count == 0) // ตรวจว่างานนี้ยังไม่มีคิวของเครื่อง
            {
                tags.Add(new AntdUI.CellTag($"{machine} · {(job.Status == "Waiting" ? "ยังไม่เข้าคิว" : "ไม่มีคิวปัจจุบัน")}", // แยกข้อความยังไม่เข้าคิวกับไม่มีคิวปัจจุบัน
                    AntdUI.TTypeMini.Default)); // ใช้สีป้ายสถานะทั่วไป
                continue; // ข้ามรายการนี้ไปตัวถัดไป
            }
            foreach (var row in rows) // สร้างป้ายตามแต่ละแถวคิว
            {
                var (text, type) = QueueStatus(row); // แปลงคิวเป็นข้อความและสีสถานะ
                string round = steps.Count(s => s == machine) > 1 ? $" รอบ {row.Round}" : ""; // งานหลายรอบเพิ่มเลขรอบในป้าย
                tags.Add(new AntdUI.CellTag($"{machine}{round} · {text}", type)); // ใส่ชื่อเครื่อง รอบ และสถานะลงป้าย
            }
        }
        return tags.Count == 0 ? [new AntdUI.CellTag("ไม่ใช้เครื่องพิมพ์")] : tags.ToArray(); // ไม่มีเครื่องในแผน ให้แสดงว่าไม่ใช้เครื่องพิมพ์
    }

    private static string SentStatus(string machine) => machine == "MK" ? "ส่งแล้ว" : "ส่งข้อมูลแล้ว";

    private (string Text, AntdUI.TTypeMini Type) QueueStatus(MachineQueueRow row)
    {
        if (row.State == "done") return ("ปล่อยคิวแล้ว", AntdUI.TTypeMini.Default);
        if (row.SentAt != null) return (SentStatus(row.Machine), AntdUI.TTypeMini.Success);
        if (_machineStatus.TryGetValue(row.Id, out var local)) return (local.Text, local.Type);
        if (row.NeedsSendReview) return ("กำลังส่ง / รอตรวจสอบ", AntdUI.TTypeMini.Warn);
        if (row.DispatchState == "not_sent") return ("ยังไม่ส่ง / ส่งไม่สำเร็จ", AntdUI.TTypeMini.Error);
        return row.State == "active"
            ? ("ถึงคิว รอส่ง", AntdUI.TTypeMini.Primary)
            : ("รอคิว", AntdUI.TTypeMini.Warn);
    }
}
