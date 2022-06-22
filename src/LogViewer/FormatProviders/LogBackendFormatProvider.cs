using System;
using System.Globalization;

namespace Bluehands.Repository.Diagnostics
{
    //1772 001214991634 26.03.2010 07:01:54 Trace.    DocumentProcessor::CheckAuthentication	Enter
    public class LogBackendFormatProvider : LogCurrentFormatProvider
    {
	    const string c_TraceLevelIdentifier = "Trace.";
	    const string c_TraceEnterPattern = "Enter";
	    const string c_TraceLeavePattern = "Leave:";

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
            var dateString = tokens[2];

            DateTime date;
            return DateTime.TryParseExact(dateString, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        public override bool IsTraceEnter(LogListViewItem item)
        {
            return item.Level == c_TraceLevelIdentifier && item.Message.EndsWith(c_TraceEnterPattern);
        }

        public override bool IsTraceLeave(LogListViewItem item)
        {
            return item.Level == c_TraceLevelIdentifier && item.Message.Contains(c_TraceLeavePattern);
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