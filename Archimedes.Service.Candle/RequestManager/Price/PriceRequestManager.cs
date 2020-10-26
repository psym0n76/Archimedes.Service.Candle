using System.Linq;
using System.Threading;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Archimedes.Library.RabbitMq;
using Archimedes.Service.Candle.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Price
{
    public class PriceRequestManager : IPriceRequestManager
    {
        private readonly ILogger<PriceRequestManager> _logger;
        private readonly Config _config;
        private readonly IProducer<PriceMessage> _producer;
        private readonly IMarketClient _markets;

        public PriceRequestManager(ILogger<PriceRequestManager> logger, IProducer<PriceMessage> producer,
            IMarketClient markets, IOptions<Config> config)
        {
            _logger = logger;
            _producer = producer;
            _markets = markets;
            _config = config.Value;
        }

        public async void SendRequestAsync(string granularity)
        {
            var markets = await _markets.GetMarketAsync(new CancellationToken());

            if (markets == null || !markets.Any())
            {
                _logger.LogWarning($"Markets not FOUND");
                return;
            }

            foreach (var market in markets)
            {
                if (market.Active && market.TimeFrameInterval == granularity) //0M
                {
                    SendToQueue(market);
                }
            }
        }

        private void SendToQueue(MarketDto market)
        {
            var request = new PriceMessage()
            {
                Market = market.Name
            };

            _producer.PublishMessage(request, "PriceRequestQueue");
            _logger.LogInformation($"Published to PriceRequestQueue: {request}");

        }
    }
}