using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Bluehands.Repository.Diagnostics.Properties;
using static Bluehands.Repository.Diagnostics.LogFilters;

namespace Bluehands.Repository.Diagnostics
{
    public class LogListViewItem
    {
        public string Filename { get; set; }
        public string Level { get; set; }
        public string ThreadId { get; set; }
        public string Time { get; set; }
        public string Instance { get; set; }
        public string Message { get; set; }
        public bool Highlighted { get; set; }
        public int LineNr { get; set; }
        public int TraceIndentLevel { get; set; }
        public StringBuilder RawMessage { get; } = new StringBuilder();
    }

    public class LogFileInfo : IDisposable
    {
        public FileInfo File { get; set; }
        public DateTime PreviousChangeDate { get; set; }

        public FileSystemWatcher FileSystemWatcher { get; set; }

        long m_BufferLength;

        public LogFileInfo(string file)
        {
            File = new FileInfo(file);
            PreviousChangeDate = DateTime.MinValue;
        }

        public List<string> Read(out bool isReload)
        {
            isReload = false;
            var buffer = new List<string>();

            try
            {
                File.Refresh();
                if (!File.Exists)
                {
                    isReload = true;
                    m_BufferLength = 0;
                    return buffer;
                }

                var data = new StringBuilder();
                var tempBuffer = new char[512];

                using (var reader = new StreamReader(File.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    PreviousChangeDate = File.LastWriteTime;

                    if (reader.BaseStream.Length == 0)
                    {
                        Thread.Sleep(50);
                    }
                    if (reader.BaseStream.Length < m_BufferLength)
                    {
                        m_BufferLength = 0;
                        isReload = true;
                    }
                    else
                    {
                        reader.BaseStream.Seek(m_BufferLength, SeekOrigin.Begin);
                    }
                    var start = m_BufferLength;
                    while (!reader.EndOfStream)
                    {
                        var readChar = reader.Read(tempBuffer, 0, 512);
                        start += readChar;

                        data.Append(new string(tempBuffer, 0, readChar));
                    }
                    m_BufferLength = start;
                }
                buffer.AddRange(Regex.Split(data.ToString(), Environment.NewLine));
            }
            catch (Exception)
            {
                return null;
            }
            return buffer;
        }

        public void Dispose()
        {
            if (FileSystemWatcher != null)
            {
                FileSystemWatcher.Dispose();
                FileSystemWatcher = null;
            }
        }
    }

    public class LogViewer
    {
        public Dispatcher Dispatcher { get; set; }

        string m_LastError = string.Empty;
        /// <summary>
        /// list of file - 'last updated in logviewer' values.
        /// </summary>
        readonly List<LogFileInfo> m_CurrentlyOpenedFiles = new List<LogFileInfo>();

        readonly List<LogListViewItem> m_AllItems = new List<LogListViewItem>();
        public ObservableCollection<LogListViewItem> VisibleItems { get; } = new ObservableCollection<LogListViewItem>();

        readonly LogFilters m_Filters = new LogFilters();

        int m_MaxVisibleLineNr = int.MaxValue;
        int m_MinVisibleLineNr = int.MinValue;

        bool m_IndentionOn;
        const char IndentChar = '-';

        ILogFormatProvider m_LogFormatProvider;

        readonly StatePersister<LogViewerState> m_StatePersister = new StatePersister<LogViewerState>("LogViewerState.xml"); 

        public enum LogColumnType
        {
            LineNr,
            Filename,
            ThreadId,
            Instance,
            Time,
            Level,
            Message
        }

        public void ReadLogFiles(IEnumerable<LogFileInfo> logFileInfos, bool add)
        {
            if (!add)
            {
                ClearLog();
            }

            var existingItemsModified = false;
            var newListItems = new List<LogListViewItem>();

            foreach (var info in logFileInfos)
            {
                var lines = info.Read(out var isReload);
                if (lines == null)
                {
                    m_LastError = "Error reading file (" + info.File.FullName + ")";
                    MessageBox.Show(m_LastError, Resources.LogViewer_ReadLogLinesFromFile_Error, MessageBoxButtons.OK);
                }

                if (isReload)
                {
                    existingItemsModified = true;
                    RemoveFileItems(info);
                }

                Dispatch(new Action<string, List<string>, List<LogListViewItem>>(AddLogLines), info.File.Name, lines, newListItems);

                if (!m_CurrentlyOpenedFiles.Contains(info))
                {
                    m_CurrentlyOpenedFiles.Add(info);
                    AddFileWatcher(info);
                }
            }

            m_AllItems.AddRange(newListItems);
            if (existingItemsModified)
            {
                ReloadItemsWithCurrentFilter(MatchesCurrentFilter, m_AllItems);
            }
            else
            {
                AddVisibleItems(newListItems.Where(MatchesCurrentFilter).ToList());
            }

            RememberCurrentlyOpenedFiles();
        }

        public LogViewerState LoadState() => m_StatePersister.Load();

        void RememberCurrentlyOpenedFiles()
        {
            var state = new LogViewerState();
            state.OpenedFiles.AddRange(m_CurrentlyOpenedFiles.Select(f => f.File.FullName));
            m_StatePersister.Save(state);
        }

        void AddFileWatcher(LogFileInfo info)
        {
            var directoryName = Path.GetDirectoryName(info.File.FullName);
            if (directoryName != null)
            {
                var fileSystemWatcher = new FileSystemWatcher(directoryName) { Filter = info.File.Name };
                fileSystemWatcher.Changed += LogFileChanged;
                fileSystemWatcher.Deleted += LogFileChanged;
                fileSystemWatcher.Created += LogFileChanged;
                info.FileSystemWatcher = fileSystemWatcher;
                fileSystemWatcher.EnableRaisingEvents = true;
                fileSystemWatcher.IncludeSubdirectories = false;
            }
        }

        void LogFileChanged(object sender, FileSystemEventArgs e)
        {
            UpdateFiles(m_CurrentlyOpenedFiles.Where(f => e.FullPath == f.File.FullName));
        }

        void AddLogLines(string filename, List<string> lines, List<LogListViewItem> destination)
        {
            m_LogFormatProvider = FormatProviderFactory.GetLineConverter(lines, m_LogFormatProvider);

            var logParser = new LogParser(FormatProviderFactory.GetLineConverter(lines, m_LogFormatProvider));

            destination.AddRange(logParser.ParseLines(lines, filename, m_AllItems.Count));
        }

        void AddVisibleItems(List<LogListViewItem> items)
        {
            var lowerIndex = 0;
            const int stepSize = 1000;
            while (lowerIndex < items.Count)
            {
                var count = Math.Min(stepSize, items.Count - lowerIndex);
                Dispatch(new Action<List<LogListViewItem>>(AddVisibleItemsRange), items.GetRange(lowerIndex, count));
                lowerIndex += count;
            }
        }

        void AddVisibleItemsRange(List<LogListViewItem> items)
        {
            foreach (var item in items)
            {
                VisibleItems.Add(item);
            }
        }

        public void ClearLog()
        {
            ClearVisibleItems();
            m_AllItems.Clear();
            m_CurrentlyOpenedFiles.ForEach(f => f.Dispose());
            m_CurrentlyOpenedFiles.Clear();
        }

        void ClearVisibleItems()
        {
            Dispatch(new Action(VisibleItems.Clear));
        }

        void Dispatch(Delegate method, params object[] args)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<Delegate, object[]>(Dispatch), method, args);
                return;
            }
            method.DynamicInvoke(args);
        }

        public void RefreshCurrentLog()
        {
            if (ModifiedFilesExist())
            {
                UpdateFiles(GetModifiedOpenedFiles());
            }
        }

        public bool ModifiedFilesExist()
        {
            foreach (var logFileInfo in m_CurrentlyOpenedFiles)
            {
                logFileInfo.File.Refresh();
                if (DateTime.Compare(logFileInfo.File.LastWriteTime, logFileInfo.PreviousChangeDate) > 0)
                {
                    return true;
                }
            }
            return false;
        }

        IEnumerable<LogFileInfo> GetModifiedOpenedFiles()
        {
            var modifiedFiles = new List<LogFileInfo>();
            foreach (var logFileInfo in m_CurrentlyOpenedFiles)
            {
                logFileInfo.File.Refresh();
                if (DateTime.Compare(logFileInfo.File.LastWriteTime, logFileInfo.PreviousChangeDate) > 0)
                {
                    modifiedFiles.Add(logFileInfo);
                }
            }
            return modifiedFiles;
        }

        void UpdateFiles(IEnumerable<LogFileInfo> filesToUpdate)
        {
            ReadLogFiles(filesToUpdate, true);
        }

        void RemoveFileItems(LogFileInfo info)
        {
            var fileName = info.File.Name;
            m_AllItems.RemoveAll(item => (item.Filename == fileName));
        }

        public void ForeachVisibleItem(Predicate<LogListViewItem> match, Action<LogListViewItem> action)
        {
            foreach (var item in VisibleItems)
            {
                if (match(item))
                {
                    action(item);
                }
            }
        }

        public int HighlightItems(Func<LogListViewItem, bool> match)
        {
            int count = 0;
            foreach (var item in VisibleItems)
            {
                var matched = match(item);
                item.Highlighted = matched;
                if (matched)
                    count++;
            }
            return count;
        }

        public void RemoveHighlighting()
        {
            foreach (var item in m_AllItems)
            {
                item.Highlighted = false;
            }
        }

        public void FilterColumn(LogColumnType column, string pattern)
        {
            var currentFilterString = m_Filters.CurrentFilterString(column);
            if (pattern == currentFilterString) return;

            if (pattern == NegationPrefix) pattern = DefaultFilterString;
            //negated all matches nothing. User is about to exclude more less...
            else if (pattern.StartsWith(NegationPrefix) && pattern.EndsWith("|")) return;

            if (!m_Filters.TrySetFilter(column, pattern))
            {
                return;
            }

            var limitingSearch = IsLimitingSearch(column, pattern, currentFilterString);
            if (!limitingSearch)
            {
                ReloadItemsWithCurrentFilter(m_Filters.Matches, m_AllItems);
            }
            else
            {
                ReloadItemsWithCurrentFilter(m_Filters.GetMatcher(column), VisibleItems);
            }
        }

        static bool IsLimitingSearch(LogColumnType column, string pattern, string currentFilterString)
        {
            if (string.IsNullOrEmpty(pattern)) return false;

            if (pattern.StartsWith(NegationPrefix))
            {
                return false;
            }

            if (column == LogColumnType.LineNr)
            {
                return false;
            }

            if (currentFilterString == DefaultFilterString)
            {
                return true;
            }

            if (pattern.StartsWith(currentFilterString) && currentFilterString.Length < pattern.Length)
            {
                var newString = pattern.Substring(currentFilterString.Length);
                if (Regex.Escape(newString).Length == newString.Length)
                {
                    return true;
                }
            }

            return false;
        }

        void ReloadItemsWithCurrentFilter(Func<LogListViewItem, bool> matcher, IEnumerable<LogListViewItem> source)
        {
            var newVisibleItems = source.Where(matcher).ToList();
            ClearVisibleItems();
            AddVisibleItems(newVisibleItems);
        }

        public void ToggleIndentionOnOff()
        {
            if (m_IndentionOn)
            {
                RemoveIndention();
                m_IndentionOn = false;
            }
            else
            {
                IndentLines();
                m_IndentionOn = true;
            }
            ReloadItemsWithCurrentFilter(MatchesCurrentFilter, m_AllItems);
        }

        void RemoveIndention()
        {
            foreach (var item in m_AllItems)
            {
                item.Message = item.Message.TrimStart(IndentChar);
            }
        }

        void IndentLines()
        {
            foreach (var item in m_AllItems)
            {
                item.Message = item.Message.PadLeft(2 * item.TraceIndentLevel + item.Message.Length, IndentChar);
            }
        }

        public bool SetLineNrLimits(string limits)
        {
            m_MinVisibleLineNr = int.MinValue;
            m_MaxVisibleLineNr = int.MaxValue;

            if (string.IsNullOrEmpty(limits))
            {
                return true;
            }

            var limitsWithoutWhiteSpace = Regex.Replace(limits, @"\s+", string.Empty);
            var match = Regex.Match(limitsWithoutWhiteSpace, ",");
            if (!match.Success)
            {
                return false;
            }

            var firstNumber = limitsWithoutWhiteSpace.Substring(0, match.Index);
            if (!int.TryParse(firstNumber, out m_MinVisibleLineNr))
            {
                m_MinVisibleLineNr = int.MinValue;
            }

            if (limitsWithoutWhiteSpace.Length > match.Index + 1)
            {
                var secondNumber = limitsWithoutWhiteSpace.Substring(match.Index + 1);
                if (!int.TryParse(secondNumber, out m_MaxVisibleLineNr))
                {
                    m_MaxVisibleLineNr = int.MaxValue;
                }
            }
            return true;
        }

        public bool MatchesCurrentFilter(LogListViewItem item) => m_Filters.Matches(item);

        bool MatchesCurrentLineNrFilter(LogListViewItem item) => item.LineNr >= m_MinVisibleLineNr && item.LineNr <= m_MaxVisibleLineNr;

        public int GetCorrespondingVisibleIndex(int index)
        {
            if ((index < 0) || (index >= VisibleItems.Count))
            {
                return index;
            }
            var currentItem = VisibleItems[index];
            var correspondingIndex = index;

            if (m_LogFormatProvider.IsTraceEnter(currentItem))
            {
                for (var i = index + 1; i < VisibleItems.Count; i++)
                {
                    if (VisibleItems[i].TraceIndentLevel == currentItem.TraceIndentLevel)
                    {
                        correspondingIndex = i;
                        break;
                    }
                }
            }
            else if (m_LogFormatProvider.IsTraceLeave(currentItem))
            {
                if (index == 0)
                {
                    correspondingIndex = 0;
                }
                else
                {
                    for (var i = index - 1; i >= 0; i--)
                    {
                        if (VisibleItems[i].TraceIndentLevel == currentItem.TraceIndentLevel)
                        {
                            correspondingIndex = i;
                            break;
                        }
                    }
                }
            }

            return correspondingIndex;
        }

        public int ProcessSearch(FindCommand findCommand)
        {
            var predicate = MakeSearchPredicate(findCommand);
            return HighlightItems(predicate);
        }

        Func<LogListViewItem, bool> MakeSearchPredicate(FindCommand findCommand)
        {
            var predicate = findCommand.Match<Func<LogListViewItem, bool>>(
                search =>
                {
                    var regex = new Regex(search.Pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    return item => regex.IsMatch(item.Message);
                },
                runtimeAtLeast => item => m_LogFormatProvider.GetTraceTimeInMs(item) >= runtimeAtLeast.Milliseconds
            );
            return predicate;
        }

        public int CountItems(FindCommand findCommand) => VisibleItems.Count(MakeSearchPredicate(findCommand));
    }

    public class LogParser
    {
        readonly ILogFormatProvider m_LogFormatProvider;

        public LogParser(ILogFormatProvider logFormatProvider) => m_LogFormatProvider = logFormatProvider;


        public IEnumerable<LogListViewItem> ParseLines(IEnumerable<string> lines, string filename, int currentLineCount = 0, bool includeRaw = false)
        {
            var currentIndentionLevel = 0;
            var previousLineCount = currentLineCount;
            var lineNr = 1;
            LogListViewItem item = null;
            foreach (var t in lines)
            {
                if (!m_LogFormatProvider.IsNewLogLine(t))
                {
                    if (item != null && t != null && !string.IsNullOrEmpty(t.Trim()))
                    {
                        item.Message = string.Concat(item.Message, Environment.NewLine, t);
                        if (includeRaw) item.RawMessage.Append(t);
                    }
                    continue;
                }
                item = ParseLogLine(t, ref currentIndentionLevel, out var succeeded);
                if (includeRaw) item.RawMessage.Append(t);
                if (succeeded)
                {
                    item.LineNr = previousLineCount + lineNr;
                    item.Filename = filename;
                    item.Highlighted = false;
                    yield return item;
                    ++lineNr;
                }
            }
        }


        LogListViewItem ParseLogLine(string line, ref int currentIndentionLevel, out bool succeeded)
        {
            var item = m_LogFormatProvider.Convert(line, out succeeded);

            if (m_LogFormatProvider.IsTraceEnter(item))
            {
                item.TraceIndentLevel = currentIndentionLevel;
                ++currentIndentionLevel;
            }
            else if ((m_LogFormatProvider.IsTraceLeave(item) && currentIndentionLevel > 0))
            {
                --currentIndentionLevel;
                item.TraceIndentLevel = currentIndentionLevel;
            }
            else
            {
                item.TraceIndentLevel = currentIndentionLevel;
            }

            succeeded = true;

            return item;
        }
    }
}
