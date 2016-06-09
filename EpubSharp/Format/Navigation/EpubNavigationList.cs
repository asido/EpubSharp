using System.Collections.Generic;

namespace EpubSharp.Schema.Navigation
{
    public class EpubNavigationList
    {
        public string Id { get; set; }
        public string Class { get; set; }
        public IReadOnlyCollection<string> NavigationLabels { get; set; }
        public List<EpubNavigationTarget> NavigationTargets { get; set; }
    }
}
