using System;

namespace DP.Base.DateTimeUtilities
{
    public enum MonthName
    {
        /// <summary>
        /// Represent month NotSet.
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// Represents January.
        /// </summary>
        January = 1,

        /// <summary>
        /// Represents February.
        /// </summary>
        February = 2,

        /// <summary>
        /// Represents March.
        /// </summary>
        March = 3,

        /// <summary>
        /// Represents April.
        /// </summary>
        April = 4,

        /// <summary>
        /// Represents May.
        /// </summary>
        May = 5,

        /// <summary>
        /// Represents June.
        /// </summary>
        June = 6,

        /// <summary>
        /// Represents July.
        /// </summary>
        July = 7,

        /// <summary>
        /// Represents August.
        /// </summary>
        August = 8,

        /// <summary>
        /// Represents September.
        /// </summary>
        September = 9,

        /// <summary>
        /// Represents October.
        /// </summary>
        October = 10,

        /// <summary>
        /// Represents November.
        /// </summary>
        November = 11,

        /// <summary>
        /// Represents December.
        /// </summary>
        December = 12,
    }

    [Flags]
    public enum DateParts
    {
        /// <summary>
        /// Represents none.
        /// </summary>
        None = 0x000000,

        /// <summary>
        /// Represents an hour.
        /// </summary>
        Hour = 0x000001,

        /// <summary>
        /// Represents a day.
        /// </summary>
        Day = 0x000002,

        /// <summary>
        /// Represents a month.
        /// </summary>
        Month = 0x000004,

        /// <summary>
        /// Represents a year.
        /// </summary>
        Year = 0x000008,

        /// <summary>
        /// Represents a hour or day or month or year.
        /// </summary>
        All = Hour | Day | Month | Year,
    }
}
