using System.Collections.Generic;
using System.Linq;

namespace FStats.Util
{
    /// <summary>
    /// Class containing convenience methods to organise stat entries into columns.
    /// </summary>
    public static class ColumnUtility
    {
        /// <summary>
        /// Given a list of entries, group them to make a stat page.
        /// </summary>
        /// <param name="template">A display info with all fields set except for the StatColumns field.</param>
        /// <param name="entries">The entries to display.</param>
        /// <param name="maxEntriesPerColumn">The maximum number of entries per column.</param>
        /// <param name="maxColumnsPerPage">The maximum number of columns per page.</param>
        /// <param name="singlePage">If this is true, will only return a single page.
        /// If there are too many entries, the last line of the central column will be replaced with an ellipsis.</param>
        /// <param name="showPageNumbers">If this is true and there are at least two pages, will add (n/m) to the end
        /// of the title for each page, where m is the number of pages and n is the (1-indexed) page number.</param>
        /// <returns>An iterable with DisplayInfos.</returns>
        public static IEnumerable<DisplayInfo> CreateDisplay(
            DisplayInfo template, 
            List<string> entries, 
            int maxEntriesPerColumn = 10,
            int maxColumnsPerPage = 4,
            bool singlePage = false,
            bool showPageNumbers = true)
        {
            if (!singlePage || entries.Count <= maxColumnsPerPage * maxEntriesPerColumn)
            {
                List<DisplayInfo> pages = entries
                    .DistributePages(maxEntriesPerColumn, maxColumnsPerPage)
                    .Select(pageInfo => pageInfo.ToColumns())
                    .Select(cols => template.Copy(cols))
                    .ToList();

                if (pages.Count == 1 || !showPageNumbers)
                {
                    return pages;
                }

                return pages.Select((info, n) => { info.Title = info.Title + $" ({n+1}/{pages.Count})"; return info; });
            }

            // Single page overflowed
            List<string> columns = entries.DistributePages(maxEntriesPerColumn - 1, maxColumnsPerPage).Select(pageInfo => pageInfo.ToColumns()).First();

            if (columns.Count % 2 == 1)
            {
                columns[(columns.Count - 1) / 2] += "\n...";
            }
            else
            {
                columns[(columns.Count / 2) - 1] += "\n...";
                columns[columns.Count / 2] += "\n...";
            }

            return new List<DisplayInfo>()
            {
                template.Copy(columns)
            };
        }

        /// <summary>
        /// Split the listed entries into columns.
        /// </summary>
        /// <param name="entries">The entries to display.</param>
        /// <param name="maxEntriesPerColumn">The maximum number of entries per column.</param>
        /// <returns>An iterator over columns represented as a list of strings.</returns>
        public static IEnumerable<List<T>> DistributeColumns<T>(this IEnumerable<T> entries, int maxEntriesPerColumn = 10)
        {
            List<T> entriesAsList = entries.ToList();
            int columnCount = entriesAsList.Count.CeilingDivide(maxEntriesPerColumn);

            for (int i = 0; i < columnCount; i++)
            {
                yield return entriesAsList.Slice(i, columnCount).ToList();
            }
        }

        /// <summary>
        /// Split the listed entries into pages.
        /// </summary>
        /// <param name="entries">The entries to display.</param>
        /// <param name="maxEntriesPerColumn">The maximum number of entries per column.</param>
        /// <param name="maxColumnsPerPage">The maximum number of columns per page.</param>
        /// <returns></returns>
        public static IEnumerable<List<List<T>>> DistributePages<T>(this IEnumerable<T> entries, int maxEntriesPerColumn = 10, int maxColumnsPerPage = 4)
        {
            IEnumerable<List<T>> pageGroups = entries.GroupSegments(maxEntriesPerColumn * maxColumnsPerPage);

            foreach (List<T> pageGroup in pageGroups)
            {
                yield return pageGroup.DistributeColumns(maxEntriesPerColumn).ToList();
            }
        }

        public static List<string> ToColumns(this List<List<string>> columns) 
        {
            return columns.Select(col => string.Join("\n", col)).ToList();
        }

        public static DisplayInfo Copy(this DisplayInfo template, List<string> columns)
        {
            DisplayInfo copy = template.Clone();
            copy.StatColumns = columns;
            return copy;
        }
    }
}
