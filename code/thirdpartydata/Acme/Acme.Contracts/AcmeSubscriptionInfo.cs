namespace Acme.Contracts
{
    public class AcmeSubscriptionInfo
    {
        public AcmeApiInfo ApiInfo { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
    }
}
