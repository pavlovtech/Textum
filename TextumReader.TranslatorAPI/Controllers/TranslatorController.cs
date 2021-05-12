using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TextumReader.Services.Translator.Models.Requests;
using TextumReader.Services.Translator.Models.Responses;
using TextumReader.Services.Translator.Services;

namespace TextumReader.Services.Translator.Controllers
{
    [ApiController]
    [Route("translator")]
    //[Authorize("read:translations")]
    public class TranslatorController : ControllerBase
    {
        private readonly ITranslator _translator;
        private readonly ILogger<TranslatorController> _logger;


        public TranslatorController(ITranslator translator, ILogger<TranslatorController> logger)
        {
            _translator = translator;
            _logger = logger;
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
            return await _translator.GetWordTranslation(translationRequest);
        }

        [HttpPost("word-examples", Name = nameof(GetExamples))]
        public async Task<IEnumerable<string>> GetExamples(WordExampleRequest wordExampleRequest)
        {
            return await _translator.GetExamples(wordExampleRequest);
        }

        [HttpPost("text-translation", Name = nameof(GetTextTranslation))]
        public async Task<TextTranslationDto> GetTextTranslation(TranslationRequest translationRequest)
        {
            return await _translator.GetTextTranslation(translationRequest);
        }
    }
}
