using System.Collections.Generic;

namespace FStats
{
    public abstract class StatController
    {
        /// <summary>
        /// Run when entering the save file.
        /// </summary>
        public abstract void Initialize();
        /// <summary>
        /// Run when leaving the save file - `() => { Initialize(); Unload(); };` should be the identity.
        /// </summary>
        public abstract void Unload();
        /// <summary>
        /// Yield a DisplayInfo object for each page that the StatController wants to display.
        /// </summary>
        public abstract IEnumerable<DisplayInfo> GetDisplayInfos();
    }
}
