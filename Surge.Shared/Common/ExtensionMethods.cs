// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Surge.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Windows.ApplicationModel;

namespace Surge.Shared.Common
{
    public static class ExtensionMethods
    {
        private static HashSet<string> s_imageTypes = new HashSet<string> { ".png", ".jpg", ".gif", ".tiff", ".bmp", ".tif", ".psd", ".jpeg" };
        private static HashSet<string> s_videoTypes = new HashSet<string> { ".avi", ".divx", ".flv", ".wmv", ".mpg", ".mpeg", ".mp4", ".mov" };

        public static string ToPercent(this long numerator, long denominator)
        {
            if (denominator == 0)
            {
                return "0%";
            }

            double value = numerator;
            value /= (double)denominator;
            value *= 100;
            return value.TrimDecimal().ToPercent();
        }

        public static string ToNoneIfEmpty(this string data)
        {
            if (data.Trim('"') == string.Empty)
            {
                return "None";
            }

            return data;
        }

        public static string ToPercent(this double percent)
        {
            return (100.0 * percent).ToString().ToPercent();
        }

        public static string ToPercent(this string percent)
        {
            return $"{percent}%";
        }

        public static string ToTimeString(this int time)
        {
            return ((long)time).ToTimeString();
        }

        public static string ToTimeString(this long time)
        {
            if (time < 0)
            {
                return "None";
            }

            var ts = TimeSpan.FromSeconds(time);

            string[] suffixes = { "days", "hours", "minutes", "seconds" };
            int[] data = { ts.Days, ts.Hours, ts.Minutes, ts.Seconds };
            var result = new List<string>();

            for (int i = 0; result.Count < 2 && i < suffixes.Length; ++i)
            {
                if (data[i] > 0)
                {
                    result.Add(string.Format("{0} {1}", data[i], suffixes[i]));
                }
            }

            return string.Join(" and ", result);
        }

        public static string ToSizeString(this long data, ServerUnits units)
        {
            return ((double)data).ToBytes(units);
        }

        public static string ToSizeString(this long data, ServerUnits units, int unitPos)
        {
            return ((double)data).ToBytes(units, unitPos);
        }

        public static string ToRatioString(this double data)
        {
            return data.TrimDecimal();
        }

        private static string TrimDecimal(this double data)
        {
            var dataString = data.ToString();

            // If it's three characters or fewer, it's already trimmed
            if (dataString.Length < 4)
            {
                return dataString;
            }

            var preDecimalLength = Math.Floor(data).ToString().Length;

            // If the second char is the decimal point, we want to return x.y
            if (preDecimalLength == 1)
            {
                return dataString.Substring(0, 3);
            }

            // We want to return everything before the decimal point
            return dataString.Substring(0, preDecimalLength);
        }

        private static string ToBytes(this double data, ServerUnits units)
        {
            var unitPos = data.GetUnit(units);
            return data.ToBytes(units, unitPos);
        }

        private static string ToBytes(this double data, ServerUnits units, int unitPos)
        {
            var localData = data;

            for (int count = 0; count < unitPos; ++count)
            {
                localData /= units.Bytes;
            }

            return String.Format("{0:0.0} {1}", localData.TrimDecimal(), units.Units[unitPos]);
        }
        private static int GetUnit(this long data, ServerUnits units)
        {
            return ((double)data).GetUnit(units);
        }

        private static int GetUnit(this double data, ServerUnits units)
        {
            int pos;
            var localData = data;

            for (pos = 0; pos < units.Units.Length && localData >= units.Bytes; ++pos)
            {
                localData /= units.Bytes;
            }

            return pos;
        }

        public static string ToFileDetailsString(this long numerator, long denominator, ServerUnits units)
        {
            var unitPos = denominator.GetUnit(units);
            return $"{numerator.ToSizeString(units, unitPos)}/{denominator.ToSizeString(units, unitPos)} ({numerator.ToPercent(denominator)})";
        }

        internal static string EscapeSlashes(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return Regex.Replace(value, @"\\", @"\\");
        }

        public static FileType GetFileType(this string ext)
        {
            var lowerCase = ext.ToLower();

            if (s_imageTypes.Contains(lowerCase))
            {
                return FileType.Image;
            }

            if (s_videoTypes.Contains(lowerCase))
            {
                return FileType.Video;
            }

            return FileType.File;
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action?.Invoke(item);
            }
        }
    }
}
