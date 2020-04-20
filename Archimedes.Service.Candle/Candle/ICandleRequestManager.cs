namespace Archimedes.Service.Candle
{
    public interface ICandleRequestManager
    {
        void SendRequest(string queueName);
    }
}