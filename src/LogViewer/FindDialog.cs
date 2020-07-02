using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FunicularSwitch;

namespace Bluehands.Repository.Diagnostics
{
    public partial class FindDialog : Form
    {
        readonly Func<Option<FindCommand>, int> m_DoSearch;
        readonly Func<FindCommand, int> m_DoCount;
        readonly Action<bool> m_Highlight;
        public string Pattern = string.Empty;
        public int RunTimeInMs = int.MinValue;

        public FindDialog(Func<Option<FindCommand>, int> doSearch, Func<FindCommand, int> doCount, Action<bool> highlight)
        {
            m_DoSearch = doSearch;
            m_DoCount = doCount;
            m_Highlight = highlight;
            InitializeComponent();
        }

        void TxtPattern_TextChanged(object sender, EventArgs e)
        {
            bool inputOk = true;
            try
            {
                Regex.Match("dummy", txtPattern.Text);
            }
            catch (Exception)
            {
                inputOk = false;
                m_ErrorProvider.SetError(txtPattern, "Invalid regular expression");
                Pattern = string.Empty;
            }
            if (inputOk)
            {
                m_ErrorProvider.SetError(txtPattern, null);
                Pattern = txtPattern.Text;
            }
        }

        void TxtRuntime_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(txtRunTime.Text, out RunTimeInMs))
            {
                RunTimeInMs = int.MinValue;
            }
        }

        void cmdOK_Click(object sender, EventArgs e)
        {
            var count = m_DoSearch(GetCommand());
            DisplayCount(count);
        }

        void btnCount_Click(object sender, EventArgs e)
        {
            var count = GetCommand().Match(c => m_DoCount(c), () => 0);
            DisplayCount(count);
        }

        void DisplayCount(int count)
        {
            lblMessage.Text = $@"Hits: {count}";
        }

        Option<FindCommand> GetCommand()
        {
            if (!string.IsNullOrEmpty(Pattern))
                return FindCommand.Search(Pattern);
            if (RunTimeInMs > 0)
                return FindCommand.RuntimeAtLeast(RunTimeInMs);
            return Option<FindCommand>.None;
        }

        void cmdAbort_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FindDialog_Load(object sender, EventArgs e)
        {
            KeyPreview = true;
        }

        private void FindDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                m_Highlight(e.Shift);
            }
        }
    }
}
