using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenQA.Selenium;

namespace TextumReader.TranslationsCollectorWorkerService.Services
{
    public class ProxyProvider
    {
        private readonly List<string> _proxies = new();

        public ProxyProvider()
        {
            var allProxies = File.ReadAllLines("proxies.txt").ToList();
            var badProxies = File.ReadAllLines("bad-proxies.txt").ToList();

            _proxies = allProxies.Except(badProxies).ToList();
        }

        public Proxy GetProxy()
        {
            if (!_proxies.Any()) throw new Exception("Out of proxies Exception");

            var rnd = new Random();

            var proxy = new Proxy();
            proxy.Kind = ProxyKind.Manual;
            var proxyUrl = _proxies[rnd.Next(0, _proxies.Count)];

            proxy.SslProxy = proxyUrl;
            proxy.HttpProxy = proxyUrl;

            return proxy;
        }

        public bool ExcludeProxy(string proxy)
        {
            File.AppendAllLines("bad-proxies.txt", new[] { proxy });
            return _proxies.Remove(proxy);
        }
    }
}