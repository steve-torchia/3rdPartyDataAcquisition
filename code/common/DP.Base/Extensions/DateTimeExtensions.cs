using System;
using System.Collections.Generic;
using System.Linq;
using DP.Base.DateTimeUtilities;

namespace DP.Base.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToSortableDateTimepattern(this DateTime dt) => string.Format(DateTimeConstants.DateTimeFormat.SortableDateTime, dt);

        public static string ToSortableDateTimepattern(this string dt) => DateTime.Parse(dt).ToSortableDateTimepattern();

        public static string ToCompressedSortableDateTimePattern(this DateTime dt) => dt.ToString(DateTimeConstants.DateTimePattern.CompressedSortableDateTime);

        public static string ToCompressedSortableDateTimePattern(this string dt) => DateTime.Parse(dt).ToCompressedSortableDateTimePattern();

        public static string ToLongFormat(this DateTime dt) => string.Format(DateTimeConstants.DateTimeFormat.LongFormatDateTime, dt);
        public static bool ContainsAllHoursForYear(this IList<DateTime> dateTimeList, int year) => dateTimeList.Where(q => q.Year == year).Count() == ((new DateTime(year, 12, 31) - new DateTime(year, 1, 1)).Days + 1) * 24;

        public static bool IsStartOfMonth(this DateTime dt)
        {
            return dt.Day == 1 && dt.TimeOfDay == TimeSpan.Zero;
        }

        public static bool IsLastIntervalOfMonth(this DateTime dt, int intervalMinutes)
        {
            if (!IsLastIntervalOfDay(dt, intervalMinutes))
            {
                return false;
            }
            
            return IsLastDayOfMonth(dt);
        }

        public static bool IsLastIntervalOfQuarter(this DateTime dt, int intervalMinutes)
        {
            if (!IsLastIntervalOfDay(dt, intervalMinutes))
            {
                return false;
            }

            return IsLastDayOfQuarter(dt);
        }

        public static bool IsLastIntervalOfYear(this DateTime dt, int intervalMinutes)
        {
            if (!IsLastIntervalOfDay(dt, intervalMinutes))
            {
                return false;
            }

            return dt.Month == 12 && dt.Day == 31;
        }

        private static bool IsLastIntervalOfDay(DateTime dt, int intervalMinutes)
        {
            var midnight = dt.Date + TimeSpan.FromDays(1);
            var minutesUntilMidnight = (int)(midnight - dt).TotalMinutes;
            return (minutesUntilMidnight == intervalMinutes);
        }

        public static bool IsLastHourOfQuarter(this DateTime dt)
        {
            if (dt.Hour != 23)
            {
                return false;
            }

            return IsLastDayOfQuarter(dt); 
        }

        private static bool IsLastDayOfQuarter(DateTime dt)
        {
            if (dt.Month == 3)
            {
                return dt.Day == 31;
            }
            else if (dt.Month == 6)
            {
                return dt.Day == 30;
            }
            else if (dt.Month == 9)
            {
                return dt.Day == 30;
            }
            else if (dt.Month == 12)
            {
                return dt.Day == 31;
            }

            return false;
        }

        public static bool IsLastHourOfMonth(this DateTime dt)
        {
            if (dt.Hour != 23)
            {
                return false;
            }

            return IsLastDayOfMonth(dt);
        }

        private static bool IsLastDayOfMonth(DateTime dt)
        {
            return dt.Day == DateTime.DaysInMonth(dt.Year, dt.Month);
        }

        public static bool IsLastHourOfYear(this DateTime dt)
        {
            return (dt.Month == 12 && IsLastHourOfMonth(dt));
        }

        public static DateTime AddYears(this DateTime dateTime, float years)
        {
            int roundedYears = (int)System.Math.Floor(years);
            var roundedDate = dateTime.AddYears(roundedYears);
            var lastYearSpan = roundedDate.AddYears(1) - roundedDate;
            return roundedDate.AddDays((years % 1) * lastYearSpan.TotalDays);
        }

        public static bool IsWeekDay(this DateTime dateTime)
        {
            if (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            return true;
        }

        public enum OnOffPeak
        {
            All,
            On,
            Off
        }

        public static bool IsOnPeakHour(this DateTime dateTime)
        {
            if (dateTime.Hour >= 8 && dateTime.Hour <= 20)
            {
                return true;
            }

            return false;
        }

        public static bool IsOnPeakLocalDateTime(this DateTime utcTime, TimeZoneInfo timeZoneInfo)
        {
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZoneInfo);
            if (localTime.IsWeekDay() && localTime.IsOnPeakHour())
            {
                return true;
            }

            return false;
        }

        public static IEnumerable<DateTime> GetHourlyTimeRange(DateTime startTime,
            DateTime endTime,
            bool isExclusiveEndTime = true)
        {
            int endTimeExclusiveAdjustment = isExclusiveEndTime ? 0 : 1;

            // RS add 0 or one if it's exclusive or not
            TimeSpan ts = endTime.AddHours(endTimeExclusiveAdjustment) - startTime;

            IEnumerable<DateTime> hoursBetween = Enumerable.Range(0, (int)ts.TotalHours).Select(
                i => startTime.AddHours(i));
            return hoursBetween;
        }

        public static IEnumerable<DateTime> GetDailyTimeRange(DateTime startTime,
            DateTime endTime,
            bool isExclusiveEndTime = true)
        {
            var totalDays = (int)(endTime - startTime).TotalDays;

            IEnumerable<DateTime> daysBetween = Enumerable.Range(0, isExclusiveEndTime ? totalDays : totalDays + 1)
                                                            .Select(i => startTime.AddDays(i));

            return daysBetween;
        }

        public static IEnumerable<DateTime> GetMonthlyTimeRange(DateTime startTime,
           DateTime endTime,
           bool isExclusiveEndTime = true)
        {
            // RS add 0 or one if it's exclusive or not
            var totalMonths = ((endTime.Year - startTime.Year) * 12) + endTime.Month - startTime.Month;

            IEnumerable<DateTime> monthsBetween = Enumerable.Range(0, isExclusiveEndTime ? totalMonths : totalMonths + 1).Select(
                i => startTime.AddMonths(i));
            return monthsBetween;
        }

        public static IEnumerable<DateTime> GetAnnualTimeRange(DateTime startTime,
           DateTime endTime,
           bool isExclusiveEndTime = true)
        {
            int endTimeExclusiveAdjustment = isExclusiveEndTime ? 0 : 1;

            // RS add 0 or one if it's exclusive or not
            var totalYears = endTime.AddYears(endTimeExclusiveAdjustment).Year - startTime.Year;

            IEnumerable<DateTime> yearsBetween = Enumerable.Range(0, totalYears).Select(
                i => startTime.AddYears(i));
            return yearsBetween;
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Sunday)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Heuristic to guess interval minutes from a series of timestamps (e.g. return 60 for hourly data, 15 for 15-min data, etc...)
        /// </summary>
        public static int GetIntervalMinutes(IEnumerable<DateTime> timeList)
        {
            var ok = (timeList != null) ? true : throw new ArgumentNullException("timeList");

            var countFromMinuteDiff = new Dictionary<int, int>();
            var currentTimeUtc = timeList.First();
            foreach (var dt in timeList.Skip(1).Take(10))
            {
                var diff = (int)(dt - currentTimeUtc).TotalMinutes;
                if (!countFromMinuteDiff.ContainsKey(diff))
                {
                    countFromMinuteDiff.Add(diff, 1);
                }
                else
                {
                    countFromMinuteDiff[diff]++;
                }

                currentTimeUtc = dt;
            }

            // some of the data comes in "dirty" so don't throw, just take most likely interval
            //ok = (ret.Count == 1) ? true : throw new Exception($"Failed to determine interval minutes {ret.Count}: different values");
            var mostCommonInterval = countFromMinuteDiff.OrderByDescending(kvp => kvp.Value).First().Key;
            return mostCommonInterval;
        }
    }
}
