using InkjetOperator.Services;

namespace InkjetOperator.Views;

public partial class OrderListUserControl
{
    private bool _recovering;

    private async Task RecoverSelectedQueueAsync()
    {
        if (_api == null || _recovering || _sending || _refreshing || _rowBusy || _preparingPrograms ||
            _pushHandling.Count > 0 || StationService.IsSt3) return;
        if (_selectedJobId is not int jobId)
        {
            Notify.WarnModal(this, "ตรวจคิวค้าง", "เลือกแถวงานที่ต้องการตรวจก่อน");
            return;
        }
        _recovering = true;
        _preparingPrograms = true; // หยุด dispatcher ในโปรแกรมนี้ระหว่างตรวจ
        try
        {
            var (rows, error) = await _api.GetMachineQueueAsync();
            if (IsDisposed) return;
            if (error != null) { Notify.ErrorDetail(this, "โหลดคิวไม่ได้", error); return; }
            var pending = rows.Where(r => r.PrintJobsId == jobId && r.NeedsSendReview).ToList();
            if (pending.Count == 0) { Notify.WarnModal(this, "ตรวจคิวค้าง", "งานนี้ไม่มีคิวที่รอตรวจผลส่ง"); return; }
            foreach (var row in pending)
            {
                if (MachineBusy.IsBusy(row.Machine) || _dispatchingMachines.ContainsKey(row.Machine)) return;
                if (string.IsNullOrWhiteSpace(row.DispatchToken))
                {
                    Notify.ErrorDetail(this, "ตรวจคิวไม่ได้", "Backend ยังไม่รองรับการตรวจคิว กรุณาอัปเดตให้เป็นรุ่นเดียวกัน");
                    return;
                }
                using var content = new QueueRecoveryUserControl();
                content.SetJob($"{JobName(jobId)}\n{row.Machine} · รอบ {row.Round} · คิว {row.Id}");
                var config = new AntdUI.Modal.Config(FindForm()!, "ตรวจผลเครื่องก่อนกู้คิว", content)
                {
                    OkText = "บันทึกผลตรวจ", CancelText = "ยกเลิก",
                    OnOk = _ => content.InvokeRequired
                        ? (bool)content.Invoke(new Func<bool>(content.ValidateRecovery))
                        : content.ValidateRecovery(),
                };
                if (AntdUI.Modal.open(config) != DialogResult.OK || IsDisposed) return;
                var result = await _api.RecoverQueueAsync(row.Id, row.DispatchToken, Guid.NewGuid().ToString(),
                    content.Outcome, content.OperatorName, content.Reason);
                if (IsDisposed) return;
                if (!result.ok)
                {
                    Notify.ErrorDetail(this, "ยังยืนยันผลไม่ได้", result.error + "\nโหลดคิวใหม่ก่อนทำต่อ ห้ามส่งซ้ำจากข้อความนี้");
                    return;
                }
                _reportedUncertainQueues.Remove(row.Id);
            }
        }
        finally
        {
            _preparingPrograms = false;
            _recovering = false;
            if (!IsDisposed) await RefreshDataAsync(force: true);
        }
    }
}
