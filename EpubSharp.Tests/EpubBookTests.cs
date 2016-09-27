using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace EpubSharp.Tests
{
    [TestFixture]
    public class EpubBookTests
    {
        [Test]
        public void EpubAsPlainTextTest1()
        {
            var book = EpubReader.Read(@"Samples/epub-assorted/boothbyg3249432494-8epub.epub");
            //File.WriteAllText("Samples/epub-assorted/boothbyg3249432494-8epub.txt", book.ToPlainText());

            Func<string, string> normalize = text => text.Replace("\r", "").Replace("\n", "").Replace(" ", "");
            var expected = File.ReadAllText(@"Samples/epub-assorted/boothbyg3249432494-8epub.txt");
            var actual = book.ToPlainText();
            Assert.AreEqual(normalize(expected), normalize(actual));

            var lines = actual.Split('\n').Select(str => str.Trim()).ToList();
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "I. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "II. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "III. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "IV. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "V. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "VI. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "VII. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "VIII. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "IX. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "X. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "XI. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "XII. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "XIII. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "XIV. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "XV. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "XVI. KAPITEL."));
            Assert.IsNotNull(lines.SingleOrDefault(e => e == "XVII. KAPITEL."));
        }

        [Test]
        public void EpubAsPlainTextTest2()
        {
            var book = EpubReader.Read(@"Samples/epub-assorted/iOS Hackers Handbook.epub");
            //File.WriteAllText("Samples/epub-assorted/iOS Hackers Handbook.txt", book.ToPlainText());

            Func<string, string> normalize = text => text.Replace("\r", "").Replace("\n", "").Replace(" ", "");
            var expected = File.ReadAllText(@"Samples/epub-assorted/iOS Hackers Handbook.txt");
            var actual = book.ToPlainText();
            Assert.AreEqual(normalize(expected), normalize(actual));
            
            var trimmed = string.Join("\n", actual.Split('\n').Select(str => str.Trim()));
            Assert.AreEqual(1, Regex.Matches(trimmed, "Chapter 1\niOS Security Basics").Count);
            Assert.AreEqual(1, Regex.Matches(trimmed, "Chapter 2\niOS in the Enterprise").Count);
            Assert.AreEqual(1, Regex.Matches(trimmed, "Chapter 3\nEncryption").Count);
            Assert.AreEqual(1, Regex.Matches(trimmed, "Chapter 4\nCode Signing and Memory Protections").Count);
            Assert.AreEqual(1, Regex.Matches(trimmed, "Chapter 5\nSandboxing").Count);
            Assert.AreEqual(1, Regex.Matches(trimmed, "Chapter 6\nFuzzing iOS Applications").Count);
            Assert.AreEqual(1, Regex.Matches(trimmed, "Chapter 7\nExploitation").Count);
            Assert.AreEqual(1, Regex.Matches(trimmed, "Chapter 8\nReturn-Oriented Programming").Count);
            Assert.AreEqual(1, Regex.Matches(trimmed, "Chapter 9\nKernel Debugging and Exploitation").Count);
            Assert.AreEqual(1, Regex.Matches(trimmed, "Chapter 10\nJailbreaking").Count);
            Assert.AreEqual(1, Regex.Matches(trimmed, "Chapter 11\nBaseband Attacks").Count);
            Assert.AreEqual(1, Regex.Matches(trimmed, "How This Book Is Organized").Count);
            Assert.AreEqual(2, Regex.Matches(trimmed, "Appendix: Resources").Count);
            Assert.AreEqual(2, Regex.Matches(trimmed, "Case Study: Pwn2Own 2010").Count);
        }
    }
}
