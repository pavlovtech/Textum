using System;
using System.Collections.Generic;
using TextumReader.DataAccess;

namespace TextumReader.Services.Words.Models
{
    public class Word : BaseModel
    {
        public string UserId { get; set; }

        public string DisplayWord { get; set; }

        public DateTimeOffset WordCreated { get; set; }

        public IEnumerable<string> Translations { get; set; }
    }
}