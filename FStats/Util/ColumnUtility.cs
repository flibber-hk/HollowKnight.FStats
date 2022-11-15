using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FStats.Util
{
    /// <summary>
    /// Class containing convenience methods to organise stat entries into columns.
    /// </summary>
    public static class ColumnUtility
    {
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
        /// <param name="pageLimit">Whether to limit to only return one page.</param>
        /// <returns></returns>
        public static IEnumerable<List<List<T>>> DistributePages<T>(this IEnumerable<T> entries, int maxEntriesPerColumn = 10, int maxColumnsPerPage = 4, bool pageLimit = false)
        {
            List<T> entriesAsList = entries.ToList();
            int pageCount = entriesAsList.Count.CeilingDivide(maxColumnsPerPage * maxEntriesPerColumn);

            if (pageLimit)
            {
                yield return entriesAsList.Take(maxColumnsPerPage * maxEntriesPerColumn).DistributeColumns(maxEntriesPerColumn).ToList();
            }
        }

        public static List<string> ToPage(this List<List<string>> columns) 
        {
            return columns.Select(col => string.Join("\n", col)).ToList();
        }

        /// <summary>
        /// Given a collection of column texts, split them into pages.
        /// </summary>
        /// <param name="template">A <see cref="DisplayInfo"/> encapsulating the remaining info about the page.</param>
        /// <param name="columnGroups"></param>
        /// <returns>An enumerator over pages.</returns>
        public static IEnumerable<DisplayInfo> ApplyColumns(this DisplayInfo template, IEnumerable<List<string>> columnGroups)
        {
            foreach (List<string> columnGroup in columnGroups)
            {
                DisplayInfo info = template.Clone();
                info.StatColumns = columnGroup;
                yield return info;
            }
        }
    }
}
