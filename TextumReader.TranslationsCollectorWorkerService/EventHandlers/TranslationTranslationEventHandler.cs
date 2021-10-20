using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using HtmlAgilityPack;
using Konsole;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SerilogTimings;
using TextumReader.TranslationsCollectorWorkerService.Abstract;
using TextumReader.TranslationsCollectorWorkerService.Exceptions;
using TextumReader.TranslationsCollectorWorkerService.Models;
using TextumReader.TranslationsCollectorWorkerService.Services;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace TextumReader.TranslationsCollectorWorkerService.EventHandlers
{
    public class TranslationTranslationEventHandler : ITranslationEventHandler
    {
        private readonly IConfiguration _config;
        private readonly CognitiveServicesTranslator _cognitiveServicesTranslator;
        private readonly ILogger<TranslationTranslationEventHandler> _logger;
        private readonly ProxyProvider _proxyProvider;
        private readonly ServiceBusReceiver _receiver;
        private readonly TelemetryClient _telemetryClient;
        private IConsole _console;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(1);

        public TranslationTranslationEventHandler(
            ServiceBusReceiver receiver,
            TelemetryClient telemetryClient,
            CognitiveServicesTranslator cognitiveServicesTranslator,
            ProxyProvider proxyProvider,
            ILogger<TranslationTranslationEventHandler> logger,
            IConfiguration config,
            IConsole console)
        {
            _config = config;
            _console = console;
            _receiver = receiver;
            _telemetryClient = telemetryClient;
            _cognitiveServicesTranslator = cognitiveServicesTranslator;
            _proxyProvider = proxyProvider;
            _logger = logger;
        }

        public async Task<List<TranslationEntity>> Handle(ServiceBusReceivedMessage message, CancellationToken stoppingToken)
        {
            _logger.LogInformation("TranslationTranslationEventHandler.Handle for message {id}", message.MessageId);

            using var handleEventOperation =
                _telemetryClient.StartOperation<DependencyTelemetry>("TranslationTranslationEventHandler.Handle");

            var (from, to, words) = JsonConvert.DeserializeObject<TranslationRequest>(message.Body.ToString());

            var translationEntities = new List<TranslationEntity>();

            var pb = new ProgressBar(_console, PbStyle.SingleLine, words.Count);

            try
            {
                pb.Refresh(0, "Init...");

                using (var driver = ConfigureChrome(out var chromeOptions))
                {
                    for (var i = 0; i < words.Count; i++)
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            driver.Quit();
                            handleEventOperation.Telemetry.Success = false;

                            throw new ApplicationException("Cancellation requested");
                        }

                        if (DateTimeOffset.UtcNow > message.LockedUntil.AddMinutes(-2))
                        {
                            _logger.LogInformation("Lock is to be expired");
                            _telemetryClient.TrackEvent("Lock is to be expired");
                            await _receiver.RenewMessageLockAsync(message, stoppingToken);
                            _logger.LogInformation("Lock renewed");
                        }

                        var result = GetTranslations(driver, @from, to, words, i, chromeOptions);

                        //_translationService.Insert(result);
                        translationEntities.Add(result);

                        pb.Refresh(i + 1, $"{words[i]}");
                    }
                }

                _logger.LogInformation("Scrapping complete");

                pb.Refresh(words.Count, "Scrapping complete");

                handleEventOperation.Telemetry.Success = true;

                return translationEntities;
            }
            catch (Exception e)
            {
                pb.Refresh(0, $"{e.Message}");
                handleEventOperation.Telemetry.Success = false;
                throw;
            }
        }

        private TranslationEntity GetTranslations(ChromeDriver driver, string @from, string to, IList<string> words, int i, ChromeOptions chromeOptions)
        {
            using var oneWordProcessingOperation = _telemetryClient.StartOperation<DependencyTelemetry>("One word processing");

            using var oneWordProcessingOperationTiming = Operation.Begin("One word processing");

            try
            {
                using (Operation.Time("driver.Navigate().GoToUrl"))
                {
                    driver.Navigate()
                        .GoToUrl($"https://translate.google.com/?sl={@from}&tl={to}&text={words[i]}&op=translate");
                }

                var result = ProcessPage(words[i], @from, to, driver, chromeOptions);

                if (result.Translations != null)
                {
                    var translations = result.Translations
                        .Where(t => t.Frequency == "Common" || t.Frequency == "Uncommon").ToList();

                    foreach (var trans in translations)
                        trans.Examples = _cognitiveServicesTranslator
                            .GetExamples(@from, to, result.Word, trans.Translation).Take(3);
                }

                oneWordProcessingOperation.Telemetry.Success = true;
                oneWordProcessingOperationTiming.Complete();
                return result;
            }
            catch (Exception e)
            {
                oneWordProcessingOperation.Telemetry.Success = false;
                oneWordProcessingOperationTiming.Cancel();
                throw;
            }
        }

        private ChromeDriver ConfigureChrome(out ChromeOptions chromeOptions)
        {
            using var time = Operation.Time("TranslationTranslationEventHandler.ConfigureChrome");

            ChromeDriverService service =
                ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            service.EnableVerboseLogging = false;
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;

            chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--window-size=1920,1080");
            chromeOptions.AddArgument("--no-sandbox");
            chromeOptions.AddArgument("--disable-gpu");
            chromeOptions.AddArgument("--disable-crash-reporter");
            chromeOptions.AddArgument("--disable-extensions");
            chromeOptions.AddArgument("--disable-in-process-stack-traces");
            chromeOptions.AddArgument("--disable-logging");
            chromeOptions.AddArgument("--disable-dev-shm-usage");
            chromeOptions.AddArgument("--log-level=3");
            chromeOptions.AddArgument("--output=/dev/null");


            if (_config.GetValue<bool>("UseProxy"))
            {
                //chromeOptions.Proxy = _proxyProvider.GetProxy();
            }

            if (_config.GetValue<bool>("Headless"))
            {
                chromeOptions.AddArgument("--headless");
            }

            return new ChromeDriver(service, chromeOptions);
        }

        private TranslationEntity ProcessPage(string word, string from, string to, ChromeDriver driver, ChromeOptions chromeOptions)
        {
            using var processPageOperationTiming = Operation.Begin("TranslationTranslationEventHandler.ProcessPage");

            var element = TryGetElement(By.CssSelector(
                    "button[class='VfPpkd-LgbsSe VfPpkd-LgbsSe-OWXEXe-k8QpJ VfPpkd-LgbsSe-OWXEXe-dgl2Hf nCP5yc AjY5Oe DuMIQc']"),
                driver, _defaultTimeout);

            if (element != null)
            {
                if (element.Text == "I agree")
                {
                    _logger.LogError("IP is compromised {chromeOptions.Proxy.HttpProxy}", chromeOptions.Proxy);
                    if (_config.GetValue<bool>("UseProxy"))
                    {
                        _proxyProvider.ExcludeProxy(chromeOptions.Proxy?.HttpProxy);
                    }
                    throw new CompromisedException($"IP is compromised {chromeOptions?.Proxy?.HttpProxy ?? "local"}");
                }
            }

            var errorEl = TryGetElement(By.CssSelector("div[class='QGDZGb']"), driver, _defaultTimeout);
            if (errorEl != null)
            {
                var text = errorEl.Text;

                if (text == "Translation error")
                {
                    _logger.LogError("IP is compromised {proxy}", chromeOptions.Proxy);

                    if (_config.GetValue<bool>("UseProxy"))
                    {
                        _proxyProvider.ExcludeProxy(chromeOptions.Proxy?.HttpProxy);
                    }

                    processPageOperationTiming.Cancel();

                    throw new CompromisedException($"IP is compromised {chromeOptions?.Proxy?.HttpProxy ?? "local"}");
                }
            }

            string mainTranslation;

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(_config.GetValue<int>("FindTranslationTimeoutInSec")));
            mainTranslation = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("VIiyi"))).Text;

            // if translations present
            var docEl = TryGetElement(By.CssSelector("div[class='I87fLc oLovEc XzOhkf']"), driver,
                _defaultTimeout);

            var doc = "";

            if (docEl != null)
            {
                doc = docEl.GetAttribute("innerHTML");;
            }
            else
            {
                // if no translations except main

                //_logger.LogInformation("Finished getting translations for {word}", word);

                processPageOperationTiming.Complete();

                return new TranslationEntity
                {
                    Word = word,
                    MainTranslation = mainTranslation,
                    From = from,
                    To = to,
                    Id = $"{from}-{to}-{word}"
                };
            }

            var html = new HtmlDocument();
            html.LoadHtml(doc);

            var nodes2 = html.DocumentNode.SelectNodes("//div/table/tbody/tr");

            var rows = nodes2.Select(tr =>
            {
                var thElements = tr.Elements("th").Select(el => el.InnerText.Trim());
                var tdElements = tr.Elements("td").Select(el => el.InnerText.Trim());

                return thElements.Concat(tdElements).ToArray();
            }).ToArray();

            var result = new List<WordTranslation>();
            var currentPartOfSpeach = "";
            foreach (var t in rows)
            {
                if (t.Length == 4)
                {
                    currentPartOfSpeach = t[0];
                    result.Add(new WordTranslation
                    {
                        PartOfSpeech = currentPartOfSpeach,
                        Translation = t[1],
                        Synonyms = t[2].Split(","),
                        Frequency = t[3]
                    });
                }

                if (t.Length == 3)
                    result.Add(new WordTranslation
                    {
                        PartOfSpeech = currentPartOfSpeach,
                        Translation = t[0],
                        Synonyms = t[1].Split(","),
                        Frequency = t[2]
                    });
            }

            //_logger.LogInformation("Finished getting translations for {word}", word);

            processPageOperationTiming.Complete();

            return new TranslationEntity
            {
                Word = word,
                From = from,
                To = to,
                MainTranslation = mainTranslation,
                Translations = result,
                Id = $"{from}-{to}-{word}"
            };
        }

        private static IWebElement TryGetElement(By @by, IWebDriver driver, TimeSpan timeout)
        {
            using var time = Operation.Time("TranslationTranslationEventHandler.TryGetElement {by}", @by.Criteria);

            try
            {
                var wait = new WebDriverWait(driver, timeout);
                var el = wait.Until(ExpectedConditions.ElementExists(@by));
                //driver.FindElement(by);
                return el;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}