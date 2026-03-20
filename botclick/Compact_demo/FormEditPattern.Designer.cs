using System.Windows.Forms;

namespace BotClickApp
{
    partial class FormEditPattern
    {
        private System.ComponentModel.IContainer components = null;
        private PatternEditorControl patternEditorControl1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.patternEditorControl1 = new BotClickApp.PatternEditorControl();
            this.SuspendLayout();
            // 
            // patternEditorControl1
            // 
            this.patternEditorControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patternEditorControl1.Location = new System.Drawing.Point(0, 0);
            this.patternEditorControl1.Name = "patternEditorControl1";
            this.patternEditorControl1.Size = new System.Drawing.Size(900, 545);
            this.patternEditorControl1.TabIndex = 0;
            // 
            // FormEditPattern
            // 
            this.ClientSize = new System.Drawing.Size(900, 545);
            this.Controls.Add(this.patternEditorControl1);
            this.Name = "FormEditPattern";
            this.Text = "Edit Pattern";
            this.ResumeLayout(false);

        }
    }
}
