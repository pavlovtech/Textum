using System;
using TextumReader.DataAccess;

namespace TextumReader.Services.TextMaterial.Models
{
    public class Text : BaseModel
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string TextContent { get; set; }
        public string InputLanguage { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}
