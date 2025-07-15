namespace DP.Base.Contracts.EmailSupport
{
    public class EmailHelpers
    {
        public const string EmailConfigSectionName = "EmailConfig";
        public const string DefaultFromEmailConfigKey = "DefaultFromEmail";
        public const string SMTPSettingConfigKey = "SMTPSetting";
        public const string HostConfigKey = $"{SMTPSettingConfigKey}:Host";
        public const string SMTPSettingPortConfigKey = $"{SMTPSettingConfigKey}:Port";
        public const string SMTPSettingUserNameConfigKey = $"{SMTPSettingConfigKey}:Username";
        public const string SMTPSettingPasswordConfigKey = $"{SMTPSettingConfigKey}:Password";
    }
}
