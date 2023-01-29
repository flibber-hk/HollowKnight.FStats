using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FStats
{
    /// <summary>
    /// Object which will be loaded and unloaded by FStats when a save file is entered and exited.
    /// </summary>
    public abstract class StatController
    {
        /// <summary>
        /// Run when entering the save file.
        /// </summary>
        public virtual void Initialize() { }
        /// <summary>
        /// Run when leaving the save file - `() => { Initialize(); Unload(); };` should be the identity.
        /// </summary>
        public virtual void Unload() { }

        /// <summary>
        /// Yield a DisplayInfo object for each page that the StatController wants to display.
        /// This method will be called if this instance is associated with the Local Settings.
        /// </summary>
        public virtual IEnumerable<DisplayInfo> GetDisplayInfos() => Enumerable.Empty<DisplayInfo>();

        /// <summary>
        /// Yield a DisplayInfo object for each page that the StatController wants to display.
        /// This method will be called if this instance is associated with the Global Settings.
        /// </summary>
        public virtual IEnumerable<DisplayInfo> GetGlobalDisplayInfos() => Enumerable.Empty<DisplayInfo>();

        /// <summary>
        /// The number of save files this StatController has been initialized with.
        /// This quantity will only be positive if this is associated with the GlobalSettings.
        /// </summary>
        [JsonProperty] public int FileCount { get; internal set; } = 0;
    }
}
