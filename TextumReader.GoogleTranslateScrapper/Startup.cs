using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using System.IO;
using TextumReader.GoogleTranslateScrapper.Services;
using Microsoft.Extensions.Logging;
using Hangfire.Console;
using Hangfire.Server;

namespace TextumReader.GoogleTranslateScrapper
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<DatabaseSettings>(
                Configuration.GetSection(nameof(DatabaseSettings)));

            services.AddSingleton<TranslationService>();
            services.AddSingleton<ProxyProvider>();

            // Add Hangfire services.
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                })
                .UseConsole());

            // Add the processing server as IHostedService
            services.AddHangfireServer();

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IBackgroundJobClient backgroundJobs, ILogger<TranslationEntity> logger /*, PerformContext context*/)
        {
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard();
            });

            var options = new BackgroundJobServerOptions { WorkerCount = int.Parse(Configuration["WorkerCount"]) };

            app.UseHangfireServer(options);


            /*
            int batchSize = 100;
            var source = File.ReadAllLines("./words/en.txt").ToArray();
            
            string[] buffer;

            for (int i = 0; i < source.Length; i += batchSize)
            {
                buffer = new string[batchSize];
                buffer = source.Skip(i).Take(batchSize).ToArray();
                
                logger.LogInformation($"Processing batch from {i} to {i + batchSize}");
                //context.WriteLine($"Processing {i} of {words.Count} words");
                backgroundJobs.Enqueue<GetTranslationsJob>(job => job.Run("en", "ru", buffer, null));
            }
            */
        }
    }
}
