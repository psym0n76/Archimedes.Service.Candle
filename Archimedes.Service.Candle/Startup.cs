using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using Archimedes.Library.Domain;
using Archimedes.Library.Hangfire;
using Archimedes.Library.Message;
using Archimedes.Service.Candle.Http;
using Archimedes.Service.Candle.Publishers;
using Archimedes.Service.Price;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Candle
{
    public class Startup
    {
        //https://www.youtube.com/watch?v=oXNslgIXIbQ

        public IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Required to ensure hangfire is setup
            Thread.Sleep(10000);


            services.AddHttpClient<IMarketClient, MarketClient>();
            services.AddScoped<IHangfireJob, HangfireJob>();

            services.AddTransient<INetQPublish<RequestPrice>,NetQPublish<RequestPrice>>();
            services.AddTransient<INetQPublish<RequestCandle>,NetQPublish<RequestCandle>>();

            services.AddTransient<IPriceRequestManager, PriceRequestManager>();
            services.AddTransient<ICandleRequestManager, CandleRequestManager>();
            services.AddLogging();
            services.Configure<Config>(Configuration.GetSection("AppSettings"));
            services.AddSingleton(Configuration);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            var config = Configuration.GetSection("AppSettings").Get<Config>();

            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(config.BuildHangfireConnection(), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                }));

            services.AddHangfireServer();

            config.SetInternetInformationServicesPermissions();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHangfireJob job,
            ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseHangfireDashboard();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            job.RunJob();
        }
    }
}
