using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Youtube_Viewers.Helpers;
using static Youtube_Viewers.Helpers.UsedProxyType;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Youtube_Viewers
{
    class Program
    {
        static string vidUrl;
        static int threadsCount;

        static int pos = 0;

        static ProxyScraper scraper;
        public static UsedProxyType proxyType;
        static Random random = new Random();
        
        static int botted = 0;
        static int errors = 0;

        static Object locker = new Object();

        static string intro = @"/_/\/_/\   /________/\ /_______/\     /_____/\     /________/\ 
\ \ \ \ \  \__.::.__\/ \::: _  \ \    \:::_ \ \    \__.::.__\/ 
 \:\_\ \ \    \::\ \    \::(_)  \/_    \:\ \ \ \      \::\ \   
  \::::_\/     \::\ \    \::  _  \ \    \:\ \ \ \      \::\ \  
    \::\ \      \::\ \    \::(_)  \ \    \:\_\ \ \      \::\ \ 
     \__\/       \__\/     \_______\/     \_____\/       \__\/ 
";

        static string gitRepo = "https://github.com/Airkek/Youtube-Viewers";

        static Regex url_re = new Regex(@"videostatsWatchtimeUrl\\"":{\\""baseUrl\\"":\\""(.+?)\\""}", RegexOptions.Compiled);

        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = $"YTBot | {gitRepo}";

            logo(ConsoleColor.Cyan);

            ThreadPool.GetMaxThreads(out int _uselessStuff, out int ioThreadsCount);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Max Thread Count: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(ioThreadsCount);
            Console.WriteLine();

            logo(ConsoleColor.Cyan);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Enter Video ID: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            vidUrl = "https://m.youtube.com/watch?v=" + Console.ReadLine().Trim();

            logo(ConsoleColor.Cyan);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Enter Threads Count: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            threadsCount = Convert.ToInt32(Console.ReadLine().Trim());

            while (true)
            {
                logo(ConsoleColor.Cyan);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Select proxy type:\r\n0. Public (Socks4 autoscrape)\r\n1. Http/s\r\n2. Socks4\r\n3. Socks5");

                Console.Write("Your choice: ");
                Console.ForegroundColor = ConsoleColor.Cyan;

                char k = Console.ReadKey().KeyChar;

                try
                {
                    int key = int.Parse(k.ToString());

                    if (key < 0 || key > 3)
                        throw new NotImplementedException();

                    proxyType = (UsedProxyType)key;
                }
                catch
                {
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

            logo(ConsoleColor.Green);

            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < threadsCount; i++)
            {
                Thread t = new Thread(worker);
                t.Start();
                threads.Add(t);
            }
            Console.ReadKey();
        }

        static void logo(ConsoleColor color)
        {
            Console.Clear();

            Console.ForegroundColor = color;
            Console.WriteLine(intro);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("GitHub: ");

            Console.ForegroundColor = color;
            Console.WriteLine(gitRepo);

            pos = Console.CursorTop;
        }

        static void log()
        {
            lock (locker)
            {
                Console.SetCursorPosition(0, pos);
                Console.WriteLine($"\r\nBotted: {botted}\r\nErrors: {errors}\r\nProxies: {scraper.Proxies.Count}\r\nThreads: {threadsCount}\r\n");
            }
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

                        double vol = random.Next(200, 1000) / 10d;

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

                        res = req.Get(vidUrl);
                        url = url_re.Match(res.ToString()).Groups[1].Value;

                        urlToGet = url.Replace(@"\\u0026", "&").Replace("%2C", ",").Replace(@"\/", "/");

                        req.AddHeader("Referrer", vidUrl);
                        req.AddHeader("Host", "m.youtube.com");
                        req.AddHeader("Proxy-Connection", "keep-alive");
                        req.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                        req.AddHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                        req.AddHeader("Accept-Encoding", "gzip, deflate");

                        req.Get(urlToGet);

                        Interlocked.Increment(ref botted);
                        log();
                    }
                }
                catch
                {
                    Interlocked.Increment(ref errors);
                    log();
                }
            }
        }
    }
}
