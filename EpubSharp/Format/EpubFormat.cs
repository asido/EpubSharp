namespace EpubSharp.Format
{
    public class EpubFormatPaths
    {
        public string OcfAbsolutePath { get; internal set; }
        public string OpfAbsolutePath { get; internal set; }
        public string NcxAbsolutePath { get; internal set; }
        public string NavAbsolutePath { get; internal set; }
    }

    public class EpubFormat
    {
        public EpubFormatPaths Paths { get; internal set; } = new EpubFormatPaths();

        public OcfDocument Ocf { get; internal set; }
        public OpfDocument Opf { get; internal set; }
        public NcxDocument Ncx { get; internal set; }
        public NavDocument Nav { get; internal set; }
    }
}
