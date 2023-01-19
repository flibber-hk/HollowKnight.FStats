using FStats.Util;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FStats.StatControllers
{
    public class AreaTransitionTimeStats : StatController
    {
        public Dictionary<string, int> TransitionCounts = new();
        public Dictionary<string, float> TransitionTimes = new();

        private float currentTransitionTime = 0;

        public override void Initialize()
        {
            ModHooks.HeroUpdateHook += CountTime;
        }

        public override void Unload()
        {
            ModHooks.HeroUpdateHook -= CountTime;
        }

        private void CountTime()
        {
            if (TransitionStats.IsTransitioning(HeroController.instance))
            {
                GameManager.instance.IncreaseGameTimer(ref currentTransitionTime);
            }
            else if (currentTransitionTime > 0)
            {
                string currentScene = GameManager.instance.sceneName;

                TransitionCounts.IncrementValue(currentScene);
                TransitionTimes.IncreaseValue(currentScene, currentTransitionTime);
                currentTransitionTime = 0;
            }
        }

        public int TransitionCountByArea(string area)
        {
            return TransitionCounts
                .Where(kvp => AreaName.CleanAreaName(kvp.Key) == area)
                .Select(kvp => kvp.Value)
                .DefaultIfEmpty(0)
                .Sum();
        }
        public float TransitionTimeByArea(string area)
        {
            return TransitionTimes
                .Where(kvp => AreaName.CleanAreaName(kvp.Key) == area)
                .Select(kvp => kvp.Value)
                .DefaultIfEmpty(0)
                .Sum();
        }


        public override IEnumerable<DisplayInfo> GetDisplayInfos()
        {
            List<string> Lines = FStatsMod.LS.Get<TimeByAreaStat>().AreaOrder
                .Select(area => $"{area} - {TransitionTimeByArea(area).PlaytimeHHMMSS()} ({TransitionCountByArea(area)})")
                .ToList();

            List<string> Columns = new()
            {
                string.Join("\n", Lines.Slice(0, 2)),
                string.Join("\n", Lines.Slice(1, 2)),
            };

            yield return new()
            {
                Title = "Transition time",
                MainStat = $"{TransitionTimes.Values.Sum().PlaytimeHHMMSS()} ({TransitionCounts.Values.Sum()} transitions)",
                StatColumns = Columns,
                Priority = BuiltinScreenPriorityValues.TransitionTimeByAreaStats,
            };
        }
    }
}
