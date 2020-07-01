using System;
using System.Globalization;

namespace Bluehands.Repository.Diagnostics
{
    public class DateThreadIdMessageFormatProvider : LogFormatProviderBase
    {
        public override bool IsNewLogLine(string line)
        {
            return !string.IsNullOrWhiteSpace(line) && line.Length > 4;
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
            int threadId;
            return DateTime.TryParse(item.Time, out dummy) && int.TryParse(item.ThreadId, out threadId);
        }

        public override LogListViewItem Convert(string line, out bool succeeded)
        {
            var tokens = line.Split(new[] { ' ', '\t' }, 3, StringSplitOptions.RemoveEmptyEntries);
            var item = new LogListViewItem();
            succeeded = false;
            if (tokens.Length < 3)
            {
                return item;
            }

            int threadId;
            if (!int.TryParse(tokens[1].Trim('(', ')', ':'), out threadId))
            {
                return item;
            }

            succeeded = true;
            item.ThreadId = threadId.ToString(CultureInfo.InvariantCulture);
            item.Time = tokens[0];
            item.Message = tokens[2];

            return item;
        }
    }
}