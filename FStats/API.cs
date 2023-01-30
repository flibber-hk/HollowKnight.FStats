using System;
using System.Collections.Generic;
using System.Linq;

namespace FStats
{
    public static class API
    {
        public delegate void ScreenGenerator(Action<DisplayInfo> registerPage);
        
        /// <summary>
        /// Event invoked when FStats generates the end screen.
        /// Subscribers may invoke the `registerPage` argument to add a page to
        /// the end screen.
        /// </summary>
        public static event ScreenGenerator OnGenerateScreen;

        internal static List<DisplayInfo> BuildAdditionalPages()
        {
            List<DisplayInfo> pages = new();
            OnGenerateScreen?.Invoke(info => pages.Add(info));
            return pages;
        }


        public delegate void StatGenerator(Action<StatController> registerStat);

        /// <summary>
        /// Event invoked when FStats generates the list of stat controllers at the start of a save file.
        /// Subscribers may invoke the `registerStat` argument to add a stat controller to the file.
        /// </summary>
        public static event StatGenerator OnGenerateFile;

        internal static List<StatController> BuildAdditionalStats()
        {
            List<StatController> stats = new();
            OnGenerateFile?.Invoke(sc => stats.Add(sc));
            return stats;
        }

        public delegate void ExtensionStatAdder(Action<string> addEntry);

        /// <summary>
        /// Event invoked when FStats builds the extension stats screen - intended for
        /// mods which want to add a small number of stats and do not mind their screen
        /// being shared with other mods.
        /// </summary>
        public static event ExtensionStatAdder OnBuildExtensionStats;

        internal static List<string> BuildExtensionStats()
        {
            List<string> stats = new();
            OnBuildExtensionStats?.Invoke(s => stats.Add(s));
            return stats;
        }

        /// <summary>
        /// This event will be applied to all screens; if any subscriber returns true
        /// for a particular <see cref="DisplayInfo"/>, that screen will not be shown.
        /// </summary>
        public static event Func<DisplayInfo, bool> DisplaySuppressor;

        internal static void FilterScreens(List<DisplayInfo> infos)
        {
            if (DisplaySuppressor is null) return;

            Delegate[] suppressors = DisplaySuppressor.GetInvocationList();

            infos.RemoveAll(ShouldRemove);

            bool ShouldRemove(DisplayInfo info)
            {
                foreach (Func<DisplayInfo, bool> suppressor in suppressors.Cast<Func<DisplayInfo, bool>>())
                {
                    if (suppressor(info)) return true;
                }
                return false;
            }
        }

        internal static List<(Type type, Func<StatController> maker)> GlobalStatTypes = new();
        /// <summary>
        /// Register a Global StatController, to be loaded across all (new) save files.
        /// 
        /// This function should be called during Mod.Initialize, or the stat controller might not be registered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RegisterGlobalStat<T>() where T : StatController, new() => GlobalStatTypes.Add((typeof(T), () => new T()));
    }
}
