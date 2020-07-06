using System;
using Archimedes.Service.Price;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Candle
{
    public class HangfireJob : IHangfireJob
    {
        private readonly ILogger<HangfireJob> _logger;
        private readonly ICandleRequestManager _candle;
        private readonly IPriceRequestManager _price;

        public HangfireJob(ILogger<HangfireJob> log, ICandleRequestManager candle, IPriceRequestManager price)
        {
            _logger = log;
            _candle = candle;
            _price = price;
        }

        public void RunJob()
        {
            _logger.LogInformation("Job started info: ");

            const string cronMinutely = "0/1 * * * *";
            const string cronMinutelyFive = "0/5 * * * *";
            const string cronMinutelyThree = "0/3 * * * *";

            try
            {
                // var crom = Hangfire.Cron.Hourly(1);
                // https://github.com/HangfireIO/Hangfire/issues/1365 cron running from a set time 

                RecurringJob.AddOrUpdate("Job: 1min Request",
                    () => _candle.SendRequestAsync("1min"),
                    cronMinutely);

                RecurringJob.AddOrUpdate("Job: 5min Request",
                    () => _candle.SendRequestAsync("5min"),
                    cronMinutelyFive);

                RecurringJob.AddOrUpdate("Job: 3min Request",
                    () => _candle.SendRequestAsync("3min"),
                    cronMinutelyThree);

                //test
                RecurringJob.AddOrUpdate("Job: 1Min Price Request",
                    () => _price.SendToQueueAsync(), cronMinutely);

            }
            catch (Exception e)
            {
                _logger.LogError($"Critical Error {e.Message} {e.StackTrace}");
            }
        }
    }
}