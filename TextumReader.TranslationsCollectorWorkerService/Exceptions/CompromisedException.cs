using System;

namespace TextumReader.TranslationsCollectorWorkerService.Exceptions
{
    public class CompromisedException : Exception
    {
        public CompromisedException()
        {
        }

        public CompromisedException(string message)
            : base(message)
        {
        }

        public CompromisedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}