using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TextumReader.Services.TextMaterial.Models;
using TextumReader.Services.TextMaterial.Services;

namespace TextumReader.Services.TextMaterial.Controllers
{
    [Route("texts")]
    [ApiController]
    public class TextsController : ControllerBase
    {
        private readonly TextsService _textService;

        public TextsController(TextsService textService)
        {
            _textService = textService;
        }

        [HttpGet(Name = nameof(GetTexts))]
        public ActionResult<List<Text>> GetTexts()
        {
            var currentUserId = HttpContext.Request.Headers["CurrentUser"][0];

            return _textService.GetByUserId(currentUserId);
        }

        [HttpGet("{id:length(24)}", Name = nameof(GetTextById))]
        public ActionResult<Text> GetTextById(string id)
        {
            var book = _textService.GetByBookId(id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }

        [HttpPost(Name = nameof(CreateText))]
        public ActionResult<Text> CreateText(Text text)
        {
            var currentUserId = HttpContext.Request.Headers["CurrentUser"][0];

            text.UserId = currentUserId;
            _textService.Create(text);

            return CreatedAtRoute(nameof(GetTextById), new { id = text.Id }, text);
        }

        [HttpPut("{id:length(24)}", Name = nameof(UpdateText))]
        public IActionResult UpdateText(string id, Text text)
        {
            var book = _textService.GetByBookId(id);

            if (book == null)
            {
                return NotFound();
            }

            _textService.Update(id, text);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}", Name = nameof(DeleteText))]
        public IActionResult DeleteText(string id)
        {
            var book = _textService.GetByBookId(id);

            if (book == null)
            {
                return NotFound();
            }

            _textService.Remove(book.Id);

            return NoContent();
        }
    }
}
