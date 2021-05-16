using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TextumReader.Services.TextMaterial.Models;
using TextumReader.Services.TextMaterial.Services;

namespace TextumReader.Services.TextMaterial.Controllers
{
    [Route("texts")]
    [ApiController]
    public class TextsController : ControllerBase
    {
        private readonly IRepository<Text> _cosmosDbService;

        public TextsController(IRepository<Text> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        [HttpGet(Name = nameof(GetTexts))]
        public async Task<IEnumerable<Text>> GetTexts()
        {
            var currentUserId = HttpContext.Request.Headers["CurrentUser"][0];

            return await _cosmosDbService.GetItemsAsync($"SELECT * FROM c WHERE c.userId = '{currentUserId}'");
        }

        [HttpGet("{id}", Name = nameof(GetTextById))]
        public async Task<ActionResult<Text>> GetTextById(string id)
        {
            var text = await _cosmosDbService.GetItemAsync(id);

            if (text == null)
            {
                return NotFound();
            }

            return text;
        }

        [HttpPost(Name = nameof(CreateText))]
        public async Task<CreatedAtRouteResult> CreateText(Text text)
        {
            var currentUserId = HttpContext.Request.Headers["CurrentUser"][0];

            text.Id = Guid.NewGuid().ToString();
            text.UserId = currentUserId;

            await _cosmosDbService.AddItemAsync(text);

            return CreatedAtRoute(nameof(GetTextById), new { id = text.Id }, text);
        }

        [HttpPut("{id}", Name = nameof(UpdateText))]
        public async Task<NoContentResult> UpdateText(string id, Text text)
        {
            await _cosmosDbService.UpdateItemAsync(id, text);

            return NoContent();
        }

        [HttpDelete("{id}", Name = nameof(DeleteText))]
        public async Task<NoContentResult> DeleteText(string id)
        {
            await _cosmosDbService.DeleteItemAsync(id);

            return NoContent();
        }
    }
}
