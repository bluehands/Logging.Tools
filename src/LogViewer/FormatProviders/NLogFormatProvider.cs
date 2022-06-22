using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Bluehands.Repository.Diagnostics
{
    public class NLogFormatProvider : LogFormatProviderBase
    {
	    const string TraceLeavePattern = "[Leave";

        public NLogFormatProvider()
            : base(CultureInfo.InvariantCulture)
        {

        }

        public override bool IsNewLogLine(string line)
        {
            if (string.IsNullOrEmpty(line) || line.Length < 10)
            {
                return false;
            }
            DateTime dummy;
            return DateTime.TryParseExact(line.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy);
        }

        public override LogListViewItem Convert(string line, out bool succeeded)
        {
            succeeded = false;
            var tokens = line.Split(' ');
            var item = new LogListViewItem();
            if (tokens.Length >= 4)
            {
                item.Time = tokens[0] + " " + tokens[1];
                item.Level = tokens[2];

                var message = new StringBuilder(tokens[3]);
                for (var i = 4; i < tokens.Length; i++)
                {
                    message.Append(" ");
                    message.Append(tokens[i].Replace("\t", " "));
                }
                item.Message = message.ToString();
                succeeded = true;
            }
            return item;
        }

        public override bool KnowsFormat(string line)
        {
            bool result;
            var item = Convert(line, out result);
            if (!result)
            {
                return false;
            }
            DateTime dummy;
            return DateTime.TryParseExact(item.Time, "yyyy-MM-dd HH:mm:ss.FFFF", CultureInfo.InvariantCulture, DateTimeStyles.None, out dummy);
        }

        public override bool IsTraceEnter(LogListViewItem item) => ((item.Level == "TRACE") && item.Message.Contains("[Enter]"));

        public override bool IsTraceLeave(LogListViewItem item) => ((item.Level == "TRACE") && item.Message.Contains("[Leave"));

        protected override double GetTimeFromTraceLeave(string message)
        {
            double result;
            try
            {
                var startIndex = message.LastIndexOf(TraceLeavePattern, StringComparison.Ordinal) + TraceLeavePattern.Length;
                var length = message.LastIndexOf("ms", StringComparison.Ordinal) - startIndex;
                var timeString = message.Substring(startIndex, length);
                result = ParseRunTime(timeString);
            }
            catch (Exception)
            {
                result = -1;
            }
            return result;
        }
    }

    public class ScriptRunnerFormatProvider : LogFormatProviderBase
    {
	    public override bool IsNewLogLine(string line)
	    {
		    if (string.IsNullOrEmpty(line) || line.Length < 10)
		    {
			    return false;
		    }
		    return DateTime.TryParseExact(line.Substring(0, 23), "yyyy-MM-dd HH:mm:ss,fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
	    }


	    static readonly Regex s_LineRegex = new(@"(?'date'\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}) \[(?'threaId'\d+|\w*\s#\d+)\s*:(?'logger'[^:]*):(?'level'[^(\s|\])]*)(\s|\])*(?'message'.*)",
			    RegexOptions.Compiled);

	    public override LogListViewItem Convert(string line, out bool succeeded)
	    {
		    var match = s_LineRegex.Match(line);
		    succeeded = match.Success;
		    var item = new LogListViewItem();
		    if (succeeded)
		    {
			    item.Instance = match.Groups["logger"].Value;
			    item.Level = match.Groups["level"].Value;
			    item.Message = match.Groups["message"].Value;
			    item.ThreadId = match.Groups["threaId"].Value;
			    item.Time = match.Groups["date"].Value;
		    }

		    return item;
	    }

	    public override bool IsTraceEnter(LogListViewItem item) => item.Message.Contains(">>>Start>>");

	    public override bool IsTraceLeave(LogListViewItem item) => item.Message.Contains("<<<Done<<<");

	    //2022-05-11 08:53:34,109 [7 :Util:INFO ] EventProcessor`1.Process: <<<Done<<< Applying event: ConnectorInstancePatched 7632243c-944c-46dd-902e-133670647ded: 00:00:00.0009121
	    protected override double GetTimeFromTraceLeave(string message)
	    {
		    double result;
		    try
		    {
			    var parts = message.Split(':');
			    if (parts.Length < 4)
				    return -1;

			    var input = $"{parts[parts.Length - 3]}:{parts[parts.Length - 2]}:{parts[parts.Length - 1]}";
			    var timeSpan = TimeSpan.Parse(input);
			    return timeSpan.TotalMilliseconds;
		    }
		    catch (Exception)
		    {
			    result = -1;
		    }
		    return result;
	    }
    }
}