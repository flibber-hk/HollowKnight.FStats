using System;
using System.Collections.Generic;
using System.Linq;
using Benchwarp;

namespace FStats.StatControllers.ModConditional
{
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
                        benchName = NameForBench(target);
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

        private string NameForBench(Bench target)
        {
            string name = Events.GetBenchName(target);

            if (AreaNameBenches.Contains(name))
            {
                name = $"{target.areaName} {name}";
            }

            return name;
        }

        public override void OnUnload()
        {
            Events.OnBenchwarp -= OnBenchwarp;
        }
        public override IEnumerable<DisplayInfo> ConditionalGetDisplayInfos()
        {
            if (BenchwarpCount.Values.Sum() == 0)
            {
                yield break;
            }

            List<string> warpInfo = BenchwarpCount.OrderByDescending(kvp => kvp.Value).Select(kvp => $"{kvp.Key} - {kvp.Value}").ToList();

            List<string> columns = new();

            if (warpInfo.Count <= 10)
            {
                columns.Add(string.Join("\n", warpInfo));
            }
            else if (warpInfo.Count <= 20)
            {
                columns.Add(string.Join("\n", warpInfo.Slice(0, 2)));
                columns.Add(string.Join("\n", warpInfo.Slice(1, 2)));
            }
            else if (warpInfo.Count <= 27)
            {
                columns.Add(string.Join("\n", warpInfo.Slice(0, 3)));
                columns.Add(string.Join("\n", warpInfo.Slice(1, 3)));
                columns.Add(string.Join("\n", warpInfo.Slice(2, 3)));
            }
            else
            {
                warpInfo = warpInfo.Take(27).ToList();
                columns.Add(string.Join("\n", warpInfo.Slice(0, 3)));
                columns.Add(string.Join("\n", warpInfo.Slice(1, 3).Append("...")));
                columns.Add(string.Join("\n", warpInfo.Slice(2, 3)));
            }

            yield return new()
            {
                Title = "Bench Warps",
                MainStat = $"Total warps: {BenchwarpCount.Values.Sum()}",
                StatColumns = columns
            };
        }
    }
}
