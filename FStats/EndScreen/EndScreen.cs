using System.Collections.Generic;
using System.Linq;
using FStats.Util;
using TMPro;
using UnityEngine;

namespace FStats.EndScreen
{
    public static class EndScreen
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

            if (infos.Count == 0) return;

            #region Percentage
            string ShownPercentage = PlayerData.instance.completionPercentage.ToString() + "%";
            string VanillaPercentage = PlayerData.instance.GetVanillaCompletion().ToString() + "%";

            if (ShownPercentage != VanillaPercentage)
            {
                // Show vanilla percentage amount if it differs from the shown amount
                self.percentageNumber.text = $"{ShownPercentage}           (Vanilla {VanillaPercentage})";
            }
            #endregion

            float dist = 3.5f;
            MoveUp(self.transform.Find("credits fleur (1)"), dist);
            MoveUp(self.transform.Find("game completion title"), dist);
            MoveUp(self.transform.Find("Percent_title"), dist);
            MoveUp(self.transform.Find("percentage_num"), dist);
            MoveUp(self.transform.Find("Time_title"), dist);
            MoveUp(self.transform.Find("time_num"), dist);

            #region Set up text columns
            int requiredColumnCount = infos.Max(info => info.StatColumns.Count);

            GameObject timeNum = self.playTimeNumber.gameObject;

            List<TextMeshPro> columns = new();
            for (int i = 0; i < requiredColumnCount; i++)
            {
                GameObject column = Object.Instantiate(timeNum);
                column.transform.SetParent(timeNum.transform.parent);
                column.AddComponent<AlphaMonitor>().tmpro_other = timeNum.GetComponent<TextMeshPro>();
                column.SetActive(true);
                columns.Add(column.GetComponent<TextMeshPro>());
            }

            GameObject timeTitle = self.transform.Find("Time_title").gameObject;
            StatScreenCycler cyc = timeTitle.AddComponent<StatScreenCycler>();
            cyc.columns = columns;
            cyc.displayInfos = infos;
            #endregion

            GameObject continueText = self.transform.Find("any button to continue").gameObject;
            Object.Destroy(continueText.GetComponent<SetTextMeshProGameText>());
            continueText.GetComponent<TextMeshPro>().text = "Press left/right for more stats, or jump/attack to continue";
        }

        private static void MoveUp(Transform t, float dist)
        {
            Vector3 pos = t.position;
            pos.y += dist;
            t.position = pos;
        }
    }
}
