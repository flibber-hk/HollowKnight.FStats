using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace FStats.GlobalStats
{
    internal class GlobalStatData
    {
        public Dictionary<string, JToken> Data { get; set; } = new();
    }
}
