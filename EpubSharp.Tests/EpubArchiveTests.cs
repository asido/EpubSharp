using NUnit.Framework;

namespace EpubSharp.Tests
{
    [TestFixture]
    public class EpubArchiveTests
    {
        [Test]
        public void FindEntryTest()
        {
            var archive = new EpubArchive("Samples/epub-assorted/Bogtyven.epub");
            Assert.NotNull(archive.FindEntry("META-INF/container.xml"));
            Assert.Null(archive.FindEntry("UNEXISTING_ENTRY"));
        }
    }
}
