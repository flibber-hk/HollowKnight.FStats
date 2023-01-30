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
                    total[scene]++;

                    if (item.WasEverObtained()) obtained[scene]++;
                }
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

            List<string> Lines = GetOwningCollection().Get<TimeByAreaStat>().AreaOrder
                .Select(area => $"{area} - {ObtainedByArea(area)}/{ItemsByArea(area)}")
                .ToList();

            List<string> Columns = new()
            {
                string.Join("\n", Lines.Slice(0, 2)),
                string.Join("\n", Lines.Slice(1, 2)),
            };

            yield return new()
            {
                Title = "Items Obtained",
                MainStat = $"{obtained.Values.Sum()}/{total.Values.Sum()}",
                StatColumns = Columns,
                Priority = BuiltinScreenPriorityValues.ICChecksDisplay,
            };
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
