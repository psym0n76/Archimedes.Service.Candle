﻿using System;
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

        public CandleRequestManager(IOptions<Config> config, ILogger<CandleRequestManager> logger, IMarketClient markets, IProducer<CandleMessage> producer)
        {
            _config = config.Value;
            _logger = logger;
            _markets = markets;
            _producer = producer;
        }

        public async Task SendRequestAsync(string granularity)
        {
            _logId = _batchLog.Start();
            
            var markets = await  _markets.GetMarketAsync(new CancellationToken());

            if (!markets.Any())
            {
                _logger.LogWarning(_batchLog.Print(_logId,$"Markets not FOUND"));
                return;
            }

            foreach (var market in markets)
            {
                if (market.Active && market.TimeFrameInterval == granularity)
                {
                    SendToQueue(market);
                }
            }

            _logger.LogInformation(_batchLog.Print(_logId));
        }
        private void SendToQueue(MarketDto market)
        {
            var timeInterval = market.TimeFrame == "Min" ? market.BrokerTimeMinInterval : market.BrokerTimeInterval;

            _batchLog.Update(_logId,$"Publish {market.Name} {market.Granularity}");
            
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
                message.CountCandleIntervals();
                _producer.PublishMessage(message, "CandleRequestQueue");
                
                _batchLog.Update(_logId, $"Published to CandleRequestQueue: {message.Market} {message.TimeFrame} {message.StartDate} {message.EndDate}");

                if (message.DateRanges.Count > 1)
                {
                    _batchLog.Update(_logId, $"Published to CandleRequestQueue: Waiting 2 secs before sending next: {message.DateRanges.Count}");
                    Thread.Sleep(2000);
                }
            }
        }
    }
}