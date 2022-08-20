using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modding;
using Newtonsoft.Json;
using UnityEngine;

namespace FStats.StatControllers
{
    /// <summary>
    /// Contains things that will be useful for multiple stat controllers
    /// </summary>
    public class Common : StatController
    {
        public static Common Instance { get; set; }

        public float CountedTime = 0f;

        /// <summary>
        /// Get a string representing t as a proportion of the total time
        /// </summary>
        public string GetTimePercentString(float t, string msg)
        {
            int percent = Mathf.RoundToInt(t / CountedTime * 100);
            return $"{t.PlaytimeHHMMSS()} {msg} ({percent:0.##}%)";
        }

        [JsonIgnore]
        public string TotalTimeString => "Total time: " + CountedTime.PlaytimeHHMMSS();

        public override void Initialize()
        {
            Instance = this;
            ModHooks.HeroUpdateHook += CountTime;
        }

        private void CountTime()
        {
            GameManager.instance.IncreaseGameTimer(ref CountedTime);
        }

        public override void Unload()
        {
            Instance = null;
            ModHooks.HeroUpdateHook -= CountTime;
        }
        public override IEnumerable<DisplayInfo> GetDisplayInfos() => Enumerable.Empty<DisplayInfo>();
    }
}
