using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HutongGames.PlayMaker;
using Modding;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;
using Vasi;

namespace FStats.StatControllers
{
    public class MiscStats : StatController
    {
        public int BreakableObjectsBroken;
        public int GrassObjectsBroken;
        public int SceneTransitions;
        // TODO - count these properly with IC involved
        public int GeoSpawned;
        public int SpawnedGeoCollected;

        public override void Initialize()
        {
            // Most breakables have the Breakable component; some others are special
            On.Breakable.Break += CountBreakable;
            IL.BreakablePoleSimple.OnTriggerEnter2D += CountBreakablePoleActivated;
            IL.BreakableInfectedVine.OnTriggerEnter2D += CountBreakableVineActivated;
            IL.InfectedBurstLarge.OnTriggerEnter2D += CountInfectionBubble;
            On.JellyEgg.Burst += CountJellyEgg;

            // This is the easiest way to count grass that is actually broken. When hit:
            // - GrassCut destroys itself (as in, the GrassCut component)
            // - TownGrass sets its gameobject to be inactive
            // - GrassSpriteBehaviour sets the isCut field, so won't be cut again
            // Each object has at most one of these components, and any breakable grass object has exactly one (I think).
            IL.GrassCut.OnTriggerEnter2D += CountGrass;
            IL.TownGrass.OnTriggerEnter2D += CountGrass;
            IL.GrassSpriteBehaviour.OnTriggerEnter2D += CountGrass;

            // Menderbug pole, gramaphones, a few crossroads poles, a few lights, something in colo (which I haven't figured out yet).
            // All seem to be broken by this fsm, which unhelpfully has the name FSM but is the only fsm which contains the "Spider Egg?" state.
            On.PlayMakerFSM.OnEnable += CountFsmBreakables;

            On.GeoControl.OnEnable += CountSpawnedGeo;
            IL.GeoControl.OnTriggerEnter2D += CountCollectedGeo;

            On.GameManager.BeginSceneTransition += CountSceneTransitions;
        }

        private void CountSceneTransitions(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            if (info.GetType() == typeof(GameManager.SceneLoadInfo))
            {
                SceneTransitions++;
            }

            orig(self, info);
        }

        public override void Unload()
        {
            On.Breakable.Break -= CountBreakable;
            IL.BreakablePoleSimple.OnTriggerEnter2D -= CountBreakablePoleActivated;
            IL.BreakableInfectedVine.OnTriggerEnter2D -= CountBreakableVineActivated;
            IL.InfectedBurstLarge.OnTriggerEnter2D -= CountInfectionBubble;
            On.JellyEgg.Burst -= CountJellyEgg;

            IL.GrassCut.OnTriggerEnter2D -= CountGrass;
            IL.TownGrass.OnTriggerEnter2D -= CountGrass;
            IL.GrassSpriteBehaviour.OnTriggerEnter2D -= CountGrass;

            On.PlayMakerFSM.OnEnable -= CountFsmBreakables;

            On.GeoControl.OnEnable -= CountSpawnedGeo;
            IL.GeoControl.OnTriggerEnter2D -= CountCollectedGeo;
        }

        // Sometimes might overcount geo in rando
        private void CountSpawnedGeo(On.GeoControl.orig_OnEnable orig, GeoControl self)
        {
            int size = 1;
            if (self.type > 0) size = 5;
            if (self.type > 1) size = 25;

            GeoSpawned += size;
            orig(self);
        }
        private void CountCollectedGeo(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext
            (
                i => i.MatchLdfld<GeoControl.Size>("value"),
                i => i.MatchCallvirt<HeroController>(nameof(HeroController.AddGeo))
            ))
            {
                cursor.GotoNext();
                cursor.EmitDelegate<Func<int, int>>((amount) => { SpawnedGeoCollected += amount; return amount; });
            }
        }

        private void CountGrass(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext
            (
                MoveType.After,
                i => i.MatchLdarg(1),
                i => i.MatchCall<GrassCut>("ShouldCut")
            ))
            {
                cursor.EmitDelegate<Func<bool, bool>>((shouldCut) => { if (shouldCut) GrassObjectsBroken += 1; return shouldCut; });
            }
        }
        private void CountInfectionBubble(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext
            (
                MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<InfectedBurstLarge>("audioSource"),
                i => i.MatchCallvirt<UnityEngine.AudioSource>("Play")
            ))
            {
                cursor.EmitDelegate<Action>(() => { BreakableObjectsBroken += 1; });
            }
        }
        private void CountBreakablePoleActivated(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext
            (
                i => i.MatchLdarg(0),
                i => i.MatchLdcI4(1),
                i => i.MatchStfld<BreakablePoleSimple>("activated")
            ))
            {
                cursor.EmitDelegate<Action>(() => { BreakableObjectsBroken += 1; });
            }
        }
        private void CountBreakableVineActivated(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext
            (
                i => i.MatchLdarg(0),
                i => i.MatchLdcI4(1),
                i => i.MatchStfld<BreakableInfectedVine>("activated")
            ))
            {
                cursor.EmitDelegate<Action>(() => { BreakableObjectsBroken += 1; });
            }
        }
        private void CountJellyEgg(On.JellyEgg.orig_Burst orig, JellyEgg self)
        {
            orig(self); BreakableObjectsBroken++;
        }

        private void CheckBreakableObject(GameObject go)
        {
            foreach (Collider2D col in go.GetComponentsInChildren<Collider2D>())
            {
                if (col.gameObject.layer == (int)GlobalEnums.PhysLayers.TERRAIN)
                {
                    return;
                }
            }

            BreakableObjectsBroken += 1;
        }

        private void CountBreakable(On.Breakable.orig_Break orig, Breakable self, float flingAngleMin, float flingAngleMax, float impactMultiplier)
        {
            if (!ReflectionHelper.GetField<Breakable, bool>(self, "isBroken"))
            {
                CheckBreakableObject(self.gameObject);
            }
            orig(self, flingAngleMin, flingAngleMax, impactMultiplier);
        }
        private void CountFsmBreakables(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.FsmName == "FSM")
            {
                if (self.Fsm.GetState("Spider Egg?") is FsmState spiderEgg)
                {
                    spiderEgg.InsertMethod(0, () => CheckBreakableObject(self.gameObject));
                }
            }
        }

        public override IEnumerable<DisplayInfo> GetDisplayInfos()
        {
            StringBuilder leftcol = new StringBuilder();
            StringBuilder rightcol = new StringBuilder();

            leftcol.AppendLine($"{BreakableObjectsBroken} objects broken");
            leftcol.AppendLine($"Cut {GrassObjectsBroken} grass");

            // Geo doesn't work with itemchanger
            // TODO - check for IC
            if (GeoSpawned != 0 && ModHooks.GetMod("ItemChangerMod") is null)
            {
                int percent = Mathf.RoundToInt(SpawnedGeoCollected * 100 / (float)GeoSpawned);
                rightcol.AppendLine($"Collected {SpawnedGeoCollected} of {GeoSpawned} spawned geo ({percent}%)");
            }

            yield return new()
            {
                Title = "Misc Stats",
                StatColumns = new()
                {
                    leftcol.ToString(),
                    rightcol.ToString(),
                }
            };
        }
    }
}
