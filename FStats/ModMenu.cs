using System;
using System.Collections.Generic;
using System.Linq;
using MenuEntry = Modding.IMenuMod.MenuEntry;

namespace FStats
{
    internal static class ModMenu
    {
        internal static List<MenuEntry> GetMenuData()
        {
            return new()
            {
                new()
                {
                    Name = "Track Global Stats",
                    Description = "Disable to prevent tracking lifetime stats.",
                    Values = new[]{ "False", "True" },
                    Saver = v => FStatsMod.GS.TrackGlobalStats = v == 1,
                    Loader = () => FStatsMod.GS.TrackGlobalStats ? 1 : 0,
                },

                new()
                {
                    Name = "Show Global Stats",
                    Description = "Whether lifetime stats should be displayed.",
                    Values = new[]{ "False", "True" },
                    Saver = v => FStatsMod.GS.ShowGlobalStats = v == 1,
                    Loader = () => FStatsMod.GS.ShowGlobalStats ? 1 : 0,
                },

                new()
                {
                    Name = "Prevent Global Stats Saving",
                    Description = "Enable to prevent saving global stats.",
                    Values = Enum.GetValues(typeof(SettingType)).Cast<SettingType>().Select(x => x.ToString()).ToArray(),
                    Saver = v => FStatsMod.GS.PreventSavingGlobalStats = (SettingType)v,
                    Loader = () => (int)FStatsMod.GS.PreventSavingGlobalStats,
                },
            };
        }
    }
}
