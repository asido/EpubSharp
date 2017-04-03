using System.IO;

namespace EpubSharp.Tests
{
    public static class Cwd
    {
        public static string Combine(string relativePath)
        {
            // VS2017 test platform has a bug, that is fixed in VS 2017 Update 1.
            // Remove this nonsense when that is released.
            return Path.Combine(@"D:\Code\EpubSharp\EpubSharp.Tests", relativePath);
        }
    }
}
