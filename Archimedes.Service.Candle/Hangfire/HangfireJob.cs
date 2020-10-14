using System;
using System.Threading;
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
            const string cronMinutelyFifteenWorking = "0/15 * * * MON,TUE,WED,THU,FRI";
            const string cronMinutelyFiveWorkingWeek = "0/5 * * * MON,TUE,WED,THU,FRI";
            const string cronHourlyOneWorkingWeek = "0 0/1 * * MON,TUE,WED,THU,FRI";
            const string cronHourlyFourWorkingWeek = "0 0/4 * * MON,TUE,WED,THU,FRI";
            const string cronDailyWorkingWeek = "0 0 ? * MON,TUE,WED,THU,FRI";

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
                    cronMinutelyFiveWorkingWeek);

                RecurringJob.AddOrUpdate("Job: 15min Candle Request",
                    () => _candle.SendRequestAsync("15Min"),
                    cronMinutelyFifteenWorking);

                RecurringJob.AddOrUpdate("Job: 1H Candle Request",
                    () => _candle.SendRequestAsync("1H"),
                    cronHourlyOneWorkingWeek);

                RecurringJob.AddOrUpdate("Job: 4H Candle Request",
                    () => _candle.SendRequestAsync("4H"),
                    cronHourlyFourWorkingWeek);

                RecurringJob.AddOrUpdate("Job: 1D Candle Request",
                    () => _candle.SendRequestAsync("1D"),
                    cronDailyWorkingWeek);


                _logger.LogInformation("Waiting 3 secs to start background Job");
                Thread.Sleep(3000);

                BackgroundJob.Enqueue(() => _price.SendRequest());

                // this are run as soon as the systme is up and running
                BackgroundJob.Enqueue(()=> _candle.SendRequestAsync("1D"));
                BackgroundJob.Enqueue(()=> _candle.SendRequestAsync("15Min"));
                BackgroundJob.Enqueue(()=> _candle.SendRequestAsync("1H"));
                BackgroundJob.Enqueue(()=> _candle.SendRequestAsync("4H"));
                
            }
            catch (Exception e)
            {
                _logger.LogError($"Critical Error {e.Message} {e.StackTrace}");
            }
        }
    }
}