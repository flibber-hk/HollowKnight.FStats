using System.Collections.Generic;
using System.Linq;
using FStats.Util;
using ItemChanger;
using ItemChanger.Placements;
using ItemChanger.Tags;
using UnityEngine;

namespace FStats.StatControllers.ModConditional
{
    [Attributes.DefaultHiddenScreen]
    public class ItemSyncData : ModConditionalDisplay
    {
        protected override IEnumerable<string> RequiredMods()
        {
            yield return "ItemChangerMod";
            yield return "ItemSyncMod"; // Not necessary because ItemSync isn't actually used
        }

        /// <summary>
        /// The total number of items obtained locally by scene, excluding the current file.
        /// </summary>
        public Dictionary<string, int> ObtainedByScene { get; set; } = new();
        /// <summary>
        /// The total number of items obtained by scene, excluding the current file.
        /// </summary>
        public Dictionary<string, int> TotalByScene { get; set; } = new();

        // The number of itemsync files, excluding the current file
        public int ItemsyncFileCount = 0;

        public override void OnInitialize()
        {
            if (FStatsMod.InitializationState == InitializationState.NewGame) return;

            if (!GatherData(out Dictionary<string, int> obtainedThisFile, out Dictionary<string, int> totalThisFile))
            {
                return;
            }

            ObtainedByScene.Subtract(obtainedThisFile);
            TotalByScene.Subtract(totalThisFile);
            ItemsyncFileCount--;
        }

        public override void OnUnload()
        {
            if (!GatherData(out Dictionary<string, int> obtainedThisFile, out Dictionary<string, int> totalThisFile))
            {
                return;
            }
            
            ObtainedByScene.Add(obtainedThisFile);
            TotalByScene.Add(totalThisFile);
            ItemsyncFileCount++;
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
                Title = "Synced items picked up locally" + SaveFileCountString(ItemsyncFileCount + 1),
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

            GatherData(out Dictionary<string, int> obtained, out Dictionary<string, int> total);

            if (total.Values.Sum() == 0)
            {
                yield break;
            }

            Render(obtained, total, out string mainStat, out List<string> statColumns);

            yield return new()
            {
                Title = "Synced items picked up locally",
                MainStat = mainStat,
                StatColumns = statColumns,
                Priority = BuiltinScreenPriorityValues.ItemSyncData,
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

            mainStat = $"{obtained.Values.Sum()}/{total.Values.Sum()} ({Mathf.RoundToInt((float)obtained.Values.Sum() / total.Values.Sum() * 100.0f)}%)";
        }

        /// <summary>
        /// Gather the relevant data about the current ItemChanger save file.
        /// It is safe to call this method even if ItemSync is not installed.
        /// </summary>
        /// <param name="obtained">Dictionary scene -> number of items obtained locally in that scene.</param>
        /// <param name="total">Dictionary scene -> number of items obtained in that scene.</param>
        /// <returns>True if this looks like an ItemSync file.
        /// The obtained and total arguments will be properly assigned regardless of the return value.</returns>
        public static bool GatherData(out Dictionary<string, int> obtained, out Dictionary<string, int> total)
        {
            bool isItemsync = false;

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
                    if (!item.WasEverObtained())
                    {
                        continue;
                    }
                    if (item.GetTags<IInteropTag>().FirstOrDefault(x => x.Message == "SyncedItemTag") is IInteropTag t)
                    {
                        isItemsync = true;

                        total[scene]++;

                        if (t.TryGetProperty<bool>("Local", out bool local) && local)
                        {
                            obtained[scene]++;
                        }
                    }
                }
            }

            return isItemsync; // Return true iff it's an itemsync room
        }
    }
}
