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

        [HttpGet]
        public ActionResult<List<Text>> GetByUserId()
        {
            var currentUserId = HttpContext.Request.Headers["CurrentUser"][0];

            return _textService.GetByUserId(currentUserId);
        }

        [HttpGet("{id:length(24)}", Name = "texts")]
        public ActionResult<Text> GetByBookId(string id)
        {
            var book = _textService.GetByBookId(id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }

        [HttpPost]
        public ActionResult<Text> Create(Text book)
        {
            _textService.Create(book);

            return CreatedAtRoute("texts", new { id = book.Id }, book);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Text text)
        {
            var book = _textService.GetByBookId(id);

            if (book == null)
            {
                return NotFound();
            }

            _textService.Update(id, text);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
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
