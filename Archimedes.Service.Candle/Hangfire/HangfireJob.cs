using System;
using System.Threading;
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
        const string CronMinutely = "0/1 * * * MON,TUE,WED,THU,FRI";
        const string CronMinutelyFifteenWorking = "0/15 * * * MON,TUE,WED,THU,FRI";
        const string CronMinutelyFiveWorkingWeek = "0/5 * * * MON,TUE,WED,THU,FRI";
        const string CronHourlyOneWorkingWeek = "0 0/1 * * MON,TUE,WED,THU,FRI";
        const string CronHourlyFourWorkingWeek = "0 0/4 * * MON,TUE,WED,THU,FRI";
        const string CronDailyWorkingWeek = "0 0 ? * MON,TUE,WED,THU,FRI";

        public HangfireJob(ILogger<HangfireJob> log, ICandleRequestManager candle, IPriceRequestManager price)
        {
            _logger = log;
            _candle = candle;
            _price = price;
        }

        public void RunJob()
        {
            _logId = _batchLog.Start();
            _batchLog.Update(_logId, "HangfireJob Started");

            try
            {
                CandleHistoryRunner();
                CandleSchedule();
                _logger.LogInformation(_batchLog.Print(_logId));
            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(_logId, "Error returned from Hangfire Job", e));
            }
        }

        private void CandleSchedule()
        {
            _batchLog.Update(_logId, "CandleSchedule");

            // https://github.com/HangfireIO/Hangfire/issues/1365 cron running from a set time 

            RecurringJob.RemoveIfExists("Job: 1min Candle Request");
            RecurringJob.AddOrUpdate("Job: 1min Candle Request",
                () => _candle.SendRequestAsync("1Min"),
                CronMinutely);
            _batchLog.Update(_logId, "Job Scheduled 1Min");

            RecurringJob.RemoveIfExists("Job: 0min Candle Request");
            RecurringJob.AddOrUpdate("Job: 0min Candle Request",
                () => _price.SendRequestAsync("0Min"),
                CronMinutely);
            _batchLog.Update(_logId, "Job Scheduled 0Min");

            RecurringJob.RemoveIfExists("Job: 5min Candle Request");
            RecurringJob.AddOrUpdate("Job: 5min Candle Request",
                () => _candle.SendRequestAsync("5Min"),
                CronMinutelyFiveWorkingWeek);
            _batchLog.Update(_logId, "Job Scheduled 5Min");

            RecurringJob.RemoveIfExists("Job: 15min Candle Request");
            RecurringJob.AddOrUpdate("Job: 15min Candle Request",
                () => _candle.SendRequestAsync("15Min"),
                CronMinutelyFifteenWorking);
            _batchLog.Update(_logId, "Job Scheduled 15Min");

            RecurringJob.RemoveIfExists("Job: 1H Candle Request");
            RecurringJob.AddOrUpdate("Job: 1H Candle Request",
                () => _candle.SendRequestAsync("1H"),
                CronHourlyOneWorkingWeek);
            _batchLog.Update(_logId, "Job Scheduled 1H");

            RecurringJob.RemoveIfExists("Job: 4H Candle Request");
            RecurringJob.AddOrUpdate("Job: 4H Candle Request",
                () => _candle.SendRequestAsync("4H"),
                CronHourlyFourWorkingWeek);
            _batchLog.Update(_logId, "Job Scheduled 4H");

            RecurringJob.RemoveIfExists("Job: 1D Candle Request");
            RecurringJob.AddOrUpdate("Job: 1D Candle Request",
                () => _candle.SendRequestAsync("1D"),
                CronDailyWorkingWeek);
            _batchLog.Update(_logId, "Job Scheduled 1D");
        }


        private void CandleHistoryRunner()
        {
            _batchLog.Update(_logId, "CandleHistoryRunner");

            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
            {
                _logger.LogWarning(_batchLog.Print(_logId,
                    $"WARNING WEEKEND {DateTime.Now.DayOfWeek} NOT running instant requests"));
                return;
            }
            //BackgroundJob.Enqueue(() => _price.SendRequestAsync("0Min"));

            _candle.SendRequestAsync("15Min");
            _batchLog.Update(_logId, "Starting 15Min Job");
            Thread.Sleep(50);


            _candle.SendRequestAsync("5Min");
            _batchLog.Update(_logId, "Starting 5Min Job");
            Thread.Sleep(50);


            _candle.SendRequestAsync("1D");
            _batchLog.Update(_logId, "Starting 1D Job");
            Thread.Sleep(50);


            _candle.SendRequestAsync("1H");
            _batchLog.Update(_logId, "Starting 1H Job");
            Thread.Sleep(50);


            _candle.SendRequestAsync("4H");
            _batchLog.Update(_logId, "Starting 4H Job");

        }
    }
}