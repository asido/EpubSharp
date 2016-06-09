using System.Collections.Generic;

namespace EpubSharp.Schema.Opf
{
    public class EpubSpine : List<EpubSpineItemRef>
    {
        public string Toc { get; set; }
    }
}
