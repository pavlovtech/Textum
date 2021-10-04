using System;

namespace TextumReader.TranslationJobProcessor.Exceptions
{
    public class Compromised : Exception
    {
        public Compromised()
        {
        }

        public Compromised(string message)
            : base(message)
        {
        }

        public Compromised(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
