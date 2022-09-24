using System;
using System.Collections.Generic;
using System.Linq;
using FStats.Util;
using Modding;

namespace FStats.StatControllers
{
    public class TimeByAreaStat : StatController
    {
        public Dictionary<string, float> TimeByScene = new();

        public float TimeBySubArea(string area)
        {
            return TimeByScene
                .Where(kvp => AreaName.CleanSubareaName(kvp.Key) == area)
                .Select(kvp => kvp.Value)
                .DefaultIfEmpty(0)
                .Sum();
        }
        public float TimeByArea(string area)
        {
            return TimeByScene
                .Where(kvp => AreaName.CleanAreaName(kvp.Key) == area)
                .Select(kvp => kvp.Value)
                .DefaultIfEmpty(0)
                .Sum();
        }

        public override void Initialize()
        {
            ModHooks.HeroUpdateHook += UpdateTimers;
        }
        public override void Unload()
        {
            ModHooks.HeroUpdateHook -= UpdateTimers;
        }

        private void UpdateTimers()
        {
            string sceneName = GameManager.instance.sceneName;
            if (!string.IsNullOrEmpty(sceneName))
            {
                if (!TimeByScene.ContainsKey(sceneName)) TimeByScene[sceneName] = 0f;

                float timer = TimeByScene[sceneName];
                GameManager.instance.IncreaseGameTimer(ref timer);
                TimeByScene[sceneName] = timer;
            }
        }

        /// <summary>
        /// The order the areas are displayed on the Time by Area screen.
        /// </summary>
        public List<string> AreaOrder
        {
            get
            {
                List<string> areas = new(AreaName.Areas);
                areas.Sort((a, b) => TimeByArea(b).CompareTo(TimeByArea(a)));
                return areas;
            }
        }

        public override IEnumerable<DisplayInfo> GetDisplayInfos()
        {
            List<string> Lines = AreaOrder
                .Select(area => $"{area} - {TimeByArea(area).PlaytimeHHMMSS()}")
                .ToList();

            List<string> Columns = new()
            {
                string.Join("\n", Lines.Slice(0, 2)),
                string.Join("\n", Lines.Slice(1, 2)),
            };

            yield return new()
            {
                Title = "Time",
                MainStat = Common.Instance.CountedTime.PlaytimeHHMMSS(),
                StatColumns = Columns,
                Priority = BuiltinScreenPriorityValues.TimeByAreaStat,
            };
        }
    }
}
