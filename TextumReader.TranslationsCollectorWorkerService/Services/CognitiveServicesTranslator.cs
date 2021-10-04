using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TextumReader.TranslationsCollectorWorkerService.Services
{
    public class CognitiveServicesTranslator
    {
        private readonly string _endpoint;
        private readonly HttpClient _httpClient = new();
        private readonly string _location;
        private readonly string _subscriptionKey;

        public CognitiveServicesTranslator()
        {
            _subscriptionKey = "14914212e11a4d08b55af85c08c9db0e";
            _location = "northeurope";
            _endpoint = "https://api.cognitive.microsofttranslator.com/";
        }

        public IEnumerable<string> GetExamples(string from, string to, string text, string translation)
        {
            // See examples of terms in context
            var route = $"/dictionary/examples?api-version=3.0&from={from}&to={to}";
            object[] body = { new { Text = text, Translation = translation } };
            var requestBody = JsonConvert.SerializeObject(body);

            var result = SendTranslationRequest(route, requestBody);

            var json = JArray.Parse(result);

            return json[0]["examples"].Select(e => $"{e["sourcePrefix"]}{e["sourceTerm"]}{e["sourceSuffix"]}");
        }

        private string SendTranslationRequest(string route, string requestBody)
        {
            using var request = new HttpRequestMessage();
            // Build the request.
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(_endpoint + route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            request.Headers.Add("Ocp-Apim-Subscription-Region", _location);

            // Send the request and get response.
            var response = _httpClient.Send(request);
            // Read response as a string.
            var result = response.Content.ReadAsStringAsync().Result;
            return result;
        }
    }
}