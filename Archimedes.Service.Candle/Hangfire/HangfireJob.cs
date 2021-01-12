using System;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Logger;
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
        private readonly BatchLog _batchLog = new BatchLog();
        private string _logId;

        public HangfireJob(ILogger<HangfireJob> log, ICandleRequestManager candle, IPriceRequestManager price)
        {
            _logger = log;
            _candle = candle;
            _price = price;
        }

        public void RunJob()
        {
            _logId = _batchLog.Start();
            _batchLog.Update(_logId,"Job started info");

            const string cronMinutely = "0/1 * * * MON,TUE,WED,THU,FRI";
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

                RecurringJob.AddOrUpdate("Job: 0min Candle Request",
                    () => _price.SendRequestAsync("0Min"),
                    cronMinutelyFiveWorkingWeek);

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



                _batchLog.Update(_logId, "Waiting 3 secs to start background Job");


                Thread.Sleep(5000);

                if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                {
                    _logger.LogWarning(_batchLog.Print(_logId, $"WARNING WEEKEND {DateTime.Now.DayOfWeek} NOT running instant requests"));
                    return;
                }

                //BackgroundJob.Enqueue(() => _price.SendRequestAsync("0Min"));

                // this are run as soon as the system is up and running
                BackgroundJob.Enqueue(() => _candle.SendRequestAsync("15Min"));
                _batchLog.Update(_logId, "Starting 15Min Job");


                Thread.Sleep(5000);
                BackgroundJob.Enqueue(() => _candle.SendRequestAsync("5Min"));
                _batchLog.Update(_logId, "Starting 5Min Job");


                Thread.Sleep(5000);
                BackgroundJob.Enqueue(() => _candle.SendRequestAsync("1D"));
                _batchLog.Update(_logId, "Starting 1D Job");

                
                Thread.Sleep(5000);
                BackgroundJob.Enqueue(() => _candle.SendRequestAsync("1H"));
                _batchLog.Update(_logId, "Starting 1H Job");


                Thread.Sleep(5000);
                BackgroundJob.Enqueue(() => _candle.SendRequestAsync("4H"));
                _batchLog.Update(_logId, "Starting 4H Job");

                _logger.LogInformation(_batchLog.Print(_logId));

            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(_logId, $"Error returned from HangfireJob", e));
            }
        }
    }
}