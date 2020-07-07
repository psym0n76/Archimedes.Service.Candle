using System.Threading.Tasks;

namespace Archimedes.Service.Candle.Publishers
{
    public interface INetQPublish<T> where T : class
    {
         Task PublishMessage(T message);
    }
}