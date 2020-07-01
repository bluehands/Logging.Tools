namespace Bluehands.Repository.Diagnostics
{
    public interface ILogFormatProvider
    {
        bool IsNewLogLine(string line);
        LogListViewItem Convert(string line, out bool succeeded);
        bool IsTraceEnter(LogListViewItem item);
        bool IsTraceLeave(LogListViewItem item);
        bool KnowsFormat(string line);
        double GetTraceTimeInMs(LogListViewItem item);
    }
}