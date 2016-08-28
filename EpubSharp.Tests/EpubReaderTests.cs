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
            ZipAndMoveAndTestEpubOpen(@"../../Samples/epub30");
        }

        [TestMethod]
        public void OpenEpub31Test()
        {
            ZipAndMoveAndTestEpubOpen(@"../../Samples/epub31");
        }

        [TestMethod]
        public void OpenEpubAssortedTest()
        {
            MoveAndTestEpubOpen(@"../../Samples/epub-assorted");
        }

        private void ZipAndMoveAndTestEpubOpen(string samplePath)
        {
            if (samplePath == null) throw new ArgumentNullException(nameof(samplePath));

            var destination = Path.Combine("Samples", Path.GetFileName(samplePath));
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            var samples = Directory.GetDirectories(samplePath, "*", SearchOption.TopDirectoryOnly).ToList();
            var archives = new List<string>();

            foreach (var source in samples)
            {
                var archiveName = Path.GetFileName(source) + ".zip";
                var archivePath = Path.Combine(destination, archiveName);
                if (!File.Exists(archivePath))
                {
                    ZipFile.CreateFromDirectory(source, archivePath);
                }
                archives.Add(archivePath);
            }

            OpenEpubTest(archives);
        }

        private void MoveAndTestEpubOpen(string samplePath)
        {
            if (samplePath == null) throw new ArgumentNullException(nameof(samplePath));

            var destination = Path.Combine("Samples", Path.GetFileName(samplePath));
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            var samples = Directory.GetFiles(samplePath);
            var archives = new List<string>();

            foreach (var source in samples)
            {
                var archiveName = Path.GetFileName(source);
                var archivePath = Path.Combine(destination, archiveName);
                if (!File.Exists(archivePath))
                {
                    File.Copy(source, archivePath);
                }
                archives.Add(archivePath);
            }

            OpenEpubTest(archives);
        }

        private void OpenEpubTest(ICollection<string> files)
        {
            var exceptions = new List<string>();

            foreach (var path in files)
            {
                try
                {
                    var book = EpubReader.Read(path);
                    AssertOcf(book.Format.Ocf, book.Format.NewOcf);
                    AssertPackage(book.Format.Package, book.Format.NewPackage);
                    AssertNcx(book.Format.Ncx, book.Format.NewNcx);
                }
                catch (Exception ex)
                {
                    exceptions.Add($"Failed to open book: '{path}'. Exception: {ex.Message}");
                }
            }

            if (exceptions.Any())
            {
                var message = $"Failed to open {exceptions.Count}/{files.Count} samples.{Environment.NewLine}{string.Join(Environment.NewLine, exceptions)}";
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

                AssertCollection(expected.Metadata, actual.Metadata, nameof(actual.Metadata), (old, @new, i) =>
                {
                    Assert.AreEqual(old[i].Name, @new[i].Name, "Metadata.Name");
                    Assert.AreEqual(old[i].Content, @new[i].Content, "Metadata.Content");
                    Assert.AreEqual(old[i].Scheme, @new[i].Scheme, "Metadata.Scheme");
                });

                Assert.AreEqual(expected.NavigationList == null, actual.NavigationList == null, "NavigationList");
                if (expected.NavigationList != null && actual.NavigationList != null)
                {
                    Assert.AreEqual(expected.NavigationList.Id, actual.NavigationList.Id, "NavigationList.Id");
                    Assert.AreEqual(expected.NavigationList.Class, actual.NavigationList.Class, "NavigationList.Class");
                    Assert.AreEqual(expected.NavigationList.Label, actual.NavigationList.Label, "NavigationList.LabelText");

                    AssertCollection(expected.NavigationList.NavigationTargets, actual.NavigationList.NavigationTargets, nameof(actual.NavigationList.NavigationTargets), (old, @new, i) =>
                    {
                        Assert.AreEqual(old[i].Id, @new[i].Id, "NavigationTarget.Id");
                        Assert.AreEqual(old[i].Class, @new[i].Class, "NavigationTarget.Class");
                        Assert.AreEqual(old[i].Label, @new[i].Label, "NavigationTarget.LabelText");
                        Assert.AreEqual(old[i].PlayOrder, @new[i].PlayOrder, "NavigationTarget.PlayOrder");
                        Assert.AreEqual(old[i].ContentSource, @new[i].ContentSource, "NavigationTarget.ContentSrc");
                    });
                }

                AssertCollection(expected.NavigationMap, actual.NavigationMap, nameof(actual.NavigationMap), (old, @new, i) =>
                {
                    Assert.AreEqual(old[i].Id, @new[i].Id, "NavigationMap.Id");
                    Assert.AreEqual(old[i].PlayOrder, @new[i].PlayOrder, "NavigationMap.PlayOrder");
                    Assert.AreEqual(old[i].LabelText, @new[i].LabelText, "NavigationMap.PlayOrder");
                    Assert.AreEqual(old[i].Class, @new[i].Class, "NavigationMap.Class");
                    Assert.AreEqual(old[i].ContentSrc, @new[i].ContentSrc, "NavigationMap.ContentSorce");
                    AssertNavigationPoints(old[i].NavigationPoints, @new[i].NavigationPoints);
                });

                AssertCollection(expected.PageList, actual.PageList, nameof(actual.PageList), (old, @new, i) =>
                {
                    Assert.AreEqual(old[i].Id, @new[i].Id, "PageList.Id");
                    Assert.AreEqual(old[i].Class, @new[i].Class, "PageList.Class");
                    Assert.AreEqual(old[i].ContentSource, @new[i].ContentSource, "PageList.ContentSrc");
                    Assert.AreEqual(old[i].Label, @new[i].Label, "PageList.LabelText");
                    Assert.AreEqual(old[i].Type, @new[i].Type, "PageList.Type");
                    Assert.AreEqual(old[i].Value, @new[i].Value, "PageList.Value");
                });
            }
        }

        private void AssertNavigationPoints(IEnumerable<NcxNavigationPoint> expected, IEnumerable<NcxNavigationPoint> actual)
        {
            AssertCollection(expected, actual, "NavigationPoint", (old, @new, i) =>
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
            });
        }

        private void AssertOcf(OcfDocument expected, OcfDocument actual)
        {
            Assert.AreEqual(expected.RootFile, actual.RootFile);
        }

        private void AssertPackage(PackageDocument expected, PackageDocument actual)
        {
            Assert.AreEqual(expected == null, actual == null, nameof(actual));
            if (expected != null && actual != null)
            {
                Assert.AreEqual(expected.EpubVersion, actual.EpubVersion, nameof(actual.EpubVersion));

                Assert.AreEqual(expected.Metadata == null, actual.Metadata == null, nameof(actual.Metadata));
                if (expected.Metadata != null && actual.Metadata != null)
                {
                    AssertCreators(expected.Metadata.Creators, actual.Metadata.Creators, nameof(actual.Metadata.Creators));
                    AssertCreators(expected.Metadata.Contributors, actual.Metadata.Contributors, nameof(actual.Metadata.Contributors));

                    AssertCollection(expected.Metadata.Coverages, actual.Metadata.Coverages, nameof(actual.Metadata.Coverages), (old, @new, i) =>
                    {
                        Assert.IsTrue(@new.Contains(old[i]), "Coverage");
                    });

                    AssertCollection(expected.Metadata.Dates, actual.Metadata.Dates, nameof(actual.Metadata.Dates), (old, @new, i) =>
                    {
                        Assert.AreEqual(old[i].Text, @new[i].Text, "Date.Text");
                        Assert.AreEqual(old[i].Event, @new[i].Event, "Date.Event");
                    });

                    AssertCollection(expected.Metadata.Descriptions, actual.Metadata.Descriptions, nameof(actual.Metadata.Descriptions), (old, @new, i) =>
                    {
                        Assert.IsTrue(@new.Contains(old[i]), "Description");
                    });

                    AssertCollection(expected.Metadata.Identifiers, actual.Metadata.Identifiers, nameof(actual.Metadata.Identifiers), (old, @new, i) =>
                    {
                        Assert.AreEqual(old[i].Id, @new[i].Id, "Identifier.Id");
                        Assert.AreEqual(old[i].Scheme, @new[i].Scheme, "Identifier.Scheme");
                        Assert.AreEqual(old[i].Text, @new[i].Text, "Identifier.Text");
                    });
                }
            }
        }

        private void AssertCreators(IEnumerable<PackageMetadataCreator> expected, IEnumerable<PackageMetadataCreator> actual, string name)
        {
            AssertCollection(expected, actual, name, (old, @new, i) =>
            {
                Assert.AreEqual(old[i].AlternateScript, @new[i].AlternateScript, $"{name}.AlternateScript");
                Assert.AreEqual(old[i].FileAs, @new[i].FileAs, $"{name}.FileAs");
                Assert.AreEqual(old[i].Role, @new[i].Role, $"{name}.Role");
                Assert.AreEqual(old[i].Text, @new[i].Text, $"{name}.Text");
            });
        }

        private void AssertCollection<T>(IEnumerable<T> expected, IEnumerable<T> actual, string name, Action<List<T>, List<T>, int> assert)
        {
            Assert.AreEqual(expected == null, actual == null, name);
            if (expected != null && actual != null)
            {
                var old = expected.ToList();
                var @new = actual.ToList();

                Assert.AreEqual(old.Count, @new.Count, $"{name}.Count");

                for (var i = 0; i < @new.Count; ++i)
                {
                    assert(old, @new, i);
                }
            }
        }
    }
}
