namespace EpubSharp.Format
{
    public class EpubFormat
    {
        public OcfDocument Ocf { get; internal set; }
        public PackageDocument Package { get; internal set; }
        public NcxDocument Ncx { get; internal set; }
    }

    public class OcfDocument
    {
        public string RootFile { get; internal set; }
    }
}
