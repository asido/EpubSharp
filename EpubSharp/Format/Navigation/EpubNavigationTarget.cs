using System.Collections.Generic;

namespace EpubSharp.Schema.Navigation
{
    public class EpubNavigationTarget
    {
        public string Id { get; set; }
        public string Class { get; set; }
        public string Value { get; set; }
        public string PlayOrder { get; set; }
        public IReadOnlyCollection<string> NavigationLabels { get; set; }
        public string ContentSource { get; set; }
    }
}
