using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace FStats.EndScreen
{
    public class StatScreenCycler : MonoBehaviour
    {
        // The gap between adjacent columns
        public const float hOffset = 8.5f;
        public const float vOffset = 0.9f;

        public List<DisplayInfo> displayInfos;
        private int index;

        private TextMeshPro title;
        private TextMeshPro mainstat;
        public List<TextMeshPro> columns;

        void Start()
        {
            title = GetComponent<TextMeshPro>();
            mainstat = transform.parent.Find("time_num").GetComponent<TextMeshPro>();

            index = 0;
            SetText(index);
        }

        void Update()
        {
            if (InputHandler.Instance.inputActions.left.WasPressed)
            {
                index = (index + displayInfos.Count - 1) % displayInfos.Count;
                SetText(index);
            }
            if (InputHandler.Instance.inputActions.right.WasPressed)
            {
                index = (index + 1) % displayInfos.Count;
                SetText(index);
            }
        }

        private void SetText(int index)
        {
            DisplayInfo info = displayInfos[index];

            title.text = info.Title;
            mainstat.text = info.MainStat;

            int i;
            for (i = 0; i < info.StatColumns.Count; i++)
            {
                columns[i].text = info.StatColumns[i];
            }
            for (; i < columns.Count; i++)
            {
                columns[i].text = string.Empty;
            }

            float yPos = transform.position.y - vOffset;

            if (!string.IsNullOrEmpty(info.MainStat))
            {
                yPos = transform.position.y - 2 * vOffset;
            }

            float xPos = transform.position.x - (hOffset / 2) * (info.StatColumns.Count - 1);
            for (i = 0; i < info.StatColumns.Count; i++)
            {
                columns[i].transform.position = new Vector3(xPos, yPos, transform.position.z);
                xPos += hOffset;
            }
        }
    }
}
