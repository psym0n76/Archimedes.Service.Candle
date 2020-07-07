using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Message.Dto;
using Archimedes.Service.Candle.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Candle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketController : ControllerBase
    {
        private readonly Config _config;
        private readonly IMarketClient _client;
        private readonly ILogger<CandleController> _logger;

        public MarketController(IOptions<Config> config, IMarketClient client, ILogger<CandleController> logger)
        {
            _client = client;
            _logger = logger;
            _config = config.Value;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MarketDto>>> GetMarketAsync(CancellationToken ct)
        {
            var data = await _client.GetMarketAsync(ct);
            return Ok(data);
        }
    }
}