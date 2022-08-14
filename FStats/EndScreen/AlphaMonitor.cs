using TMPro;
using UnityEngine;

namespace FStats.EndScreen
{
    public class AlphaMonitor : MonoBehaviour
    {
        private TextMeshPro tmpro_self;
        public TextMeshPro tmpro_other;

        public void Start()
        {
            tmpro_self = gameObject.GetComponent<TextMeshPro>();
        }

        public void Update()
        {
            Color col = tmpro_self.color;
            col.a = tmpro_other.color.a;
            tmpro_self.color = col;
        }
    }
}
