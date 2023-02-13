using System.Collections.Generic;
using System.Linq;
using FStats.Util;
using ItemChanger;
using ItemChanger.Placements;
using ItemChanger.Tags;

namespace FStats.StatControllers.ModConditional
{
    public class ICChecksPerMinuteDisplay : ModConditionalDisplay
    {
        protected override IEnumerable<string> RequiredMods()
        {
            yield return "ItemChangerMod";
        }

        public Dictionary<string, float> ItemChangerTimeByScene = new();

        public override void OnInitialize()
        {
            if (FStatsMod.LS.Get<ICChecksDisplay>()?.EndScreenReached != true)
            {
                return;
            }

            Dictionary<string, float> timeByScene = FStatsMod.LS.Get<TimeByAreaStat>().TimeByScene;
            ItemChangerTimeByScene.Subtract(timeByScene);
        }

        public override void OnUnload()
        {
            if (FStatsMod.LS.Get<ICChecksDisplay>()?.EndScreenReached != true)
            {
                return;
            }

            Dictionary<string, float> timeByScene = FStatsMod.LS.Get<TimeByAreaStat>().TimeByScene;
            ItemChangerTimeByScene.Add(timeByScene);
        }


        public override IEnumerable<DisplayInfo> ConditionalGetGlobalDisplayInfos()
        {
            // Use ItemSyncData to gather data
            // ItemSyncData.GatherData(out Dictionary<string, int> itemsByScene, out _);

            Dictionary<string, int> itemsByScene = new();
            itemsByScene.Add(GetOwningCollection().Get<ItemSyncData>().ObtainedByScene);

            Dictionary<string, float> timeByScene = new();
            timeByScene.Add(ItemChangerTimeByScene);

            if (ItemChanger.Internal.Ref.Settings != null)
            {
                ItemSyncData.GatherData(out Dictionary<string, int> obtainedThisFile, out _);
                itemsByScene.Add(obtainedThisFile);

                Dictionary<string, float> timeBySceneThisFile = FStatsMod.LS.Get<TimeByAreaStat>().TimeByScene;
                timeByScene.Add(timeBySceneThisFile);
            }

            if (itemsByScene.Values.Sum() == 0) yield break;

            Render(itemsByScene, timeByScene, out string mainStat, out List<string> statColumns);

            yield return new()
            {
                Title = "Items collected per minute" + SaveFileCountString(GetOwningCollection().Get<ICChecksDisplay>().ItemChangerFileCount + 1),
                MainStat = mainStat,
                StatColumns = statColumns,
                Priority = BuiltinScreenPriorityValues.ICChecksPerMinuteDisplay,
            };
        }

        public override IEnumerable<DisplayInfo> ConditionalGetDisplayInfos()
        {
            if (ItemChanger.Internal.Ref.Settings == null)
            {
                yield break;
            }

            ItemSyncData.GatherData(out Dictionary<string, int> obtained, out _);
            Dictionary<string, float> timeByScene = new(FStatsMod.LS.Get<TimeByAreaStat>().TimeByScene);

            if (obtained.Values.Sum() == 0)
            {
                yield break;
            }

            Render(obtained, timeByScene, out string mainStat, out List<string> statColumns);

            yield return new()
            {
                Title = "Items collected per minute",
                MainStat = mainStat,
                StatColumns = statColumns,
                Priority = BuiltinScreenPriorityValues.ICChecksPerMinuteDisplay,
            };

        }

        private void Render(Dictionary<string, int> itemsByScene, Dictionary<string, float> timeByScene, out string mainStat, out List<string> statColumns)
        {
            int Checks(string area) => itemsByScene
                .Where(kvp => AreaName.CleanAreaName(kvp.Key) == area)
                .Select(kvp => kvp.Value)
                .DefaultIfEmpty(0)
                .Sum();

            float Time(string area) => timeByScene
                .Where(kvp => AreaName.CleanAreaName(kvp.Key) == area)
                .Select(kvp => kvp.Value)
                .DefaultIfEmpty(0)
                .Sum();

            List<string> Lines = new();
            
            foreach (string area in GetOwningCollection().Get<TimeByAreaStat>().AreaOrder)
            {
                int checks = Checks(area);
                if (checks == 0) continue;
                float time = Time(area);
                if (time == 0) continue;
                Lines.Add($"{area} - {checks / time * 60}");
            }

            mainStat = $"{itemsByScene.Values.Sum() / timeByScene.Values.Sum() * 60}";
            statColumns = new()
            {
                string.Join("\n", Lines.Slice(0, 2)),
                string.Join("\n", Lines.Slice(1, 2)),
            };
        }
    }
}
