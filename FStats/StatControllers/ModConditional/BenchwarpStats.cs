using System.Collections.Generic;
using System.Linq;
using Benchwarp;
using FStats.Util;

namespace FStats.StatControllers.ModConditional
{
    /// <summary>
    /// Factor out NameForBench method into a separate class to prevent serialization issues when Benchwarp is not installed.
    /// </summary>
    internal static class BenchwarpExtensions
    {
        private static readonly HashSet<string> AreaNameBenches = new()
        {
            "Hot Springs",
            "Toll",
            "Stag",
            "Waterfall",
            "Dark Room",
            "Cornifer",
            "Entrance",
            "Atrium",
            "Balcony"
        };

        internal static string BenchDisplayName(this Bench target)
        {
            string name = Events.GetBenchName(target);

            if (AreaNameBenches.Contains(name))
            {
                name = $"{target.areaName} {name}";
            }

            return name;
        }

    }

    public class BenchwarpStats : ModConditionalDisplay
    {
        protected override IEnumerable<string> RequiredMods()
        {
            yield return "Benchwarp";
        }

        public Dictionary<string, int> BenchwarpCount = new();

        public override void OnInitialize()
        {
            Events.OnBenchwarp += OnBenchwarp;
        }

        private void OnBenchwarp()
        {
            string benchName;

            string sceneName = PlayerData.instance.GetString(nameof(PlayerData.respawnScene));
            string markerName = PlayerData.instance.GetString(nameof(PlayerData.respawnMarkerName));

            switch (markerName)
            {
                case BenchMaker.DEPLOYED_BENCH_RESPAWN_MARKER_NAME:
                    benchName = "Deployed Bench";
                    break;
                case ItemChanger.StartDef.RESPAWN_MARKER_NAME:
                    benchName = "Start";
                    break;
                default:
                    BenchKey key = new(sceneName, markerName);

                    Bench target = Bench.Benches.FirstOrDefault(x => x.ToBenchKey() == key);

                    if (target is not null)
                    {
                        benchName = target.BenchDisplayName();
                        break;
                    }

                    // Warping to a hard save
                    benchName = sceneName switch
                    {
                        ItemChanger.SceneNames.Tutorial_01 => "King's Pass",
                        ItemChanger.SceneNames.Town => "Dirtmouth Hardsave",
                        ItemChanger.SceneNames.Deepnest_Spider_Town when markerName == "Death Respawn Marker Hegemol" => "Herrah Hardsave",
                        ItemChanger.SceneNames.Deepnest_Spider_Town when markerName == "Death Respawn Marker" => "Trap bench Hardsave",
                        ItemChanger.SceneNames.Ruins2_Watcher_Room => "Lurien Hardsave",
                        ItemChanger.SceneNames.Fungus3_archive_02 => "Monomon Hardsave",
                        ItemChanger.SceneNames.GG_Waterways => "Junk Pit Hardsave",
                        ItemChanger.SceneNames.Crossroads_ShamanTemple => "Ancestral Mound Hardsave",
                        ItemChanger.SceneNames.Ruins1_24 => "Soul Master",
                        ItemChanger.SceneNames.Room_Wyrm => "Cast-off Shell",
                        ItemChanger.SceneNames.Abyss_10 => "Shade Cloak",
                        ItemChanger.SceneNames.Abyss_05 => "Palace Grounds",
                        ItemChanger.SceneNames.Fungus2_21 => "City Crest Hardsave",
                        _ => null,
                    };
                    break;
            }

            if (string.IsNullOrEmpty(benchName))
            {
                FStatsMod.instance.LogWarn($"No bench found for key ({sceneName}, {markerName})");
                return;
            }

            BenchwarpCount.IncrementValue(benchName);
        }

        public override void OnUnload()
        {
            Events.OnBenchwarp -= OnBenchwarp;
        }
        private IEnumerable<DisplayInfo> ConditionalGetDisplayInfosBoth(bool global)
        {
            if (BenchwarpCount.Values.Sum() == 0)
            {
                return Enumerable.Empty<DisplayInfo>();
            }

            List<string> warpInfo = BenchwarpCount.OrderByDescending(kvp => kvp.Value).Select(kvp => $"{kvp.Key} - {kvp.Value}").ToList();

            DisplayInfo template = new()
            {
                Title = "Bench Warps" + SaveFileCountString(),
                MainStat = $"Total warps: {BenchwarpCount.Values.Sum()}",
                Priority = BuiltinScreenPriorityValues.BenchwarpStats,
            };

            return ColumnUtility.CreateDisplay(template, warpInfo, singlePage: global, maxColumnsPerPage: 3);
        }

        public override IEnumerable<DisplayInfo> ConditionalGetGlobalDisplayInfos() => ConditionalGetDisplayInfosBoth(global: true);
        public override IEnumerable<DisplayInfo> ConditionalGetDisplayInfos() => ConditionalGetDisplayInfosBoth(global: false);
    }
}
