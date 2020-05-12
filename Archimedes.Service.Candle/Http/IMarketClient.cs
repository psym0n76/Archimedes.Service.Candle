using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Archimedes.Service.Candle.Http
{
    public interface IMarketClient
    {
        Task<IEnumerable<MarketDto>> GetMarketAsync(CancellationToken ct);
    }
}
