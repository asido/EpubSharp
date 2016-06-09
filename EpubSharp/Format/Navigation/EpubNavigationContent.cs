namespace EpubSharp.Schema.Navigation
{
    public class EpubNavigationContent
    {
        public string Id { get; set; }
        public string Source { get; set; }

        public override string ToString()
        {
            return "Source: " + Source;
        }
    }
}
