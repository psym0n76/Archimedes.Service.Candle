using System.Collections.Generic;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.EasyNetQ;
using Archimedes.Library.Message;
using Archimedes.Service.Candle;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Price
{
    public class PriceRequestManager : IPriceRequestManager
    {
        //validation https://lostechies.com/jimmybogard/2007/10/24/entity-validation-with-visitors-and-extension-methods/
        private readonly ILogger<HangfireJob> _logger;
        private readonly INetQPublish<RequestPrice> _publish;

        public PriceRequestManager(IOptions<Config> config, ILogger<HangfireJob> logger,
            INetQPublish<RequestPrice> publish)
        {
            _logger = logger;
            _publish = publish;
        }

        public async Task SendToQueueAsync()
        {
            var request = new RequestPrice()
            {
                Status = "Status Text",
                Properties = new List<string>(),
                Text = "Test Text"
            };

            await _publish.PublishMessage(request);

            _logger.LogInformation($"Sending request to rabbit: {request}");
        }
    }
}