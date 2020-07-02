namespace Bluehands.Repository.Diagnostics
{
    partial class FindDialog
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
            this.components = new System.ComponentModel.Container();
            this.txtPattern = new System.Windows.Forms.TextBox();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdAbort = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtRunTime = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.m_ErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.btnCount = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.m_ErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // txtPattern
            // 
            this.txtPattern.Location = new System.Drawing.Point(181, 11);
            this.txtPattern.Name = "txtPattern";
            this.txtPattern.Size = new System.Drawing.Size(244, 20);
            this.txtPattern.TabIndex = 0;
            this.txtPattern.TextChanged += new System.EventHandler(this.TxtPattern_TextChanged);
            // 
            // cmdOK
            // 
            this.cmdOK.CausesValidation = false;
            this.cmdOK.Location = new System.Drawing.Point(181, 84);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 2;
            this.cmdOK.Text = "Find";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdAbort
            // 
            this.cmdAbort.CausesValidation = false;
            this.cmdAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdAbort.Location = new System.Drawing.Point(350, 84);
            this.cmdAbort.Name = "cmdAbort";
            this.cmdAbort.Size = new System.Drawing.Size(75, 23);
            this.cmdAbort.TabIndex = 4;
            this.cmdAbort.Text = "Close";
            this.cmdAbort.UseVisualStyleBackColor = true;
            this.cmdAbort.Click += new System.EventHandler(this.cmdAbort_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(147, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Search in Module / Message:";
            // 
            // txtRunTime
            // 
            this.txtRunTime.Location = new System.Drawing.Point(181, 46);
            this.txtRunTime.Name = "txtRunTime";
            this.txtRunTime.Size = new System.Drawing.Size(244, 20);
            this.txtRunTime.TabIndex = 1;
            this.txtRunTime.TextChanged += new System.EventHandler(this.TxtRuntime_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "RunTime at least (ms):";
            // 
            // m_ErrorProvider
            // 
            this.m_ErrorProvider.ContainerControl = this;
            // 
            // btnCount
            // 
            this.btnCount.CausesValidation = false;
            this.btnCount.Location = new System.Drawing.Point(266, 84);
            this.btnCount.Name = "btnCount";
            this.btnCount.Size = new System.Drawing.Size(75, 23);
            this.btnCount.TabIndex = 3;
            this.btnCount.Text = "Count";
            this.btnCount.UseVisualStyleBackColor = true;
            this.btnCount.Click += new System.EventHandler(this.btnCount_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblMessage.Location = new System.Drawing.Point(12, 87);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.ReadOnly = true;
            this.lblMessage.Size = new System.Drawing.Size(163, 13);
            this.lblMessage.TabIndex = 1;
            this.lblMessage.TabStop = false;
            this.lblMessage.TextChanged += new System.EventHandler(this.TxtRuntime_TextChanged);
            // 
            // FindDialog
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdAbort;
            this.ClientSize = new System.Drawing.Size(444, 121);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdAbort);
            this.Controls.Add(this.btnCount);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.txtRunTime);
            this.Controls.Add(this.txtPattern);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find";
            this.Load += new System.EventHandler(this.FindDialog_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FindDialog_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.m_ErrorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtPattern;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdAbort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRunTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ErrorProvider m_ErrorProvider;
        private System.Windows.Forms.Button btnCount;
        private System.Windows.Forms.TextBox lblMessage;
    }
}