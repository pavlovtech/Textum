﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TextumReader.Services.Translator.Models.Requests;
using TextumReader.Services.Translator.Models.Responses;

namespace TextumReader.Services.Translator.Controllers
{
    [ApiController]
    [Route("translator")]
    //[Authorize("read:translations")]
    public class TranslatorController : ControllerBase
    {
        private readonly ILogger<TranslatorController> _logger;

        private readonly string _subscriptionKey;
        private readonly string _endpoint;
        private readonly string _location;

        public TranslatorController(IConfiguration configuration, ILogger<TranslatorController> logger)
        {
            _logger = logger;
            _subscriptionKey = configuration["AzureSubscriptionKey"];
            _location = configuration["AzureLocation"];
            _endpoint = configuration["AzureTranslatorEndpoint"];
        }

        /// <remarks>
        /// Sample request:
        ///
        ///     POST /word-translation
        ///     {
        ///        "from": "en",
        ///        "to": "ru",
        ///        "text": "Test"
        ///     }
        ///
        /// </remarks>
        [HttpPost("word-translation", Name = nameof(GetWordTranslation))]
        public async Task<WordTranslationsDto> GetWordTranslation(TranslationRequest translationRequest)
        {
            _logger.LogDebug(new EventId(1, "1"), HttpContext.Request.Headers["CurrentUser"][0]);

            // See many translation options
            string route = $"/dictionary/lookup?api-version=3.0&from={translationRequest.From}&to={translationRequest.To}";
            string wordToTranslate = translationRequest.Text;
            object[] body = { new { Text = wordToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);

            var result = await SendTranslationRequest(route, requestBody);

            var json = JArray.Parse(result);

            var word = json[0]["displaySource"].ToString();

            var translationNodes = (JArray)json[0]["translations"];

            IList<WordTranslationDto> translations = translationNodes.Select(t => new WordTranslationDto
            {
                Translation = (string)t["displayTarget"],
                PartOfSpeech = (string)t["posTag"],
            }).ToList();


            return new WordTranslationsDto
            {
                Word = word,
                Translations = translations
            };
        }

        [HttpPost("word-examples", Name = nameof(GetExamples))]
        public async Task<IEnumerable<string>> GetExamples(WordExampleRequest wordExampleRequest)
        {
            // See examples of terms in context
            string route = $"/dictionary/examples?api-version=3.0&from={wordExampleRequest.From}&to={wordExampleRequest.To}";
            object[] body = { new { Text = wordExampleRequest.Text, Translation = wordExampleRequest.Translation } };
            var requestBody = JsonConvert.SerializeObject(body);

            string result = await SendTranslationRequest(route, requestBody);

            var json = JArray.Parse(result);

            return json[0]["examples"].Select(e => $"{e["sourcePrefix"]}{e["sourceTerm"]}{e["sourceSuffix"]}");
        }

        [HttpPost("text-translation", Name = nameof(GetTextTranslation))]
        public async Task<TextTranslationDto> GetTextTranslation(TranslationRequest translationRequest)
        {
            // Input and output languages are defined as parameters.
            string route = $"/translate?api-version=3.0&from={translationRequest.From}&to={translationRequest.To}";
            string textToTranslate = translationRequest.Text;
            object[] body = { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);

            string result = await SendTranslationRequest(route, requestBody);

            var json = JArray.Parse(result);

            var translation = json[0]["translations"][0]["text"].ToString();

            return new TextTranslationDto
            {
                Translation = translation
            };
        }

        private async Task<string> SendTranslationRequest(string route, string requestBody)
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage();
            // Build the request.
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(_endpoint + route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            request.Headers.Add("Ocp-Apim-Subscription-Region", _location);

            // Send the request and get response.
            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            // Read response as a string.
            string result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}