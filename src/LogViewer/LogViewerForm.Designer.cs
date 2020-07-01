namespace Bluehands.Repository.Diagnostics
{
    partial class LogViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogViewerForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeFilesWithCurrentFilterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.openFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.threadAnalyzerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toggleIndentionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleShowInfoColumnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiScrollDown = new System.Windows.Forms.ToolStripMenuItem();
            this.tabLayout = new System.Windows.Forms.TableLayoutPanel();
            this.txtLineNr = new System.Windows.Forms.TextBox();
            this.txtThreadIdFilter = new System.Windows.Forms.TextBox();
            this.txtInstanceFilter = new System.Windows.Forms.TextBox();
            this.txtTimeFilter = new System.Windows.Forms.TextBox();
            this.txtLevelFilter = new System.Windows.Forms.TextBox();
            this.txtMessageFilter = new System.Windows.Forms.TextBox();
            this.txtFilenameFilter = new System.Windows.Forms.TextBox();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.lmListView = new Bluehands.Repository.Diagnostics.LogMessageListView();
            this.logFileReader = new System.ComponentModel.BackgroundWorker();
            this.filterWorker = new System.ComponentModel.BackgroundWorker();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.jumpToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showMethodContentsOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.tabLayout.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.logToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(893, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.addToolStripMenuItem,
            this.mergeFilesWithCurrentFilterToolStripMenuItem,
            this.toolStripSeparator3,
            this.openFolderToolStripMenuItem,
            this.toolStripSeparator4,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.MiOpenClick);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.addToolStripMenuItem.Text = "&Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddToolStripMenuItemClick);
            // 
            // mergeFilesWithCurrentFilterToolStripMenuItem
            // 
            this.mergeFilesWithCurrentFilterToolStripMenuItem.Name = "mergeFilesWithCurrentFilterToolStripMenuItem";
            this.mergeFilesWithCurrentFilterToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.mergeFilesWithCurrentFilterToolStripMenuItem.Text = "&Merge files with current filter";
            this.mergeFilesWithCurrentFilterToolStripMenuItem.Click += new System.EventHandler(this.MergeFilesWithCurrentFilterClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(223, 6);
            // 
            // openFolderToolStripMenuItem
            // 
            this.openFolderToolStripMenuItem.Name = "openFolderToolStripMenuItem";
            this.openFolderToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.openFolderToolStripMenuItem.Text = "Open &Log Folder";
            this.openFolderToolStripMenuItem.Click += new System.EventHandler(this.OpenFolderToolStripMenuItemClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(223, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItemClick);
            // 
            // logToolStripMenuItem
            // 
            this.logToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem,
            this.searchMenuItem,
            this.findToolStripMenuItem,
            this.toolStripMenuItem1,
            this.refreshToolStripMenuItem,
            this.toolStripSeparator1,
            this.threadAnalyzerToolStripMenuItem,
            this.toolStripSeparator2,
            this.toggleIndentionToolStripMenuItem,
            this.toggleShowInfoColumnsToolStripMenuItem,
            this.tsmiScrollDown});
            this.logToolStripMenuItem.Name = "logToolStripMenuItem";
            this.logToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.logToolStripMenuItem.Text = "&Log";
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(261, 22);
            this.clearToolStripMenuItem.Text = "&Close all";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.ClearToolStripMenuItemClick);
            // 
            // searchMenuItem
            // 
            this.searchMenuItem.Name = "searchMenuItem";
            this.searchMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.searchMenuItem.Size = new System.Drawing.Size(261, 22);
            this.searchMenuItem.Text = "Find...";
            this.searchMenuItem.Click += new System.EventHandler(this.SearchMenuItemClick);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.findToolStripMenuItem.Size = new System.Drawing.Size(261, 22);
            this.findToolStripMenuItem.Text = "Find &Next";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.FindToolStripMenuItemClick);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F3)));
            this.toolStripMenuItem1.Size = new System.Drawing.Size(261, 22);
            this.toolStripMenuItem1.Text = "Find &Previous";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.FindPreviousToolStripMenuItemClick);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(261, 22);
            this.refreshToolStripMenuItem.Text = "&Refresh ";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.RefreshToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(258, 6);
            // 
            // threadAnalyzerToolStripMenuItem
            // 
            this.threadAnalyzerToolStripMenuItem.Name = "threadAnalyzerToolStripMenuItem";
            this.threadAnalyzerToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.threadAnalyzerToolStripMenuItem.Size = new System.Drawing.Size(261, 22);
            this.threadAnalyzerToolStripMenuItem.Text = "Thread Analyzer";
            this.threadAnalyzerToolStripMenuItem.Click += new System.EventHandler(this.ThreadAnalyzerToolStripMenuItemClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(258, 6);
            // 
            // toggleIndentionToolStripMenuItem
            // 
            this.toggleIndentionToolStripMenuItem.CheckOnClick = true;
            this.toggleIndentionToolStripMenuItem.Name = "toggleIndentionToolStripMenuItem";
            this.toggleIndentionToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.toggleIndentionToolStripMenuItem.Size = new System.Drawing.Size(261, 22);
            this.toggleIndentionToolStripMenuItem.Text = "&Indent Traces";
            this.toggleIndentionToolStripMenuItem.Click += new System.EventHandler(this.ToggleIndentionToolStripMenuItemClick);
            // 
            // toggleShowInfoColumnsToolStripMenuItem
            // 
            this.toggleShowInfoColumnsToolStripMenuItem.CheckOnClick = true;
            this.toggleShowInfoColumnsToolStripMenuItem.Name = "toggleShowInfoColumnsToolStripMenuItem";
            this.toggleShowInfoColumnsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.toggleShowInfoColumnsToolStripMenuItem.Size = new System.Drawing.Size(261, 22);
            this.toggleShowInfoColumnsToolStripMenuItem.Text = "&Hide info columns";
            this.toggleShowInfoColumnsToolStripMenuItem.Click += new System.EventHandler(this.ToggleShowInfoColumnsToolStripMenuItemClick);
            // 
            // tsmiScrollDown
            // 
            this.tsmiScrollDown.CheckOnClick = true;
            this.tsmiScrollDown.Name = "tsmiScrollDown";
            this.tsmiScrollDown.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.tsmiScrollDown.Size = new System.Drawing.Size(261, 22);
            this.tsmiScrollDown.Text = "Always Scrolled Down (Tail)";
            this.tsmiScrollDown.CheckedChanged += new System.EventHandler(this.ScrollDownCheckedChanged);
            // 
            // tabLayout
            // 
            this.tabLayout.ColumnCount = 7;
            this.tabLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tabLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 79F));
            this.tabLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 107F));
            this.tabLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.tabLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tabLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.tabLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 413F));
            this.tabLayout.Controls.Add(this.txtLineNr, 0, 0);
            this.tabLayout.Controls.Add(this.txtThreadIdFilter, 2, 0);
            this.tabLayout.Controls.Add(this.txtInstanceFilter, 3, 0);
            this.tabLayout.Controls.Add(this.txtTimeFilter, 4, 0);
            this.tabLayout.Controls.Add(this.txtLevelFilter, 5, 0);
            this.tabLayout.Controls.Add(this.txtMessageFilter, 6, 0);
            this.tabLayout.Controls.Add(this.txtFilenameFilter, 1, 0);
            this.tabLayout.Controls.Add(this.elementHost1, 0, 1);
            this.tabLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabLayout.Location = new System.Drawing.Point(0, 24);
            this.tabLayout.Name = "tabLayout";
            this.tabLayout.RowCount = 2;
            this.tabLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tabLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tabLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tabLayout.Size = new System.Drawing.Size(893, 538);
            this.tabLayout.TabIndex = 3;
            // 
            // txtLineNr
            // 
            this.txtLineNr.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLineNr.Location = new System.Drawing.Point(0, 0);
            this.txtLineNr.Margin = new System.Windows.Forms.Padding(0);
            this.txtLineNr.Name = "txtLineNr";
            this.txtLineNr.Size = new System.Drawing.Size(20, 20);
            this.txtLineNr.TabIndex = 14;
            this.txtLineNr.TextChanged += new System.EventHandler(this.TxtLineNrTextChanged);
            // 
            // txtThreadIdFilter
            // 
            this.txtThreadIdFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtThreadIdFilter.Location = new System.Drawing.Point(99, 0);
            this.txtThreadIdFilter.Margin = new System.Windows.Forms.Padding(0);
            this.txtThreadIdFilter.Name = "txtThreadIdFilter";
            this.txtThreadIdFilter.Size = new System.Drawing.Size(107, 20);
            this.txtThreadIdFilter.TabIndex = 4;
            this.txtThreadIdFilter.TextChanged += new System.EventHandler(this.TxtThreadIdFilterTextChanged);
            // 
            // txtInstanceFilter
            // 
            this.txtInstanceFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInstanceFilter.Location = new System.Drawing.Point(206, 0);
            this.txtInstanceFilter.Margin = new System.Windows.Forms.Padding(0);
            this.txtInstanceFilter.Name = "txtInstanceFilter";
            this.txtInstanceFilter.Size = new System.Drawing.Size(92, 20);
            this.txtInstanceFilter.TabIndex = 5;
            this.txtInstanceFilter.TextChanged += new System.EventHandler(this.TxtInstanceFilterTextChanged);
            // 
            // txtTimeFilter
            // 
            this.txtTimeFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTimeFilter.Location = new System.Drawing.Point(298, 0);
            this.txtTimeFilter.Margin = new System.Windows.Forms.Padding(0);
            this.txtTimeFilter.Name = "txtTimeFilter";
            this.txtTimeFilter.Size = new System.Drawing.Size(110, 20);
            this.txtTimeFilter.TabIndex = 6;
            this.txtTimeFilter.TextChanged += new System.EventHandler(this.TxtTimeFilterTextChanged);
            // 
            // txtLevelFilter
            // 
            this.txtLevelFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLevelFilter.Location = new System.Drawing.Point(408, 0);
            this.txtLevelFilter.Margin = new System.Windows.Forms.Padding(0);
            this.txtLevelFilter.Name = "txtLevelFilter";
            this.txtLevelFilter.Size = new System.Drawing.Size(92, 20);
            this.txtLevelFilter.TabIndex = 7;
            this.txtLevelFilter.TextChanged += new System.EventHandler(this.TxtLevelFilterTextChanged);
            // 
            // txtMessageFilter
            // 
            this.txtMessageFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessageFilter.Location = new System.Drawing.Point(500, 0);
            this.txtMessageFilter.Margin = new System.Windows.Forms.Padding(0);
            this.txtMessageFilter.Name = "txtMessageFilter";
            this.txtMessageFilter.Size = new System.Drawing.Size(413, 20);
            this.txtMessageFilter.TabIndex = 8;
            this.txtMessageFilter.TextChanged += new System.EventHandler(this.TxtMessageFilterTextChanged);
            // 
            // txtFilenameFilter
            // 
            this.txtFilenameFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtFilenameFilter.Location = new System.Drawing.Point(20, 0);
            this.txtFilenameFilter.Margin = new System.Windows.Forms.Padding(0);
            this.txtFilenameFilter.Name = "txtFilenameFilter";
            this.txtFilenameFilter.Size = new System.Drawing.Size(79, 20);
            this.txtFilenameFilter.TabIndex = 9;
            this.txtFilenameFilter.TextChanged += new System.EventHandler(this.TxtFilenameFilterTextChanged);
            // 
            // elementHost1
            // 
            this.tabLayout.SetColumnSpan(this.elementHost1, 7);
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(0, 20);
            this.elementHost1.Margin = new System.Windows.Forms.Padding(0);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(913, 518);
            this.elementHost1.TabIndex = 11;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = this.lmListView;
            // 
            // logFileReader
            // 
            this.logFileReader.DoWork += new System.ComponentModel.DoWorkEventHandler(this.LogFileReaderDoWork);
            this.logFileReader.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.LogFileReaderRunWorkerCompleted);
            // 
            // filterWorker
            // 
            this.filterWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.FilterWorkerDoWork);
            this.filterWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.FilterWorkerRunWorkerCompleted);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.jumpToToolStripMenuItem,
            this.showMethodContentsOnlyToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(224, 48);
            // 
            // jumpToToolStripMenuItem
            // 
            this.jumpToToolStripMenuItem.Name = "jumpToToolStripMenuItem";
            this.jumpToToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.jumpToToolStripMenuItem.Text = "Jump to Enter/Leave      J";
            this.jumpToToolStripMenuItem.Click += new System.EventHandler(this.JumpToToolStripMenuItemClick);
            // 
            // showMethodContentsOnlyToolStripMenuItem
            // 
            this.showMethodContentsOnlyToolStripMenuItem.Name = "showMethodContentsOnlyToolStripMenuItem";
            this.showMethodContentsOnlyToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.showMethodContentsOnlyToolStripMenuItem.Text = "Show method contents only";
            this.showMethodContentsOnlyToolStripMenuItem.Click += new System.EventHandler(this.ShowMethodContentsOnlyToolStripMenuItemClick);
            // 
            // LogViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(893, 562);
            this.Controls.Add(this.tabLayout);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "LogViewerForm";
            this.Text = "Bluehands LogViewer";
            this.Shown += new System.EventHandler(this.LogViewerFormShown);
            this.SizeChanged += new System.EventHandler(this.LogViewerFormSizeChanged);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.LogViewerFormKeyUp);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabLayout.ResumeLayout(false);
            this.tabLayout.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tabLayout;
        private System.Windows.Forms.TextBox txtThreadIdFilter;
        private System.Windows.Forms.TextBox txtInstanceFilter;
        private System.Windows.Forms.TextBox txtTimeFilter;
        private System.Windows.Forms.TextBox txtLevelFilter;
        private System.Windows.Forms.TextBox txtMessageFilter;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.TextBox txtFilenameFilter;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private LogMessageListView lmListView;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.TextBox txtLineNr;
        private System.ComponentModel.BackgroundWorker logFileReader;
        private System.ComponentModel.BackgroundWorker filterWorker;
        private System.Windows.Forms.ToolStripMenuItem toggleIndentionToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem jumpToToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showMethodContentsOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleShowInfoColumnsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsmiScrollDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem threadAnalyzerToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem openFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mergeFilesWithCurrentFilterToolStripMenuItem;
    }
}

