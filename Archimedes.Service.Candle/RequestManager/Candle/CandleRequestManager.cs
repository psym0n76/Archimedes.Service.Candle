using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Extensions;
using Archimedes.Library.Message;
using Archimedes.Service.Candle.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Archimedes.Library.Message.Dto;
using Archimedes.Library.RabbitMq;

namespace Archimedes.Service.Candle
{
    public class CandleRequestManager : ICandleRequestManager
    {
        //validation https://lostechies.com/jimmybogard/2007/10/24/entity-validation-with-visitors-and-extension-methods/
        private readonly Config _config;
        private readonly ILogger<CandleRequestManager> _logger;
        private readonly IMarketClient _markets;
        private readonly IProducer<CandleMessage> _producer;

        public CandleRequestManager(IOptions<Config> config, ILogger<CandleRequestManager> logger, IMarketClient markets, IProducer<CandleMessage> producer)
        {
            _config = config.Value;
            _logger = logger;
            _markets = markets;
            _producer = producer;
        }

        public async Task SendRequestAsync(string granularity)
        {
            var markets = await  _markets.GetMarketAsync(new CancellationToken());

            if (markets == null || !markets.Any())
            {
                _logger.LogWarning($"No Active Markets returned");
                return;
            }

            foreach (var market in markets)
            {
                if (market.Active && market.TimeFrameInterval == granularity)
                {
                    SendToQueue(market);
                }
            }
        }
        private void SendToQueue(MarketDto market)
        {
            var message = new CandleMessage
            {
                StartDate = market.MaxDate,
                EndDate = DateTime.Now.RoundDownTime(market.Interval),
                Market = market.Name,
                TimeFrame = market.TimeFrame,
                Interval = market.Interval,
                MaxIntervals = _config.MaxIntervalCandles
            };

            message.CountCandleIntervals();
            message.CalculateDateRanges();

            var requestMessage = string.Empty;

            foreach (var range in message.DateRanges)
            {
                message.StartDate = range.StartDate;
                message.EndDate = range.EndDate;

                requestMessage += $"{message}\n";

                _producer.PublishMessage(message, nameof(message));
            }

            _logger.LogInformation($"Candle Request created and published to Queue: {requestMessage}");
        }
    }
}