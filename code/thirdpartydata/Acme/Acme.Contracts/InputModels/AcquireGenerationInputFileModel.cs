namespace Acme.Contracts
{
    /// <summary>
    /// Model used for acquring generation from Acme, specifically to get the path to a file containing the list of projects we need data for
    /// </summary>
    public class AcquireGenerationInputFileModel
    {
        public string ProjectList { get; set; }
    }
}