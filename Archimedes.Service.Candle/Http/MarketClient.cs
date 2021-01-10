using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Extensions;
using Archimedes.Library.Logger;
using Archimedes.Library.Message.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Candle.Http
{
    public class MarketClient : IMarketClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<MarketClient> _logger;
        private readonly BatchLog _batchLog = new BatchLog();
        private string _logId;

        public MarketClient(HttpClient httpClient, IOptions<Config> config, ILogger<MarketClient> logger)
        {
            httpClient.BaseAddress = new Uri($"{config.Value.ApiRepositoryUrl}");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
            _logger = logger;
        }

        public async Task<List<MarketDto>> GetMarketAsync(CancellationToken ct = default)
        {

            try
            {
                _logId = _batchLog.Start();
                _batchLog.Update(_logId, "GET GetMarketAsync");
                
                var response = await _client.GetAsync($"market", ct);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.RequestMessage != null)

                        _logger.LogWarning(
                            _batchLog.Print(_logId,
                                $"GET Failed: {response.ReasonPhrase} from {response.RequestMessage.RequestUri}"));

                    return new List<MarketDto>();
                }

                var markets = await response.Content.ReadAsAsync<List<MarketDto>>();
                _logger.LogInformation(_batchLog.Print(_logId, $"Returned {markets.Count} Market(s)"));

                return markets;

            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(_logId, $"Error returned from MessageClient", e));
                return new List<MarketDto>();
            }
        }
    }
}