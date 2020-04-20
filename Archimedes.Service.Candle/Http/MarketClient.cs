using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Archimedes.Service.Candle.Http
{
    public class MarketClient : IMarketClient
    {
        private readonly HttpClient _client;
        private const string RequestUri = "api/market";

        public MarketClient(HttpClient httpClient, IOptions<Config> config)
        {
            httpClient.BaseAddress = new Uri($"{config.Value.ApiRepositoryUrl}");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
        }

        public async Task<IEnumerable<MarketDto>> Get()
        {
            var response = await _client.GetAsync(RequestUri);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var market = JsonConvert.DeserializeObject<IEnumerable<MarketDto>>(responseJson);

            return market;
        }
    }
}