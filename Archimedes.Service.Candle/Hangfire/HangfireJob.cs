using Hangfire;
using Archimedes.Library.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Candle
{
    public class HangfireJob : IHangfireJob
    {
        private readonly Config _config;
        private readonly ILogger<HangfireJob> _logger;
        private readonly ICandleRequestManager _candle;

        public HangfireJob(IOptions<Config> config, ILogger<HangfireJob> log, ICandleRequestManager candle)
        {
            _config = config.Value;
            _logger = log;
            _candle = candle;
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
                    () => _candle.SendRequest("1min"),
                    cronMinutely);

                RecurringJob.AddOrUpdate("Job: 5min Request",
                    () => _candle.SendRequest("5min"),
                    cronMinutelyFive);

                RecurringJob.AddOrUpdate("Job: 3min Request",
                    () => _candle.SendRequest("3min"),
                    cronMinutelyThree);
            }
            finally
            {
                _logger.LogError("Critical Error");
            }
        }
    }
}