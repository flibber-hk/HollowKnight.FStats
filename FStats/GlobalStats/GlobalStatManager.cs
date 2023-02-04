using FStats.Interfaces;
using Modding.Patches;
using Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FStats.GlobalStats
{
    public class GlobalStatManager : IStatCollection
    {
        private static readonly ILogger _logger = new SimpleLogger("FStats.GlobalStatManager");

        private List<string> _loadedStats { get; set; }

        private GlobalStatData _serializedData;
        internal Dictionary<string, StatController> TrackedStats { get; }

        public T Get<T>() where T : StatController
        {
            return TrackedStats.OfType<T>().FirstOrDefault();
        }

        IEnumerable<StatController> IStatCollection.EnumerateActiveStats()
        {
            foreach (string typeName in _loadedStats ?? Enumerable.Empty<string>())
            {
                yield return TrackedStats[typeName];
            }
        }

        internal void AddGlobalStats()
        {
            foreach ((Type t, Func<StatController> maker) in API.GlobalStatTypes)
            {
                string typeName = t.FullName;

                if (TrackedStats.ContainsKey(typeName)) continue;
                _logger.LogDebug($"Adding new {typeName}");
                TrackedStats.Add(typeName, maker());
            }
        }

        internal GlobalStatManager(Dictionary<string, StatController> stats)
        {
            TrackedStats = stats;
        }


        public List<string> Initialize(List<string> typeNames)
        {
            if (!FStatsMod.GS.TrackGlobalStats) return new();

            if (_loadedStats is not null) return new(_loadedStats);

            List<string> loadedTypeNames = new();
            foreach (string typeName in typeNames)
            {
                if (TrackedStats.TryGetValue(typeName, out StatController sc))
                {
                    loadedTypeNames.Add(typeName);
                    sc.Initialize();
                }
            }
            return loadedTypeNames;
        }

        public List<string> InitializeAll()
        {
            if (!FStatsMod.GS.TrackGlobalStats) return new();

            if (_loadedStats is not null)
            {
                return new(_loadedStats);
            }

            _loadedStats = new();

            foreach ((string typeName, StatController sc) in TrackedStats)
            {
                if (sc is null) continue;
                sc.Initialize();
                sc.FileCount += 1;
                _loadedStats.Add(typeName);
            }

            return _loadedStats;
        }

        public void Unload()
        {
            foreach (string typeName in _loadedStats ?? Enumerable.Empty<string>())
            {
                TrackedStats[typeName]?.Unload();
            }
            _loadedStats = null;
        }

        public List<DisplayInfo> GenerateDisplays()
        {
            if (!FStatsMod.GS.TrackGlobalStats
                || !FStatsMod.GS.ShowGlobalStats)
            {
                return new();
            }

            List<DisplayInfo> infos = new();

            foreach (string typeName in _loadedStats ?? Enumerable.Empty<string>())
            {
                StatController sc = TrackedStats[typeName];
                if (!FStatsMod.GS.ShouldDisplay(sc)) continue;

                foreach (DisplayInfo info in sc.GetGlobalDisplayInfos())
                {
                    infos.Add(info);
                }
            }

            foreach (DisplayInfo info in infos)
            {
                info.StatColumns = info.StatColumns?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new();
            }

            infos = infos
                .Where(x => !string.IsNullOrEmpty(x.Title))
                .OrderBy(x => x.Priority)
                .ToList();

            API.FilterScreens(infos);

            return infos;
        }
    }
}
