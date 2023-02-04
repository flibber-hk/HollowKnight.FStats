using FStats.Attributes;
using FStats.Util;
using Modding;
using System.Collections.Generic;
using System.Linq;

namespace FStats.StatControllers
{
    [ScreenNameOverride(typeof(SkillTimeline))]
    internal class GlobalSkillTimeline : StatController
    {
        private static Dictionary<string, string> BoolNames => SkillTimeline.BoolNames;

        public Dictionary<string, float> SkillObtainTimes = new();
        public Dictionary<string, int> SkillObtainCounts = new();

        public float AverageTime(string key) => SkillObtainTimes[key] / SkillObtainCounts[key];

        private string Render(string key)
        {
            if (!SkillObtainTimes.ContainsKey(key)) return string.Empty;
            if (!SkillObtainCounts.TryGetValue(key, out int count)) return string.Empty;
            if (count == 0) return string.Empty;

            string fileOrFiles = count == 1 ? "(1)" : $"({count} files)";

            return $"{key}: {AverageTime(key).PlaytimeHHMMSS()} {fileOrFiles}";
        }


        public override void Initialize()
        {
            ModHooks.SetPlayerIntHook += RecordPlayerDataInt;
            ModHooks.SetPlayerBoolHook += RecordPlayerDataBool;
        }
        public override void Unload()
        {
            ModHooks.SetPlayerIntHook -= RecordPlayerDataInt;
            ModHooks.SetPlayerBoolHook -= RecordPlayerDataBool;
        }
        private int RecordPlayerDataInt(string name, int orig)
        {
            if (PlayerData.instance.GetIntInternal(name) >= orig) return orig;

            Record($"{name}/{orig}");
            return orig;
        }
        private bool RecordPlayerDataBool(string name, bool orig)
        {
            if (PlayerData.instance.GetBoolInternal(name) || !orig) return orig;

            if (orig) Record(name);
            return orig;
        }

        private void Record(string s)
        {
            if (BoolNames.ContainsKey(s))
            {
                // Get from the local settings when recording
                SkillObtainTimes.IncreaseValue(s, FStatsMod.LS.Get<Common>().CountedTime);
                SkillObtainCounts.IncrementValue(s);
            }
        }

        public override IEnumerable<DisplayInfo> GetGlobalDisplayInfos()
        {
            List<string> lines = BoolNames
                .Where(kvp => SkillObtainTimes.ContainsKey(kvp.Key) && SkillObtainCounts.TryGetValue(kvp.Key, out int count) && count > 0)
                .OrderBy(kvp => AverageTime(kvp.Key))
                .Select(kvp => Render(kvp.Key))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            if (lines.Count == 0)
            {
                return Enumerable.Empty<DisplayInfo>();
            }

            DisplayInfo template = new()
            {
                Title = "Average time to get skills",
                Priority = BuiltinScreenPriorityValues.SkillTimeline,
            };

            return ColumnUtility.CreateDisplay(template, lines);
        }
    }
}
