using System.Threading.Tasks;

namespace Archimedes.Service.Price
{
    public interface IPriceRequestManager
    {
        Task SendRequestAsync(string granularity);
    }
}