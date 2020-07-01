using System;
using System.Globalization;
using System.Text;

namespace Bluehands.Repository.Diagnostics
{
    public class NLogFormatProvider : LogFormatProviderBase
    {
        private const string c_TraceLeavePattern = "[Leave";

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

        public override bool IsTraceEnter(LogListViewItem item)
        {
            return ((item.Level == "TRACE") && item.Message.Contains("[Enter]"));
        }
        public override bool IsTraceLeave(LogListViewItem item)
        {
            return ((item.Level == "TRACE") && item.Message.Contains("[Leave"));
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
    }
}