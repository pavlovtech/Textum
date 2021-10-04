using System;
using System.IO;
using Serilog;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TextumReader.TranslationJobProcessor.Abstract;
using TextumReader.TranslationJobProcessor.EventHandlers;
using TextumReader.TranslationJobProcessor.Services;

namespace TextumReader.TranslationJobProcessor
{
    class Program
    {
        static async Task Main()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            var config = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("Application Starting");

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, collection) =>
                {
                    collection.AddTransient<ITranslationsProcessingService, TranslationsProcessingService>();
                    collection.AddTransient<ITranslationEventHandler, TranslationTranslationEventHandler>();
                    collection.AddSingleton<CognitiveServicesTranslator>();
                    collection.AddSingleton<ProxyProvider>();
                    collection.AddSingleton<IConfiguration>(config);
                })
                .UseSerilog()
                .Build();

            var svc = ActivatorUtilities.CreateInstance<TranslationsProcessingService>(host.Services);

            await svc.Run();
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("app.settings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(
                    $"app.settings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                    optional: true)
                .AddEnvironmentVariables();
        }
    }
}
