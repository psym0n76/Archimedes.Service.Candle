using System.Threading.Tasks;

namespace Archimedes.Service.Candle
{
    public interface ICandleRequestManager
    {
        Task SendRequestAsync(string granularity);
    }
}