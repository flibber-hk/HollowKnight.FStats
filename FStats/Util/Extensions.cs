using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FStats.Util
{
    public static class Extensions
    {
        public static int CeilingDivide(this int n, int d)
        {
            return Mathf.CeilToInt((float)n / d);
        }

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

        public static float IncreaseValue<TKey>(this Dictionary<TKey, float> dict, TKey key, float increment)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = 0;
            }
            dict[key] += increment;

            return dict[key];
        }

        public static int IncreaseValue<TKey>(this Dictionary<TKey, int> dict, TKey key, int increment)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = 0;
            }
            dict[key] += increment;

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

        /// <summary>
        /// Given an IEnumerable of objects, groups them in blocks of length `count` and yields them.
        /// (The last block may have length less than `count`).
        /// </summary>
        public static IEnumerable<List<T>> GroupSegments<T>(this IEnumerable<T> ts, int count)
        {
            List<T> accumulator = new();

            foreach (T t in ts)
            {
                accumulator.Add(t);
                if (accumulator.Count == count)
                {
                    yield return new(accumulator);
                    accumulator.Clear();
                }
            }

            yield return new(accumulator);
        }

        public static void Add<T>(this Dictionary<T, int> self, Dictionary<T, int> other)
        {
            foreach ((T key, int value) in other)
            {
                self.IncreaseValue(key, value);
            }
        }

        public static void Subtract<T>(this Dictionary<T, int> self, Dictionary<T, int> other)
        {
            foreach ((T key, int value) in other)
            {
                self.IncreaseValue(key, -value);
            }
        }

        public static void Add<T>(this Dictionary<T, float> self, Dictionary<T, float> other)
        {
            foreach ((T key, float value) in other)
            {
                self.IncreaseValue(key, value);
            }
        }

        public static void Subtract<T>(this Dictionary<T, float> self, Dictionary<T, float> other)
        {
            foreach ((T key, float value) in other)
            {
                self.IncreaseValue(key, -value);
            }
        }

    }
}
