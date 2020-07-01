namespace Bluehands.Repository.Diagnostics
{
    public class JustMessageFormatProvider : LogFormatProviderBase
    {
        public override bool IsNewLogLine(string line)
        {
            return true;
        }

        public override LogListViewItem Convert(string line, out bool succeeded)
        {
            succeeded = true;
            return new LogListViewItem { Message = line };
        }
    }
}