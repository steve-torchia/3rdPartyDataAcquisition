using System;
using System.ComponentModel;

namespace DP.Base.Extensions
{
    public static class CoersionExtensions
    {
        public static bool ToBooleanSafe(this object o, bool defaultValue = default(bool)) => o == null ? defaultValue : ToBooleanSafe(o.ToString(), defaultValue);
        public static bool ToBooleanSafe(this bool? b, bool defaultValue = default(bool)) => b == null ? defaultValue : ToBooleanSafe(b.ToString(), defaultValue);
        public static bool ToBooleanSafe(this string s, bool defaultValue = default(bool))
        {
            bool value;
            return bool.TryParse(s, out value) ? value : defaultValue;
        }

        public static T ToEnumSafe<T>(this string s, T defaultValue = default(T), bool ignoreCase = true) where T : struct => s.ToEnumSafeNullable<T>(ignoreCase) ?? defaultValue;

        public static T? ToEnumSafeNullable<T>(this string s, bool ignoreCase = true)
            where T : struct
        {
            T value;
            if (Enum.TryParse(s, ignoreCase, out value))
            {
                return value;
            }

            if (s.IsNullOrWhiteSpace())
            {
                return null;
            }

            value = (T)Enum.ToObject(typeof(T), s[0]);
            return Enum.IsDefined(typeof(T), value) ? value : (T?)null;
        }

        public static int ToInt32Safe(this object o, int defaultValue = default(int)) => o == null ? defaultValue : ToInt32Safe(o.ToString(), defaultValue);
        public static int ToInt32Safe(this int? i, int defaultValue = default(int)) => i == null ? defaultValue : ToInt32Safe(i.ToString(), defaultValue);
        public static int ToInt32Safe(this string s, int defaultValue = default(int))
        {
            int value;
            return int.TryParse(s, out value) ? value : defaultValue;
        }

        public static T? ToNullable<T>(this string s)
            where T : struct
        {
            if (typeof(T) == typeof(int))
            {
                int value;
                return (int.TryParse(s, out value) ? value : (int?)null) as T?;
            }

            if (typeof(T) == typeof(bool))
            {
                bool value;
                return (bool.TryParse(s, out value) ? value : (bool?)null) as T?;
            }

            if (s.IsNullOrWhiteSpace())
            {
                return null;
            }

            try
            {
                return (T?)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(s);
            }
            catch
            {
                return null;
            }
        }
    }
}
