using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
                    EpubReader.Read(archivePath);
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
    }
}
