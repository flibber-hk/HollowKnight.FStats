using System;
using System.Collections.Generic;
using System.Linq;
using FStats.Util;
using ItemChanger;
using ItemChanger.Placements;
using ItemChanger.Tags;

namespace FStats.StatControllers.ModConditional
{
    /// <summary>
    /// Number of checks obtained per area
    /// </summary>
    public class ICChecksDisplay : ModConditionalDisplay
    {
        protected override IEnumerable<string> RequiredMods()
        {
            yield return "ItemChangerMod";
        }

        /// <summary>
        /// Only assigned in a local stat controller.
        /// </summary>
        public bool EndScreenReached { get; set; } = false;

        public int ItemChangerFileCount;

        /// <summary>
        /// The total number of items obtained by scene, excluding the current file.
        /// </summary>
        public Dictionary<string, int> ObtainedByScene { get; set; } = new();
        /// <summary>
        /// The total number of items placed by scene, excluding the current file.
        /// </summary>
        public Dictionary<string, int> TotalByScene { get; set; } = new();

        public override void OnInitialize()
        {
            if (FStatsMod.LS.Get<ICChecksDisplay>()?.EndScreenReached != true)
            {
                return;
            }

            GatherData(out Dictionary<string, int> obtained, out Dictionary<string, int> total);
            ObtainedByScene.Subtract(obtained);
            TotalByScene.Subtract(total);
            ItemChangerFileCount -= 1;
        }

        public override void OnUnload()
        {
            if (FStatsMod.LS.Get<ICChecksDisplay>()?.EndScreenReached != true)
            {
                return;
            }

            GatherData(out Dictionary<string, int> obtained, out Dictionary<string, int> total);
            ObtainedByScene.Add(obtained);
            TotalByScene.Add(total);
            ItemChangerFileCount += 1;
        }

        public override IEnumerable<DisplayInfo> ConditionalGetGlobalDisplayInfos()
        {
            // Make new dictionaries so they don't get modified
            Dictionary<string, int> obtainedToDisplay = new();
            Dictionary<string, int> totalToDisplay = new();
            obtainedToDisplay.Add(ObtainedByScene);
            totalToDisplay.Add(TotalByScene);

            if (ItemChanger.Internal.Ref.Settings != null)
            {
                GatherData(out Dictionary<string, int> obtained, out Dictionary<string, int> total);
                obtainedToDisplay.Add(obtained);
                totalToDisplay.Add(total);
            }

            if (totalToDisplay.Values.Sum() == 0) yield break;

            Render(obtainedToDisplay, totalToDisplay, out string mainStat, out List<string> statColumns);

            yield return new()
            {
                Title = "Items Obtained" + SaveFileCountString(ItemChangerFileCount + 1),
                MainStat = mainStat,
                StatColumns = statColumns,
                Priority = BuiltinScreenPriorityValues.ItemSyncData,
            };
        }


        public override IEnumerable<DisplayInfo> ConditionalGetDisplayInfos()
        {
            if (ItemChanger.Internal.Ref.Settings == null)
            {
                yield break;
            }

            if (!IsGlobal) EndScreenReached = true;

            GatherData(out Dictionary<string, int> obtained, out Dictionary<string, int> total);

            if (total.Values.Sum() == 0)
            {
                yield break;
            }

            Render(obtained, total, out string mainStat, out List<string> statColumns);

            yield return new()
            {
                Title = "Items Obtained",
                MainStat = mainStat,
                StatColumns = statColumns,
                Priority = BuiltinScreenPriorityValues.ICChecksDisplay,
            };
        }

        private void Render(Dictionary<string, int> obtained, Dictionary<string, int> total, out string mainStat, out List<string> statColumns)
        {
            int ObtainedByArea(string area) => obtained
                .Where(kvp => AreaName.CleanAreaName(kvp.Key) == area)
                .Select(kvp => kvp.Value)
                .DefaultIfEmpty(0)
                .Sum();
            int ItemsByArea(string area) => total
                .Where(kvp => AreaName.CleanAreaName(kvp.Key) == area)
                .Select(kvp => kvp.Value)
                .DefaultIfEmpty(0)
                .Sum();

            List<string> Lines = GetOwningCollection().Get<TimeByAreaStat>().AreaOrder
                .Select(area => $"{area} - {ObtainedByArea(area)}/{ItemsByArea(area)}")
                .ToList();

            statColumns = new()
            {
                string.Join("\n", Lines.Slice(0, 2)),
                string.Join("\n", Lines.Slice(1, 2)),
            };

            mainStat = $"{obtained.Values.Sum()}/{total.Values.Sum()}";
        }

        /// <summary>
        /// Gather the relevant data about the current ItemChanger save file.
        /// </summary>
        /// <param name="obtained">Dictionary scene -> obtained items in that scene.</param>
        /// <param name="total">Dictionary scene -> placed items in that scene.</param>
        public static void GatherData(out Dictionary<string, int> obtained, out Dictionary<string, int> total)
        {
            obtained = new();
            total = new();

            foreach (AbstractPlacement pmt in ItemChanger.Internal.Ref.Settings.Placements.Values.SelectValidPlacements())
            {
                if (pmt.Name == LocationNames.Start) continue;

                string scene = string.Empty;
                if (pmt is IPrimaryLocationPlacement ip && ip.Location.name != LocationNames.Start)
                {
                    scene = ip.Location.sceneName ?? AreaName.Other;
                }
                if (string.IsNullOrEmpty(scene))
                {
                    continue;
                }

                if (!obtained.ContainsKey(scene)) obtained.Add(scene, 0);
                if (!total.ContainsKey(scene)) total.Add(scene, 0);

                foreach (AbstractItem item in pmt.Items.SelectValidItems())
                {
                    total[scene]++;

                    if (item.WasEverObtained()) obtained[scene]++;
                }
            }
        }
    }

    internal static class ICExtensions
    {
        public static IEnumerable<AbstractItem> SelectValidItems(this IEnumerable<AbstractItem> items)
        {
            foreach (AbstractItem item in items)
            {
                if (item.GetTag<CompletionWeightTag>() is not CompletionWeightTag c || c.Weight != 0)
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<AbstractPlacement> SelectValidPlacements(this IEnumerable<AbstractPlacement> placements)
        {
            foreach (AbstractPlacement placement in placements)
            {
                if (placement.GetTag<CompletionWeightTag>() is not CompletionWeightTag c || c.Weight != 0)
                {
                    yield return placement;
                }
            }
        }
    }
}
