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
            const string cronMinutelyFifteen = "0/15 * * * *";

            try
            {
                // https://github.com/HangfireIO/Hangfire/issues/1365 cron running from a set time 

                RecurringJob.RemoveIfExists("Job: 1min Candle Request");
                RecurringJob.AddOrUpdate("Job: 1min Candle Request",
                    () => _candle.SendRequestAsync("1Min"),
                    cronMinutely);

                RecurringJob.RemoveIfExists("Job: 5min Candle Request");
                RecurringJob.AddOrUpdate("Job: 5min Candle Request",
                    () => _candle.SendRequestAsync("5Min"),
                    cronMinutelyFive);

                RecurringJob.AddOrUpdate("Job: 15min Candle Request",
                    () => _candle.SendRequestAsync("15Min"),
                    cronMinutelyFifteen);

                BackgroundJob.Enqueue(() => _price.SendRequest());

            }
            catch (Exception e)
            {
                _logger.LogError($"Critical Error {e.Message} {e.StackTrace}");
            }
        }
    }
}