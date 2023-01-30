using System.Collections.Generic;
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
                    Name = "Save Global Stats",
                    Description = "Disable to prevent saving lifetime stats at the end of this session.",
                    Values = new[]{ "True", "False" }, // Swapped because the name does not match the setting
                    Saver = v => FStatsMod.GS.PreventSavingGlobalStats = v == 1,
                    Loader = () => FStatsMod.GS.PreventSavingGlobalStats ? 1 : 0,
                },
            };
        }
    }
}
