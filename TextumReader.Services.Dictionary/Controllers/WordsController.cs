using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TextumReader.Services.Dictionary.Services;
using TextumReader.Services.Words.Models;

namespace TextumReader.Services.Words.Controllers
{
    [Route("words")]
    [ApiController]
    public class WordsController : ControllerBase
    {
        private readonly WordsService _wordsService;

        public WordsController(WordsService wordsService)
        {
            _wordsService = wordsService;
        }

        [HttpGet(Name = nameof(GetWords))]
        public ActionResult<List<Word>> GetWords()
        {
            var currentUserId = HttpContext.Request.Headers["CurrentUser"][0];

            return _wordsService.GetWords(currentUserId);
        }

        [HttpGet("{id:length(24)}", Name = nameof(GetWordById))]
        public ActionResult<Word> GetWordById(string id)
        {
            var word = _wordsService.GetWord(id);

            if (word == null) return NotFound();

            return word;
        }

        [HttpPost(Name = nameof(CreateWord))]
        public ActionResult<Word> CreateWord(Word word)
        {
            _wordsService.Create(word);

            return CreatedAtRoute(nameof(GetWordById), new {id = word.Id}, word);
        }

        [HttpPut("{id:length(24)}", Name = nameof(UpdateWord))]
        public IActionResult UpdateWord(string id, Word word)
        {
            var oldWord = _wordsService.GetWord(id);

            if (oldWord == null) return NotFound();

            _wordsService.Update(id, word);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}", Name = nameof(DeleteWord))]
        public IActionResult DeleteWord(string id)
        {
            var word = _wordsService.GetWord(id);

            if (word == null) return NotFound();

            _wordsService.Remove(word.Id);

            return NoContent();
        }
    }
}