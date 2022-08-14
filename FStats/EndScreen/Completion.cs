using System.Collections.Generic;

namespace FStats.EndScreen
{
    public static class Completion
    {
        private static readonly Dictionary<string, int> _percentageBools = new Dictionary<string, int>()
        {
            [nameof(PlayerData.killedFalseKnight)] = 1,
            [nameof(PlayerData.hornet1Defeated)] = 1,
            [nameof(PlayerData.hornetOutskirtsDefeated)] = 1,
            [nameof(PlayerData.killedMantisLord)] = 1,
            [nameof(PlayerData.killedMageLord)] = 1,
            [nameof(PlayerData.killedDungDefender)] = 1,
            [nameof(PlayerData.killedBlackKnight)] = 1,
            [nameof(PlayerData.killedInfectedKnight)] = 1,
            [nameof(PlayerData.killedMimicSpider)] = 1,
            [nameof(PlayerData.killedMegaJellyfish)] = 1,
            [nameof(PlayerData.killedTraitorLord)] = 1,
            [nameof(PlayerData.killedJarCollector)] = 1,
            [nameof(PlayerData.killedBigFly)] = 1,
            [nameof(PlayerData.killedMawlek)] = 1,
            [nameof(PlayerData.killedHiveKnight)] = 1,
            [nameof(PlayerData.colosseumBronzeCompleted)] = 1,
            [nameof(PlayerData.colosseumSilverCompleted)] = 1,
            [nameof(PlayerData.colosseumGoldCompleted)] = 1,
            [nameof(PlayerData.killedGhostAladar)] = 1,
            [nameof(PlayerData.killedGhostHu)] = 1,
            [nameof(PlayerData.killedGhostXero)] = 1,
            [nameof(PlayerData.killedGhostMarkoth)] = 1,
            [nameof(PlayerData.killedGhostNoEyes)] = 1,
            [nameof(PlayerData.killedGhostMarmu)] = 1,
            [nameof(PlayerData.killedGhostGalien)] = 1,
            [nameof(PlayerData.hasCyclone)] = 1,
            [nameof(PlayerData.hasDashSlash)] = 1,
            [nameof(PlayerData.hasUpwardSlash)] = 1,
            [nameof(PlayerData.hasDash)] = 2,
            [nameof(PlayerData.hasWalljump)] = 2,
            [nameof(PlayerData.hasDoubleJump)] = 2,
            [nameof(PlayerData.hasAcidArmour)] = 2,
            [nameof(PlayerData.hasSuperDash)] = 2,
            [nameof(PlayerData.hasShadowDash)] = 2,
            [nameof(PlayerData.hasKingsBrand)] = 2,
            [nameof(PlayerData.lurienDefeated)] = 1,
            [nameof(PlayerData.hegemolDefeated)] = 1,
            [nameof(PlayerData.monomonDefeated)] = 1,
            [nameof(PlayerData.hasDreamNail)] = 1,
            [nameof(PlayerData.dreamNailUpgraded)] = 1,
            [nameof(PlayerData.mothDeparted)] = 1,
            [nameof(PlayerData.killedGrimm)] = 1,
            [nameof(PlayerData.hasGodfinder)] = 1
        };

        public static int GetVanillaCompletion(this PlayerData pd)
        {
            int count = 0;
            pd.CountCharms();
            count += pd.GetInt(nameof(pd.charmsOwned));

            if (pd.GetBool(nameof(pd.killedNightmareGrimm)) || pd.GetBool(nameof(pd.destroyedNightmareLantern)))
            {
                count += 1;
            }

            count += pd.GetInt(nameof(pd.nailSmithUpgrades));
            count += pd.GetInt(nameof(pd.maxHealthBase)) - 5;

            switch (pd.GetInt(nameof(pd.MPReserveMax)))
            {
                case 99: count += 3; break;
                case 66: count += 2; break;
                case 33: count += 1; break;
            }

            count += pd.GetInt(nameof(pd.fireballLevel));
            count += pd.GetInt(nameof(pd.quakeLevel));
            count += pd.GetInt(nameof(pd.screamLevel));

            if (pd.bossDoorStateTier1.completed) count += 1;
            if (pd.bossDoorStateTier2.completed) count += 1;
            if (pd.bossDoorStateTier3.completed) count += 1;
            if (pd.bossDoorStateTier4.completed) count += 1;


            foreach (var kvp in _percentageBools)
            {
                if (pd.GetBool(kvp.Key)) count += kvp.Value;
            }

            return count;
        }
    }
}
