using System.Collections.Generic;
using System.Linq;
using Modding;

namespace FStats.StatControllers
{
    public class KeyTimeline : StatController
    {
        internal static readonly Dictionary<string, string> BoolNames = new()
        {
            [nameof(PlayerData.hasDreamGate)] = "Dreamgate",
            [nameof(PlayerData.dreamNailUpgraded)] = "Awoken Dream Nail",
            [nameof(PlayerData.hasCyclone)] = "Cyclone Slash",
            [nameof(PlayerData.hasDashSlash)] = "Great Slash",
            [nameof(PlayerData.hasUpwardSlash)] = "Dash Slash",
            [nameof(PlayerData.lurienDefeated)] = "Lurien",
            [nameof(PlayerData.monomonDefeated)] = "Monomon",
            [nameof(PlayerData.hegemolDefeated)] = "Herrah",
            [nameof(PlayerData.royalCharmState) + "/1"] = "White Fragment",
            [nameof(PlayerData.royalCharmState) + "/2"] = "White Fragment",
            [nameof(PlayerData.royalCharmState) + "/3"] = "Kingsoul",
            [nameof(PlayerData.gotShadeCharm)] = "Void Heart",
            [nameof(PlayerData.hasSlykey)] = "Shopkeeper's Key",
            [nameof(PlayerData.hasWhiteKey)] = "Elegant Key",
            [nameof(PlayerData.hasLoveKey)] = "Love Key",
            [nameof(PlayerData.hasTramPass)] = "Tram Pass",
            [nameof(ItemChanger.Modules.ElevatorPass.hasElevatorPass)] = "Elevator Pass",
            [nameof(PlayerData.hasLantern)] = "Lumafly Lantern",
            [nameof(PlayerData.hasKingsBrand)] = "King's Brand",
            [nameof(PlayerData.hasCityKey)] = "City Crest",
            [nameof(PlayerData.gotCharm_40)] = "Grimmchild",
            [nameof(PlayerData.gotCharm_17)] = "Spore Shroom",
            [nameof(PlayerData.gotCharm_10)] = "Defender's Crest",
            ["Dreamer"] = "Dreamer",
        };

        internal static Dictionary<string, List<string>> Exclusions = new()
        {
            [nameof(PlayerData.gotCharm_17)] = new() { nameof(RandoPlus.GlobalSettings.MrMushroom) },
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

        private bool DupeDreamerObtained(int dreamers)
        {
            if (SkillObtainTimeline.ContainsKey(nameof(PlayerData.lurienDefeated)))
            {
                dreamers--;
            }
            if (SkillObtainTimeline.ContainsKey(nameof(PlayerData.monomonDefeated)))
            {
                dreamers--;
            }
            if (SkillObtainTimeline.ContainsKey(nameof(PlayerData.hegemolDefeated)))
            {
                dreamers--;
            }
            return dreamers > 0;
        }

        private int RecordPlayerDataInt(string name, int orig)
        {
            if (name == "guardiansDefeated" && DupeDreamerObtained(orig) && !SkillObtainTimeline.ContainsKey("Dreamer"))
            {
                SkillObtainTimeline["Dreamer"] = Common.Instance.CountedTime;
                return orig;
            }
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
            List<string> Lines = BoolNames
                .Where(kvp => SkillObtainTimeline.ContainsKey(kvp.Key))
                .Where(kvp => !IsExcluded(kvp.Key))
                .OrderBy(kvp => SkillObtainTimeline[kvp.Key])
                .Select(kvp => $"{kvp.Value}: {SkillObtainTimeline[kvp.Key].PlaytimeHHMMSS()}")
                .ToList();

            List<string> Columns;

            if (Lines.Count <= 10)
            {
                Columns = new() { string.Join("\n", Lines) };
            }
            else if (Lines.Count <= 20)
            {
                Columns = new()
                {
                    string.Join("\n", Lines.Slice(0, 2)),
                    string.Join("\n", Lines.Slice(1, 2)),
                };
            }
            else
            {
                Columns = new()
                {
                    string.Join("\n", Lines.Slice(0, 3)),
                    string.Join("\n", Lines.Slice(1, 3)),
                    string.Join("\n", Lines.Slice(2, 3)),
                };
            }

            yield return new()
            {
                Title = "Key Timeline",
                MainStat = Common.Instance.TotalTimeString,
                StatColumns = Columns,
            };
        }

        private bool IsExcluded(string skillName)
        {
            if (skillName == nameof(PlayerData.gotCharm_17))
            {
                //RandoPlus.GlobalSettings randoPlus = new RandoPlus.GlobalSettings();
                //RandomizerMod.Settings.PoolSettings randorando = new RandomizerMod.Settings.PoolSettings();
                //if (!RandoPlus.GS.MrMushroom)
                //{
                //    return true;
                //}
            }

            return false;
        }
    }
}