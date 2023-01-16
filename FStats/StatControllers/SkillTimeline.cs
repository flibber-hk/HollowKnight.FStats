using System;
using System.Collections.Generic;
using System.Linq;
using FStats.Util;
using Modding;

namespace FStats.StatControllers
{
    public class SkillTimeline : StatController
    {
        internal static readonly Dictionary<string, string> BoolNames = new()
        {
            [nameof(PlayerData.hasDash)] = "Mothwing Cloak",
            [nameof(PlayerData.hasWalljump)] = "Mantis Claw",
            [nameof(PlayerData.hasSuperDash)] = "Crystal Heart",
            [nameof(PlayerData.hasDoubleJump)] = "Monarch Wings",
            [nameof(PlayerData.hasShadowDash)] = "Shade Cloak",
            [nameof(PlayerData.hasAcidArmour)] = "Isma's Tear",
            [nameof(PlayerData.hasDreamNail)] = "Dream Nail",
            [nameof(PlayerData.fireballLevel) + "/1"] = "Vengeful Spirit",
            [nameof(PlayerData.fireballLevel) + "/2"] = "Shade Soul",
            [nameof(PlayerData.quakeLevel) + "/1"] = "Desolate Dive",
            [nameof(PlayerData.quakeLevel) + "/2"] = "Descending Dark",
            [nameof(PlayerData.screamLevel) + "/1"] = "Howling Wraiths",
            [nameof(PlayerData.screamLevel) + "/2"] = "Abyss Shriek",
            [nameof(ItemChanger.Modules.SplitClaw.hasWalljumpLeft)] = "Left Mantis Claw",
            [nameof(ItemChanger.Modules.SplitClaw.hasWalljumpRight)] = "Right Mantis Claw",
            [nameof(ItemChanger.Modules.SplitCloak.canDashLeft)] = "Left Mothwing Cloak",
            [nameof(ItemChanger.Modules.SplitCloak.canDashRight)] = "Right Mothwing Cloak",
            [nameof(ItemChanger.Modules.SplitSuperdash.hasSuperdashLeft)] = "Left Crystal Heart",
            [nameof(ItemChanger.Modules.SplitSuperdash.hasSuperdashRight)] = "Right Crystal Heart",
            [nameof(ItemChanger.Modules.SwimSkill.canSwim)] = "Swim",
            [nameof(ItemChanger.Modules.FocusSkill.canFocus)] = "Focus",
            [nameof(ItemChanger.Modules.SplitNail.canSideslashLeft)] = ItemChanger.ItemNames.Leftslash,
            [nameof(ItemChanger.Modules.SplitNail.canSideslashRight)] = ItemChanger.ItemNames.Rightslash,
            [nameof(ItemChanger.Modules.SplitNail.canUpslash)] = ItemChanger.ItemNames.Upslash,
            // [nameof(ItemChanger.Modules.SplitNail.canDownslash)] = ItemChanger.ItemNames.Downslash,
        };

        internal static Dictionary<string, List<string>> Exclusions = new()
        {
            [nameof(PlayerData.hasWalljump)] = new() { nameof(ItemChanger.Modules.SplitClaw.hasWalljumpLeft), nameof(ItemChanger.Modules.SplitClaw.hasWalljumpRight) },
            [nameof(PlayerData.hasDash)] = new() { nameof(ItemChanger.Modules.SplitCloak.canDashLeft), nameof(ItemChanger.Modules.SplitCloak.canDashRight) },
            [nameof(PlayerData.hasSuperDash)] = new() { nameof(ItemChanger.Modules.SplitSuperdash.hasSuperdashLeft), nameof(ItemChanger.Modules.SplitSuperdash.hasSuperdashRight) },
        };

        public Dictionary<string, float> SkillObtainTimeline = new();

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
            Record($"{name}/{orig}");
            return orig;
        }
        private bool RecordPlayerDataBool(string name, bool orig)
        {
            if (orig) Record(name);
            return orig;
        }

        private void Record(string s)
        {
            if (BoolNames.ContainsKey(s) && !SkillObtainTimeline.ContainsKey(s))
            {
                SkillObtainTimeline[s] = Common.Instance.CountedTime;
            }
        }


        public override IEnumerable<DisplayInfo> GetDisplayInfos()
        {
            List<string> lines = BoolNames
                .Where(kvp => SkillObtainTimeline.ContainsKey(kvp.Key))
                .Where(kvp => !IsExcluded(kvp.Key))
                .OrderBy(kvp => SkillObtainTimeline[kvp.Key])
                .Select(kvp => $"{kvp.Value}: {SkillObtainTimeline[kvp.Key].PlaytimeHHMMSS()}")
                .ToList();

            if (lines.Count == 0)
            {
                return Enumerable.Empty<DisplayInfo>();
            }

            DisplayInfo template = new()
            {
                Title = "Skill Timeline",
                MainStat = Common.Instance.TotalTimeString,
                Priority = BuiltinScreenPriorityValues.SkillTimeline,
            };

            return ColumnUtility.CreateDisplay(template, lines);
        }

        private bool IsExcluded(string skillName)
        {
            if (Exclusions.TryGetValue(skillName, out List<string> exclusions))
            {
                if (exclusions.Any(x => SkillObtainTimeline.ContainsKey(x)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
