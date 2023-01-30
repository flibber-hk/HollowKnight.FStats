using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public override IEnumerable<DisplayInfo> ConditionalGetDisplayInfos()
        {
            if (ItemChanger.Internal.Ref.Settings == null)
            {
                yield break;
            }

            Dictionary<string, int> obtained = new();

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

                foreach (AbstractItem item in pmt.Items.SelectValidItems())
                {
                    if (item.GetTags<IInteropTag>().FirstOrDefault(x => x.Message == "SyncedItemTag") is IInteropTag t
                        && t.TryGetProperty<bool>("Local", out bool local) && !local)
                    {
                        continue;
                    }

                    if (item.WasEverObtained())
                    {
                        obtained[scene]++;
                    }
                }
            }

            int Checks(string area) => obtained
                .Where(kvp => AreaName.CleanAreaName(kvp.Key) == area)
                .Select(kvp => kvp.Value)
                .DefaultIfEmpty(0)
                .Sum();

            List<string> Lines = new();
            TimeByAreaStat tba = GetOwningCollection().Get<TimeByAreaStat>();
            if (tba is null)
            {
                yield break;
            }
            foreach (string area in tba.AreaOrder)
            {
                int checks = Checks(area);
                if (checks == 0) continue;
                float time = tba.TimeByArea(area);
                if (time == 0) continue;
                Lines.Add($"{area} - {checks / time * 60}");
            }

            List<string> Columns = new()
            {
                string.Join("\n", Lines.Slice(0, 2)),
                string.Join("\n", Lines.Slice(1, 2)),
            };

            yield return new()
            {
                Title = "Items collected per minute",
                MainStat = $"{obtained.Values.Sum() / GetOwningCollection().Get<Common>().CountedTime * 60}",
                StatColumns = Columns,
                Priority = BuiltinScreenPriorityValues.ICChecksPerMinuteDisplay,
            };
        }
    }
}
