using System.Collections.Generic;
using System.Linq;

namespace FStats
{
	internal static class Extensions
	{
		public static string PlaytimeHHMMSS(this float t)
		{
			PlayTime pt = new() { RawTime = t };

			if (!pt.HasMinutes)
			{
				return string.Format("{0:0}s", (int)pt.Seconds);
			}
			if (!pt.HasHours)
			{
				return string.Format("{0:0}m {1:00}s", (int)pt.Minutes, (int)pt.Seconds);
			}
			return string.Format("{0:0}h {1:00}m {2:00}s", (int)pt.Hours, (int)pt.Minutes, (int)pt.Seconds);
		}

		public static string PlaytimeHHMM(this float t)
		{
			PlayTime pt = new() { RawTime = t };

			if (pt.HasHours)
			{
				return string.Format("{0:0}h {1:00}m", (int)pt.Hours, (int)pt.Minutes);
			}
			return string.Format("{0:0}m", (int)pt.Minutes);
		}

		public static IEnumerable<T> Slice<T>(this IEnumerable<T> enumerable, int start, int modulus)
        {
			int count = 0;
			foreach (T item in enumerable)
            {
				if (count >= start && (count - start) % modulus == 0)
                {
					yield return item;
				}

				count++;
            }
        }

		public static int IncrementValue<TKey>(this Dictionary<TKey, int> dict, TKey key)
        {
			if (!dict.ContainsKey(key))
            {
				dict[key] = 0;
            }
			dict[key]++;

			return dict[key];
        }
	}
}
