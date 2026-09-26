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
        // ได้คิวล่าสุดแล้ว ใช้ผลจาก Backend แทนข้อความชั่วคราวของรอบก่อน
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

    private AntdUI.CellTag[] BuildMachineStatus(PrintJob job)
    {
        var tags = new List<AntdUI.CellTag>();
        var steps = MarkingMethodService.Resolve(job.PlanRouting?.MarkingMethod).Steps;
        foreach (var machine in steps.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var rows = _queueRows.Where(r => r.PrintJobsId == job.Id && r.Machine == machine)
                .Concat(_machineStatus.Values.Where(s => s.Row.PrintJobsId == job.Id && s.Row.Machine == machine).Select(s => s.Row))
                .GroupBy(r => r.Id).Select(g => g.First()).OrderBy(r => r.Round).ToList();
            if (rows.Count == 0)
            {
                // ประวัติส่งเก่าอาจเป็นงานก่อน Restore จึงใช้ยืนยันรอบปัจจุบันไม่ได้
                tags.Add(new AntdUI.CellTag($"{machine} · {(job.Status == "Waiting" ? "ยังไม่เข้าคิว" : "ไม่มีคิวปัจจุบัน")}",
                    AntdUI.TTypeMini.Default));
                continue;
            }
            foreach (var row in rows)
            {
                var (text, type) = QueueStatus(row);
                string round = steps.Count(s => s == machine) > 1 ? $" รอบ {row.Round}" : "";
                tags.Add(new AntdUI.CellTag($"{machine}{round} · {text}", type));
            }
        }
        return tags.Count == 0 ? [new AntdUI.CellTag("ไม่ใช้เครื่องพิมพ์")] : tags.ToArray();
    }

    private (string Text, AntdUI.TTypeMini Type) QueueStatus(MachineQueueRow row)
    {
        if (row.State == "done") return ("ปล่อยคิวแล้ว", AntdUI.TTypeMini.Default);
        if (row.SentAt != null) return ("ส่งแล้ว", AntdUI.TTypeMini.Success);
        if (_machineStatus.TryGetValue(row.Id, out var local)) return (local.Text, local.Type);
        // sending จาก Backend อย่างเดียวไม่ยืนยันว่าโปรแกรมต้นทางยังทำงานอยู่
        if (row.NeedsSendReview) return ("กำลังส่ง / รอตรวจสอบ", AntdUI.TTypeMini.Warn);
        if (row.DispatchState == "not_sent") return ("ยังไม่ส่ง / ส่งไม่สำเร็จ", AntdUI.TTypeMini.Error);
        return row.State == "active"
            ? ("ถึงคิว รอส่ง", AntdUI.TTypeMini.Primary)
            : ("รอคิว", AntdUI.TTypeMini.Warn);
    }
}
