using System.Collections.Generic;
using System.Linq;
using FStats.Util;
using UnityEngine;

namespace FStats.EndScreen
{
    public static class EndScreenManager
    {
        internal static bool ShouldDisplay => FStatsMod.LS.InitializedOnNewGame;

        public static void Hook()
        {
            On.GameCompletionScreen.Start += GameCompletionScreen_Start;
            // Allow the player to use left and right to cycle stat screens
            On.InputHandler.CutsceneInput += PreventCutsceneSkip;
            SkipToEndScreen.Hook();
        }


        private static void PreventCutsceneSkip(On.InputHandler.orig_CutsceneInput orig, InputHandler self)
        {
            if (!ShouldDisplay) { orig(self); return; }

            if (!(self.inputActions.jump.IsPressed || self.inputActions.attack.IsPressed) 
                && GameManager.instance.sceneName == "End_Game_Completion"
                && (Input.anyKeyDown || self.gameController.AnyButton.WasPressed)) return;

            orig(self);
        }

        private static void GameCompletionScreen_Start(On.GameCompletionScreen.orig_Start orig, GameCompletionScreen self)
        {
            orig(self);
            if (!ShouldDisplay) return;

            List<DisplayInfo> infos = new();
            foreach (StatController c in FStatsMod.LS.Data)
            {
                if (FStatsMod.GS.ShouldDisplay(c))
                {
                    foreach (DisplayInfo info in c.GetDisplayInfos())
                    {
                        infos.Add(info);
                    }
                }
            }
            foreach (DisplayInfo info in API.BuildAdditionalPages())
            {
                infos.Add(info);
            }

            foreach (DisplayInfo info in infos)
            {
                info.StatColumns = info.StatColumns?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new();
            }

            infos = infos
                .Where(x => !string.IsNullOrEmpty(x.Title))
                .OrderBy(x => x.Priority)
                .CycleTo(x => x.Priority >= BuiltinScreenPriorityValues.TimeByAreaStat)
                .ToList();

            // Filter out screens that have been excluded through the API event.
            API.FilterScreens(infos);

            if (infos.Count == 0) return;

            List<DisplayInfo> globalInfos = FStatsMod.GlobalStats?.GenerateDisplays() ?? new();

            // Set up the screen
            EndScreenObjectHolder holder = EndScreenObjectHolder.Setup(self);
            NavigationManager nav = new(infos, globalInfos, out DisplayInfo initial);

            StatScreenCycler cyc = self.playTimeNumber.gameObject.AddComponent<StatScreenCycler>();
            cyc.ObjectHolder = holder;
            cyc.NavigationManager = nav;
            cyc.OnStart += () => holder.Display(initial);
        }
    }
}
