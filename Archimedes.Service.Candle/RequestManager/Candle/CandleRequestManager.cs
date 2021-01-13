using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Extensions;
using Archimedes.Library.Logger;
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
        private readonly BatchLog _batchLog = new BatchLog();
        private string _logId;

        public CandleRequestManager(IOptions<Config> config, ILogger<CandleRequestManager> logger,
            IMarketClient markets, IProducer<CandleMessage> producer)
        {
            _config = config.Value;
            _logger = logger;
            _markets = markets;
            _producer = producer;
        }

        public async Task SendRequestAsync(string granularity)
        {
            try
            {
                _logId = _batchLog.Start();
                await SendRequest(granularity);
                _logger.LogInformation(_batchLog.Print(_logId));

            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print($"{_logId}","Error returned from CandleRequestManager",e));
            }
        }

        private async Task SendRequest(string granularity)
        {
            var markets = await _markets.GetMarketAsync(new CancellationToken());

            if (!markets.Any())
            {
                _logger.LogWarning(_batchLog.Print(_logId, $"Markets not FOUND"));
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
            var timeInterval = market.TimeFrame == "Min" ? market.BrokerTimeMinInterval : market.BrokerTimeInterval;

            _batchLog.Update(_logId, $"Publish {market.Name} {market.Granularity}");

            var message = new CandleMessage
            {
                StartDate = market.MaxDate,
                EndDate = DateTime.Now.RoundDownTime(market.Interval.ToMinutes(market.TimeFrame)),
                Market = market.Name,
                TimeFrame = market.TimeFrame,
                TimeFrameBroker = timeInterval,
                Interval = market.Interval,
                MaxIntervals = _config.MaxIntervalCandles,
                MarketId = market.Id,
                ElapsedTime = DateTime.Now,
                ExternalMarketId = market.ExternalMarketId
            };

            message.CountCandleIntervals();
            message.CalculateDateRanges();

            foreach (var range in message.DateRanges)
            {
                message.StartDate = range.StartDate;
                message.EndDate = range.EndDate;

                if (message.StartDate > message.EndDate)
                {
                    _batchLog.Update(_logId, $"Published to CandleRequestQueue: WARNING Start > End {message.StartDate} {message.EndDate}");
                    break;
                }

                message.CountCandleIntervals();
                _producer.PublishMessage(message, "CandleRequestQueue");

                _batchLog.Update(_logId, $"Published to CandleRequestQueue: {message.Market} {message.TimeFrame} {message.StartDate} {message.EndDate}");

                if (message.DateRanges.Count > 1)
                {
                    _batchLog.Update(_logId, $"Published to CandleRequestQueue: Waiting 1 secs before sending next: {message.DateRanges.Count}");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}