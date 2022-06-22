using System.Windows.Forms;

namespace Bluehands.Repository.Diagnostics
{
    public partial class LogItemDetailView : Form
    {
        public LogItemDetailView(string threadId)
        {
            InitializeComponent();

            Text = "DetailView - Thread " + threadId;
        }

        public void SetText(string text)
        {
            m_Output.Text = text;
        }

        void LogItemDetailView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Hide();
            }
        }
    }
}
