using System.Collections.Generic;
using Archimedes.Library.Message;
using Archimedes.Library.RabbitMq;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Price
{
    public class PriceRequestManager : IPriceRequestManager
    {
        private readonly ILogger<PriceRequestManager> _logger;
        private readonly IProducer<PriceMessage> _producer;

        public PriceRequestManager(ILogger<PriceRequestManager> logger, IProducer<PriceMessage> producer)
        {
            _logger = logger;
            _producer = producer;
        }

        public void SendRequest()
        {
            var request = new PriceMessage()
            {
                Properties = new List<string>(),
                Text = "Test Text"
            };

            _logger.LogInformation($"Price Request created and published to Queue: {request}");

            _producer.PublishMessage(request, "PriceRequestQueue");

        }
    }
}