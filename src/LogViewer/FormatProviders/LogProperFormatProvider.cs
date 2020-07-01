using System;
using System.Globalization;
using System.Text;

namespace Bluehands.Repository.Diagnostics
{
    public class LogProperFormatProvider : LogFormatProviderBase
    {
        private const string c_TraceEnterPattern = "[Enter]";
        private const string c_TraceLeavePattern = "[Leave]";

        public override bool IsNewLogLine(string line)
        {
            int dummy;
            if (line.Length < 5)
            {
                return false;
            }
            return int.TryParse(line.Substring(0, 2), out dummy) && int.TryParse(line.Substring(3, 2), out dummy);
        }

        //12.01.2010 14:56:25 - Log    - Bluehands.Repository.Log.PerfCounter - planning phase duration (s): 0.344
        public override LogListViewItem Convert(string line, out bool succeeded)
        {
            succeeded = false;
            var item = new LogListViewItem();
            var tokens = line.Split(' ');
            if (tokens.Length > 5)
            {
                item.Time = string.Concat(tokens[0], " ", tokens[1]);
                item.Level = tokens[3];
                var message = new StringBuilder(tokens[5]);
                for (int i = 5; i < tokens.Length; i++)
                {
                    message.Append(" ");
                    message.Append(tokens[i]);
                }
                item.Message = message.ToString();
                succeeded = true;
            }
            return item;
        }

        public override bool KnowsFormat(string line)
        {
            bool result;
            Convert(line, out result);
            if (!result)
            {
                return false;
            }
            var tokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
            {
                return false;
            }
            var dateString = tokens[0] + " " + tokens[1];
            DateTime date;
            //26.03.2010 08:27:56
            return DateTime.TryParseExact(dateString, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        public override bool IsTraceEnter(LogListViewItem item)
        {
            return item.Message.EndsWith(c_TraceEnterPattern);
        }

        public override bool IsTraceLeave(LogListViewItem item)
        {
            return item.Message.EndsWith(c_TraceLeavePattern);
        }

        protected override double GetTimeFromTraceLeave(string message)
        {
            double result;
            const string secondsPattern = "(s):";

            try
            {
                var startIndex = message.LastIndexOf(secondsPattern) + secondsPattern.Length;
                var length = message.LastIndexOf(c_TraceLeavePattern) - startIndex;
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