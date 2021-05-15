using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TextumReader.Services.Translator.DTO.Responses;
using TextumReader.Services.Translator.Services;

namespace TextumReader.Services.Translator.Controllers
{
    [ApiController]
    [Route("translator")]
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
        [HttpGet("word-translation", Name = nameof(GetWordTranslation))]
        public async Task<WordTranslations> GetWordTranslation([FromQuery]string from, [FromQuery]string to, [FromQuery]string text)
        {
            return await _translator.GetWordTranslation(from, to, text);
        }

        [HttpGet("word-examples", Name = nameof(GetExamples))]
        public async Task<IEnumerable<string>> GetExamples([FromQuery] string from, [FromQuery] string to, [FromQuery] string text, [FromQuery] string translation)
        {
            return await _translator.GetExamples(from, to, text, translation);
        }

        [HttpPost("text-translation", Name = nameof(GetTextTranslation))]
        public async Task<TextTranslation> GetTextTranslation(TranslationRequest translationRequest)
        {
            return await _translator.GetTextTranslation(translationRequest.From, translationRequest.To, translationRequest.Text);
        }
    }
}
