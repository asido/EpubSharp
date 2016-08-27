using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using EpubSharp.Format;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EpubSharp.Tests
{
    [TestClass]
    public class EpubReaderTests
    {
        [TestMethod]
        public void OpenEpub30Test()
        {
            OpenEpubTest(@"../../Samples/epub30");
        }

        [TestMethod]
        public void OpenEpub31Test()
        {
            OpenEpubTest(@"../../Samples/epub31");
        }

        private void OpenEpubTest(string samplePath)
        {
            if (samplePath == null) throw new ArgumentNullException(nameof(samplePath));

            var exceptions = new List<string>();

            var destination = Path.Combine("Samples", Path.GetFileName(samplePath));
            if (Directory.Exists(destination))
            {
                Directory.Delete(destination, true);
            }
            Directory.CreateDirectory(destination);

            var samples = Directory.GetDirectories(samplePath, "*", SearchOption.TopDirectoryOnly).ToList();

            foreach (var source in samples)
            {
                var archiveName = Path.GetFileName(source) + ".zip";
                var archivePath = Path.Combine(destination, archiveName);
                ZipFile.CreateFromDirectory(source, archivePath);

                try
                {
                    var book = EpubReader.Read(archivePath);
                    AssertOcf(book.Format.Ocf, book.Format.NewOcf);
                    AssertNcx(book.Format.Ncx, book.Format.NewNcx);
                }
                catch (Exception ex)
                {
                    exceptions.Add($"Failed to open book: '{archiveName}'. Exception: {ex.Message}");
                }
            }

            if (exceptions.Any())
            {
                var message = $"Failed to open {exceptions.Count}/{samples.Count} samples.{Environment.NewLine}{string.Join(Environment.NewLine, exceptions)}";
                Assert.Fail(message);
            }
        }

        private void AssertNcx(NcxDocument expected, NcxDocument actual)
        {
            Assert.AreEqual(expected == null, actual == null, nameof(actual));
            if (expected != null && actual != null)
            {
                Assert.AreEqual(expected.DocAuthor, actual.DocAuthor, nameof(actual.DocAuthor));
                Assert.AreEqual(expected.DocTitle, actual.DocTitle, nameof(actual.DocTitle));

                Assert.AreEqual(expected.Metadata == null, actual.Metadata == null, "Metadata");
                if (expected.Metadata != null && actual.Metadata != null)
                {
                    var old = expected.Metadata.ToList();
                    var @new = actual.Metadata.ToList();

                    Assert.AreEqual(old.Count, @new.Count, "Metadata.Count");

                    for (var i = 0; i < @new.Count; ++i)
                    {
                        Assert.AreEqual(old[i].Name, @new[i].Name, "Metadata.Name");
                        Assert.AreEqual(old[i].Content, @new[i].Content, "Metadata.Content");
                        Assert.AreEqual(old[i].Scheme, @new[i].Scheme, "Metadata.Scheme");
                    }
                }

                Assert.AreEqual(expected.NavigationList == null, actual.NavigationList == null, "NavigationList");
                if (expected.NavigationList != null && actual.NavigationList != null)
                {
                    Assert.AreEqual(expected.NavigationList.Id, actual.NavigationList.Id, "NavigationList.Id");
                    Assert.AreEqual(expected.NavigationList.Class, actual.NavigationList.Class, "NavigationList.Class");
                    Assert.AreEqual(expected.NavigationList.Label, actual.NavigationList.Label, "NavigationList.LabelText");

                    var old = expected.NavigationList.NavigationTargets.ToList();
                    var @new = actual.NavigationList.NavigationTargets.ToList();

                    Assert.AreEqual(old.Count, @new.Count, "NavigationTargets.Count");

                    for (var i = 0; i < @new.Count; ++i)
                    {
                        Assert.AreEqual(old[i].Id, @new[i].Id, "NavigationTarget.Id");
                        Assert.AreEqual(old[i].Class, @new[i].Class, "NavigationTarget.Class");
                        Assert.AreEqual(old[i].Label, @new[i].Label, "NavigationTarget.LabelText");
                        Assert.AreEqual(old[i].PlayOrder, @new[i].PlayOrder, "NavigationTarget.PlayOrder");
                        Assert.AreEqual(old[i].ContentSource, @new[i].ContentSource, "NavigationTarget.ContentSrc");
                    }
                }

                Assert.AreEqual(expected.NavigationMap == null, actual.NavigationMap == null, "NavigationMap");
                if (expected.NavigationMap != null && actual.NavigationMap != null)
                {
                    var old = expected.NavigationMap.ToList();
                    var @new = actual.NavigationMap.ToList();

                    for (var i = 0; i < @new.Count; ++i)
                    {
                        Assert.AreEqual(old[i].Id, @new[i].Id, "NavigationMap.Id");
                        Assert.AreEqual(old[i].PlayOrder, @new[i].PlayOrder, "NavigationMap.PlayOrder");
                        Assert.AreEqual(old[i].LabelText, @new[i].LabelText, "NavigationMap.PlayOrder");
                        Assert.AreEqual(old[i].Class, @new[i].Class, "NavigationMap.Class");
                        Assert.AreEqual(old[i].ContentSrc, @new[i].ContentSrc, "NavigationMap.ContentSorce");
                        AssertNavigationPoints(old[i].NavigationPoints, @new[i].NavigationPoints);
                    }
                }

                Assert.AreEqual(expected.PageList == null, actual.PageList == null, "PageList");
                if (expected.PageList != null && actual.PageList != null)
                {
                    var old = expected.PageList.ToList();
                    var @new = actual.PageList.ToList();

                    for (var i = 0; i < @new.Count; ++i)
                    {
                        Assert.AreEqual(old[i].Id, @new[i].Id, "PageList.Id");
                        Assert.AreEqual(old[i].Class, @new[i].Class, "PageList.Class");
                        Assert.AreEqual(old[i].ContentSource, @new[i].ContentSource, "PageList.ContentSrc");
                        Assert.AreEqual(old[i].Label, @new[i].Label, "PageList.LabelText");
                        Assert.AreEqual(old[i].Type, @new[i].Type, "PageList.Type");
                        Assert.AreEqual(old[i].Value, @new[i].Value, "PageList.Value");
                    }
                }
            }
        }

        private void AssertNavigationPoints(IEnumerable<NcxNavigationPoint> expected, IEnumerable<NcxNavigationPoint> actual)
        {
            var old = expected.ToList();
            var @new = actual.ToList();

            for (var i = 0; i < @new.Count; ++i)
            {
                Assert.AreEqual(old[i].Id, @new[i].Id, "NavigationPoint.Id");
                Assert.AreEqual(old[i].Class, @new[i].Class, "NavigationPoint.Class");
                Assert.AreEqual(old[i].ContentSrc, @new[i].ContentSrc, "NavigationPoint.ContentSrc");
                Assert.AreEqual(old[i].LabelText, @new[i].LabelText, "NavigationPoint.LabelText");
                Assert.AreEqual(old[i].PlayOrder, @new[i].PlayOrder, "NavigationPoint.PlayOrder");
                Assert.AreEqual(old[i].NavigationPoints == null, @new[i].NavigationPoints == null, "NavigationPoint.NavigationPoints");
                if (old[i].NavigationPoints != null && @new[i].NavigationPoints != null)
                {
                    AssertNavigationPoints(old[i].NavigationPoints, @new[i].NavigationPoints);
                }
            }
        }

        private void AssertOcf(OcfDocument expected, OcfDocument actual)
        {
            Assert.AreEqual(expected.RootFile, actual.RootFile);
        }
    }
}
