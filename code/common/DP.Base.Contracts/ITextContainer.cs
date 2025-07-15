namespace DP.Base.Contracts
{
    public interface ITextContainer
    {
        string Text { get; set; }
    }

    public class TextContainer : ITextContainer
    {
        public TextContainer()
        {
        }

        public TextContainer(string text)
        {
            this.Text = text;
        }

        public string Text { get; set; }
    }
}
