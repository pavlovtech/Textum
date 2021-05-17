using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TextumReader.DataAccess;
using TextumReader.Services.TextMaterial.Models;

namespace TextumReader.Services.TextMaterial.Controllers
{
    [Route("texts")]
    [ApiController]
    public class TextsController : ControllerBase
    {
        private readonly IRepository<Text> _cosmosDbService;
        private readonly IMemoryCache _memoryCache;

        public TextsController(IRepository<Text> cosmosDbService, IMemoryCache memoryCache)
        {
            _cosmosDbService = cosmosDbService;
            _memoryCache = memoryCache;
        }

        [HttpGet(Name = nameof(GetTexts))]
        public async Task<IEnumerable<Text>> GetTexts()
        {
            var currentUserId = HttpContext.Request.Headers["CurrentUser"][0];

            IEnumerable<Text> cacheEntry;

            string key = $"texts-{currentUserId}";

            _memoryCache.TryGetValue(key, out cacheEntry);

            if (cacheEntry != null)
            {
                return cacheEntry;
            }

            var result = await _cosmosDbService.GetItemsAsync($"SELECT * FROM c WHERE c.userId = '{currentUserId}'");

            _memoryCache.Set(key, result, TimeSpan.FromDays(30));

            return result;
        }

        [HttpGet("{id}", Name = nameof(GetTextById))]
        public async Task<ActionResult<Text>> GetTextById(string id)
        {
            Text cacheEntry;

            string key = $"text-{id}";

            _memoryCache.TryGetValue(key, out cacheEntry);

            if (cacheEntry != null)
            {
                return cacheEntry;
            }

            var text = await _cosmosDbService.GetItemAsync(id);

            if (text == null)
            {
                return NotFound();
            }

            _memoryCache.Set(key, text, TimeSpan.FromDays(30));

            return text;
        }

        [HttpPost(Name = nameof(CreateText))]
        public async Task<CreatedAtRouteResult> CreateText(Text text)
        {
            var currentUserId = HttpContext.Request.Headers["CurrentUser"][0];

            text.Id = Guid.NewGuid().ToString();
            text.UserId = currentUserId;

            await _cosmosDbService.AddItemAsync(text);

            _memoryCache.Set($"text-{text.Id}", text, TimeSpan.FromDays(30));
            _memoryCache.Remove($"texts-{currentUserId}");

            return CreatedAtRoute(nameof(GetTextById), new { id = text.Id }, text);
        }

        [HttpPut("{id}", Name = nameof(UpdateText))]
        public async Task<NoContentResult> UpdateText(string id, Text text)
        {
            await _cosmosDbService.UpdateItemAsync(id, text);

            _memoryCache.Set($"text-{id}", text, TimeSpan.FromDays(30));

            return NoContent();
        }

        [HttpDelete("{id}", Name = nameof(DeleteText))]
        public async Task<NoContentResult> DeleteText(string id)
        {
            var currentUserId = HttpContext.Request.Headers["CurrentUser"][0];

            await _cosmosDbService.DeleteItemAsync(id);

            _memoryCache.Remove($"text-{id}");
            _memoryCache.Remove($"texts-{currentUserId}");

            return NoContent();
        }
    }
}
