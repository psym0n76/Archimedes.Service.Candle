using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Archimedes.Service.Candle.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Archimedes.Library.EasyNetQ;
using Archimedes.Library.Message.Dto;

namespace Archimedes.Service.Candle
{
    public class CandleRequestManager : ICandleRequestManager
    {
        //validation https://lostechies.com/jimmybogard/2007/10/24/entity-validation-with-visitors-and-extension-methods/
        private readonly Config _config;
        private readonly ILogger<CandleRequestManager> _logger;
        private readonly IMarketClient _markets;
        private readonly INetQPublish<RequestCandle> _publish;

        public CandleRequestManager(IOptions<Config> config, ILogger<CandleRequestManager> logger, IMarketClient markets, INetQPublish<RequestCandle> publish)
        {
            _config = config.Value;
            _logger = logger;
            _markets = markets;
            _publish = publish;
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
                    await SendToQueue(market);
                }
            }
        }
        private async Task SendToQueue(MarketDto market)
        {
            var endDate = DateTime.Now.RoundDownTime(market.Interval);

            var request = new RequestCandle(market.MaxDate, endDate, _config.MaxIntervalCandles)
            {
                Market = market.Name,
                TimeFrame = market.TimeFrame,
                Interval = market.Interval,
            };

            var requestMessage = "";

            foreach (var range in request.DateRanges)
            {
                request.StartDate = range.StartDate;
                request.EndDate = range.EndDate;

                requestMessage += $"{request}\n";

                //create a requestMessgeDto

               await _publish.PublishMessage(request);
            }

            _logger.LogInformation($"Candle Request created and published to Queue: {requestMessage}");

        }
    }
}