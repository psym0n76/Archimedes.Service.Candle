using System.Collections.Generic;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Archimedes.Service.Candle;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Price
{
    public class PriceRequestManager : IPriceRequestManager
    {
        //validation https://lostechies.com/jimmybogard/2007/10/24/entity-validation-with-visitors-and-extension-methods/
        private readonly Config _config;
        private readonly ILogger<HangfireJob> _logger;

        public PriceRequestManager(IOptions<Config> config, ILogger<HangfireJob> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public async Task SendToQueueAsync()
        {
            var request = new RequestPrice()
            {
                Status = "Status Text",
                Properties = new List<string>(),
                Text = "Test Text"
            };

            using var bus = RabbitHutch.CreateBus($"host={_config.RabbitHutchConnection}");
            await bus.PublishAsync(request);

            _logger.LogInformation($"Sending request to rabbit: {request}");
        }
    }
}