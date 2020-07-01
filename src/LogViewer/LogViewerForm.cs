using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Input;
using Bluehands.Repository.Diagnostics.Properties;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using System.IO;
using System.Reflection;
using Application = System.Windows.Forms.Application;
using DragEventArgs = System.Windows.DragEventArgs;
using Point = System.Drawing.Point;

namespace Bluehands.Repository.Diagnostics
{
    public partial class LogViewerForm : Form
    {
        public delegate void ListViewItemParameterDelegate(LogListViewItem item);
        public delegate void NoParameterDelegate();

        private readonly LogViewer m_LogViewer;
        private readonly Timer m_ScrollDownTimer;

        private string m_StartUpFile;
        private string m_LogFilePath;

        public LogViewerForm(string startUpFile)
        {
            InitializeComponent();
            lmListView.KeyUp += LmListViewKeyUp;
            lmListView.MouseUp += LmListViewMouseUp;
            lmListView.Drop += ListViewDrop;
            lmListView.AddHandler(UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(PreviewMouseWheel));
            Application.Idle += OnIdle;
            m_LogViewer = new LogViewer();
            lmListView.ListView.ItemsSource = m_LogViewer.VisibleItems;
            m_LogViewer.Dispatcher = lmListView.Dispatcher;
            m_StartUpFile = startUpFile;

            Load += LogViewerFormLoad;
            Closing += LogViewerFormClosing;
            m_LogViewer.VisibleItems.CollectionChanged += VisibleItemsCollectionChanged;

            m_ScrollDownTimer = new Timer { Interval = 300 };
            m_ScrollDownTimer.Tick += ScrollDownTimerTick;
        }

        private static void LogViewerFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();
        }

        void LogViewerFormLoad(object sender, EventArgs e)
        {
            SetTitle(Path.GetFileName(m_StartUpFile));
            tsmiScrollDown.Checked = Settings.Default.AlwaysScrolledDown;
            toggleShowInfoColumnsToolStripMenuItem.Checked = Settings.Default.HideInfoColumns;
            if (Settings.Default.HideInfoColumns)
            {
                ToggleShowInfoColumns();
            }
        }

        private void SetTitle(string files)
        {
            var assembly = Assembly.GetAssembly(GetType());
            Text = !string.IsNullOrEmpty(files)
                       ? string.Format("Bluehands LogViewer {1} ({0})", files, assembly.GetName().Version)
                       : Resources.LogViewerForm_Title + " v" + assembly.GetName().Version;
        }

        public LogViewerForm()
            : this(null)
        {
        }

        void OnIdle(object sender, EventArgs e)
        {
            ResizeFilterTextboxes();
        }

        private void ResizeFilterTextboxes()
        {
            var gridView = lmListView.ListView.View as GridView;
            if (gridView != null)
            {
                if (!double.IsNaN(gridView.Columns[0].Width) &&
                    (tabLayout.ColumnStyles[0].Width != (int)gridView.Columns[0].Width + 4))
                {
                    tabLayout.ColumnStyles[0].Width = (int)gridView.Columns[0].Width + 4;
                }
                for (int i = 1; i < gridView.Columns.Count; i++)
                {
                    if (!double.IsNaN(gridView.Columns[i].Width) &&
                        (tabLayout.ColumnStyles[i].Width != (int)gridView.Columns[i].Width))
                    {
                        tabLayout.ColumnStyles[i].Width = (int)gridView.Columns[i].Width;
                    }
                }
                AdaptLastColumnWidth(gridView);
            }
        }

        private void AdaptLastColumnWidth(GridView gridView)
        {
            int widthOfLeftColmuns = 0;
            for (int i = 0; i < gridView.Columns.Count - 1; i++)
            {
                widthOfLeftColmuns += (int)tabLayout.ColumnStyles[i].Width;
            }

            int desiredSizeOfLastColumn = Width - widthOfLeftColmuns - 40;

            if (desiredSizeOfLastColumn > 0)
            {
                gridView.Columns[gridView.Columns.Count - 1].Width = desiredSizeOfLastColumn;
            }
        }

        private readonly Dictionary<int, double> m_ColumnWidthsBeforeHide = new Dictionary<int, double>();
        private void ToggleShowInfoColumns()
        {
            var gridView = lmListView.ListView.View as GridView;
            if (gridView != null)
            {
                for (var i = 1; i < 4; i++)
                {
                    var oldWidth = gridView.Columns[i].Width;
                    if (double.IsNaN(oldWidth) || (oldWidth <= 0))
                    {
                        var width = 30d;
                        if (m_ColumnWidthsBeforeHide.ContainsKey(i))
                        {
                            width = m_ColumnWidthsBeforeHide[i];
                        }
                        gridView.Columns[i].Width = width;
                    }
                    else
                    {
                        m_ColumnWidthsBeforeHide[i] = oldWidth;
                        gridView.Columns[i].Width = 0;
                    }
                }
            }
        }

        private void ShowContextMenu(Point point)
        {
            contextMenuStrip1.Show(point);
        }

        private void JumpToCorrespondingLine()
        {
            int correspondingIndex = m_LogViewer.GetCorrespondingVisibleIndex(lmListView.ListView.SelectedIndex);
            if ((correspondingIndex >= 0) && (correspondingIndex < lmListView.ListView.Items.Count))
            {
                lmListView.ListView.SelectedIndex = correspondingIndex;
                lmListView.ListView.ScrollIntoView(lmListView.ListView.Items[correspondingIndex]);
            }
        }

        private void LimitToSelectedMethod()
        {
            int correspondingIndex = m_LogViewer.GetCorrespondingVisibleIndex(lmListView.ListView.SelectedIndex);
            if ((correspondingIndex >= 0) && (correspondingIndex < lmListView.ListView.Items.Count))
            {
                var selectedLineNr = m_LogViewer.VisibleItems[lmListView.ListView.SelectedIndex].LineNr;
                var correspondingLineNr = m_LogViewer.VisibleItems[correspondingIndex].LineNr;
                txtLineNr.Text = string.Format("{0}, {1}",
                                               Math.Min(selectedLineNr, correspondingLineNr),
                                               Math.Max(selectedLineNr, correspondingLineNr));
            }
        }

        private void ScrollDown()
        {
            if (lmListView.ListView.Items.Count > 0)
            {
                var last = lmListView.ListView.Items[lmListView.ListView.Items.Count - 1];
                lmListView.ListView.ScrollIntoView(last);
            }
        }

        private void ShowThreadAnalyzer()
        {
            var selectedItems = from object selectedItem in lmListView.ListView.SelectedItems
                                select lmListView.ListView.Items.IndexOf(selectedItem)
                                    into index
                                    select m_LogViewer.VisibleItems[index].LineNr;

            var analyzer = new ThreadAnalyzerForm(selectedItems, m_LogViewer);
            analyzer.ShowDialog(this);
        }

        #region GUI modifiers

        private void ClearListView()
        {
            SetTitle(string.Empty);
            m_LogViewer.ClearLog();
        }
        #endregion

        #region Event handler
        private void MiOpenClick(object sender, EventArgs e)
        {
            StartReadLogFiles(GetLogFileNames(), false);
        }
        private void AddToolStripMenuItemClick(object sender, EventArgs e)
        {
            StartReadLogFiles(GetLogFileNames(), true);
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }

        private void OpenFolderToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(m_LogFilePath) &&
                Directory.Exists(m_LogFilePath))
            {
                Process.Start(m_LogFilePath);
            }
        }

        private string[] GetLogFileNames()
        {
            var result = new string[0];
            using (var dlg = new OpenFileDialog())
            {
                dlg.Multiselect = true;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    result = dlg.FileNames;
                }
            }
            return result;
        }

        private void ListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Data is System.Windows.DataObject && ((System.Windows.DataObject)e.Data).ContainsFileDropList())
            {
                var filePaths = ((System.Windows.DataObject)e.Data).GetFileDropList();
                if (filePaths.Count > 0)
                {
                    StartReadLogFiles(filePaths.Cast<string>().ToArray(), false);
                }
            }
        }

        void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (lmListView.ListView.Items.Count == 0 || !tsmiScrollDown.Checked)
            {
                return;
            }

            if (!IsScrolledToBottom())
            {
                tsmiScrollDown.Checked = false;
            }
        }

        bool IsScrolledToBottom()
        {
            var vsp = (VirtualizingStackPanel)typeof(ItemsControl).InvokeMember("_itemsHost",
                                                       BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic,
                                                       null, lmListView.ListView, null);

            // ReSharper disable CompareOfFloatsByEqualityOperator
            return vsp.ScrollOwner.VerticalOffset == vsp.ScrollOwner.ScrollableHeight;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        #endregion

        #region form events

        private void ClearToolStripMenuItemClick(object sender, EventArgs e)
        {
            ClearListView();
        }

        private void SearchMenuItemClick(object sender, EventArgs e)
        {
            ProcessSearch(OpenFindDialog());
        }

        private void FindToolStripMenuItemClick(object sender, EventArgs e)
        {
            SelectNextHighlightedItem(false);
        }

        private void FindPreviousToolStripMenuItemClick(object sender, EventArgs e)
        {
            SelectNextHighlightedItem(true);
        }

        private void ProcessSearch(FindDialogResult dlgResult)
        {
            m_LogViewer.ProcessSearch(dlgResult);
            lmListView.ListView.Items.Refresh();
            SelectNextHighlightedItem(false);
        }

        private FindDialogResult OpenFindDialog()
        {
            var result = new FindDialogResult();
            using (var dlg = new FindDialog())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    result.RunTimeInMs = dlg.RunTimeInMs;
                    result.SearchPattern = dlg.Pattern;
                }
            }
            return result;
        }

        private void TxtLevelFilterTextChanged(object sender, EventArgs e)
        {
            FilterColumn(LogViewer.LogColumnType.Level, txtLevelFilter.Text);
        }

        private void TxtThreadIdFilterTextChanged(object sender, EventArgs e)
        {
            FilterColumn(LogViewer.LogColumnType.ThreadId, txtThreadIdFilter.Text);
        }

        private void TxtInstanceFilterTextChanged(object sender, EventArgs e)
        {
            FilterColumn(LogViewer.LogColumnType.Instance, txtInstanceFilter.Text);
        }

        private void TxtTimeFilterTextChanged(object sender, EventArgs e)
        {
            FilterColumn(LogViewer.LogColumnType.Time, txtTimeFilter.Text);
        }

        private void TxtMessageFilterTextChanged(object sender, EventArgs e)
        {
            FilterColumn(LogViewer.LogColumnType.Message, txtMessageFilter.Text);
        }

        private void TxtFilenameFilterTextChanged(object sender, EventArgs e)
        {
            FilterColumn(LogViewer.LogColumnType.Filename, txtFilenameFilter.Text);
        }

        private void TxtLineNrTextChanged(object sender, EventArgs e)
        {
            if (m_LogViewer.SetLineNrLimits(txtLineNr.Text))
            {
                FilterColumn(LogViewer.LogColumnType.LineNr, txtLineNr.Text);
            }
        }

        private void RefreshToolStripMenuItemClick(object sender, EventArgs e)
        {
            RefreshCurrentLog();
        }

        private void LmListViewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var keyData = new Keys();

            switch (e.Key)
            {
                case Key.J:
                    keyData = Keys.J;
                    break;
                case Key.Escape:
                    keyData = Keys.Escape;
                    break;
            }

            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
            {
                keyData |= Keys.Control;
            }

            LogViewerFormKeyUp(sender, new KeyEventArgs(keyData));
        }

        private void LogViewerFormKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.J:
                    JumpToCorrespondingLine();
                    break;
                case Keys.Escape:
                    m_LogViewer.RemoveHighlighting();
                    lmListView.ListView.Items.Refresh();
                    break;
            }
        }

        private void RefreshCurrentLog()
        {
            m_LogViewer.RefreshCurrentLog();
        }

        private LogListViewItem m_CurrentSearchItem;
        private void SelectNextHighlightedItem(bool backwards)
        {
            if ((lmListView.ListView.Items.Count == 0) || (m_LogViewer.VisibleItems.Count == 0))
            {
                return;
            }

            int index;
            int startIndex = index = GetStartIndex();
            do
            {
                index = GetNextItemIndex(index, backwards);
                var item = (LogListViewItem)lmListView.ListView.Items[index];
                if (item.Highlighted)
                {
                    m_CurrentSearchItem = item;
                    SelectAndScrollIntoView(item);
                    break;
                }
            } while (index != startIndex);
        }

        private int GetStartIndex()
        {
            if (m_CurrentSearchItem == null)
            {
                m_CurrentSearchItem = lmListView.ListView.SelectedItem as LogListViewItem;
            }
            int startIndex = 0;
            if (m_CurrentSearchItem != null)
            {
                startIndex = lmListView.ListView.Items.IndexOf(m_CurrentSearchItem);
            }
            if (startIndex < 0)
            {
                startIndex = 0;
            }
            return startIndex;
        }

        private int GetNextItemIndex(int index, bool backwards)
        {
            var itemCount = lmListView.ListView.Items.Count;
            index = backwards ? index - 1 : index + 1;
            index = (index + itemCount) % (itemCount);
            return index;
        }

        private void LmListViewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                var position = e.GetPosition(lmListView);
                position = lmListView.PointToScreen(position);
                ShowContextMenu(new Point((int)position.X, (int)position.Y));
            }
        }

        private void LogViewerFormSizeChanged(object sender, EventArgs e)
        {
            ResizeFilterTextboxes();
        }

        private void LogViewerFormShown(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(m_StartUpFile))
            {
                StartReadLogFiles(new[] { m_StartUpFile }, false);
            }
            else
            {
                var state = m_LogViewer.LoadState();
                if (state.OpenedFiles.Count > 0)
                {
                    StartReadLogFiles(state.OpenedFiles.ToArray(), false);
                }
                else
                {
                    m_StartUpFile = string.Empty;
                    openFolderToolStripMenuItem.Enabled = false;
                }
            }
        }

        private void ToggleIndentionToolStripMenuItemClick(object sender, EventArgs e)
        {
            m_LogViewer.ToggleIndentionOnOff();
        }

        private void ScrollDownCheckedChanged(object sender, EventArgs e)
        {
            if (tsmiScrollDown.Checked)
            {
                ScrollDown();
            }
            Settings.Default.AlwaysScrolledDown = tsmiScrollDown.Checked;
        }

        private void VisibleItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (tsmiScrollDown.Checked)
            {
                m_ScrollDownTimer.Start();
            }
        }

        private void ScrollDownTimerTick(object sender, EventArgs e)
        {
            m_ScrollDownTimer.Stop();
            ScrollDown();
        }

        private void JumpToToolStripMenuItemClick(object sender, EventArgs e)
        {
            JumpToCorrespondingLine();
        }

        private void ShowMethodContentsOnlyToolStripMenuItemClick(object sender, EventArgs e)
        {
            LimitToSelectedMethod();
        }

        private void ToggleShowInfoColumnsToolStripMenuItemClick(object sender, EventArgs e)
        {
            ToggleShowInfoColumns();
            Settings.Default.HideInfoColumns = toggleShowInfoColumnsToolStripMenuItem.Checked;
        }

        private void WatchForNewestFileCheckedChanged(object sender, EventArgs e)
        {

        }

        private void ThreadAnalyzerToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowThreadAnalyzer();
        }

        #endregion

        #region read log files

        private void StartReadLogFiles(string[] filenames, bool append)
        {
            if (filenames.Any())
            {
                m_LogFilePath = Path.GetDirectoryName(filenames.First());
                openFolderToolStripMenuItem.Enabled = true;
            }
            else
            {
                openFolderToolStripMenuItem.Enabled = false;
            }
            SetTitle(string.Join(", ", filenames));
            logFileReader.RunWorkerAsync(new object[] { filenames, append });
        }

        private void ReadLogFiles(IEnumerable<string> filenames, bool append)
        {
            var infos = filenames.Select(filename => new LogFileInfo(filename)).ToList();
            m_LogViewer.ReadLogFiles(infos, append);
        }

        private void LogFileReaderDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var args = e.Argument as object[];
            if ((args != null) && (args.Length == 2))
            {
                var filenames = args[0] as string[];
                var append = (bool)args[1];
                if (filenames != null)
                {
                    ReadLogFiles(filenames, append);
                }
            }
        }

        private void LogFileReaderRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) { }

        #endregion

        #region filter lines

        private object[] m_LastFilterParams;
        private object m_SelectedItemBeforeFilterStart;
        private void FilterColumn(LogViewer.LogColumnType columnType, string pattern)
        {
            m_SelectedItemBeforeFilterStart = lmListView.ListView.SelectedItem;

            var filterParams = new object[] { columnType, pattern };

            if (filterWorker.IsBusy)
            {
                m_LastFilterParams = filterParams;
                return;
            }
            filterWorker.RunWorkerAsync(filterParams);
        }

        private void FilterWorkerDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var args = e.Argument as object[];
            if ((args != null) && (args.Length == 2))
            {
                var entryType = (LogViewer.LogColumnType)args[0];
                var pattern = args[1] as string;

                m_LogViewer.FilterColumn(entryType, pattern);
            }
        }

        private void FilterWorkerRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (m_LastFilterParams != null)
            {
                filterWorker.RunWorkerAsync(m_LastFilterParams);
                m_LastFilterParams = null;
            }
            SelectAndScrollIntoView(m_SelectedItemBeforeFilterStart);
        }

        private void SelectAndScrollIntoView(object item)
        {
            lmListView.ListView.SelectedItem = item;
            lmListView.ListView.ScrollIntoView(item);
        }

        #endregion

        public class FindDialogResult
        {
            public string SearchPattern { get; set; }
            public int RunTimeInMs { get; set; }
        }
    }
}
