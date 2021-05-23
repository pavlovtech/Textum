using Hangfire.Console;
using Hangfire.Server;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using TextumReader.GoogleTranslateScrapper.Services;

namespace TextumReader.GoogleTranslateScrapper
{
    public class GetTranslationsJob
    {
        private TranslationService _translationService;
        private readonly ProxyProvider _proxyProvider;
        private readonly ILogger<TranslationEntity> _logger;

        public GetTranslationsJob(TranslationService translationService, ProxyProvider proxyProvider, ILogger<TranslationEntity> logger)
        {
            _translationService = translationService;
            _proxyProvider = proxyProvider;
            _logger = logger;
        }

        public void Run(string from, string to, IList<string> words)
        {
            // In this case, PerformContext will not be substituted,
            // you should add all the null-checks.
            Run(from, to, words, null);
        }

        public void Run(string from, string to, IList<string> words, PerformContext context)
        {
            // create progress bar
            var progress = context.WriteProgressBar();

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");

            chromeOptions.Proxy = _proxyProvider.GetProxy();

            using (var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), chromeOptions))
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

                for (int i = 0; i < words.Count; i++)
                {
                    driver.Navigate().GoToUrl($"https://translate.google.com/?sl={from}&tl={to}&text={words[i]}&op=translate");
                    ProcessPage(words[i], context, driver, chromeOptions);
                    // update value for previously created progress bar
                    double percent = ((double)i / words.Count) * 100;
                    progress.SetValue(percent);
                }
            }
        }

        private void ProcessPage(string word, PerformContext context, ChromeDriver driver, ChromeOptions chromeOptions)
        {
            _logger.LogInformation($"Started getting translations for {word}", word);
            context.WriteLine($"Started getting translations for {word}");
            
            if (IsElementPresent(By.CssSelector(
                "button[class='VfPpkd-LgbsSe VfPpkd-LgbsSe-OWXEXe-k8QpJ VfPpkd-LgbsSe-OWXEXe-dgl2Hf nCP5yc AjY5Oe DuMIQc']")))
            {
                var text = driver
                    .FindElement(By.CssSelector(
                        "button[class='VfPpkd-LgbsSe VfPpkd-LgbsSe-OWXEXe-k8QpJ VfPpkd-LgbsSe-OWXEXe-dgl2Hf nCP5yc AjY5Oe DuMIQc']"))
                    .Text;
                if (text == "I agree")
                {
                    _logger.LogError("Proxy is compromised {chromeOptions.Proxy.HttpProxy}", chromeOptions.Proxy);
                    context.WriteLine($"Proxy is compromised {chromeOptions.Proxy.HttpProxy}");

                    _proxyProvider.ExcludeProxy(chromeOptions.Proxy.HttpProxy);

                    throw new ProxyCompromizedException($"Proxy is compromised {chromeOptions.Proxy.HttpProxy}");
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
                _translationService.Insert(new TranslationEntity
                {
                    Word = word,
                    MainTranslation = mainTranslation
                });

                return;
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
                        PartOfSpeach = currentPartOfSpeach,
                        Translation = t[1],
                        Synonyms = t[2].ToString().Split(","),
                        Frequency = t[3]
                    });
                }

                if (t.Length == 3)
                {
                    result.Add(new WordTranslation
                    {
                        PartOfSpeach = currentPartOfSpeach,
                        Translation = t[0],
                        Synonyms = t[1].ToString().Split(","),
                        Frequency = t[2]
                    });
                }
            }

            _translationService.Insert(new TranslationEntity
            {
                Word = word,
                MainTranslation = mainTranslation,
                Translations = result
            });

            _logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"Finished getting translations for {word}", word);
            context.WriteLine($"Finished getting translations for {word}");

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
