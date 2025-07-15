namespace Ingress.Lib.Base
{
    public class BlobConfigInfo : IBlobConfigInfo
    {
        public string ConnectionString { get; set; }

        public string ContainerName { get; set; }

        public BlobConfigInfo()
        {
        }
    }
}
