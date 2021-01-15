using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Logger;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Archimedes.Library.RabbitMq;
using Archimedes.Service.Candle.Http;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Price
{
    public class PriceRequestManager : IPriceRequestManager
    {
        private readonly ILogger<PriceRequestManager> _logger;
        private readonly IProducer<PriceMessage> _producer;
        private readonly IMarketClient _markets;
        private readonly BatchLog _batchLog = new BatchLog();
        private string _logId;

        public PriceRequestManager(ILogger<PriceRequestManager> logger, IProducer<PriceMessage> producer,
            IMarketClient markets)
        {
            _logger = logger;
            _producer = producer;
            _markets = markets;
        }

        public async Task SendRequestAsync(string granularity)
        {
            _logId = _batchLog.Start();
            _batchLog.Update(_logId,$"PriceRequestManager sent for {granularity}");
            
            var markets = await _markets.GetMarketAsync(new CancellationToken());

            if (!markets.Any())
            {
                _logger.LogError(_batchLog.Print(_logId, "Markets not returned from Table"));
                return;
            }

            foreach (var market in markets)
            {
                if (!market.Active || market.TimeFrameInterval != granularity) continue;
                
                _batchLog.Update(_logId, $"Publish to PriceRequestQueue {market.Name} {granularity}");
                SendToQueue(market);
            }
            
            _logger.LogInformation(_batchLog.Print(_logId));
        }

        private void SendToQueue(MarketDto market)
        {
            var request = new PriceMessage()
            {
                Id = Guid.NewGuid().ToString(),
                Market = market.Name
            };

            _producer.PublishMessage(request, "PriceRequestQueue");
            _batchLog.Update(_logId,$"Published to PriceRequestQueue");
        }
    }
}