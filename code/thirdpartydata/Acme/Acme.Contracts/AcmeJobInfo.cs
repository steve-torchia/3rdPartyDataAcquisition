namespace Acme.Contracts
{
    public class AcmeJobInfo
    {

        /// <summary>
        /// The ID of the job that gets kicked off at Acme
        /// </summary>
        public string Status { get; set; }
        public string ResultsUrl { get; set; }
        public string Message { get; set; }
        public string StartWork { get; set; }
        public string EndWork { get; set; }

    }
}
