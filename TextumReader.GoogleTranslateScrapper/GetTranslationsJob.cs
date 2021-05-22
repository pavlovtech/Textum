using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TextumReader.GoogleTranslateScrapper
{
    public class GetTranslationsJob
    {
        private TranslationService _translationService;

        public GetTranslationsJob(TranslationService translationService)
        {
            _translationService = translationService;
        }

        public void Run(string from, string to, string word)
        {
            var translationResult = new List<string>();

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");

            using (var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), chromeOptions))
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

                driver.Navigate().GoToUrl($"https://translate.google.com/?sl={from}&tl={to}&text={word}&op=translate");

                var mainTranslation = driver.FindElementByClassName("VIiyi");

                var translations = driver.FindElementsByClassName("kgnlhe");

                translationResult.Add(mainTranslation.Text);
                translationResult.AddRange(translations.Select(t => t.Text));

                _translationService.Create(new Translation { 
                    Word = word,
                    Translations = translationResult
                });
            }
        }
    }
}
