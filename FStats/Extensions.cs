using System;
using System.Collections.Generic;

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

		/// <summary>
		/// Return an enumerable with elements moved to the end of the input until the first element 
		/// matching the selector is reached.
		/// If none of the elements match the selector, return a copy of the original.
		/// </summary>
		public static IEnumerable<T> CycleTo<T>(this IEnumerable<T> values, Func<T, bool> selector)
		{
			List<T> initialSegment = new();
			bool found = false;
			
			foreach (T current in values)
			{
				if (!found && selector(current))
				{
					found = true;
                }
				
				if (found)
				{
					yield return current;
                }
				else
				{
					initialSegment.Add(current);
				}
			}

			foreach (T current in initialSegment)
			{
				yield return current;
			}
		}
	}
}
