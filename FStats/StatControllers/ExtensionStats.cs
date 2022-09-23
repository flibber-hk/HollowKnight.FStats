using System;
using System.Collections.Generic;
using FStats.Util;

namespace FStats.StatControllers
{
    public class ExtensionStats : StatController
    {
        public override void Initialize() { }
        public override void Unload() { }

        public override IEnumerable<DisplayInfo> GetDisplayInfos()
        {
            List<string> stats = API.BuildExtensionStats();
            if (stats.Count == 0) yield break;

            List<string> columns = new();

            int numColumns = (int)Math.Ceiling(stats.Count / 11f);

            for (int i = 0; i < numColumns; i++)
            {
                columns.Add(string.Join("\n", stats.Slice(i, numColumns)));
            }

            yield return new()
            {
                Title = "Extension Stats",
                MainStat = "",
                StatColumns = columns,
                Priority = BuiltinScreenPriorityValues.ExtensionStats,
            };
        }
    }
}
