﻿using System;
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
    public class LogFilters
    {
        public const string DefaultFilterString = ".*";
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

        public void SetRegexFilter(LogViewer.LogColumnType column, string pattern)
        {
            var access = MakeLineAccess(column);
            var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var filter = new ColumnFilter(pattern, l => regex.IsMatch(access(l) ?? string.Empty));
            m_Filters[column] = filter;
        }

        public void ResetColumnFilter(LogViewer.LogColumnType column) => m_Filters[column] = s_MatchAllFilter;

        public string CurrentFilterString(LogViewer.LogColumnType column) => m_Filters[column].FilterString;
    }

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
    }

    public class Pair<T1, T2>
    {
        public T1 First { get; set; }
        public T2 Second { get; set; }
        public Pair(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
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
        const char c_IndentChar = '-';

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
                bool isReload;
                var lines = info.Read(out isReload);
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

        public LogViewerState LoadState()
        {
            return m_StatePersister.Load();
        }

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

            //var currentIndentionLevel = 0;
            //var previousLineCount = m_AllItems.Count;
            //var lineNr = 1;
            //LogListViewItem item = null;
            //foreach (var t in lines)
            //{
            //    bool succeeded;
            //    if (!m_LogFormatProvider.IsNewLogLine(t))
            //    {
            //        if (item != null && t != null && !string.IsNullOrEmpty(t.Trim()))
            //        {
            //            item.Message = string.Concat(item.Message, Environment.NewLine, t);
            //        }
            //        continue;
            //    }
            //    item = ParseLogLine(t, ref currentIndentionLevel, out succeeded);
            //    if (succeeded)
            //    {
            //        item.LineNr = previousLineCount + lineNr;
            //        item.Filename = filename;
            //        item.Highlighted = false;
            //        destination.Add(item);
            //        ++lineNr;
            //    }
            //}
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

        public void HighlightItems(Predicate<LogListViewItem> match)
        {
            foreach (var item in VisibleItems)
            {
                item.Highlighted = match(item);
            }
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
            var limitingSearch = false;
            if (string.IsNullOrEmpty(pattern))
            {
                m_Filters.ResetColumnFilter(column);
            }
            else
            {
                var currentFilterString = m_Filters.CurrentFilterString(column);
                if (currentFilterString == DefaultFilterString)
                {
                    limitingSearch = true;
                }
                else if (column == LogColumnType.LineNr)
                {
                }
                else if (pattern.StartsWith(currentFilterString))
                {
                    //check newly typed string for regex metacharacters
                    if (currentFilterString.Length < pattern.Length)
                    {
                        var newString = pattern.Substring(currentFilterString.Length);
                        if (Regex.Escape(newString).Length == newString.Length)
                        {
                            limitingSearch = true;
                        }
                    }
                    //new search string is identical with the old one-> nothing to to here
                    else
                    {
                        return;
                    }
                }
                if (IsValidPattern(pattern))
                {
                    m_Filters.SetRegexFilter(column, pattern);
                }
                else
                {
                    return;
                }
            }

            if (!limitingSearch)
            {
                ReloadItemsWithCurrentFilter(m_Filters.Matches, m_AllItems);
            }
            else
            {
                Func<LogListViewItem, bool> matcher = m_Filters.GetMatcher(column);
                ReloadItemsWithCurrentFilter(matcher, VisibleItems);
            }
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
                item.Message = item.Message.TrimStart(c_IndentChar);
            }
        }

        void IndentLines()
        {
            foreach (var item in m_AllItems)
            {
                item.Message = item.Message.PadLeft(2 * item.TraceIndentLevel + item.Message.Length, c_IndentChar);
            }
        }

        static bool IsValidPattern(string pattern)
        {
            var validPattern = true;
            try
            {
                // ReSharper disable ReturnValueOfPureMethodIsNotUsed
                Regex.IsMatch("lskdfjl", pattern);
                // ReSharper restore ReturnValueOfPureMethodIsNotUsed
            }
            catch
            {
                validPattern = false;
            }
            return validPattern;
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

        bool MatchesCurrentFilter(LogListViewItem item) => m_Filters.Matches(item);

        bool MatchesCurrentLineNrFilter(LogListViewItem item)
        {
            return (item.LineNr >= m_MinVisibleLineNr) && (item.LineNr <= m_MaxVisibleLineNr);
        }

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

        public void ProcessSearch(LogViewerForm.FindDialogResult dlgResult)
        {
            Predicate<LogListViewItem> predicate;
            if (!string.IsNullOrEmpty(dlgResult.SearchPattern) && dlgResult.RunTimeInMs >= 0)
            {
                predicate = item =>
                               Regex.Match(item.Message, dlgResult.SearchPattern).Success &&
                               m_LogFormatProvider.GetTraceTimeInMs(item) >= dlgResult.RunTimeInMs;
            }
            else if (string.IsNullOrEmpty(dlgResult.SearchPattern) && dlgResult.RunTimeInMs >= 0)
            {
                predicate = item =>
                               m_LogFormatProvider.GetTraceTimeInMs(item) >= dlgResult.RunTimeInMs;
            }
            else if (!string.IsNullOrEmpty(dlgResult.SearchPattern) && dlgResult.RunTimeInMs < 0)
            {
                predicate = item =>
                               Regex.Match(item.Message, dlgResult.SearchPattern).Success;
            }
            else
            {
                predicate = item => false;
            }
            HighlightItems(predicate);
        }
    }

    public class LogParser
    {
        readonly ILogFormatProvider m_LogFormatProvider;

        public LogParser(ILogFormatProvider logFormatProvider) => m_LogFormatProvider = logFormatProvider;


        public IEnumerable<LogListViewItem> ParseLines(IEnumerable<string> lines, string filename, int currentLineCount = 0)
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
                    }
                    continue;
                }
                item = ParseLogLine(t, ref currentIndentionLevel, out var succeeded);
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
