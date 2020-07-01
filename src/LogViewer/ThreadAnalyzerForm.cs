using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Bluehands.Repository.Diagnostics
{
    public partial class ThreadAnalyzerForm : Form
    {
        private TableLayoutPanel m_Container;
        private readonly List<ThreadLogList> m_Columns = new List<ThreadLogList>();
        private Dictionary<string, List<LogListViewItem>> m_Items;

        public ThreadAnalyzerForm(IEnumerable<int> selectedItems, LogViewer logViewer)
        {
            InitializeComponent();

            var items = GetItems(logViewer, selectedItems);
            if (items.Count() > 0)
            {
                ShowItems(items);
            }
            Load += ThreadAnalyzerForm_Load;
        }

        void ThreadAnalyzerForm_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < m_Container.ColumnStyles.Count; i++)
            {
                m_Container.ColumnStyles[i].SizeType = SizeType.Percent;
                m_Container.ColumnStyles[i].Width = 100f / m_Items.Keys.Count;
            }
        }

        private void ShowItems(IEnumerable<LogListViewItem> items)
        {
            var startLineNr = items.Min(i => i.LineNr);
            var endLineNr = items.Max(i => i.LineNr);
            m_Items = SplitItemsPerThread(items);
            CreateContainer();
            foreach (var threadId in m_Items.Keys)
            {
                var list = CreateThreadList(threadId);
                FillList(list, m_Items[threadId], startLineNr, endLineNr);
            }
        }

        private void CreateContainer()
        {
            m_Container = new TableLayoutPanel
                              {
                                  ColumnCount = m_Items.Keys.Count,
                                  RowCount = 1,
                                  Dock = DockStyle.Fill,
                                  AutoSize = true,
                              };
            for (int i = 0; i < m_Items.Keys.Count; i++)
            {
                m_Container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / m_Items.Keys.Count));
            }
            Controls.Add(m_Container);
        }

        private static void FillList(ThreadLogList list, IEnumerable<LogListViewItem> items, int startLineNr, int endLineNr)
        {
            var currentLine = startLineNr;
            foreach (var item in items)
            {
                while(currentLine < item.LineNr)
                {
                    list.Add("");
                    currentLine++;
                }
                list.Add(item.Message);
                currentLine++;
            }
            while (currentLine <= endLineNr)
            {
                list.Add("");
                currentLine++;
            }
        }


        private ThreadLogList CreateThreadList(string threadId)
        {
            var result = new ThreadLogList(threadId);
            result.SelectionChanged += OnSelectionChanged;
            result.ScrollChanged += OnScrollChanged;
            m_Columns.Add(result);
            m_Container.Controls.Add(result);
            result.Init();

            return result;
        }

        void OnSelectionChanged(object sender, EventArgs e)
        {
            var senderList = (ThreadLogList)sender;
            foreach (var column in m_Columns)
            {
                if (column == sender) { continue; }

                column.Sync(senderList);
            }
        }

        void OnScrollChanged(object sender, EventArgs e)
        {
            var senderList = (ThreadLogList)sender;
            foreach (var column in m_Columns)
            {
                if (column == sender) { continue; }

                column.Sync(senderList);
            }
        }

        private static Dictionary<string, List<LogListViewItem>> SplitItemsPerThread(IEnumerable<LogListViewItem> items)
        {
            var result = new Dictionary<string, List<LogListViewItem>>();

            foreach (var item in items)
            {
                var threadId = item.ThreadId ?? string.Empty;

                if (!result.ContainsKey(threadId))
                {
                    result.Add(threadId, new List<LogListViewItem>());
                }
                result[threadId].Add(item);
            }

            return result;
        }

        private static IEnumerable<LogListViewItem> GetItems(LogViewer logViewer, IEnumerable<int> selectedItems)
        {
            var items = new List<LogListViewItem>();

            int itemCount = selectedItems.Count();
            Predicate<LogListViewItem> match =
                item => itemCount > 1 ? selectedItems.Contains(item.LineNr) : true;
            logViewer.ForeachVisibleItem(match, items.Add);

            return items;
        }
    }

    public class ThreadLogList : TableLayoutPanel
    {
        private LogItemDetailView m_DetailView;
        private readonly string m_ThreadId;
        private SyncScrollListBox m_List;

        public event EventHandler SelectionChanged;
        public event EventHandler ScrollChanged;

        public ThreadLogList(string threadId)
        {
            m_ThreadId = threadId;
            m_DetailView = new LogItemDetailView(m_ThreadId);
            m_DetailView.Closed += OnDetailViewClosed;
        }

        void OnDetailViewClosed(object sender, EventArgs e)
        {
            m_DetailView = null;
        }

        public void Init()
        {
            Dock = DockStyle.Fill;
            ColumnCount = 1;
            RowCount = 2;
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
            RowStyles.Add(new RowStyle(SizeType.AutoSize));
            ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            m_List = new SyncScrollListBox
                         {
                             Dock = DockStyle.Fill,
                             SelectionMode = SelectionMode.One,
                             HorizontalScrollbar = true,
                         };
            m_List.SelectedIndexChanged += OnSelectedIndexChanged;
            m_List.ScrollChanged += OnScrollChanged;
            m_List.DoubleClick += OnDoubleClick;
            Controls.Add(m_List, 0, 1);
            
            var threadLabel = new Label{Text = m_ThreadId};
            Controls.Add(threadLabel, 0, 0);
        }

        void OnDoubleClick(object sender, EventArgs e)
        {
            if (m_List.SelectedIndex >= 0)
            {
                if (m_DetailView == null || m_DetailView.IsDisposed)
                {
                    m_DetailView = new LogItemDetailView(m_ThreadId);
                }
                if (!m_DetailView.Visible)
                {
                    m_DetailView.Show(this);
                    m_List.Focus();
                }
            }
        }

        void OnScrollChanged(object sender, EventArgs e)
        {
            if (ScrollChanged != null)
            {
                ScrollChanged(this, e);
            }
        }

        void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_List.SelectedIndex >= 0)
            {
                if (m_DetailView == null || m_DetailView.IsDisposed)
                {
                    m_DetailView = new LogItemDetailView(m_ThreadId);
                }
                m_DetailView.SetText(m_List.Items[m_List.SelectedIndex].ToString());
            }
            if (SelectionChanged != null)
            {
                SelectionChanged(this, e);
            }
        }

        public void Add(string row)
        {
            m_List.Items.Add(row);
        }

        public void Sync(ThreadLogList senderList)
        {
            m_List.TopIndex = senderList.m_List.TopIndex;
            m_List.SelectedIndex = senderList.m_List.SelectedIndex;
        }
    }

    internal class SyncScrollListBox : ListBox
    {
        public event EventHandler ScrollChanged;

        private const int WM_VSCROLL = 0x115;
        private const int WM_MOUSEWHEEL = 0x020A;

        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == WM_VSCROLL || msg.Msg == WM_MOUSEWHEEL)
            {
                if (ScrollChanged != null)
                {
                    if (ScrollChanged != null)
                    {
                        ScrollChanged(this, new EventArgs());
                    }
                }
            }
            base.WndProc(ref msg);
        }
    }
}
