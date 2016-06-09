using System;

namespace EpubSharp.Schema.Opf
{
    public class EpubSpineItemRef
    {
        public string IdRef { get; set; }
        public bool IsLinear { get; set; }

        public override string ToString()
        {
            return "IdRef: " + IdRef;
        }
    }
}
