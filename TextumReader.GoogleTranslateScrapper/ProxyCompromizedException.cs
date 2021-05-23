using System;

namespace TextumReader.GoogleTranslateScrapper
{
    public class ProxyCompromizedException : Exception
    {
        public ProxyCompromizedException()
        {
        }

        public ProxyCompromizedException(string message)
            : base(message)
        {
        }

        public ProxyCompromizedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
