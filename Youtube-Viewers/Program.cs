using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Leaf.xNet;
using Youtube_Viewers.Helpers;

namespace Youtube_Viewers
{
    class Program
    {
        static string id;
        static int threadsCount;

        static int pos = 0;

        static ProxyQueue scraper;
        static ProxyType proxyType;
        static bool updateProxy = false;

        static int botted = 0;
        static int errors = 0;

        static string viewers = "Parsing...";
        static string title = "Parsing...";

        public static string[] Urls = new[] {
            "https://raw.githubusercontent.com/clarketm/proxy-list/master/proxy-list-raw.txt",
            "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks4.txt",
            "https://api.proxyscrape.com/?request=getproxies&proxytype=socks4&timeout=9000&ssl=yes",
            "https://www.proxy-list.download/api/v1/get?type=socks4"
        };

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
            if (!File.Exists("proxy_url.txt"))
            {
                File.AppendAllText("proxy_url.txt", string.Join("\r\n", Urls));
            }

            Console.Title = $"YTBot | {gitRepo}";
            Logo(ConsoleColor.Cyan);

            id = dialog("Enter Video ID");
            threadsCount = Convert.ToInt32(dialog("Enter Threads Count"));

            while (true)
            {
                Logo(ConsoleColor.Cyan);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Select proxy type:\r\n1. Http/s\r\n2. Socks4\r\n3. Socks5");

                Console.Write("Your choice: ");
                Console.ForegroundColor = ConsoleColor.Cyan;

                char k = Console.ReadKey().KeyChar;

                try
                {
                    int key = int.Parse(k.ToString());
                    switch (key)
                    {
                        case 1:
                            proxyType = ProxyType.HTTP;
                            break;
                        case 2:
                            proxyType = ProxyType.Socks4;
                            break;
                        case 3:
                            proxyType = ProxyType.Socks5;
                            break;
                        default:
                            throw new Exception();
                    }
                }
                catch
                {
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\r\nSelected {proxyType} proxy");

                break;
            }

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Update proxies by urls?:\r\n1. Yes\r\n2. No");

                Console.Write("Your choice: ");

                char k = Console.ReadKey().KeyChar;

                try
                {
                    int pt = int.Parse(k.ToString());
                    switch (pt)
                    {
                        case 1:
                            updateProxy = true;
                            break;

                        case 2:
                            break;

                        default:
                            throw new Exception();
                    }
                }
                catch
                {
                    continue;
                }
                break;
            }

            if (updateProxy)
            {
                Urls = File.ReadAllText("proxy_url.txt").Trim().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                Console.WriteLine("Proxy links: \r\n" + string.Join("\r\n", Urls));
                Console.WriteLine("You can set your own links in 'proxy_url.txt' file");

                string totalProxies = String.Empty;

                foreach(string proxyUrl in Urls)
                {
                    Console.WriteLine($"Downloading proxies from '{proxyUrl}'");
                    using (HttpRequest req = new HttpRequest())
                    {
                        totalProxies += req.Get(proxyUrl).ToString() + "\r\n";
                    }
                }

                scraper = new ProxyQueue(totalProxies, proxyType);
            }
            else
            {
                Console.WriteLine("Select proxy list");

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Proxy list (*.txt)|*.txt";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                scraper = new ProxyQueue(File.ReadAllText(dialog.FileName), proxyType);
            }

            Console.WriteLine($"Loaded {scraper.Length} proxies");

            Logo(ConsoleColor.Green);

            List<Thread> threads = new List<Thread>();

            Thread logWorker = new Thread(Log);
            logWorker.Start();
            threads.Add(logWorker);

            if (updateProxy)
            {
                Thread proxyWorker = new Thread(proxyUpdater);
                proxyWorker.Start();
                threads.Add(proxyWorker);
            }
            
            for (int i = 0; i < threadsCount; i++)
            {
                Thread t = new Thread(Worker);
                t.Start();
                threads.Add(t);
            }

            foreach (Thread t in threads)
                t.Join();

            Console.ReadKey();
        }

        static string dialog(string question)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{question}: ");
            Console.ForegroundColor = ConsoleColor.Cyan;

            string val = Console.ReadLine().Trim();

            Logo(ConsoleColor.Cyan);
            return val;
        }

        private static void proxyUpdater()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int sec = 600;
            while (true)
            {
                if (stopwatch.ElapsedMilliseconds / 1000 >= sec)
                {
                    string proxies = String.Empty;
                    foreach(string proxyUrl in Urls)
                    {
                        using (HttpRequest req = new HttpRequest())
                        {
                            try
                            {
                                proxies += req.Get(proxyUrl).ToString() + "\r\n";
                            }
                            catch
                            {
                            }
                        }
                    }

                    scraper.SafeUpdate(proxies);
                    sec += 600;
                }
                Thread.Sleep(1000);
            }
        }

        static void Logo(ConsoleColor color)
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

        static void Log()
        {
            while (true)
            {
                Console.SetCursorPosition(0, pos);
                Console.WriteLine($"\r\nBotted: {botted}\r\nErrors: {errors}\r\nProxies: {scraper.Length}          \r\nThreads: {threadsCount}\r\nTitle: {title}          \r\nViewers: {viewers}          \r\n");
                Thread.Sleep(250);
            }
        }

        static string buildUrl(Dictionary<string, string> args)
        {
            var url = "https://s.youtube.com/api/stats/watchtime?";
            foreach(var arg in args)
            {
                url += $"{arg.Key}={arg.Value}&";
            }
            return url;
        }

        static void Worker()
        {
            while (true)
            {
                try
                {
                    using (var req = new HttpRequest()
                    {
                        Proxy = scraper.Next(),
                        Cookies = new CookieStorage()
                    })
                    {
                        req.UserAgentRandomize();
                        req.Cookies.Container.Add(new Uri("https://www.youtube.com"), new Cookie("CONSENT", "YES+cb.20210629-13-p0.en+FX+407"));

                        var sres = req.Get($"https://www.youtube.com/watch?v={id}").ToString();
                        var viewersTemp = string.Join("", RegularExpressions.Viewers.Match(sres).Groups[1].Value.Where(char.IsDigit));

                        if (!string.IsNullOrEmpty(viewersTemp))
                            viewers = viewersTemp;

                        title = RegularExpressions.Title.Match(sres).Groups[1].Value;

                        var url = RegularExpressions.ViewUrl.Match(sres).Groups[1].Value;
                        url = url.Replace(@"\u0026", "&").Replace("%2C", ",").Replace(@"\/", "/");

                        var query = System.Web.HttpUtility.ParseQueryString(url);

                        var cl = query.Get(query.AllKeys[0]);
                        var ei = query.Get("ei");
                        var of = query.Get("of");
                        var vm = query.Get("vm");
                        var cpn = GetCPN();

                        var start = DateTime.UtcNow;

                        var st = random.Next(1000, 10000);
                        var et = GetCmt(start);
                        var lio = GetLio(start);

                        var rt = random.Next(10, 200);

                        var lact = random.Next(1000, 8000);
                        var rtn = rt + 300;

                        var args = new Dictionary<string, string>
                        {
                            ["ns"] = "yt",
                            ["el"] = "detailpage",
                            ["cpn"] = cpn,
                            ["docid"] = id,
                            ["ver"] = "2",
                            ["cmt"] = et.ToString(),
                            ["ei"] = ei,
                            ["fmt"] = "243",
                            ["fs"] = "0",
                            ["rt"] = rt.ToString(),
                            ["of"] = of,
                            ["euri"] = "",
                            ["lact"] = lact.ToString(),
                            ["live"] = "dvr",
                            ["cl"] = cl,
                            ["state"] = "playing",
                            ["vm"] = vm,
                            ["volume"] = "100",
                            ["cbr"] = "Firefox",
                            ["cbrver"] = "83.0",
                            ["c"] = "WEB",
                            ["cplayer"] = "UNIPLAYER",
                            ["cver"] = "2.20201210.01.00",
                            ["cos"] = "Windows",
                            ["cosver"] = "10.0",
                            ["cplatform"] = "DESKTOP",
                            ["delay"] = "5",
                            ["hl"] = "en_US",
                            ["rtn"] = rtn.ToString(),
                            ["aftm"] = "140",
                            ["rti"] = rt.ToString(),
                            ["muted"] = "0",
                            ["st"] = st.ToString(),
                            ["et"] = et.ToString(),
                            ["lio"] = lio.ToString()
                        };

                        string urlToGet = buildUrl(args);
                        req.AcceptEncoding ="gzip, deflate";
                        req.AddHeader("Host", "www.youtube.com");
                        req.Get(urlToGet.Replace("watchtime", "playback"));

                        req.AcceptEncoding ="gzip, deflate";
                        req.AddHeader("Host", "www.youtube.com");
                        req.Get(urlToGet);
                        
                        Interlocked.Increment(ref botted);
                    }
                }
                catch
                {
                    Interlocked.Increment(ref errors);
                }

                Thread.Sleep(1);
            }
        }
        
        public static double GetCmt(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var start = date.ToUniversalTime() - origin;
            var now = DateTime.UtcNow.ToUniversalTime() - origin;
            var value = (now.TotalSeconds - start.TotalSeconds).ToString("#.000");
            return double.Parse(value);
        }

        public static double GetLio(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var start = date.ToUniversalTime() - origin;
            var value = start.TotalSeconds.ToString("#.000");
            return double.Parse(value);
        }
    
        private static Random random = new Random();
        public static string GetCPN()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
            return new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
