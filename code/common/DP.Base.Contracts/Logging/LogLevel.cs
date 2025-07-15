namespace DP.Base.Contracts.Logging
{
    public enum LogLevel
    {
        /// <summary>
        /// Logging turned off.
        /// </summary>
        Off = -1,

        /// <summary>
        /// Log everything - Verbose, Debug, Info, Warn, Error and Fatal.
        /// </summary>
        Verbose = 0,

        /// <summary>
        /// Log Debug, Info, Warn, Error and Fatal.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Log Info, Warn, Error and Fatal.
        /// </summary>
        Info = 2,

        /// <summary>
        /// Log Warning, Error and Fatal exceptions.
        /// </summary>
        Warn = 3,

        /// <summary>
        /// Log Error and Fatal exceptions.
        /// </summary>
        Error = 4,

        /// <summary>
        /// Log only Fatal exceptions.
        /// </summary>
        Fatal = 5,
    }
}
