using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TextumReader.DataAccess;
using TextumReader.Services.Words.Models;

namespace TextumReader.Services.Words.Controllers
{
    [Route("words")]
    [ApiController]
    public class WordsController : ControllerBase
    {
        private readonly IRepository<Word> _cosmosDbService;

        public WordsController(IRepository<Word> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        [HttpGet(Name = nameof(GetWords))]
        public async Task<IEnumerable<Word>> GetWords()
        {
            var currentUserId = HttpContext.Request.Headers["CurrentUser"][0];

            var result = await _cosmosDbService.GetItemsAsync($"SELECT * FROM c WHERE c.userId = '{currentUserId}'");

            return result;
        }

        [HttpGet("{id:length(24)}", Name = nameof(GetWordById))]
        public async Task<ActionResult<Word>> GetWordById(string id)
        {
            var word = await _cosmosDbService.GetItemAsync($"SELECT * FROM c WHERE c.id = '{id}'");

            if (word == null) return NotFound();

            return word;
        }

        [HttpPost(Name = nameof(CreateWord))]
        public async Task<ActionResult<Word>> CreateWord(Word word)
        {
            var currentUserId = HttpContext.Request.Headers["CurrentUser"][0];

            word.Id = Guid.NewGuid().ToString();
            word.UserId = currentUserId;

            await _cosmosDbService.AddItemAsync(word);

            return CreatedAtRoute(nameof(GetWordById), new {id = word.Id}, word);
        }

        [HttpPut("{id}", Name = nameof(UpdateWord))]
        public async Task<IActionResult> UpdateWord(string id, Word word)
        {
            await _cosmosDbService.UpdateItemAsync(id, word);

            return NoContent();
        }

        [HttpDelete("{id}", Name = nameof(DeleteWord))]
        public async Task<IActionResult> DeleteWord(string id)
        {
            await _cosmosDbService.DeleteItemAsync(id);

            return NoContent();
        }
    }
}