using System.Threading.Tasks;
using Archimedes.Library.Domain;
using EasyNetQ;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Candle.Publishers
{
    public class NetQPublish<T> : INetQPublish<T> where T : class
    {
        private readonly Config _config;
        
        public NetQPublish(IOptions<Config> config)
        {
            _config = config.Value;
        }

        public async Task PublishMessage(T message)
        {
            using var bus = RabbitHutch.CreateBus(_config.RabbitHutchConnection);
            await bus.PublishAsync(message);
        }
    }
}