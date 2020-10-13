using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Extensions;
using Archimedes.Library.Message.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Candle.Http
{
    public class MarketClient : IMarketClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<MarketClient> _logger;

        public MarketClient(HttpClient httpClient, IOptions<Config> config, ILogger<MarketClient> logger)
        {
            httpClient.BaseAddress = new Uri($"{config.Value.ApiRepositoryUrl}");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
            _logger = logger;
        }

        public async Task<IList<MarketDto>> GetMarketAsync(CancellationToken ct = default)
        {
            var response = await _client.GetAsync($"market", ct);

            if (response.IsSuccessStatusCode)
            {
                var markets = await response.Content.ReadAsAsync<IList<MarketDto>>();

               _logger.LogInformation($"Received {markets.Count} Market records");

                return markets;
            }

            _logger.LogWarning($"Failed to Get {response.ReasonPhrase} from {_client.BaseAddress}/market");
            return Array.Empty<MarketDto>();
        }
    }
}