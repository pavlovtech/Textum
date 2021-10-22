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
using Serilog.Events;
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
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithThreadName()
                .Enrich.WithProcessName()
                .Enrich.WithProcessId()
                .Enrich.FromLogContext()
                //.WriteTo.Seq("http://localhost:5341", LogEventLevel.Debug, 1000, null, "O09szsLe4sMkpUgGqkyU")
                .ReadFrom.Configuration(config)
                .CreateLogger();

            Log.Logger.Information("Application Starting");

            CreateHostBuilder(args, config).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration config)
        {
            return Host.CreateDefaultBuilder(args)
                .UseWindowsService(options =>
                {
                    options.ServiceName = "GoogleTranslateScraper";
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var client = new ServiceBusClient(config.GetValue<string>("ServiceBusConnectionString"));

                    var receiver = client.CreateReceiver(config.GetValue<string>("QueueName"), new ServiceBusReceiverOptions()
                    {
                        PrefetchCount = 0
                    });

                    var cosmosClient = new CosmosClient(config.GetValue<string>("CosmosDbConnectionString"),
                        new CosmosClientOptions
                        {
                            SerializerOptions = new CosmosSerializationOptions
                            {
                                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                            }
                        });

                    services.AddSingleton<ITranslationEventHandler, PlaywrightTranslationEventHandler>();
                    services.AddSingleton<CognitiveServicesTranslator>();
                    services.AddSingleton<ProxyProvider>();
                    services.AddSingleton(cosmosClient);
                    services.AddSingleton(receiver);

                    services.AddHostedService<GoogleTranslateScraperWorker>();
                    services.AddApplicationInsightsTelemetryWorkerService();

                    services.Configure<HostOptions>(
                        opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(15));
                })
                .UseConsoleLifetime()
                .UseSerilog((context, provider, loggerConfig) =>
                {
                    loggerConfig
                        .WriteTo.File("log.txt", LogEventLevel.Debug)
                        .WriteTo.ApplicationInsights(provider.GetRequiredService<TelemetryConfiguration>(),
                            TelemetryConverter.Traces);
                });
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