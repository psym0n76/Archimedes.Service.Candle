using System;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Archimedes.Service.Candle
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Info("Initialise Main");
                logger.Info("Initialise Main - Pausing to waiting 30 Sec(s) for HangFire DB to be setup");
                //Thread.Sleep(30000);
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                logger.Error("Stopped program because of exception");
                logger.Error(e.Message);
                logger.Error($"\n\n{e.StackTrace}");
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventLog();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).UseNLog();

    }
}