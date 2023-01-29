using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace FStats.EndScreen
{
    /// <summary>
    /// Class to manage the objects on the end screen.
    /// </summary>
    public class EndScreenObjectHolder
    {
        // The gap between adjacent columns
        public static float HOffset { get; } = 8.5f;
        public static float VOffset { get; } = 0.9f;

        public static int ColumnCount { get; } = 4;

        public TextMeshPro Title { get; init; }
        public TextMeshPro MainStat { get; init; }
        public List<TextMeshPro> Columns { get; init; }

        // Private constructor because the static method is how to make one
        private EndScreenObjectHolder() { }

        /// <summary>
        /// Given the unmodified game completion screen, move the objects around and
        /// return an instance of this class with the references set.
        /// </summary>
        public static EndScreenObjectHolder Setup(GameCompletionScreen gcs)
        {
            #region Percentage
            string ShownPercentage = PlayerData.instance.completionPercentage.ToString() + "%";
            string VanillaPercentage = PlayerData.instance.GetVanillaCompletion().ToString() + "%";

            if (ShownPercentage != VanillaPercentage)
            {
                // Show vanilla percentage amount if it differs from the shown amount
                gcs.percentageNumber.text = $"{ShownPercentage}           (Vanilla {VanillaPercentage})";
            }
            #endregion

            float dist = 3.5f;
            MoveUp(gcs.transform.Find("credits fleur (1)"), dist);
            MoveUp(gcs.transform.Find("game completion title"), dist);
            MoveUp(gcs.transform.Find("Percent_title"), dist);
            MoveUp(gcs.transform.Find("percentage_num"), dist);
            MoveUp(gcs.transform.Find("Time_title"), dist);
            MoveUp(gcs.transform.Find("time_num"), dist);

            #region Set up text columns
            GameObject timeNum = gcs.playTimeNumber.gameObject;

            List<TextMeshPro> columns = new();
            for (int i = 0; i < ColumnCount; i++)
            {
                GameObject column = UObject.Instantiate(timeNum);
                column.transform.SetParent(timeNum.transform.parent);
                column.AddComponent<AlphaMonitor>().tmpro_other = timeNum.GetComponent<TextMeshPro>();
                column.SetActive(true);
                columns.Add(column.GetComponent<TextMeshPro>());
            }
            #endregion

            GameObject continueText = gcs.transform.Find("any button to continue").gameObject;
            UObject.Destroy(continueText.GetComponent<SetTextMeshProGameText>());
            continueText.GetComponent<TextMeshPro>().text = "Press directions for more stats, or jump/attack to continue";

            EndScreenObjectHolder holder = new()
            {
                Title = gcs.transform.Find("Time_title").GetComponent<TextMeshPro>(),
                MainStat = gcs.transform.Find("time_num").GetComponent<TextMeshPro>(),
                Columns = columns,
            };

            return holder;
        }

        /// <summary>
        /// Given a DisplayInfo object, show it on the screen.
        /// </summary>
        public void Display(DisplayInfo info)
        {
            Title.text = info.Title;
            MainStat.text = info.MainStat;

            int i;
            for (i = 0; i < info.StatColumns.Count; i++)
            {
                Columns[i].text = info.StatColumns[i];
            }
            for (; i < Columns.Count; i++)
            {
                Columns[i].text = string.Empty;
            }

            float yPos = Title.transform.position.y - VOffset;

            if (!string.IsNullOrEmpty(info.MainStat))
            {
                yPos = Title.transform.position.y - 2 * VOffset;
            }

            float xPos = Title.transform.position.x - (HOffset / 2) * (info.StatColumns.Count - 1);
            for (i = 0; i < info.StatColumns.Count; i++)
            {
                Columns[i].transform.position = new Vector3(xPos, yPos, Title.transform.position.z);
                xPos += HOffset;
            }
        }

        private static void MoveUp(Transform t, float dist)
        {
            Vector3 pos = t.position;
            pos.y += dist;
            t.position = pos;
        }
    }
}
