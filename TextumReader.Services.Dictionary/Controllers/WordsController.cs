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

        [HttpGet]
        public ActionResult<List<Word>> GetByUser(string userId)
        {
            return _wordsService.GetWords(userId);
        }

        [HttpGet("{id:length(24)}", Name = "words")]
        public ActionResult<Word> GetById(string id)
        {
            var word = _wordsService.GetWord(id);

            if (word == null) return NotFound();

            return word;
        }

        [HttpPost]
        public ActionResult<Word> Create(Word word)
        {
            _wordsService.Create(word);

            return CreatedAtRoute("words", new {id = word.Id}, word);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Word word)
        {
            var oldWord = _wordsService.GetWord(id);

            if (oldWord == null) return NotFound();

            _wordsService.Update(id, word);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var word = _wordsService.GetWord(id);

            if (word == null) return NotFound();

            _wordsService.Remove(word.Id);

            return NoContent();
        }
    }
}