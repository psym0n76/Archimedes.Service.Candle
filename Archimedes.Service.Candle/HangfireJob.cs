using Archimedes.Library.Message;
using EasyNetQ;
using Hangfire;
using System.Collections.Generic;
using Archimedes.Library.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Candle
{
    public class HangfireJob : IHangfireJob
    {
        private readonly Config  _config;
        private readonly ILogger<HangfireJob> _logger;

        public HangfireJob(IOptions<Config> config, ILogger<HangfireJob> log)
        {
            _config = config.Value;
            _logger = log;
        }

        public void RunJob()
        {
            _logger.LogInformation("Job started info: ");

            const string cronMinutely = "0/1 * * * *";
            const string cronMinutelyFive = "0/5 * * * *";
            const string cronMinutelyThree = "0/3 * * * *";

            // var crom = Hangfire.Cron.Hourly(1);
            // https://github.com/HangfireIO/Hangfire/issues/1365 cron running from a set time 

            RecurringJob.AddOrUpdate("job1",
                () => SendCandleRequest("1min"),
                cronMinutely);

            RecurringJob.AddOrUpdate("job2",
                () => SendCandleRequest("5min"),
                cronMinutelyFive);

            RecurringJob.AddOrUpdate("job3",
                () => SendCandleRequest("3min"),
                cronMinutelyThree);
        }


        public void SendCandleRequest(string queueName)
        {
            var request = new RequestCandle()
            {
                Properties = new List<string>(),
                Status = "status",
                Text = queueName
            };

            _logger.LogInformation("Sending request to rabbit queue: " + queueName);

            using (var bus = RabbitHutch.CreateBus($"host={_config.RabbitHutchConnection}"))
            {
                bus.Publish(request);
            }
        }
    }
}