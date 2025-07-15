using System;

namespace DP.Base.Contracts
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        public virtual DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }
    }
}
