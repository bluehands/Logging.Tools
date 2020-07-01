using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Bluehands.Repository.Diagnostics
{
    public partial class FindDialog : Form
    {
        public string Pattern = string.Empty;
        public int RunTimeInMs = int.MinValue;

        public FindDialog()
        {
            InitializeComponent();
        }

        private void TxtPattern_TextChanged(object sender, EventArgs e)
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

        private void TxtRuntime_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(txtRunTime.Text, out RunTimeInMs))
            {
                m_ErrorProvider.SetError(txtRunTime, "Not an integer");
                RunTimeInMs = int.MinValue;
            }
            else
            {
                m_ErrorProvider.SetError(txtPattern, null);
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
