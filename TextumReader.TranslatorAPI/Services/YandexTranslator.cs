using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using RestSharp;
using TextumReader.Services.Translator.DTO.Responses;

namespace TextumReader.Services.Translator.Services
{
    public class YandexTranslator : ITranslator
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _key = "dict.1.1.20210512T145107Z.17044363933bf6a8.1e8da230f1f256ace88bac6d9cccce74dd51f185";
        private readonly string _url = "https://dictionary.yandex.net/api/v1/dicservice.json/lookup?";

        public YandexTranslator(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<WordTranslations> GetWordTranslation(string from, string to, string text)
        {
            // See many translation options
            string route = $"{_url}?key={_key}&lang={from}-{to}&text={text}";

            var client = new RestClient(_url);

            if (_env.IsDevelopment())
            {
                client.Proxy = new WebProxy(new Uri("https://209.127.191.180:9279"));
            }

            var request = new RestRequest($"?key={_key}&lang={from}-{to}&text={text}", DataFormat.Json);

            var response = await client.ExecuteAsync(request);

            var json = JArray.Parse(response.Content);


            var translationNodes = (JArray)json["def"];

            IList<WordTranslation> translations = translationNodes.Select(t => t["tr"]).Select(t => new WordTranslation((string)t["text"], (string)t["pos"])).ToList();


            return new(text, translations);
        }

        public async Task<IEnumerable<string>> GetExamples(string from, string to, string text, string translation)
        {
            throw new NotImplementedException();
        }

        public async Task<TextTranslation> GetTextTranslation(string from, string to, string text)
        {
            throw new NotImplementedException();
        }
    }
}
