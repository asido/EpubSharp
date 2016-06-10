using System;

namespace EpubSharp
{
    public class EpubException : Exception
    {
        public EpubException(string message) : base(message) { }
    }
}
