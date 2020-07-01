using System.Globalization;

namespace Bluehands.Repository.Diagnostics
{
    public abstract class LogFormatProviderBase : ILogFormatProvider
    {
        private CultureInfo m_CultureInfo;

        protected LogFormatProviderBase()
            : this(CultureInfo.CreateSpecificCulture("de"))
        {
        }

        protected LogFormatProviderBase(CultureInfo cultureInfo)
        {
            m_CultureInfo = cultureInfo;
        }

        public abstract bool IsNewLogLine(string line);
        public abstract LogListViewItem Convert(string line, out bool succeeded);
        public virtual bool IsTraceEnter(LogListViewItem item)
        {
            return false;
        }

        public virtual bool IsTraceLeave(LogListViewItem item)
        {
            return false;
        }

        public virtual bool KnowsFormat(string line)
        {
            bool result;
            Convert(line, out result);
            return result;
        }

        public virtual double GetTraceTimeInMs(LogListViewItem item)
        {
            if (IsTraceLeave(item))
            {
                return GetTimeFromTraceLeave(item.Message);
            }
            return int.MinValue;
        }

        protected virtual double GetTimeFromTraceLeave(string message)
        {
            return int.MinValue;
        }

        public double ParseRunTime(string timeString)
        {
            double result;
            if (!double.TryParse(timeString, NumberStyles.Number, m_CultureInfo, out result))
            {
                var cultureInfo = SwitchCultureInfo(m_CultureInfo);
                if (double.TryParse(timeString, NumberStyles.Number, cultureInfo, out result))
                {
                    m_CultureInfo = cultureInfo;
                }
                else
                {
                    result = -1;
                }
            }
            return result;
        }

        protected static CultureInfo SwitchCultureInfo(CultureInfo cultureInfo)
        {
            if (cultureInfo.Name == "de")
            {
                return CultureInfo.InvariantCulture;
            }
            return CultureInfo.CreateSpecificCulture("de");
        }
    }
}