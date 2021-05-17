using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TextumReader.Services.Translator.DTO.Responses;

namespace TextumReader.Services.Translator.Services
{
    public class CognitiveServicesTranslator : ITranslator
    {
        private readonly string _subscriptionKey;
        private readonly string _endpoint;
        private readonly string _location;
        private readonly HttpClient _httpClient;

        public CognitiveServicesTranslator(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;

            _subscriptionKey = configuration["AzureSubscriptionKey"];
            _location = configuration["AzureLocation"];
            _endpoint = configuration["AzureTranslatorEndpoint"];
        }

        public async Task<WordTranslations> GetWordTranslation(string from, string to, string text)
        {
            // See many translation options
            string route = $"/dictionary/lookup?api-version=3.0&from={from}&to={to}";
            string wordToTranslate = text;
            object[] body = { new { Text = wordToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);

            var result = await SendTranslationRequest(route, requestBody);

            var json = JArray.Parse(result);

            var word = json[0]["displaySource"].ToString();

            var translationNodes = (JArray)json[0]["translations"];

            var translations = translationNodes.Select(t =>
                new WordTranslation((string) t["displayTarget"], (string) t["posTag"]));

            var translationResult = new WordTranslations(text, translations);

            return translationResult;
        }

        public async Task<IEnumerable<string>> GetExamples(string from, string to, string text, string translation)
        {
            // See examples of terms in context
            string route = $"/dictionary/examples?api-version=3.0&from={from}&to={to}";
            object[] body = { new { Text = text, Translation = translation } };
            var requestBody = JsonConvert.SerializeObject(body);

            string result = await SendTranslationRequest(route, requestBody);

            var json = JArray.Parse(result);

            return json[0]["examples"].Select(e => $"{e["sourcePrefix"]}{e["sourceTerm"]}{e["sourceSuffix"]}");
        }

        public async Task<TextTranslation> GetTextTranslation(string from, string to, string text)
        {
            // Input and output languages are defined as parameters.
            string route = $"/translate?api-version=3.0&from={from}&to={to}";
            string textToTranslate = text;
            object[] body = { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);

            string result = await SendTranslationRequest(route, requestBody);

            var json = JArray.Parse(result);

            var translation = json[0]["translations"]?[0]["text"]?.ToString();

            return new(translation);
        }

        private async Task<string> SendTranslationRequest(string route, string requestBody)
        {
            using var request = new HttpRequestMessage();
            // Build the request.
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(_endpoint + route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            request.Headers.Add("Ocp-Apim-Subscription-Region", _location);

            // Send the request and get response.
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            // Read response as a string.
            string result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}
