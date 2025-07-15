namespace DP.Base.Contracts
{
    public interface IDataHashProvider
    {
        ulong DataVersion { get; }
    }

    public class SimpleDataHashProvider : IDataHashProvider
    {
        public ulong DataVersion { get; set; }
    }
}
