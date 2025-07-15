using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DP.Base.DateTimeUtilities
{
    public static class DateTimeUtils
    {
        
#pragma warning disable SA1309 // Field names must not begin with underscore
        private static object _yearMonthDayOfWeekCountSync = new object();
        private static int[,,] _yearMonthDayOfWeekCount;
#pragma warning restore SA1309 // Field names must not begin with underscore

        //!? Literal year alert
        public static readonly int YearMonthDayOfWeekCountStartYear = DateTime.UtcNow.Year;

        // What is happening here???
        public static int[,,] YearMonthDayOfWeekCount
        {
            get
            {
                if (_yearMonthDayOfWeekCount != null)
                {
                    return _yearMonthDayOfWeekCount;
                }

                lock (_yearMonthDayOfWeekCountSync)
                {
                    if (_yearMonthDayOfWeekCount != null)
                    {
                        return _yearMonthDayOfWeekCount;
                    }

                    var tmpYearMonthDayOfWeekCount = new int[50, 12, 7];

                    DateTime startDate = new DateTime(YearMonthDayOfWeekCountStartYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    DateTime endDate = startDate.AddYears(50);
                    DateTime curDate = startDate;

                    while (curDate < endDate)
                    {
                        tmpYearMonthDayOfWeekCount[curDate.Year - YearMonthDayOfWeekCountStartYear, curDate.Month - 1, (int)curDate.DayOfWeek] = tmpYearMonthDayOfWeekCount[curDate.Year - YearMonthDayOfWeekCountStartYear, curDate.Month - 1, (int)curDate.DayOfWeek] + 1;
                        curDate = curDate.AddDays(1);
                    }

                    _yearMonthDayOfWeekCount = tmpYearMonthDayOfWeekCount;
                }

                return _yearMonthDayOfWeekCount;
            }
        }

        public static DateTime AdjustToClosestDayOfWeek(DateTime originalDateTime, DayOfWeek targetDayOfWeek)
        {
            if (originalDateTime.DayOfWeek == targetDayOfWeek)
            {
                return originalDateTime;
            }

            int tdow = (int)targetDayOfWeek;
            int originalDayOfWeek = (int)originalDateTime.DayOfWeek;

            int dayOfWeekOffset = tdow - originalDayOfWeek;
            if (dayOfWeekOffset > 3)
            {
                dayOfWeekOffset = tdow - (originalDayOfWeek + 7);
            }
            else if (dayOfWeekOffset < -3)
            {
                dayOfWeekOffset = (tdow + 7) - originalDayOfWeek;
            }

            return originalDateTime.AddDays(dayOfWeekOffset);
        }

        /// <summary>
        /// Helpful utility (for DST) to get the first Sunday of November for a given year
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static DateTime FirstSundayOfNovember(int year)
        {
            var firstDayOfNovember = new DateTime(year, 11, 1);
            int daysUntilFirstSunday = ((int)DayOfWeek.Sunday - (int)firstDayOfNovember.DayOfWeek + 7) % 7;
            return firstDayOfNovember.AddDays(daysUntilFirstSunday);
        }

        /// <summary>
        /// Helpful utility (for DST) to get the second Sunday of March for a given year
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static DateTime SecondSundayOfMarch(int year)
        {
            var firstDayOfMarch = new DateTime(year, 3, 1);
            int daysUntilFirstSunday = ((int)DayOfWeek.Sunday - (int)firstDayOfMarch.DayOfWeek + 7) % 7;
            var firstSundayOfMarch = firstDayOfMarch.AddDays(daysUntilFirstSunday);
            var secondSundayOfMarch = firstSundayOfMarch.AddDays(7);
            return secondSundayOfMarch;
        }

        /// <summary>
        /// Helpful utility (for DST) to get the last Sunday of March for a given year
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static DateTime LastSundayOfMarch(int year)
        {
            var firstDayOfApril = new DateTime(year, 4, 1);
            int daysUntilLastSunday = ((int)DayOfWeek.Sunday - (int)firstDayOfApril.DayOfWeek + 7) % 7;
            return firstDayOfApril.AddDays(daysUntilLastSunday - 7);
        }

        /// <summary>
        /// Helpful utility (for DST) to get the last Sunday of October for a given year
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static DateTime LastSundayOfOctober(int year)
        {
            var firstDayOfNovember = new DateTime(year, 11, 1);
            int daysUntilLastSunday = ((int)DayOfWeek.Sunday - (int)firstDayOfNovember.DayOfWeek + 7) % 7;
            return firstDayOfNovember.AddDays(daysUntilLastSunday - 7);
        }

        public static bool IsDaylightSavingsDate(DateTime dt, DSTRegion dstRegion)
        {
            if (dstRegion != DSTRegion.NorthAmerica) throw new NotImplementedException("DST logic not yet implemented outside of North America");

            if (dt.Month == 3)
            {
                // 2nd Sunday in March
                if (dt.DayOfWeek != DayOfWeek.Sunday) return false;
                return dt.Day > 7 && dt.Day <= 14;    // second week
            }
            else if (dt.Month == 11)
            {
                // 1st Sunday in November
                if (dt.DayOfWeek != DayOfWeek.Sunday) return false;
                return dt.Day <= 7;    // first week
            }

            return false;
        }

        /// <summary>
        /// The time when the clock is set forward one hour (Standard Time)
        /// </summary>
        public static DateTime GetDSTStart(int year, DSTRegion dstRegion)
        {
            if (dstRegion == DSTRegion.NorthAmerica)
                // In North America, Daylight Saving Time begins 2nd Sunday in March 02:00 
                return DateTimeUtils.SecondSundayOfMarch(year).AddHours(2);
            else
                // In Europe, Daylight Saving Time begins last Sunday in March 01:00
                return DateTimeUtils.LastSundayOfMarch(year).AddHours(1);
        }

        public static DateTime GetDSTEnd(int year, DSTRegion dstRegion)
        {
            if (dstRegion == DSTRegion.NorthAmerica)
                // In North America, Daylight Saving Time ends 1st Sunday in November 02:00
                return DateTimeUtils.FirstSundayOfNovember(year).AddHours(2);

            else
                // In Europe, Daylight Saving Time ends last Sunday in October 01:00
                return DateTimeUtils.LastSundayOfOctober(year).AddHours(1);
        }

        public static int GetDSTStartMonth(DSTRegion dstRegion)
        {
            if (dstRegion == DSTRegion.NorthAmerica)
                return 3;
            else if (dstRegion == DSTRegion.Europe)
                return 3;
            else 
                throw new Exception($"Unknown DSTRegion: {dstRegion}");
        }

        public static int GetDSTEndMonth(DSTRegion dstRegion)
        {
            if (dstRegion == DSTRegion.NorthAmerica)
                return 11;
            else if (dstRegion == DSTRegion.Europe)
                return 10;  // Europe
            else
                throw new Exception($"Unknown DSTRegion: {dstRegion}");
        }

        public static DateTime ConvertToDaylightSavingTime(DateTime dt, DSTRegion dstRegion)
        {
            // In North America, Daylight Saving Time is between 2nd Sunday in March 02:00 and 1st Sunday in November 02:00
            var dstStart = GetDSTStart(dt.Year, dstRegion);
            var dstEnd = GetDSTEnd(dt.Year, dstRegion);

            if (dt >= dstStart && dt <= dstEnd)
                return dt.AddHours(1);
            
            return dt; // no conversion required if outside of DST start/end
        }

        public static DateTime ConvertToStandardTime(DateTime dstTime, DSTRegion dstRegion, bool isSecondDuplicateHour = false)
        {
            var dstStart = GetDSTStart(dstTime.Year, dstRegion);
            var dstEnd = GetDSTEnd(dstTime.Year, dstRegion);

            // When DST starts, the clock is turned ahead by 1 hour, which means that first hour after dstStart does not exist
            if (dstTime >= dstStart && dstTime < dstStart.AddHours(1))
                throw new ArgumentOutOfRangeException("Illegal DST time");

            // Check if we are in the range of possible DST times.  If not, no change
            if (dstTime < dstStart || dstTime >= dstEnd)
                return dstTime;

            // When DST ends, the clock is turned back by 1 hour, which means the Standard Time hour is duplicated
            // For example, in North America, the clock shifts back at DST 02:00, which means that the 01:00 - 02:00 hour is repeated in DST time,
            // so the translation to Standard Time is ambiguous.
            // To disambiguate, we use the flag "isSecondDuplicateHour" where the caller can indicate if dstTime should be treated
            // as the "first time through" or the "second time through"

            var isDuplicateHour = (dstTime >= dstEnd.AddHours(-1) && dstTime < dstEnd); // this is the duplicate 01:00 - 02:00 on the day when DST ends
            if (!isDuplicateHour)
                return dstTime.AddHours(-1);

            if (isSecondDuplicateHour)
                return dstTime;

            return dstTime.AddHours(-1);
        }

        public static long ToDateNumber(DateTime dateTime, DateParts parts)
        {
            long value = (((parts & DateParts.Year) == DateParts.Year) ? (long)dateTime.Year * 1000000L : 0L)
                        + (((parts & DateParts.Month) == DateParts.Month) ? (long)dateTime.Month * 10000L : 0L)
                        + (((parts & DateParts.Day) == DateParts.Day) ? (long)dateTime.Day * 100L : 0L)
                        + (((parts & DateParts.Hour) == DateParts.Hour) ? (long)dateTime.Hour : 0L);

            return value;
        }

        // Shouldn't this just crash if it is a value with 0 for year, month, or day rather than trying to make something out of it?
        public static DateTime FromDateNumber(long value)
        {
            int year = (int)(value / 1000000L);
            if (year == 0)
            {
                year = 4; //using year 4 because it is a leap year so will be able to handle feb29
            }

            int month = (int)((value / 10000L) % 100L);
            if (month == 0)
            {
                month = 1;
            }

            int day = (int)((value / 100L) % 100L);
            if (day == 0)
            {
                day = 1;
            }

            return new DateTime(year,
                month,
                day,
                (int)(value % 100L),
                0,
                0,
                DateTimeKind.Unspecified);
        }

        public static List<TypicalWeekOfYearKey> GetTypicalWeekOfYearKey(DateTime dateTime)
        {
            var keyYear = dateTime.Year;

            DateTime adjustedStartOfYearDate = GetAdjustedStartOfYear(dateTime.Year);
            List<TypicalWeekOfYearKey> retList = new List<TypicalWeekOfYearKey>()
            {
                 new TypicalWeekOfYearKey(keyYear,
                                                        (int)((dateTime - adjustedStartOfYearDate).TotalDays / 7),
                                                        (int)dateTime.DayOfWeek,
                                                        dateTime.Hour),
            };

            //if in jan or december key might not be in same year or might be in 2 years
            if (dateTime.Month != 1 && dateTime.Month != 12)
            {
                return retList;
            }

            if (dateTime.Month == 12)
            {
                //can be used in next year, alway used in current year;
                var nextYearAdjustedStartOfYearDate = GetAdjustedStartOfYear(dateTime.Year + 1);
                if (nextYearAdjustedStartOfYearDate <= dateTime)
                {
                    retList.Add(new TypicalWeekOfYearKey(keyYear + 1,
                                                        (int)((dateTime - nextYearAdjustedStartOfYearDate).TotalDays / 7),
                                                        (int)dateTime.DayOfWeek,
                                                        dateTime.Hour));
                }

                return retList;
            }

            //if(dateTime.Month == 1)
            var focusDateDelta = (dateTime - adjustedStartOfYearDate).TotalDays;
            if (focusDateDelta < 0) //date not used in current year, only in previous year date is jan1 but adjusted start of year is jan2
            {
                adjustedStartOfYearDate = GetAdjustedStartOfYear(keyYear - 1);

                return new List<TypicalWeekOfYearKey>()
                {
                    new TypicalWeekOfYearKey(keyYear - 1,
                                                    (int)((dateTime - adjustedStartOfYearDate).TotalDays / 7),
                                                    (int)dateTime.DayOfWeek,
                                                    dateTime.Hour),
                };
            }

            var previousYearAdjustedStartOfYearDate = GetAdjustedStartOfYear(dateTime.Year - 1);
            if (previousYearAdjustedStartOfYearDate.AddDays(53 * 7) > dateTime) //also in previous year
            {
                retList.Add(new TypicalWeekOfYearKey(keyYear - 1,
                                                    (int)((dateTime - previousYearAdjustedStartOfYearDate).TotalDays / 7),
                                                    (int)dateTime.DayOfWeek,
                                                    dateTime.Hour));
            }

            return retList;
        }

        public static DateTime GetAdjustedStartOfYear(int year)
        {
            var jan1 = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

            var previousSunday = jan1.AddDays(-(int)jan1.DayOfWeek);
            var nextSunday = previousSunday.AddDays(7);
            DateTime adjustedStartOfYearDate;
            if (System.Math.Abs((jan1 - previousSunday).TotalDays) > System.Math.Abs((nextSunday - jan1).TotalDays))
            {
                adjustedStartOfYearDate = nextSunday;
            }
            else
            {
                adjustedStartOfYearDate = previousSunday;
            }

            return adjustedStartOfYearDate;
        }

        public static TimeZoneInfo GetTimeZoneFromName(string name)
        {
            if (NameToTimeZoneMapping.TryGetValue(name.ToUpper(), out var returnVal))
            {
                return returnVal;
            }
            else
            {
                return null;
            }
        }

        public static Dictionary<string, TimeZoneInfo> NameToTimeZoneMapping = new Dictionary<string, TimeZoneInfo>()
        {
            { "Atlantic Standard Time".ToUpper(), TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time") },
            { "US Eastern Standard Time".ToUpper(), TimeZoneInfo.FindSystemTimeZoneById("US Eastern Standard Time") },
            { "Central Standard Time".ToUpper(), TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time") },
            { "US Mountain Standard Time".ToUpper(), TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time") },
            { "Pacific Standard Time".ToUpper(), TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time") },
            { "Alaskan Standard Time".ToUpper(), TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time") },
            { "Hawaiian Standard Time".ToUpper(), TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time") },
            { "Samoa Standard Time".ToUpper(), TimeZoneInfo.FindSystemTimeZoneById("Samoa Standard Time") },
            { "West Pacific Standard Time".ToUpper(), TimeZoneInfo.FindSystemTimeZoneById("West Pacific Standard Time") },
            { "AST", TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time") },
            { "EST", TimeZoneInfo.FindSystemTimeZoneById("US Eastern Standard Time") },
            { "CST", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time") },
            { "MST", TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time") },
            { "PST", TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time") },
            { "AKST", TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time") },
            { "HST", TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time") },
            { "HAST", TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time") },
            { "SST", TimeZoneInfo.FindSystemTimeZoneById("Samoa Standard Time") },
            { "CHST", TimeZoneInfo.FindSystemTimeZoneById("West Pacific Standard Time") },
            { "ADT", TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time") },
            { "EDT", TimeZoneInfo.FindSystemTimeZoneById("US Eastern Standard Time") },
            { "CDT", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time") },
            { "MDT", TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time") },
            { "PDT", TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time") },
            { "AKDT", TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time") },
            { "HDT", TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time") },
            { "HADT", TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time") },
            { "SDT", TimeZoneInfo.FindSystemTimeZoneById("Samoa Standard Time") },
            { "CHDT", TimeZoneInfo.FindSystemTimeZoneById("West Pacific Standard Time") },
            { "AT", TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time") },
            { "ET", TimeZoneInfo.FindSystemTimeZoneById("US Eastern Standard Time") },
            { "CT", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time") },
            { "MT", TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time") },
            { "PT", TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time") },
            { "AKT", TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time") },
            { "HT", TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time") },
            { "HAT", TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time") },
            { "ST", TimeZoneInfo.FindSystemTimeZoneById("Samoa Standard Time") },
            { "CHT", TimeZoneInfo.FindSystemTimeZoneById("West Pacific Standard Time") },
            { "UTC", TimeZoneInfo.Utc }
        };

        public class PeakOffPeak
        {
            private int peakStartDay;
            private int peakEndDay;
            private int peakStartHourBegining;
            private int peakEndHourBegining;
            public TimeZoneInfo TimeZoneInfo { get; private set; }

            public PeakOffPeak(DayOfWeek peakStartDay, DayOfWeek peakEndDay, int peakStartHourBegining, int peakEndHourBegining)
            {
                if (peakStartHourBegining < 0 || peakEndHourBegining < 0 ||
                    peakStartHourBegining >= 24 || peakEndHourBegining >= 24)
                {
                    throw new ArgumentOutOfRangeException("hourBegining", "must use hour begining 0-23");
                }

                this.peakStartDay = (int)peakStartDay;
                this.peakEndDay = (int)peakEndDay;

                this.peakStartHourBegining = peakStartHourBegining;
                this.peakEndHourBegining = peakEndHourBegining;
            }

            public PeakOffPeak(DayOfWeek peakStartDay, DayOfWeek peakEndDay, int peakStartHourBegining, int peakEndHourBegining, TimeZoneInfo timeZoneInfo)
                : this(peakStartDay, peakEndDay, peakStartHourBegining, peakEndHourBegining)
            {
                this.TimeZoneInfo = timeZoneInfo;
            }

            public bool IsPeak(DateTime dateTime)
            {
                if (this.TimeZoneInfo != null)
                {
                    dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, this.TimeZoneInfo);
                }

                var dow = (int)dateTime.DayOfWeek;
                var hour = dateTime.Hour;

                if (this.peakEndDay > this.peakStartDay)
                {
                    if (dow < this.peakStartDay ||
                        dow > this.peakEndDay)
                    {
                        return false;
                    }
                }
                else
                {
                    if (dow < this.peakStartDay &&
                        dow > this.peakEndDay)
                    {
                        return false;
                    }
                }

                if (this.peakEndHourBegining > this.peakStartHourBegining)
                {
                    if (hour < this.peakStartHourBegining ||
                        hour > this.peakEndHourBegining)
                    {
                        return false;
                    }
                }
                else
                {
                    if (hour < this.peakStartHourBegining &&
                        hour > this.peakEndHourBegining)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        public static int HoursInYear(int year)
        {
            return DaysInYear(year) * 24;
        }

        public static int WeeksInYear(int year)
        {
            return DateTimeFormatInfo.InvariantInfo.Calendar.GetWeekOfYear(new DateTime(year, 12, 31), CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        public static bool DateRangeOverlap(DateTime? from1, DateTime? to1, DateTime? from2, DateTime? to2, bool ignoreTime = false)
        {
            from1 = from1 ?? DateTime.MinValue;
            to1 = to1 ?? DateTime.MaxValue;
            from2 = from2 ?? DateTime.MinValue;
            to2 = to2 ?? DateTime.MaxValue;
            return DateRangeOverlap(from1.Value, to1.Value, from2.Value, to2.Value, ignoreTime);
        }

        /// <summary>
        /// Returns true if date range defined by from1/to1 overlaps date range defined by from2/to2.
        /// A range which ends at the same time the next one begins would not be considered overlapping
        /// </summary>
        public static bool DateRangeOverlap(DateTime from1, DateTime to1, DateTime from2, DateTime to2, bool ignoreTime = false)
        {
            if (ignoreTime)
            {
                from1 = from1.Date;
                from2 = from2.Date;
                to1 = to1.Date;
                to2 = to2.Date;
            }

            return (from1 <= to2 && from2 <= to1);
        }

        public static bool Overlap(DateTime? from, DateTime? to, int? fromMonth, int? fromDay, int? toMonth, int? toDay)
        {
            from = from ?? DateTime.MinValue;
            to = to ?? DateTime.MaxValue;
            return Overlap(from.Value, to.Value, fromMonth, fromDay, toMonth, toDay);
        }

        // NOTE: weird looking function, but this is used by tariffs which specify seasons as from/to month/day 
        public static bool Overlap(DateTime from, DateTime to, int? fromMonth, int? fromDay, int? toMonth, int? toDay)
        {
            if (fromMonth == null || toMonth == null)
            {
                return true;    // if not specified, assume it's not restricted by season
            }

            fromDay = fromDay ?? 1;
            toDay = toDay ?? 1;     // should this be end of month?

            if (from.AddYears(1) < to)
            {
                return true;    // if more than a year span is specified, there will always be overlap
            }

            if (fromMonth < toMonth)
            {
                // this is the case where the season doesn't "wrap" the end of the year
                var seasonStart = new DateTime(from.Year, fromMonth.Value, fromDay.Value);
                var seasonEnd = new DateTime(to.Year, toMonth.Value, toDay.Value);

                return DateRangeOverlap(from, to, seasonStart, seasonEnd);
            }
            else
            {
                var seasonStart = new DateTime(to.Year, 1, 1);
                var seasonEnd = new DateTime(to.Year, toMonth.Value, toDay.Value);
                if (DateRangeOverlap(from, to, seasonStart, seasonEnd))
                {
                    return true;
                }

                seasonStart = new DateTime(from.Year, fromMonth.Value, fromDay.Value);
                seasonEnd = new DateTime(to.Year, 12, 31);
                return DateRangeOverlap(from, to, seasonStart, seasonEnd);
            }
        }

        public static Dictionary<int, StartEndIndexes> IndexByYear(IList<DateTime> dates)
        {
            // find start/end indexes of each year in the file
            var startEndIndexesByYear = new Dictionary<int, StartEndIndexes>();
            DateTime currentTime = DateTime.MinValue;
            for (int i = 0; i < dates.Count; i++)
            {
                var rowDate = (DateTime)dates[i];
                if (rowDate.Year != currentTime.Year)
                {
                    if (currentTime != DateTime.MinValue && rowDate != currentTime.AddHours(1))
                    {
                        throw new Exception("Dates must be sequential");
                    }

                    // starting a new year
                    startEndIndexesByYear.Add(rowDate.Year, new StartEndIndexes { StartIndex = i });

                    // end the old year
                    if (startEndIndexesByYear.TryGetValue(currentTime.Year, out StartEndIndexes sey))
                    {
                        sey.EndIndex = i - 1;
                    }
                }

                currentTime = rowDate;
            }

            startEndIndexesByYear[currentTime.Year].EndIndex = dates.Count - 1;

            CheckForCompleteYears(startEndIndexesByYear);
            return startEndIndexesByYear;
        }

        private static void CheckForCompleteYears(Dictionary<int, StartEndIndexes> startEndIndexesByYear)
        {
            // make sure we have complete years
            var badHourCounts = startEndIndexesByYear
                .Select(e => new { Year = e.Key, HourCount = e.Value.EndIndex - e.Value.StartIndex + 1 })
                .Where(e => DateTime.IsLeapYear(e.Year) ? e.HourCount != 8784 : e.HourCount != 8760);

            if (badHourCounts.Any())
            {
                var msg = string.Join(",", badHourCounts.Select(e => $"{e.Year}: {e.HourCount}"));
                throw new Exception($"File must contain full years: {msg}");
            }
        }

        public static int DaysInYear(int year)
        {
            return DateTime.IsLeapYear(year) ? 366 : 365;
        }

        public static DateTime RoundUp(this DateTime dt, TimeSpan d)
        {
            var modTicks = dt.Ticks % d.Ticks;
            var delta = modTicks != 0 ? d.Ticks - modTicks : 0;
            return new DateTime(dt.Ticks + delta, dt.Kind);
        }

        public static DateTime RoundDown(this DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }

        public static DateTime RoundToNearest(this DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            bool roundUp = delta > d.Ticks / 2;
            var offset = roundUp ? d.Ticks : 0;

            return new DateTime(dt.Ticks + offset - delta, dt.Kind);
        }

        public static bool CheckContiguous(List<DateTime> timestamps, TimeSpan expectedInterval, bool throwOnError = false)
        {
            for (int i = 0; i < timestamps.Count - 1; i++)
            {
                var interval = (timestamps[i + 1] - timestamps[i]);
                if (interval != expectedInterval)
                {
                    if (throwOnError)
                        throw new Exception($"Unexpected time between intervals at index {i}: expected {expectedInterval}, found {interval}");

                    return false;
                }
            }
            return true;
        }
    }

    public class StartEndIndexes
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public override string ToString()
        {
            return $"#= {this.EndIndex - this.StartIndex + 1} {this.StartIndex}-{this.EndIndex}";
        }
    }
}