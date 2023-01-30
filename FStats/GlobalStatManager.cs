using System;
using System.Collections.Generic;
using System.Linq;

namespace FStats
{
    internal class GlobalStatManager
    {
        private int _loadedCount;
        private List<StatController> _trackedStats { get; init; }

        public static GlobalStatManager Load()
        {
            return new() { _trackedStats = new() };
            // throw new NotImplementedException();
        }

        public void Save() 
        {
            // throw new NotImplementedException(); 
        }

        public void Initialize(int count)
        {
            if (_loadedCount > 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                _trackedStats[i].Initialize();
            }
            _loadedCount = count;
        }

        public int InitializeAll()
        {
            if (_loadedCount > 0)
            {
                return _loadedCount;
            }

            foreach (StatController controller in _trackedStats)
            {
                controller.Initialize();
                controller.FileCount += 1;
            }
            _loadedCount = _trackedStats.Count;
            return _loadedCount;
        }

        public void Unload()
        {
            for (int i = _loadedCount - 1; i >= 0; i--)
            {
                _trackedStats[i].Unload();
            }
            _loadedCount = 0;
        }

        public List<DisplayInfo> GenerateDisplays()
        {
            List<DisplayInfo> infos = new();

            for (int i = 0; i < _loadedCount; i++)
            {
                StatController sc = _trackedStats[i];
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
