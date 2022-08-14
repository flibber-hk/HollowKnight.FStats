using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStats
{
    public class DisplayInfo
    {
        /// <summary>
        /// The title of the display
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Text shown below the title.
        /// </summary>
        public string MainStat { get; set; }
        /// <summary>
        /// The text in each column.
        /// </summary>
        public List<string> StatColumns { get; set; }
    }

    public abstract class StatController
    {
        /// <summary>
        /// Run when entering the save file
        /// </summary>
        public abstract void Initialize();
        /// <summary>
        /// Run when leaving the save file - () => { Initialize(); Unload(); }; should be the identity.
        /// </summary>
        public abstract void Unload();
        /// <summary>
        /// Get the info used to build the screen. Will be called once when viewing the completion screen.
        /// </summary>
        /// <param name="info">The info needed to build the display.</param>
        /// <returns>True/false if the display should be shown.</returns>
        public abstract bool TryGetDisplayInfo(out DisplayInfo info);
    }
}
