using FluentAssertions;
using Xunit;

namespace EpubSharp.Tests.Extensions
{
    public class StringExtTests
    {
        [Fact]
        public void CanGetAbsolutePathInIdealScenario()
        {
            "file.txt".ToAbsolutePath("one/two/").Should().Be("/one/two/file.txt");
            "file.txt".ToAbsolutePath("/one/two/").Should().Be("/one/two/file.txt");
        }

        [Fact]
        public void CanGetAbsolutePathByTrimmingPathFilename()
        {
            "file.txt".ToAbsolutePath("one/two").Should().Be("/one/file.txt");
            "file.txt".ToAbsolutePath("/one/two").Should().Be("/one/file.txt");
        }

        [Fact]
        public void CanGetAbsolutePathFromFileAndFile()
        {
            "bar.txt".ToAbsolutePath("foo.txt").Should().Be("/bar.txt");
            "bar.txt".ToAbsolutePath("/one/foo.txt").Should().Be("/one/bar.txt");
            "/one/bar.txt".ToAbsolutePath("foo.txt").Should().Be("/one/bar.txt");
            "two/bar.txt".ToAbsolutePath("/one/foo.txt").Should().Be("/one/two/bar.txt");
            "/two/bar.txt".ToAbsolutePath("/one/foo.txt").Should().Be("/two/bar.txt");
        }

        [Fact]
        public void CanGetAbsolutePathForRelativeFile()
        {
            "./foo.txt".ToAbsolutePath("/one/").Should().Be("/one/foo.txt");
            "./two/foo.txt".ToAbsolutePath("/one/").Should().Be("/one/two/foo.txt");
            "../foo.txt".ToAbsolutePath("/one/").Should().Be("/foo.txt");
            "../two/foo.txt".ToAbsolutePath("/one/").Should().Be("/two/foo.txt");
        }
    }
}
