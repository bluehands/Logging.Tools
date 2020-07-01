using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;

namespace Bluehands.Repository.Diagnostics
{
    /// <summary>
    /// Interaction logic for LogMessageListView.xaml
    /// </summary>
    public partial class LogMessageListView
    {
        private GridViewColumnHeader sortColumn;
        private ListSortDirection sortDirection = ListSortDirection.Ascending;

        public LogMessageListView()
        {
            InitializeComponent();
        }

        private void GridViewColumnHeader_Clicked(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null)
            {

                if (headerClicked != sortColumn)
                {
                    if (sortColumn != null)
                    {
                        sortColumn.Column.HeaderTemplate = null;
                    }
                    sortColumn = headerClicked;
                    sortDirection = ListSortDirection.Ascending;
                }
                else
                {
                    if (sortDirection == ListSortDirection.Ascending)
                    {
                        sortDirection = ListSortDirection.Descending;
                    }
                    else
                    {
                        sortDirection = ListSortDirection.Ascending;
                    }
                }

                if (sortDirection == ListSortDirection.Ascending)
                {
                    sortColumn.Column.HeaderTemplate = Resources["HeaderTemplateArrowDown"] as DataTemplate;
                }
                else
                {
                    sortColumn.Column.HeaderTemplate = Resources["HeaderTemplateArrowUp"] as DataTemplate;
                }

                string header = headerClicked.Column.Header as string;
                Sort(header);
            }
        }

        private void Sort(string sortBy)
        {
            ListCollectionView view = (ListCollectionView)CollectionViewSource.GetDefaultView(ListView.ItemsSource);
            switch (sortBy)
            {
                case "Filename":
                    view.CustomSort = new LogItemFilenameComparer(sortDirection);
                    break;
                case "ThreadID":
                    view.CustomSort = new LogItemThreadIdComparer(sortDirection);
                    break;
                case "Instance":
                    view.CustomSort = new LogItemInstanceComparer(sortDirection);
                    break;
                case "Time":
                    view.CustomSort = new LogItemTimeComparer(sortDirection);
                    break;
                case "Module / Message":
                    view.CustomSort = new LogItemMessageComparer(sortDirection);
                    break;
                case "Level":
                    view.CustomSort = new LogItemLevelComparer(sortDirection);
                    break;
                default:
                    view.CustomSort = new LogItemLineNrComparer(sortDirection);
                    break;
            }
            ListView.Items.Refresh();
        }

        internal abstract class LogListViewItemComparer : System.Collections.IComparer
        {
            private readonly ListSortDirection sortOrder;

            protected LogListViewItemComparer()
            {
                sortOrder = ListSortDirection.Ascending;
            }

            protected LogListViewItemComparer(ListSortDirection sortOrder)
            {
                this.sortOrder = sortOrder;
            }

            #region IComparer Members

            public int Compare(object x, object y)
            {
                LogListViewItem item1 = x as LogListViewItem;
                LogListViewItem item2 = y as LogListViewItem;

                int returnVal = CompareItems(item1, item2);

                if (sortOrder == ListSortDirection.Descending)
                {
                    returnVal *= -1;
                }
                return returnVal;
            }
            #endregion

            public abstract int CompareItems(LogListViewItem x, LogListViewItem y);
        }

        internal class LogItemFilenameComparer : LogListViewItemComparer
        {
            public LogItemFilenameComparer(ListSortDirection sortOrder) : base(sortOrder) { }
            public LogItemFilenameComparer() : base(ListSortDirection.Ascending) { }
            public override int CompareItems(LogListViewItem x, LogListViewItem y)
            {
                return String.Compare(x.Filename, y.Filename);
            }
        }
        internal class LogItemThreadIdComparer : LogListViewItemComparer
        {
            public LogItemThreadIdComparer(ListSortDirection sortOrder) : base(sortOrder) { }
            public LogItemThreadIdComparer() : base(ListSortDirection.Ascending) { }
            public override int CompareItems(LogListViewItem x, LogListViewItem y)
            {
                return String.Compare(x.ThreadId, y.ThreadId);
            }
        }
        internal class LogItemInstanceComparer : LogListViewItemComparer
        {
            public LogItemInstanceComparer(ListSortDirection sortOrder) : base(sortOrder) { }
            public LogItemInstanceComparer() : base(ListSortDirection.Ascending) { }
            public override int CompareItems(LogListViewItem x, LogListViewItem y)
            {
                return String.Compare(x.Instance, y.Instance);
            }
        }
        internal class LogItemMessageComparer : LogListViewItemComparer
        {
            public LogItemMessageComparer(ListSortDirection sortOrder) : base(sortOrder) { }
            public LogItemMessageComparer() : base(ListSortDirection.Ascending) { }
            public override int CompareItems(LogListViewItem x, LogListViewItem y)
            {
                return String.Compare(x.Message.Trim(), y.Message.Trim());
            }
        }
        internal class LogItemLevelComparer : LogListViewItemComparer
        {
            public LogItemLevelComparer(ListSortDirection sortOrder) : base(sortOrder) { }
            public LogItemLevelComparer() : base(ListSortDirection.Ascending) { }
            public override int CompareItems(LogListViewItem x, LogListViewItem y)
            {
                return String.Compare(x.Level, y.Level);
            }
        }
        internal class LogItemTimeComparer : LogListViewItemComparer
        {
            public LogItemTimeComparer(ListSortDirection sortOrder) : base(sortOrder) { }
            public LogItemTimeComparer() : base(ListSortDirection.Ascending) { }
            public override int CompareItems(LogListViewItem x, LogListViewItem y)
            {
                var result = String.Compare(x.Time, y.Time);
                if (result == 0)
                {
                   result = x.LineNr - y.LineNr;
                }
                return result;
            }
        }
        internal class LogItemLineNrComparer : LogListViewItemComparer
        {
            public LogItemLineNrComparer(ListSortDirection sortOrder) : base(sortOrder) { }
            public LogItemLineNrComparer() : base(ListSortDirection.Ascending) { }
            public override int CompareItems(LogListViewItem x, LogListViewItem y)
            {
                return x.LineNr - y.LineNr;
            }
        }
    }

    public class LevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = string.Empty;
            if (value == null)
            {
                return result;
            }
            var level = value.ToString().ToUpperInvariant();
            
            if (level.StartsWith("WARN"))
            {
                return "WARN";
            }
            return level;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
