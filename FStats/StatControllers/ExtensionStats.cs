using System.Collections.Generic;
using System.Linq;
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
            if (stats.Count == 0) return Enumerable.Empty<DisplayInfo>();

            DisplayInfo template = new()
            {
                Title = "Extension Stats",
                MainStat = "",
                Priority = BuiltinScreenPriorityValues.ExtensionStats,
            };

            return ColumnUtility.CreateDisplay(template, stats, maxEntriesPerColumn: 11);
        }
    }
}
