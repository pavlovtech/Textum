using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TextumReader.Services.Dictionary.Models;
using TextumReader.Services.Dictionary.Services;

namespace TextumReader.Services.Dictionary.Controllers
{
    [ApiController]
    [Route("dictionaries")]
    public class DictionariesController : ControllerBase
    {
        private readonly DictionariesService _dictService;

        public DictionariesController(DictionariesService dictService)
        {
            _dictService = dictService;
        }

        [HttpGet]
        public ActionResult<List<WordsDictionary>> Get() => _dictService.Get();

        [HttpGet("{id:length(24)}", Name = "dictionaries")]
        public ActionResult<WordsDictionary> Get(string id)
        {
            var dict = _dictService.Get(id);

            if (dict == null)
            {
                return NotFound();
            }

            return dict;
        }

        [HttpPost]
        public ActionResult<WordsDictionary> Create(WordsDictionary dict)
        {
            _dictService.Create(dict);

            return CreatedAtRoute("dictionaries", new { id = dict.Id }, dict);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, WordsDictionary text)
        {
            var dict = _dictService.Get(id);

            if (dict == null)
            {
                return NotFound();
            }

            _dictService.Update(id, text);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var dict = _dictService.Get(id);

            if (dict == null)
            {
                return NotFound();
            }

            _dictService.Remove(dict.Id);

            return NoContent();
        }
    }
}
