namespace Bluehands.Repository.Diagnostics
{
    partial class LogItemDetailView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.m_Output = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // m_Output
            // 
            this.m_Output.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_Output.Location = new System.Drawing.Point(0, 0);
            this.m_Output.Name = "m_Output";
            this.m_Output.ReadOnly = true;
            this.m_Output.Size = new System.Drawing.Size(663, 46);
            this.m_Output.TabIndex = 0;
            this.m_Output.Text = "";
            // 
            // LogItemDetailView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(663, 46);
            this.Controls.Add(this.m_Output);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "LogItemDetailView";
            this.Text = "LogItemDetailView";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.LogItemDetailView_KeyUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox m_Output;
    }
}