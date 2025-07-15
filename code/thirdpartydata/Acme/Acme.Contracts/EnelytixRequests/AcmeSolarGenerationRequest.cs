namespace Acme.Contracts
{
    public class AcmeSolarGenerationRequest : AcmeRequestBase
    {
        public string ArrayType { get; set; }
        public string Tilt { get; set; }
        public string ModuleType { get; set; }
        public string Azimuth { get; set; }
        public string DcAcRatio { get; set; }
        public string IsBifacial { get; set; }
        public string GroundCoverageRatio { get; set; }
        public string UseSnowLossModel { get; set; }
    }
}
