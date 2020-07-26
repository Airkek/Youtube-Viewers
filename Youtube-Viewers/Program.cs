using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Youtube_Viewers.Helpers;
using static Youtube_Viewers.Helpers.UsedProxyType;
using System.Threading.Tasks;

namespace Youtube_Viewers
{
    class Program
    {
        static string id;
        static int threadsCount;

        static int pos = 0;

        static ProxyScraper scraper;
        public static UsedProxyType proxyType;
        
        static int botted = 0;
        static int errors = 0;

        static Object locker = new Object();
        static Object loglocker = new Object();

        static string intro = @"/_/\/_/\   /________/\ /_______/\     /_____/\     /________/\ 
\ \ \ \ \  \__.::.__\/ \::: _  \ \    \:::_ \ \    \__.::.__\/ 
 \:\_\ \ \    \::\ \    \::(_)  \/_    \:\ \ \ \      \::\ \   
  \::::_\/     \::\ \    \::  _  \ \    \:\ \ \ \      \::\ \  
    \::\ \      \::\ \    \::(_)  \ \    \:\_\ \ \      \::\ \ 
     \__\/       \__\/     \_______\/     \_____\/       \__\/ 
";

        static string gitRepo = "https://github.com/Airkek/Youtube-Viewers";

        [STAThread]
        static void Main(string[] args)
        {
            Application.DoEvents();

            Console.Title = $"YTBot | {gitRepo}";

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(intro);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("GitHub: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(gitRepo);

            pos = Console.CursorTop;
            ThreadPool.GetMaxThreads(out int _uselessStuff, out int ioThreadsCount);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Max Thread Count: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(ioThreadsCount);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Enter Video ID: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            id = Console.ReadLine().Trim();
         
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Enter Threads Count: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            threadsCount = Convert.ToInt32(Console.ReadLine().Trim());

            while (true)
            {
                Application.DoEvents();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Select proxy type:\r\n0. Public (Socks4 autoscrape)\r\n1. Http/s\r\n2. Socks4\r\n3. Socks5");

                Console.Write("Your choice: ");
                Console.ForegroundColor = ConsoleColor.Cyan;

                char k = Console.ReadKey().KeyChar;

                try
                {
                    proxyType = (UsedProxyType)int.Parse(k.ToString());
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\r\nUnknown proxy type");
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\r\nSelected {proxyType} proxy");

                break;
            }

            if (proxyType != Public)
            {
                Console.WriteLine("Open file with proxy list");

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Proxy list (*.txt)|*.txt";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                scraper = new ProxyScraper(dialog.FileName);
            }

            else
                scraper = new ProxyScraper();
            Application.DoEvents();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Clear();
            Console.WriteLine(intro);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("GitHub: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(gitRepo);

            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < threadsCount; i++)
            {
                Thread t = new Thread(worker);
                t.Start();
                threads.Add(t);
            }
            Application.DoEvents();
            Console.ReadKey();
        }

        static void log()
        {
            Application.DoEvents();
            Console.SetCursorPosition(0, pos);
            Console.WriteLine($"\r\nBotted: {botted}\r\nErrors: {errors}\r\nProxies: {scraper.Proxies.Count}\r\nThreads: {threadsCount}\r\n");
        }

        static void worker(Object s)
        {
            while (true)
            {
                try
                {
                    using (HttpRequest req = new HttpRequest())
                    {

                        HttpResponse res;
                        Proxy proxy;
                        string url;
                        string urlToGet;
                        string cl;
                        string ei;
                        string of;
                        string vm;

                        Application.DoEvents();

                        lock (locker)
                            if (proxyType == Public && scraper.Time < (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds - 150)
                                scraper.Scrape();

                        proxy = scraper.Next();

                        switch (proxyType)
                        {
                            case Https:
                                req.Proxy = proxy.Http;
                                break;

                            case Public:
                            case Socks4:
                                req.Proxy = proxy.Socks4;
                                break;

                            case Socks5:
                                req.Proxy = proxy.Socks5;
                                break;
                        }

                        req.UserAgent = UserAgent.Get();
                        res = req.Get($"https://m.youtube.com/watch?v={id}?disable_polymer=1");
                        url = res.ToString().Split(new string[] { "videostatsWatchtimeUrl\\\":{\\\"baseUrl\\\":\\\"" }, StringSplitOptions.None)[1];
                        url = url.Split(new string[] { "\\\"}" }, StringSplitOptions.None)[0];
                        url = url.Replace(@"\\u0026", "&").Replace("%2C", ",").Replace(@"\/", "/");

                        cl = url.Split(new string[] { "cl=" }, StringSplitOptions.None)[1].Split('&')[0];
                        ei = url.Split(new string[] { "ei=" }, StringSplitOptions.None)[1].Split('&')[0];
                        of = url.Split(new string[] { "of=" }, StringSplitOptions.None)[1].Split('&')[0];
                        vm = url.Split(new string[] { "vm=" }, StringSplitOptions.None)[1].Split('&')[0];

                        urlToGet = $"https://s.youtube.com/api/stats/watchtime?ns=yt&el=detailpage&cpn=isWmmj2C9Y2vULKF&docid={id}&ver=2&cmt=7334&ei={ei}&fmt=133&fs=0&rt=1003&of={of}&euri&lact=4418&live=dvr&cl={cl}&state=playing&vm={vm}&volume=98.5&c=MWEB&cver=2.20200313.03.00&cplayer=UNIPLAYER&cbrand=apple&cbr=Safari%20Mobile&cbrver=12.1.15E148&cmodel=iphone&cos=iPhone&cosver=12_2&cplatform=MOBILE&delay=5&hl=ru&cr=GB&rtn=1303&afmt=140&lio=1556394045.182&idpj=&ldpj=&rti=1003&muted=0&st=7334&et=7634";

                        req.AddHeader("Referrer", $"https://m.youtube.com/watch?v={id}");
                        req.AddHeader("Host", "m.youtube.com");
                        req.AddHeader("Proxy-Connection", "keep-alive");
                        req.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                        req.AddHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                        req.AddHeader("Accept-Encoding", "gzip, deflate");

                        req.Get(urlToGet);

                        lock (loglocker)
                        {
                            Application.DoEvents();
                            botted++;
                            log();
                        }
                    }
                }
                catch (Exception)
                {
                    lock (loglocker)
                    {
                        Application.DoEvents();
                        errors++;
                        log();
                    }

                }
            }
        }
    }
}
