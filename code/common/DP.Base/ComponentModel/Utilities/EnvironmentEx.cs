namespace DP.Base.Utilities
{
    public static class EnvironmentEx
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern ulong GetTickCount64();

        public static uint GetTickDelta(int startTick, int endTick)
        {
            uint uStartTick = (uint)startTick;
            uint uEndTick = (uint)endTick;

            return GetTickDelta(uStartTick, uEndTick);
        }

        public static uint GetTickDelta(uint uStartTick, uint uEndTick)
        {
            //check for overflow
            if (uEndTick < uStartTick)
            {
                return uEndTick + (uint.MaxValue - uStartTick);
            }

            return uEndTick - uStartTick;
        }

        public static ulong GetTickDelta(long startTick, long endTick)
        {
            ulong uStartTick = (ulong)startTick;
            ulong uEndTick = (ulong)endTick;

            return GetTickDelta(uStartTick, uEndTick);
        }

        public static ulong GetTickDelta(ulong uStartTick, ulong uEndTick)
        {
            //check for overflow
            if (uEndTick < uStartTick)
            {
                return uEndTick + (ulong.MaxValue - uStartTick);
            }

            return uEndTick - uStartTick;
        }

        public static ulong TickCount64
        {
            get
            {
                return GetTickCount64();
            }
        }
    }
}
