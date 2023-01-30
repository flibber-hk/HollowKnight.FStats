using System;
using System.Collections.Generic;
using System.Linq;
using FStats.Util;
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
        // TODO - make a common interface to get stats?
        [Obsolete("Use FStatsMod.LS.Get<Common>() to get the local instance")] public static Common Instance => FStatsMod.LS.Get<Common>();

        public float CountedTime = 0f;

        /// <summary>
        /// Get a string representing t as a proportion of the total time
        /// </summary>
        public string GetTimePercentString(float t, string msg)
        {
            int percent = Mathf.RoundToInt(t / CountedTime * 100);
            return $"{t.PlaytimeHHMMSS()} {msg} ({percent:0.##}%)";
        }

        /// <summary>
        /// Get a string representing t as a proportion of the total time
        /// </summary>
        public string GetTimePercentString(float t)
        {
            int percent = Mathf.RoundToInt(t / CountedTime * 100);
            return $"{t.PlaytimeHHMMSS()} ({percent:0.##}%)";
        }


        [JsonIgnore]
        public string TotalTimeString => "Total time: " + CountedTime.PlaytimeHHMMSS();

        public override void Initialize()
        {
            ModHooks.HeroUpdateHook += CountTime;
        }

        private void CountTime()
        {
            GameManager.instance.IncreaseGameTimer(ref CountedTime);
        }

        public override void Unload()
        {
            ModHooks.HeroUpdateHook -= CountTime;
        }
    }
}
