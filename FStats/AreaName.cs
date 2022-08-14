using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FStats
{
    public static class AreaName
    {
        public const string Other = "Other";

        private static readonly List<string> suffixes = new() { "_boss_defeated", "_boss", "_preload" };

        private static Dictionary<string, string> sceneToArea;

        public static readonly List<string> Areas = new()
        {
            "Black Egg Temple",
            "Dirtmouth",
            "Forgotten Crossroads",
            "Greenpath",
            "Fog Canyon",
            "Fungal Wastes",
            "Deepnest",
            "Kingdom's Edge",
            "Ancient Basin",
            "Royal Waterways",
            "City of Tears",
            "Resting Grounds",
            "Crystal Peak",
            "Queen's Gardens",
            "Howling Cliffs",
            "White Palace",
            "Godhome",
            Other,
        };

        public static void LoadData()
        {
            JsonSerializer js = new JsonSerializer
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
            };

            Stream json = typeof(AreaName).Assembly.GetManifestResourceStream("FStats.Resources.SceneToArea.json");
            StreamReader sr = new StreamReader(json);
            JsonTextReader jtr = new JsonTextReader(sr);
            sceneToArea = js.Deserialize<Dictionary<string, string>>(jtr);
        }

        /// <summary>
        /// Given a scene, returns the area containing that scene. Areas are those considered by area rando, plus Godhome and White Palace.
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static string CleanSubareaName(string scene)
        {
            foreach (string suffix in suffixes)
            {
                if (scene.EndsWith(suffix))
                {
                    scene = scene.Substring(0, scene.Length - suffix.Length);
                }
            }
            if (sceneToArea.TryGetValue(scene, out string area))
            {
                return area;
            }
            else if (scene.StartsWith("GG_"))
            {
                // GG_Waterways, Pipeway, Shortcut and Lurker are in the JSON
                return "Godhome";
            }
            else if (scene == "Room_Tram_RG")
            {
                switch (PlayerData.instance.GetInt(nameof(PlayerData.tramRestingGroundsPosition)))
                {
                    case 0:
                        return "Forgotten Crossroads";
                    case 1:
                        return "Resting Grounds";
                    default:
                        FStatsMod.instance.Log("Unexpected Upper Tram Position");
                        break;
                }
            }
            else if (scene == "Room_Tram")
            {
                switch (PlayerData.instance.GetInt(nameof(PlayerData.tramLowerPosition)))
                {
                    case 0:
                        return "Deepnest";
                    case 1:
                        return "Ancient Basin";
                    case 2:
                        return "Kingdom's Edge";
                    default:
                        FStatsMod.instance.Log("Unexpected Lower Tram Position");
                        break;
                }
            }

            else if (scene == Other) return Other;

            return string.Empty;
        }

        /// <summary>
        /// Given a scene, returns the map region containing that scene. Black Egg Temple, Godhome and White Palace are counted separately.
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static string CleanAreaName(string scene)
        {
            string area = CleanSubareaName(scene);

            switch (area)
            {
                case "Black Egg Temple":
                    return "Black Egg Temple";
                case "Dirtmouth":
                    return "Dirtmouth";
                case "Forgotten Crossroads":
                case "Ancestral Mound":
                    return "Forgotten Crossroads";
                case "Greenpath":
                case "Lake of Unn":
                case "Stone Sanctuary":
                    return "Greenpath";
                case "Fog Canyon":
                case "Overgrown Mound":
                case "Teacher's Archives":
                    return "Fog Canyon";
                case "Queen's Station":
                case "Fungal Wastes":
                case "Mantis Village":
                case "Fungal Core":
                    return "Fungal Wastes";
                case "Deepnest":
                case "Distant Village":
                case "Beast's Den":
                case "Failed Tramway":
                case "Weavers' Den":
                    return "Deepnest";
                case "Kingdom's Edge":
                case "Cast Off Shell":
                case "Colosseum":
                case "Hive":
                    return "Kingdom's Edge";
                case "Ancient Basin":
                case "Palace Grounds":
                case "Abyss":
                    return "Ancient Basin";
                case "Royal Waterways":
                case "Isma's Grove":
                case "Junk Pit":
                    return "Royal Waterways";
                case "City of Tears":
                case "Soul Sanctum":
                case "King's Station":
                case "Tower of Love":
                case "Pleasure House":
                    return "City of Tears";
                case "Resting Grounds":
                case "Blue Lake":
                case "Spirits' Glade":
                    return "Resting Grounds";
                case "Crystal Peak":
                case "Hallownest's Crown":
                case "Crystallized Mound":
                    return "Crystal Peak";
                case "Queen's Gardens":
                    return "Queen's Gardens";
                case "King's Pass":
                case "Howling Cliffs":
                case "Stag Nest":
                    return "Howling Cliffs";
                case "White Palace":
                    return "White Palace";
                case "Godhome":
                    return "Godhome";
                default:
                    return area;
            }
        }

    }
}
