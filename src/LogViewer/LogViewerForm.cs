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
using System.Threading.Tasks;
using FunicularSwitch;
using Application = System.Windows.Forms.Application;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Drawing.Point;

namespace Bluehands.Repository.Diagnostics
{
    public partial class LogViewerForm : Form
    {
        public delegate void ListViewItemParameterDelegate(LogListViewItem item);
        public delegate void NoParameterDelegate();

        readonly LogViewer m_LogViewer;
        readonly Timer m_ScrollDownTimer;

        string m_StartUpFile;
        string m_LogFilePath;

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

        static void LogViewerFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
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

        void SetTitle(string files)
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

        void ResizeFilterTextboxes()
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

        void AdaptLastColumnWidth(GridView gridView)
        {
            int widthOfLeftColumns = 0;
            for (int i = 0; i < gridView.Columns.Count - 1; i++)
            {
                widthOfLeftColumns += (int)tabLayout.ColumnStyles[i].Width;
            }

            int desiredSizeOfLastColumn = Width - widthOfLeftColumns - 40;

            if (desiredSizeOfLastColumn > 0)
            {
                gridView.Columns[gridView.Columns.Count - 1].Width = desiredSizeOfLastColumn;
            }
        }

        readonly Dictionary<int, double> m_ColumnWidthsBeforeHide = new Dictionary<int, double>();

        void ToggleShowInfoColumns()
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

        void ShowContextMenu(Point point)
        {
            contextMenuStrip1.Show(point);
        }

        void JumpToCorrespondingLine()
        {
            int correspondingIndex = m_LogViewer.GetCorrespondingVisibleIndex(lmListView.ListView.SelectedIndex);
            if ((correspondingIndex >= 0) && (correspondingIndex < lmListView.ListView.Items.Count))
            {
                lmListView.ListView.SelectedIndex = correspondingIndex;
                lmListView.ListView.ScrollIntoView(lmListView.ListView.Items[correspondingIndex]);
            }
        }

        void LimitToSelectedMethod()
        {
            int correspondingIndex = m_LogViewer.GetCorrespondingVisibleIndex(lmListView.ListView.SelectedIndex);
            if ((correspondingIndex >= 0) && (correspondingIndex < lmListView.ListView.Items.Count))
            {
                var selectedLineNr = m_LogViewer.VisibleItems[lmListView.ListView.SelectedIndex].LineNr;
                var correspondingLineNr = m_LogViewer.VisibleItems[correspondingIndex].LineNr;
                txtLineNr.Text = $@"{Math.Min(selectedLineNr, correspondingLineNr)}, {Math.Max(selectedLineNr, correspondingLineNr)}";
            }
        }

        void ScrollDown()
        {
            if (lmListView.ListView.Items.Count > 0)
            {
                var last = lmListView.ListView.Items[lmListView.ListView.Items.Count - 1];
                lmListView.ListView.ScrollIntoView(last);
            }
        }

        void ShowThreadAnalyzer()
        {
            var selectedItems = from object selectedItem in lmListView.ListView.SelectedItems
                                select lmListView.ListView.Items.IndexOf(selectedItem)
                                    into index
                                select m_LogViewer.VisibleItems[index].LineNr;

            var analyzer = new ThreadAnalyzerForm(selectedItems, m_LogViewer);
            analyzer.ShowDialog(this);
        }

        #region GUI modifiers

        void ClearListView()
        {
            SetTitle(string.Empty);
            m_LogViewer.ClearLog();
        }
        #endregion

        #region Event handler

        void MiOpenClick(object sender, EventArgs e)
        {
            StartReadLogFiles(GetLogFileNames(), false);
        }

        void AddToolStripMenuItemClick(object sender, EventArgs e)
        {
            StartReadLogFiles(GetLogFileNames(), true);
        }

        void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }

        void OpenFolderToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(m_LogFilePath) &&
                Directory.Exists(m_LogFilePath))
            {
                Process.Start(m_LogFilePath);
            }
        }

        string[] GetLogFileNames()
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

        void ListViewDrop(object sender, DragEventArgs e)
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

        void ClearToolStripMenuItemClick(object sender, EventArgs e)
        {
            ClearListView();
        }

        void FindToolStripMenuItemClick(object sender, EventArgs e)
        {
            SelectNextHighlightedItem(false);
        }

        void FindPreviousToolStripMenuItemClick(object sender, EventArgs e)
        {
            SelectNextHighlightedItem(true);
        }

        void SearchMenuItemClick(object sender, EventArgs e)
        {
            OpenFindDialog();
        }

        int ProcessSearch(Option<FindCommand> dlgResult)
        {
            var count = dlgResult.Match(
                r => m_LogViewer.ProcessSearch(r), 
                () => m_LogViewer.HighlightItems(i => false));
            
            lmListView.ListView.Items.Refresh();
            SelectNextHighlightedItem(false);
            return count;
        }

        void OpenFindDialog()
        {
            var existing = Application.OpenForms.OfType<FindDialog>().FirstOrDefault();
            if (existing != null)
            {
                existing.SetPattern(GetSelectedText());
                existing.BringToFront();
                return;
            }

            var dlg = new FindDialog(
                c => ProcessSearch(c),
                c => m_LogViewer.CountItems(c),
                backwards => SelectNextHighlightedItem(backwards)
                );
            dlg.SetPattern(GetSelectedText());
            dlg.Show(this);
        }

        void TxtLevelFilterTextChanged(object sender, EventArgs e)
        {
            FilterColumn(LogViewer.LogColumnType.Level, txtLevelFilter.Text);
        }

        void TxtThreadIdFilterTextChanged(object sender, EventArgs e)
        {
            FilterColumn(LogViewer.LogColumnType.ThreadId, txtThreadIdFilter.Text);
        }

        void TxtInstanceFilterTextChanged(object sender, EventArgs e)
        {
            FilterColumn(LogViewer.LogColumnType.Instance, txtInstanceFilter.Text);
        }

        void TxtTimeFilterTextChanged(object sender, EventArgs e)
        {
            FilterColumn(LogViewer.LogColumnType.Time, txtTimeFilter.Text);
        }

        void TxtMessageFilterTextChanged(object sender, EventArgs e)
        {
            FilterColumn(LogViewer.LogColumnType.Message, txtMessageFilter.Text);
        }

        void TxtFilenameFilterTextChanged(object sender, EventArgs e)
        {
            FilterColumn(LogViewer.LogColumnType.Filename, txtFilenameFilter.Text);
        }

        void TxtLineNrTextChanged(object sender, EventArgs e)
        {
            if (m_LogViewer.SetLineNrLimits(txtLineNr.Text))
            {
                FilterColumn(LogViewer.LogColumnType.LineNr, txtLineNr.Text);
            }
        }

        void RefreshToolStripMenuItemClick(object sender, EventArgs e)
        {
            RefreshCurrentLog();
        }

        void LmListViewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
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

        void LogViewerFormKeyUp(object sender, KeyEventArgs e)
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

        void RefreshCurrentLog()
        {
            m_LogViewer.RefreshCurrentLog();
        }

        LogListViewItem m_CurrentSearchItem;

        void SelectNextHighlightedItem(bool backwards)
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

        public string GetSelectedText() => TextBoxHelper.LastSelected;

        int GetStartIndex()
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

        int GetNextItemIndex(int index, bool backwards)
        {
            var itemCount = lmListView.ListView.Items.Count;
            index = backwards ? index - 1 : index + 1;
            index = (index + itemCount) % (itemCount);
            return index;
        }

        void LmListViewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                var position = e.GetPosition(lmListView);
                position = lmListView.PointToScreen(position);
                ShowContextMenu(new Point((int)position.X, (int)position.Y));
            }
        }

        void LogViewerFormSizeChanged(object sender, EventArgs e)
        {
            ResizeFilterTextboxes();
        }

        void LogViewerFormShown(object sender, EventArgs e)
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

        void ToggleIndentionToolStripMenuItemClick(object sender, EventArgs e)
        {
            m_LogViewer.ToggleIndentionOnOff();
        }

        void ScrollDownCheckedChanged(object sender, EventArgs e)
        {
            if (tsmiScrollDown.Checked)
            {
                ScrollDown();
            }
            Settings.Default.AlwaysScrolledDown = tsmiScrollDown.Checked;
        }

        void VisibleItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (tsmiScrollDown.Checked)
            {
                m_ScrollDownTimer.Start();
            }
        }

        void ScrollDownTimerTick(object sender, EventArgs e)
        {
            m_ScrollDownTimer.Stop();
            ScrollDown();
        }

        void JumpToToolStripMenuItemClick(object sender, EventArgs e)
        {
            JumpToCorrespondingLine();
        }

        void ShowMethodContentsOnlyToolStripMenuItemClick(object sender, EventArgs e)
        {
            LimitToSelectedMethod();
        }

        void ToggleShowInfoColumnsToolStripMenuItemClick(object sender, EventArgs e)
        {
            ToggleShowInfoColumns();
            Settings.Default.HideInfoColumns = toggleShowInfoColumnsToolStripMenuItem.Checked;
        }

        void ThreadAnalyzerToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowThreadAnalyzer();
        }

        #endregion

        #region read log files

        void StartReadLogFiles(string[] filenames, bool append)
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

        void ReadLogFiles(IEnumerable<string> filenames, bool append)
        {
            var infos = filenames.Select(filename => new LogFileInfo(filename)).ToList();
            m_LogViewer.ReadLogFiles(infos, append);
        }

        void LogFileReaderDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
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

        void LogFileReaderRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) { }

        #endregion

        #region filter lines

        object[] m_LastFilterParams;
        object m_SelectedItemBeforeFilterStart;

        void FilterColumn(LogViewer.LogColumnType columnType, string pattern)
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

        void FilterWorkerDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var args = e.Argument as object[];
            if ((args != null) && (args.Length == 2))
            {
                var entryType = (LogViewer.LogColumnType)args[0];
                var pattern = args[1] as string;

                m_LogViewer.FilterColumn(entryType, pattern);
            }
        }

        void FilterWorkerRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (m_LastFilterParams != null)
            {
                filterWorker.RunWorkerAsync(m_LastFilterParams);
                m_LastFilterParams = null;
            }
            SelectAndScrollIntoView(m_SelectedItemBeforeFilterStart);
        }

        void SelectAndScrollIntoView(object item)
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

        void MergeFilesWithCurrentFilterClick(object sender, EventArgs e)
        {
            try
            {
                var logFiles = GetLogFileNames().OrderBy(s => s).ToList();
                if (!logFiles.Any())
                    return;

                // ReSharper disable once AssignNullToNotNullAttribute
                var outputFile = Path.Combine(Path.GetDirectoryName(logFiles[0]), Path.GetFileNameWithoutExtension(logFiles[0]) + "_merged.log");
                if (File.Exists(outputFile))
                    File.Delete(outputFile);

                foreach (var logFile in logFiles)
                {
                    var lines = File.ReadLines(logFile).ToList();
                    var logParser = new LogParser(FormatProviderFactory.GetLineConverter(lines, null));
                    var parsedLines = logParser.ParseLines(lines, logFile, includeRaw: true);
                    File.AppendAllLines(outputFile, parsedLines.Where(m_LogViewer.MatchesCurrentFilter).Select(l => l.RawMessage.ToString()));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Failed: {ex}", @"Error");
            }
        }
    }

    public abstract class FindCommand
    {
        public static FindCommand Search(string pattern) => new Search_(pattern);
        public static FindCommand RuntimeAtLeast(int milliseconds) => new RuntimeAtLeast_(milliseconds);

        public class Search_ : FindCommand
        {
            public string Pattern { get; }

            public Search_(string pattern) : base(UnionCases.Search)
            {
                Pattern = pattern;
            }
        }

        public class RuntimeAtLeast_ : FindCommand
        {
            public int Milliseconds { get; }

            public RuntimeAtLeast_(int milliseconds) : base(UnionCases.RuntimeAtLeast)
            {
                Milliseconds = milliseconds;
            }
        }

        internal enum UnionCases
        {
            Search,
            RuntimeAtLeast
        }

        internal UnionCases UnionCase { get; }
        FindCommand(UnionCases unionCase)
        {
            UnionCase = unionCase;
        }

        public override string ToString() => Enum.GetName(typeof(UnionCases), UnionCase) ?? UnionCase.ToString();
        bool Equals(FindCommand other) => UnionCase == other.UnionCase;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FindCommand)obj);
        }

        public override int GetHashCode() => (int)UnionCase;
    }

    public static class FindCommandExtension
    {
        public static T Match<T>(this FindCommand findCommand, Func<FindCommand.Search_, T> search, Func<FindCommand.RuntimeAtLeast_, T> runtimeAtLeast)
        {
            switch (findCommand.UnionCase)
            {
                case FindCommand.UnionCases.Search:
                    return search((FindCommand.Search_)findCommand);
                case FindCommand.UnionCases.RuntimeAtLeast:
                    return runtimeAtLeast((FindCommand.RuntimeAtLeast_)findCommand);
                default:
                    throw new ArgumentException($"Unknown type implementing FindCommand: {findCommand.GetType().Name}");
            }
        }

        public static async Task<T> Match<T>(this FindCommand findCommand, Func<FindCommand.Search_, Task<T>> search, Func<FindCommand.RuntimeAtLeast_, Task<T>> runtimeAtLeast)
        {
            switch (findCommand.UnionCase)
            {
                case FindCommand.UnionCases.Search:
                    return await search((FindCommand.Search_)findCommand).ConfigureAwait(false);
                case FindCommand.UnionCases.RuntimeAtLeast:
                    return await runtimeAtLeast((FindCommand.RuntimeAtLeast_)findCommand).ConfigureAwait(false);
                default:
                    throw new ArgumentException($"Unknown type implementing FindCommand: {findCommand.GetType().Name}");
            }
        }

        public static async Task<T> Match<T>(this Task<FindCommand> findCommand, Func<FindCommand.Search_, T> search, Func<FindCommand.RuntimeAtLeast_, T> runtimeAtLeast) => (await findCommand.ConfigureAwait(false)).Match(search, runtimeAtLeast);
        public static async Task<T> Match<T>(this Task<FindCommand> findCommand, Func<FindCommand.Search_, Task<T>> search, Func<FindCommand.RuntimeAtLeast_, Task<T>> runtimeAtLeast) => await(await findCommand.ConfigureAwait(false)).Match(search, runtimeAtLeast).ConfigureAwait(false);
    }
}