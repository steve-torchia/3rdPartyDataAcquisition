using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DP.Base.Utilities
{
    public class TaskUtilities
    {
        /// <summary>
        /// Generate a non-blocking random delay
        /// </summary>
        /// <param name="minDelayMilliseconds">inclusive lower bound in milliseconds</param>
        /// <param name="maxDelayMilliseconds">exclusive upper bound in milliseconds</param>
        /// <returns>elapsed milliseconds </returns>
        public static long RandomDelay(int minDelayMilliseconds, int maxDelayMilliseconds)
        {
            if (minDelayMilliseconds < 0)
            {
                throw new Exception("minDelay must be greater than zero");
            }

            if (maxDelayMilliseconds < minDelayMilliseconds)
            {
                throw new Exception("maxDelay must be greater than or equal to mindDelay");
            }

            var tmp = Task.Run(
                async () =>
                {
                    var rnd = new Random(Environment.TickCount);
                    Stopwatch sw = Stopwatch.StartNew();
                    await Task.Delay(rnd.Next(minDelayMilliseconds, maxDelayMilliseconds));
                    sw.Stop();
                    return sw.ElapsedMilliseconds;
                });

            return tmp.Result;
        }
    }
}
