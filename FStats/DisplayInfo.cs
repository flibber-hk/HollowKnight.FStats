using System;
using System.Collections.Generic;

namespace FStats
{
    public class DisplayInfo
    {
        /// <summary>
        /// The title of the display. Must not be null or empty.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Text shown below the title. May be empty - will not be shown if so.
        /// </summary>
        public string MainStat { get; set; }
        /// <summary>
        /// The text in each column.
        /// </summary>
        public List<string> StatColumns { get; set; }
        /// <summary>
        /// Stat screens will be cycled in increasing order of priority.
        /// </summary>
        public double Priority { get; set; } = 0;

        /// <summary>
        /// If this is not null, then any display for which it returns true will not be shown.
        /// </summary>
        public Func<DisplayInfo, bool> DisplaySuppressor = null;

        /// <summary>
        /// Creates and returns a shallow copy of this DisplayInfo object.
        /// </summary>
        public DisplayInfo Clone() => (DisplayInfo)MemberwiseClone();
    }
}
