using System;
using System.Collections.Generic;
using System.Linq;

namespace Bluehands.Repository.Diagnostics
{
    public static class FormatProviderFactory
    {
        public enum FormatProviderType
        {
            LogCurrent,
            LogProper,
            LogBackend,
            NLog,
            DateThreadIdMessage,
            ScriptRunner,
            JustMessage
        }

        public static ILogFormatProvider GetLineConverter(FormatProviderType provider)
        {
            switch (provider)
            {
                case FormatProviderType.LogCurrent:
                    return new LogCurrentFormatProvider();
                case FormatProviderType.LogBackend:
                    return new LogBackendFormatProvider();
                case FormatProviderType.LogProper:
                    return new LogProperFormatProvider();
                case FormatProviderType.NLog:
                    return new NLogFormatProvider();
                case FormatProviderType.DateThreadIdMessage:
                    return new DateThreadIdMessageFormatProvider();
                case FormatProviderType.ScriptRunner:
	                return new ScriptRunnerFormatProvider();
                case FormatProviderType.JustMessage:
                    return new JustMessageFormatProvider();
                default:
                    throw new ArgumentOutOfRangeException("provider");
            }
        }

        public static ILogFormatProvider GetLineConverter(ICollection<string> logLines, ILogFormatProvider defaultProvider)
        {
            var result = defaultProvider ?? new JustMessageFormatProvider();
            if ((logLines == null) || (logLines.Count == 0))
            {
                return result;
            }
            var firstNonEmptyLine = logLines.FirstOrDefault(l => !string.IsNullOrEmpty(l.Trim()));
            if (firstNonEmptyLine == null)
            {
                return result;
            }

            foreach (FormatProviderType providerType in Enum.GetValues(typeof(FormatProviderType)))
            {
                var provider = GetLineConverter(providerType);
                if (provider.KnowsFormat(firstNonEmptyLine))
                {
                    return provider;
                }
            }
            return result;
        }
    }
}