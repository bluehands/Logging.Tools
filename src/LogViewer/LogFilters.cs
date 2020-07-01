using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bluehands.Repository.Diagnostics
{
    public class LogFilters
    {
        public const string DefaultFilterString = ".*";
        public const string NegationPrefix = "! ";
        static readonly ColumnFilter s_MatchAllFilter = new ColumnFilter(DefaultFilterString, MatchAll);
        static bool MatchAll(LogListViewItem _) => true;
        readonly Dictionary<LogViewer.LogColumnType, ColumnFilter> m_Filters = new Dictionary<LogViewer.LogColumnType, ColumnFilter>();

        public LogFilters()
        {
            foreach (LogViewer.LogColumnType value in Enum.GetValues(typeof(LogViewer.LogColumnType)))
            {
                m_Filters[value] = s_MatchAllFilter;
            }
        }

        public bool Matches(LogListViewItem item) => m_Filters.Values.All(f => f.Matches(item));

        public Func<LogListViewItem, bool> GetMatcher(LogViewer.LogColumnType column) => m_Filters[column].Matches;

        static Func<LogListViewItem, string> MakeLineAccess(LogViewer.LogColumnType column)
        {
            switch (column)
            {
                case LogViewer.LogColumnType.LineNr:
                    return l => l.LineNr.ToString();
                case LogViewer.LogColumnType.Filename:
                    return l => l.Filename;
                case LogViewer.LogColumnType.ThreadId:
                    return l => l.ThreadId;
                case LogViewer.LogColumnType.Instance:
                    return l => l.Instance;
                case LogViewer.LogColumnType.Time:
                    return l => l.Time;
                case LogViewer.LogColumnType.Level:
                    return l => l.Level;
                case LogViewer.LogColumnType.Message:
                    return l => l.Message;
                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
        }

        class ColumnFilter
        {
            public string FilterString { get; }
            public Func<LogListViewItem, bool> Matches { get; }

            public ColumnFilter(string filterString, Func<LogListViewItem, bool> matches)
            {
                FilterString = filterString;
                Matches = matches;
            }
        }

        public bool TrySetFilter(LogViewer.LogColumnType column, string pattern)
        {
            try
            {
                SetFilter(column, pattern);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SetFilter(LogViewer.LogColumnType column, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                m_Filters[column] = s_MatchAllFilter;
                return;
            }

            var isNegated = pattern.StartsWith(NegationPrefix) && pattern.Length > NegationPrefix.Length;
            var regexPattern = pattern;
            if (isNegated)
                regexPattern = pattern.Substring(NegationPrefix.Length);

            var access = MakeLineAccess(column);
            var regex = new Regex(regexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var filter = new ColumnFilter(pattern, l =>
            {
                var isMatch = regex.IsMatch(access(l) ?? string.Empty);
                return isNegated ? !isMatch : isMatch;
            });
            m_Filters[column] = filter;
        }

        public void ResetColumnFilter(LogViewer.LogColumnType column) => m_Filters[column] = s_MatchAllFilter;

        public string CurrentFilterString(LogViewer.LogColumnType column) => m_Filters[column].FilterString;
    }
}