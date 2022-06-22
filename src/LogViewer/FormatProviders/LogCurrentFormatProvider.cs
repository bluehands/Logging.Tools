using System;
using System.Globalization;
using System.Text;

namespace Bluehands.Repository.Diagnostics
{
    public class LogCurrentFormatProvider : LogFormatProviderBase
    {
	    const string c_TraceLeavePattern = "[Leave";

        public override bool IsNewLogLine(string line)
        {
            if (string.IsNullOrEmpty(line) || line.Length < 4)
            {
                return false;
            }
            int dummy;
            return int.TryParse(line.Substring(0, 4), out dummy);
        }

        public override LogListViewItem Convert(string line, out bool succeeded)
        {
            succeeded = false;
            var tokens = line.Split(' ');
            var item = new LogListViewItem();
            if (tokens.Length > 6)
            {
                item.ThreadId = tokens[0];
                item.Instance = tokens[1];
                item.Time = tokens[2] + " " + tokens[3];
                item.Level = tokens[4];
                var message = new StringBuilder(tokens[5]);
                for (int i = 6; i < tokens.Length; i++)
                {
                    message.Append(" ");
                    message.Append(tokens[i].Replace("\t", " "));
                }
                item.Message = message.ToString();
                succeeded = true;
            }
            return item;
        }
        public override bool IsTraceEnter(LogListViewItem item)
        {
            return ((item.Level == "Trace") && item.Message.Contains("[Enter]"));
        }
        public override bool IsTraceLeave(LogListViewItem item)
        {
            return ((item.Level == "Trace") && item.Message.Contains("[Leave"));
        }
        protected override double GetTimeFromTraceLeave(string message)
        {
            double result;
            try
            {
                var startIndex = message.LastIndexOf(c_TraceLeavePattern) + c_TraceLeavePattern.Length;
                var length = message.LastIndexOf("ms") - startIndex;
                var timeString = message.Substring(startIndex, length);
                result = ParseRunTime(timeString);
            }
            catch (Exception)
            {
                result = -1;
            }
            return result;
        }

        public override bool KnowsFormat(string line)
        {
            bool result;
            var item = Convert(line, out result);
            if (!result)
            {
                return false;
            }
            //2010-03-10 14:36:41,715
            DateTime date;
            return DateTime.TryParseExact(item.Time, "yyyy-MM-dd HH:mm:ss,FFF", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }
    }
}