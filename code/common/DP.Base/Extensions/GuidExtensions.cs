using System;

namespace DP.Base.Extensions
{
    public static class GuidExtensions
    {
        public static bool IsEmpty(this Guid g) => g == Guid.Empty;
        public static bool IsNullOrEmpty(this Guid? g) => !g.HasValue || g.Value == Guid.Empty;
        public static string ToShortString(this Guid g) => g.ToString().Substring(0, 8);
    }
}
