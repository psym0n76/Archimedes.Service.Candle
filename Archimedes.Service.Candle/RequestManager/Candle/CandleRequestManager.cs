using System;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Archimedes.Service.Candle.Http;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Candle
{
    public class CandleRequestManager : ICandleRequestManager
    {
        //validation https://lostechies.com/jimmybogard/2007/10/24/entity-validation-with-visitors-and-extension-methods/
        private readonly Config _config;
        private readonly ILogger<HangfireJob> _logger;
        private readonly IMarketClient _markets;

        public CandleRequestManager(IOptions<Config> config, ILogger<HangfireJob> logger, IMarketClient markets)
        {
            _config = config.Value;
            _logger = logger;
            _markets = markets;
        }

        public async Task SendRequestAsync(string granularity)
        {
            var markets = await  _markets.GetMarketAsync(new CancellationToken());

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
            var endDate = DateTime.Now.RoundDownTime(market.Interval);

            var request = new RequestCandle(market.MaxDate, endDate, _config.MaxIntervalCandles)
            {
                Market = market.Name,
                TimeFrame = market.TimeFrame,
                Interval = market.Interval,
            };

            foreach (var range in request.DateRanges)
            {
                request.StartDate = range.StartDate;
                request.EndDate = range.EndDate;

                using var bus = RabbitHutch.CreateBus($"{_config.RabbitHutchConnection}");
                bus.Publish(request);
            }

            _logger.LogInformation($"Sending request to rabbit: {request}");
        }
    }
}