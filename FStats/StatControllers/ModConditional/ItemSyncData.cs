using System;
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

        public override IEnumerable<DisplayInfo> ConditionalGetDisplayInfos()
        {
            if (ItemChanger.Internal.Ref.Settings == null)
            {
                yield break;
            }

            Dictionary<string, int> obtained = new();
            Dictionary<string, int> total = new();

            foreach (AbstractPlacement pmt in ItemChanger.Internal.Ref.Settings.Placements.Values.SelectValidPlacements())
            {
                if (pmt.Name == "Start") continue;

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
                        total[scene]++;

                        if (t.TryGetProperty<bool>("Local", out bool local) && local)
                        {
                            obtained[scene]++;
                        }
                    }
                }
            }

            if (total.Values.Sum() == 0)
            {
                yield break;
            }

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

            List<string> Lines = FStatsMod.LS.Get<TimeByAreaStat>().AreaOrder
                .Select(area => $"{area} - {ObtainedByArea(area)}/{ItemsByArea(area)}")
                .ToList();

            List<string> Columns = new()
            {
                string.Join("\n", Lines.Slice(0, 2)),
                string.Join("\n", Lines.Slice(1, 2)),
            };

            yield return new()
            {
                Title = "Synced items picked up locally",
                MainStat = $"{obtained.Values.Sum()}/{total.Values.Sum()} ({Mathf.RoundToInt((float)obtained.Values.Sum() / total.Values.Sum() * 100.0f)}%)",
                StatColumns = Columns,
                Priority = BuiltinScreenPriorityValues.ItemSyncData,
            };
        }
    }
}
