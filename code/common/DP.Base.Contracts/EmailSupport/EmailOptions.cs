﻿namespace DP.Base.Contracts.EmailSupport
{
    public class EmailOptions
    {
        public string DefaultFromEmail { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
