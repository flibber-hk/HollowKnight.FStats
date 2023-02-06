using System;
using System.Collections.Generic;
using System.Reflection;
using FStats.Attributes;
using Modding;
using Modding.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FStats
{
    public class GlobalSettings
    {
        public bool TrackGlobalStats { get; set; } = true;
        public bool ShowGlobalStats { get; set; } = true;
        
        /// <summary>
        /// This can be set to true in a game session to prevent global stats from being saved.
        /// The intended use case would be that the player realised too late that they did not
        /// want to upset their global stats.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))] public SettingType PreventSavingGlobalStats { get; set; } = SettingType.Never;

        public Dictionary<string, bool> DisplayedScreens;
         
        public bool ShouldDisplay(StatController c)
        {
            Type type = c.GetType();
            if (type.GetCustomAttribute<ScreenNameOverrideAttribute>() is ScreenNameOverrideAttribute scoa)
            {
                type = scoa.Type;
            }

            if (type.GetCustomAttribute<GlobalSettingsExcludeAttribute>() is not null)
            {
                return true;
            }

            string screenName = type.Name;

            if (DisplayedScreens.TryGetValue(screenName, out bool shouldDisplay))
            {
                return shouldDisplay;
            }

            shouldDisplay = type.GetCustomAttribute<DefaultHiddenScreenAttribute>() == null;
            DisplayedScreens[screenName] = shouldDisplay;
            return shouldDisplay;
        }

        public GlobalSettings()
        {
            DisplayedScreens = new();
            foreach (Type type in typeof(GlobalSettings).Assembly.GetTypesSafely())
            {
                if (!type.IsAbstract && type.IsSubclassOf(typeof(StatController)))
                {
                    string key = type.Name;
                    bool displayed = type.GetCustomAttribute<Attributes.DefaultHiddenScreenAttribute>() == null;
                    DisplayedScreens[key] = displayed;
                }
            }
        }

        public void LoadFrom(GlobalSettings gs)
        {
            TrackGlobalStats = gs.TrackGlobalStats;
            ShowGlobalStats = gs.ShowGlobalStats;
            PreventSavingGlobalStats = gs.PreventSavingGlobalStats;

            foreach ((string key, bool displayed) in gs.DisplayedScreens)
            {
                DisplayedScreens[key] = displayed;
            }
        }
    }
}
