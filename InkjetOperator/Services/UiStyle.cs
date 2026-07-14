using System.Drawing;
using System.Windows.Forms;

namespace InkjetOperator.Services
{
    public static class UiStyle
    {
        /// <summary>สไตล์ตารางมาตรฐาน: ฟอนต์ 10pt แถวสูงพออ่าน หัวคอลัมน์กึ่งกลาง+สูงอัตโนมัติ</summary>
        public static void ApplyGrid(params DataGridView[] grids)
        {
            foreach (var dgv in grids)
            {
                dgv.Font = new Font("Segoe UI", 10F);
                dgv.RowTemplate.Height = 32;
                dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }
    }
}
