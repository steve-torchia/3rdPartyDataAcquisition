namespace DP.Base.DateTimeUtilities
{
    public static class DateTimeConstants
    {
        public static readonly System.DateTime Epoch = new System.DateTime(1970, 1, 1);

        public static class PortfolioBaseYearConstants
        {
            // RS VM SSA TODO todo update or keep track of this better
            // Jack Murphy says this should be manually changed, not automatically, so to keep it hardcoded
            public static System.DateTime PortfolioBaseDateTime = new System.DateTime(2017, 1, 1);
            // Jack Murphy says we should eventually remove this and replace everywhere it is used with ForecastEndYear from MAGlobalConfig - deferred to 2.3
            public static System.DateTime PortfolioBaseFinalDateTime = new System.DateTime(2041, 12, 31, 23, 0, 0);
        }

        public static class DateTimePattern
        {
            public const string CompressedSortableDateTime = "yyyyMMddHHmmss";
            public const string SortableDateTime = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";
            public const string SortableYear = "yyyy";
            public const string MonthDayYear = "MMddyyyy";
        }

        public static class DateTimeFormat
        {
            public const string SortableDateTime = "{0:s}";
            public const string LongFormatDateTime = "{0:yyyy-MM-dd HH:mm:ss.fff}";
        }
    }
}
