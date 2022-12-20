using System;
using System.Collections.Generic;
using System.Linq;

namespace FStats.Util
{
    public static class CollectionUtils
    {
        public static Dictionary<T, int> EmptyCounter<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(x => x, x => 0);
        }

    }
}
