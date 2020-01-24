using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Archimedes.Service.Candle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandleController : ControllerBase
    {
        // GET: api/Candle
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new[] { "candle", "candle" };
        }

        // GET: api/Candle/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Candle
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Candle/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
