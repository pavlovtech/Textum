using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TextumReader.GoogleTranslateScrapper;
using TextumReader.TranslationJobProcessor.Abstract;
using TextumReader.TranslationJobProcessor.Models;
using TextumReader.TranslationJobProcessor.Services;

namespace TextumReader.TranslationJobProcessor.EventHandlers
{
    public class TranslationTranslationEventHandler : ITranslationEventHandler
    {
        private readonly CognitiveServicesTranslator _cognitiveServicesTranslator;
        private readonly ProxyProvider _proxyProvider;
        private readonly ILogger<TranslationTranslationEventHandler> _logger;

        public TranslationTranslationEventHandler(CognitiveServicesTranslator cognitiveServicesTranslator, ProxyProvider proxyProvider, ILogger<TranslationTranslationEventHandler> logger)
        {
            _cognitiveServicesTranslator = cognitiveServicesTranslator;
            _proxyProvider = proxyProvider;
            _logger = logger;
        }
        
        public async Task<List<TranslationEntity>> Handle(ServiceBusReceivedMessage m)
        {
            _logger.LogInformation("==================== TranslationTranslationEventHandler.Handle =================");

            var (@from, to, words) = JsonConvert.DeserializeObject<TranslationRequest>(m.Body.ToString());

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");

            //chromeOptions.Proxy = _proxyProvider.GetProxy();

            var translationEntities = new List<TranslationEntity>();

            using (var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), chromeOptions))
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

                for (int i = 0; i < words.Count; i++)
                {
                    driver.Navigate().GoToUrl($"https://translate.google.com/?sl={from}&tl={to}&text={words[i]}&op=translate");
                    
                    var result = await ProcessPage(words[i], from, to, driver, chromeOptions, m);

                    if(result.Translations != null)
                    {
                        var translations = result.Translations.Where(t => t.Frequency == "Common" || t.Frequency == "Uncommon").ToList();

                        foreach (var trans in translations)
                        {
                            trans.Examples = _cognitiveServicesTranslator.GetExamples(from, to, result.Word, trans.Translation).Take(3);
                        }
                    }

                    //_translationService.Insert(result);
                    translationEntities.Add(result);
                }
            }

            _logger.LogInformation("Complete job");

            return translationEntities;
        }

        private async Task<TranslationEntity> ProcessPage(string word, string from, string to, ChromeDriver driver, ChromeOptions chromeOptions, ServiceBusReceivedMessage m)
        {
            _logger.LogInformation($"Started getting translations for {word}", word);
            
            if (IsElementPresent(By.CssSelector(
                "button[class='VfPpkd-LgbsSe VfPpkd-LgbsSe-OWXEXe-k8QpJ VfPpkd-LgbsSe-OWXEXe-dgl2Hf nCP5yc AjY5Oe DuMIQc']")))
            {
                var text = driver
                    .FindElement(By.CssSelector(
                        "button[class='VfPpkd-LgbsSe VfPpkd-LgbsSe-OWXEXe-k8QpJ VfPpkd-LgbsSe-OWXEXe-dgl2Hf nCP5yc AjY5Oe DuMIQc']"))
                    .Text;
                if (text == "I agree")
                {
                    _logger.LogInformation("IP is compromised {chromeOptions.Proxy.HttpProxy}", chromeOptions.Proxy);

                    _proxyProvider.ExcludeProxy(chromeOptions.Proxy.HttpProxy);

                    throw new ProxyCompromizedException($"IP is compromised {chromeOptions.Proxy.HttpProxy}");
                }
            }

            var mainTranslation = driver.FindElementByClassName("VIiyi").Text;

            // if no translations except main
            string doc = "";
            if (IsElementPresent(By.CssSelector("div[class='I87fLc oLovEc XzOhkf']")))
            {
                doc = driver.FindElement(By.CssSelector("div[class='I87fLc oLovEc XzOhkf']")).GetAttribute("innerHTML");
            }
            else
            {
                _logger.LogInformation($"Finished getting translations for {word}", word);

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
            string currentPartOfSpeach = "";
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
                {
                    result.Add(new WordTranslation
                    {
                        PartOfSpeech = currentPartOfSpeach,
                        Translation = t[0],
                        Synonyms = t[1].Split(","),
                        Frequency = t[2]
                    });
                }
            }
                
            _logger.LogInformation($"Finished getting translations for {word}", word);

            return new TranslationEntity
            {
                Word = word,
                From = from,
                To = to,
                MainTranslation = mainTranslation,
                Translations = result,
                Id = $"{from}-{to}-{word}"
            };

            bool IsElementPresent(By by)
            {
                try
                {
                    driver.FindElement(@by);
                    return true;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            }
        }
    }
}
