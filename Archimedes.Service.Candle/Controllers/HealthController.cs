using System;
using Microsoft.AspNetCore.Mvc;
using Archimedes.Library.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Candle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly Config _config;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IOptions<Config> config, ILogger<HealthController> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Get()
        {
            try
            {
                _logger.LogInformation($"{_config.ApplicationName} Version: {_config.AppVersion}");
                return Ok($"{_config.ApplicationName} Version: {_config.AppVersion}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error {e.Message} {e.StackTrace}");
                return BadRequest();
            }
        }
    }
}