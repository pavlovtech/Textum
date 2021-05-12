using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using TextumReader.Services.Translator.Models.Requests;
using TextumReader.Services.Translator.Models.Responses;

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

        public async Task<WordTranslationsDto> GetWordTranslation(TranslationRequest translationRequest)
        {
            // See many translation options
            string route = $"{_url}?key={_key}&lang={translationRequest.From}-{translationRequest.From}&text={translationRequest.Text}";

            var client = new RestClient(_url);

            if (_env.IsDevelopment())
            {
                client.Proxy = new WebProxy(new Uri("https://209.127.191.180:9279"));
            }

            var request = new RestRequest($"?key={_key}&lang={translationRequest.From}-{translationRequest.From}&text={translationRequest.Text}", DataFormat.Json);

            var response = client.Get(request);

            var json = JArray.Parse(response.Content);


            var translationNodes = (JArray)json["def"];

            IList<WordTranslationDto> translations = translationNodes.Select(t => t["tr"]).Select(t => new WordTranslationDto
            {
                Translation = (string)t["text"],
                PartOfSpeech = (string)t["pos"],
            }).ToList();


            return new WordTranslationsDto
            {
                Word = translationRequest.Text,
                Translations = translations
            };
        }

        public async Task<IEnumerable<string>> GetExamples(WordExampleRequest wordExampleRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<TextTranslationDto> GetTextTranslation(TranslationRequest translationRequest)
        {
            throw new NotImplementedException();
        }
    }
}
