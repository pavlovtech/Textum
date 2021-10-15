using System;
using System.IO;
using Azure.Messaging.ServiceBus;
using Konsole;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TextumReader.TranslationsCollectorWorkerService.Abstract;
using TextumReader.TranslationsCollectorWorkerService.EventHandlers;
using TextumReader.TranslationsCollectorWorkerService.Services;

namespace TextumReader.TranslationsCollectorWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            var config = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                //.WriteTo.Console()
                .WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces)
                .CreateLogger();

            Log.Logger.Information("Application Starting");

            CreateHostBuilder(args, config).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration config)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var client = new ServiceBusClient(config.GetValue<string>("ServiceBusConnectionString"));

                    var receiver = client.CreateReceiver(config.GetValue<string>("QueueName"));

                    var cosmosClient = new CosmosClient(config.GetValue<string>("CosmosDbConnectionString"),
                        new CosmosClientOptions
                        {
                            SerializerOptions = new CosmosSerializationOptions
                            {
                                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                            },
                            //AllowBulkExecution = true
                        });

                    services.AddSingleton<ITranslationEventHandler, TranslationTranslationEventHandler>();
                    services.AddSingleton<CognitiveServicesTranslator>();
                    services.AddSingleton<ProxyProvider>();
                    services.AddSingleton(cosmosClient);
                    services.AddSingleton(receiver);

                    services.AddHostedService<GoogleTranslateScraperWorker>();
                    services.AddApplicationInsightsTelemetryWorkerService();

                    /*services.Configure<HostOptions>(
                        opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(15));*/

                    var console = Window.OpenBox("translations", 120, 29);
                    services.AddSingleton<IConsole>(console);
                })
                .UseConsoleLifetime()
                .UseSerilog();
        }

        private static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                    true)
                .AddEnvironmentVariables();
        }
    }
}