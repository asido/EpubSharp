namespace EpubSharp.Format
{
    public class EpubFormat
    {
        public OcfDocument Ocf { get; internal set; }
        public OpfDocument Opf { get; internal set; }
        public NcxDocument Ncx { get; internal set; }
    }
}
