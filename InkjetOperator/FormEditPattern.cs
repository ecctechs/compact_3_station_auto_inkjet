using System.Drawing;
using System.Windows.Forms;
using InkjetOperator.Controls;

namespace InkjetOperator
{
    public class FormEditPattern : Form
    {
        public FormEditPattern()
        {
            this.Text = "Pattern Editor";
            this.Size = new Size(900, 545);
            this.MinimumSize = new Size(700, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            var editor = new PatternEditorControl
            {
                Dock = DockStyle.Fill,
            };

            this.Controls.Add(editor);
        }
    }
}
