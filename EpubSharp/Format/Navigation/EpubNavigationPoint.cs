using System;
using System.Collections.Generic;

namespace EpubSharp.Schema.Navigation
{
    public class EpubNavigationPoint
    {
        public string Id { get; set; }
        public string Class { get; set; }
        public string PlayOrder { get; set; }
        public List<EpubNavigationLabel> NavigationLabels { get; set; }
        public EpubNavigationContent Content { get; set; }
        public List<EpubNavigationPoint> ChildNavigationPoints { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Content.Source: {Content.Source}";
        }
    }
}
