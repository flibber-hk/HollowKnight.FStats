using System;
using System.Collections.Generic;
using System.Reflection;
using Modding;
using Modding.Utils;

namespace FStats
{
    public class GlobalSettings
    {
        public Dictionary<string, bool> DisplayedScreens;
         
        public bool ShouldDisplay(StatController c)
        {
            Type type = c.GetType();
            string screenName = type.Name;

            if (DisplayedScreens.TryGetValue(screenName, out bool shouldDisplay))
            {
                return shouldDisplay;
            }

            shouldDisplay = type.GetCustomAttribute<Attributes.DefaultHiddenScreenAttribute>() == null;
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
            foreach ((string key, bool displayed) in gs.DisplayedScreens)
            {
                DisplayedScreens[key] = displayed;
            }
        }
    }
}
