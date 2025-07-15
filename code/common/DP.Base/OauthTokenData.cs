using System;

namespace DP.Base
{
    public class OauthTokenData
    {
        public const string Key = nameof(OauthTokenData);

        public string AccessToken { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }
}