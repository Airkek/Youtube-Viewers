using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Leaf.xNet;
using Youtube_Viewers.CLI.Utils;
using Youtube_Viewers.Config;
using Youtube_Viewers.Core;
using Youtube_Viewers.Core.Enums;
using Youtube_Viewers.Core.Objects;
using Youtube_Viewers.ProxyTools;

namespace Youtube_Viewers.CLI
{
    internal static class Program
    {
        private static ConfigScheme Config = ConfigScheme.Read();
        private static ProxyQueue Scraper = null;

        private static string StreamId = string.Empty;

        private static int Botted = 0;
        private static int Failed = 0;
        private static VideoStats Stats = new VideoStats("Connecting...", 0);
        
        [STAThread]
        public static void Main(string[] args)
        {
            XConsole.SetTitle();
            XConsole.PrintLogo(ConsoleColor.Cyan);

            StreamId = XConsole.InlineAsk("Enter Stream ID");

            if (Config is null)
            {
                Config = new ConfigScheme
                {
                    Threads = XConsole.InlineAskInt("Enter Threads Count"),
                    ProxyType = XConsole.SelectProxy(),
                    ParseProxies = XConsole.AskBool("Update proxies by urls?")
                };

                if (Config.ParseProxies)
                    Config.ScrapeTimeout = XConsole.InlineAskInt("Proxy update timeout (seconds)");
                
                Config.Save();
            }
            
            if (Config.ParseProxies)
            {
                using (var req = new HttpRequest())
                {
                    Console.WriteLine("Proxy links: \r\n" + string.Join("\r\n", Config.ScrapeUrls));
                    Console.WriteLine("You can set your own links in 'proxy_url.txt' file");

                    var totalProxies = string.Empty;

                    foreach(var proxyUrl in Config.ScrapeUrls)
                    {
                        XConsole.Print($"Downloading proxies from '{proxyUrl}': ");

                        try
                        {
                            totalProxies += req.Get(proxyUrl) + "\r\n";
                            XConsole.PrintLine("Success", ConsoleColor.Green);
                        }
                        catch
                        {
                            XConsole.PrintLine("Error", ConsoleColor.Red);
                        }
                    }

                    Scraper = new ProxyQueue(totalProxies, Config.ProxyType);
                }
            }
            else
            {
                XConsole.PrintLine("Select proxy list");

                var dialog = new OpenFileDialog {Filter = "Proxy list (*.txt)|*.txt"};

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                Scraper = new ProxyQueue(File.ReadAllText(dialog.FileName), Config.ProxyType);
            }
            
            XConsole.PrintLine($"Loaded {Scraper.Length} proxies");
            
            XConsole.PrintLogo(ConsoleColor.Green);
            
            new Thread(PrintStatsWorker).Start();
            
            for(var i = 0; i < Config.Threads; i++)
                new Thread(MainWorker).Start();
            
            if(Config.ParseProxies)
                new Thread(ProxySrapeWorker).Start();

            while (true) ; // все потоки бесконечные, нет смысла их дожидаться
        }

        private static void MainWorker()
        {
            var core = new ViewersCore(StreamId);

            while (true)
            {
                core.Proxy = Scraper.Next();
                var result = core.IncreaseViews();

                switch (result)
                {
                    case WorkerResponseStatus.Botted:
                        Botted++;
                        break;
                    case WorkerResponseStatus.Failed:
                        Failed++;
                        break;
                    default:
                        break;
                }

                if(!string.IsNullOrWhiteSpace(core.Stats.Title))
                    Stats = core.Stats;
            }
        }

        private static void PrintStatsWorker()
        {
            while (true)
            {
                XConsole.PrintStats(Stats, Botted, Failed, Scraper.Length);
                Thread.Sleep(250);
            }
        }
        
        private static void ProxySrapeWorker()
        {
            var req = new HttpRequest();
            while (true)
            {
                try
                {
                    var totalProxies = string.Empty;

                    foreach(var proxyUrl in Config.ScrapeUrls)
                    {
                        try
                        {
                            totalProxies += req.Get(proxyUrl) + "\r\n";
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    if(string.IsNullOrEmpty(totalProxies))
                        continue;
                    
                    Scraper = new ProxyQueue(totalProxies, Config.ProxyType);
                    Thread.Sleep(Config.ScrapeTimeout * 1000);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }
}