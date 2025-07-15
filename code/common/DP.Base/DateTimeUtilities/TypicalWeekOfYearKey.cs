namespace DP.Base.DateTimeUtilities
{
    public class TypicalWeekOfYearKey
    {
        public TypicalWeekOfYearKey()
        {
        }

        public TypicalWeekOfYearKey(long key)
        {
            this.Year = (int)(key / 1000000L);
            this.WeekOfYear = (int)((key % 1000000L) / 10000L);
            this.DayOfWeek = (int)((key % 10000L) / 100L);
            this.Hour = (int)(key % 100L);
        }

        public TypicalWeekOfYearKey(int year, int weekOfYear, int dayOfWeek, int hour)
        {
            this.Year = year;
            this.WeekOfYear = weekOfYear;
            this.DayOfWeek = dayOfWeek;
            this.Hour = hour;
        }

        public int DayOfWeek { get; set; }

        public int Hour { get; set; }

        public int Year { get; set; }

        public int WeekOfYear { get; set; }

        public long Key
        {
            get
            {
                return (long)this.Year * 1000000L +
                                           (long)this.WeekOfYear * 10000L +
                                           (long)this.DayOfWeek * 100L +
                                           (long)this.Hour;
            }
        }

        public override string ToString()
        {
            return string.Format("{0:0000} {1:00} {2:00} {3:00}", this.Year, this.WeekOfYear, this.DayOfWeek, this.Hour);
        }

        public override int GetHashCode()
        {
            return (int)this.Key;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TypicalWeekOfYearKey;
            if (other == null)
            {
                return false;
            }

            return this.Year == other.Year &&
                    this.WeekOfYear == other.WeekOfYear &&
                    this.DayOfWeek == other.DayOfWeek &&
                    this.Hour == other.Hour;
        }
    }
}
