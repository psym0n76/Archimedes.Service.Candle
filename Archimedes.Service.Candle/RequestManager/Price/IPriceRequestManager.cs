namespace Archimedes.Service.Price
{
    public interface IPriceRequestManager
    {
        void SendRequestAsync(string granularity);
    }
}