using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Modding;
using UnityEngine;

namespace FStats.StatControllers
{
    public class CombatStats : StatController
    {
        public int KillCount = 0;
        public Dictionary<AttackTypes, int> ReducedDamageByType = Enum.GetValues(typeof(AttackTypes)).Cast<AttackTypes>().ToDictionary(type => type, type => 0);
        public int DamageTaken = 0;
        public int DeathCount = 0;
        public float TimeWithoutShade = 0f;
        public int GeoLostToDeaths = 0;
        public float TimeAtOneHP = 0f;

        public override void Initialize()
        {
            ModHooks.HeroUpdateHook += IncreaseTimers;
            On.HealthManager.SendDeathEvent += CountKills;
            On.HealthManager.TakeDamage += CountEnemyDamage;

            On.PlayerData.TakeHealth += CountDamageTaken;
            ModHooks.AfterPlayerDeadHook += CountDeath;
            ModHooks.SetPlayerIntHook += CountGeoLost;
        }

        private int CountGeoLost(string name, int orig)
        {
            if (name == nameof(PlayerData.geoPool) && PlayerData.instance.GetBool(nameof(PlayerData.soulLimited)))
                GeoLostToDeaths += PlayerData.instance.GetInt(nameof(PlayerData.geoPool));
            return orig;
        }

        private void CountDamageTaken(On.PlayerData.orig_TakeHealth orig, PlayerData self, int amount)
        {
            DamageTaken += amount;

            orig(self, amount);
        }
        private void CountDeath()
        {
            DeathCount++;
        }

        private void IncreaseTimers()
        {
            string shadeScene = PlayerData.instance.GetString(nameof(PlayerData.shadeScene));
            if (!string.IsNullOrEmpty(shadeScene) && shadeScene != "None")
            {
                GameManager.instance.IncreaseGameTimer(ref TimeWithoutShade);
            }
            if (PlayerData.instance.GetInt(nameof(PlayerData.health)) == 1 && PlayerData.instance.GetInt(nameof(PlayerData.healthBlue)) <= 0)
            {
                GameManager.instance.IncreaseGameTimer(ref TimeAtOneHP);
            }
        }

        private void CountEnemyDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            // This doesn't count some types of damage - e.g. Dreamshield - and some types (e.g. Cdash) are counted in "generic"
            // I'm not sure what is counted in generic
            int damage = Mathf.RoundToInt(hitInstance.DamageDealt * hitInstance.Multiplier);
            if (self.damageOverride) damage = 1;
            // If self.hp <= 0, they did 0 damage. If self.hp >= damage, they did damage damage. If self.hp is between the two, they did self.hp damage. i.e. Mathf.Clamp is correct
            ReducedDamageByType[hitInstance.AttackType] += Mathf.Clamp(self.hp, 0, damage);

            orig(self, hitInstance);
        }

        private void CountKills(On.HealthManager.orig_SendDeathEvent orig, HealthManager self)
        {
            orig(self); KillCount++;
        }


        public override void Unload()
        {
            ModHooks.HeroUpdateHook -= IncreaseTimers;
            On.HealthManager.SendDeathEvent -= CountKills;
            On.HealthManager.TakeDamage -= CountEnemyDamage;

            On.PlayerData.TakeHealth -= CountDamageTaken;
            ModHooks.AfterPlayerDeadHook -= CountDeath;
            ModHooks.SetPlayerIntHook -= CountGeoLost;
        }

        public override IEnumerable<DisplayInfo> GetDisplayInfos()
        {
            StringBuilder leftcol = new StringBuilder();
            StringBuilder rightcol = new StringBuilder();

            leftcol.AppendLine($"{KillCount} enemies killed");

            leftcol.AppendLine($"{ReducedDamageByType[AttackTypes.Nail]} nail damage");
            leftcol.AppendLine($"{ReducedDamageByType[AttackTypes.Spell]} spell damage");


            rightcol.AppendLine($"{DamageTaken} damage taken");
            if (DeathCount > 0)
            {
                rightcol.AppendLine($"{DeathCount} deaths");
                rightcol.AppendLine(Common.Instance.GetTimePercentString(TimeWithoutShade, "without shade"));
                if (GeoLostToDeaths > 0) rightcol.AppendLine($"{GeoLostToDeaths} geo lost to deaths");
            }
            rightcol.AppendLine(Common.Instance.GetTimePercentString(TimeAtOneHP, "at 1HP"));

            string LeftColumn = leftcol.ToString();
            string RightColumn = rightcol.ToString();

            yield return new()
            {
                Title = "Combat Stats",
                StatColumns = new() { LeftColumn, RightColumn },
                Priority = BuiltinScreenPriorityValues.CombatStats,
            };
        }
    }
}
