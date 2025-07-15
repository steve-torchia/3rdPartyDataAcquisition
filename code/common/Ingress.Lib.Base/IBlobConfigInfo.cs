namespace Ingress.Lib.Base
{
    public interface IBlobConfigInfo
    {
        public string ConnectionString { get; set; }

        public string ContainerName { get; set; }
    }
}
