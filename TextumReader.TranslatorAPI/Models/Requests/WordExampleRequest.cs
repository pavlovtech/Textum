namespace TextumReader.Services.Translator.Models.Requests
{
    public class WordExampleRequest
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Text { get; set; }
        public string Translation { get; set; }
    }
}
