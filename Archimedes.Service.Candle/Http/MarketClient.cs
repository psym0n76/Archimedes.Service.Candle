using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Extensions;
using Archimedes.Library.Message.Dto;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Candle.Http
{
    public class MarketClient : IMarketClient
    {
        private readonly HttpClient _client;
        private const string RequestUri = "market";
        private readonly Config _config;

        public MarketClient(HttpClient httpClient, IOptions<Config> config)
        {
            httpClient.BaseAddress = new Uri($"{config.Value.ApiRepositoryUrl}");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
            _config = config.Value;
        }

        public async Task<IEnumerable<MarketDto>> GetMarketAsync(CancellationToken ct = default)
        {
            //var request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            //var response = await _client.SendAsync(request, ct);
            var response = await _client.GetAsync($"{_config.ApiRepositoryUrl}/market",ct);

            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<MarketDto>();
            }

            return await response.Content.ReadAsAsync<IEnumerable<MarketDto>>();
        }
    }
}