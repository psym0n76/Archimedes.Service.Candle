using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Candle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandleController : ControllerBase
    {
        private readonly Config _config;

        // GET: api/Candle
        public CandleController(IOptions<Config> config)
        {
            _config = config.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(new[] {"candle", "candle", "version: " + _config.AppVersion});
        }

        [HttpGet("{message}")]
        public IEnumerable<string> Get(string message)
        {
            return new[] {"candle", "candle", "version: " + _config.AppVersion, message};
        }

        [HttpGet("{message}/{message2}")]
        public IEnumerable<string> Get(string message, string message2)
        {
            return new[] {"candle", "candle", "version: " + _config.AppVersion, message, message2};
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
