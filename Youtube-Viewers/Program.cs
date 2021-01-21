using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using Leaf.xNet;
using Youtube_Viewers.Helpers;
using HttpRequest = Leaf.xNet.HttpRequest;
using HttpResponse = Leaf.xNet.HttpResponse;

namespace Youtube_Viewers
{
    internal class Program
    {
        private static string id;
        private static int threadsCount;

        private static int pos;

        private static ProxyQueue scraper;
        private static ProxyType proxyType;
        private static bool updateProxy;

        private static int botted;
        private static int errors;

        private static string viewers = "Connecting";
        private static string title = "Connecting";

        public static string[] Urls =
        {
            "https://raw.githubusercontent.com/clarketm/proxy-list/master/proxy-list-raw.txt",
            "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks4.txt",
            "https://api.proxyscrape.com/?request=getproxies&proxytype=socks4&timeout=9000&ssl=yes",
            "https://www.proxy-list.download/api/v1/get?type=socks4"
        };

        private static readonly string intro = @"/_/\/_/\   /________/\ /_______/\     /_____/\     /________/\ 
\ \ \ \ \  \__.::.__\/ \::: _  \ \    \:::_ \ \    \__.::.__\/ 
 \:\_\ \ \    \::\ \    \::(_)  \/_    \:\ \ \ \      \::\ \   
  \::::_\/     \::\ \    \::  _  \ \    \:\ \ \ \      \::\ \  
    \::\ \      \::\ \    \::(_)  \ \    \:\_\ \ \      \::\ \ 
     \__\/       \__\/     \_______\/     \_____\/       \__\/ 
";

        private static readonly string gitRepo = "https://github.com/Airkek/Youtube-Viewers";

        [STAThread]
        private static void Main()
        {
            if (!File.Exists("proxy_url.txt")) File.AppendAllText("proxy_url.txt", string.Join("\r\n", Urls));

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

                var k = Console.ReadKey().KeyChar;

                try
                {
                    var key = int.Parse(k.ToString());
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

                var k = Console.ReadKey().KeyChar;

                try
                {
                    var pt = int.Parse(k.ToString());
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

            reProxy:
            if (updateProxy)
            {
                Urls = File.ReadAllText("proxy_url.txt").Trim().Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
                Console.WriteLine("Proxy links: \r\n" + string.Join("\r\n", Urls));
                Console.WriteLine("You can set your own links in 'proxy_url.txt' file");

                var totalProxies = string.Empty;

                using (var req = new HttpRequest
                {
                    ConnectTimeout = 3000
                })
                {
                    foreach (var proxyUrl in Urls)
                    {
                        Console.ResetColor();
                        Console.Write($"Downloading proxies from '{proxyUrl}': ");
                        {
                            try
                            {
                                totalProxies += req.Get(proxyUrl) + "\r\n";
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Success");
                                Console.ResetColor();
                            }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Error");
                                Console.ResetColor();
                            }
                        }
                    }
                }

                if (totalProxies.Length == 0)
                {
                    MessageBox.Show("Couldn't update proxies by url. You will have to do manually");
                    updateProxy = false;
                    goto reProxy;
                }

                scraper = new ProxyQueue(totalProxies, proxyType);
            }
            else
            {
                Console.WriteLine("Select proxy list");

                var dialog = new OpenFileDialog();
                dialog.Filter = "Proxy list (*.txt)|*.txt";

                if (dialog.ShowDialog() != DialogResult.OK) return;

                scraper = new ProxyQueue(File.ReadAllText(dialog.FileName), proxyType);
            }

            Console.WriteLine($"Loaded {scraper.Length} proxies");

            Logo(ConsoleColor.Green);

            var threads = new List<Thread>();

            var logWorker = new Thread(Log);
            logWorker.Start();
            threads.Add(logWorker);

            if (updateProxy)
            {
                var proxyWorker = new Thread(proxyUpdater);
                proxyWorker.Start();
                threads.Add(proxyWorker);
            }

            for (var i = 0; i < threadsCount; i++)
            {
                var t = new Thread(Worker);
                t.Start();
                threads.Add(t);
            }

            foreach (var t in threads)
                t.Join();

            Console.ReadKey();
        }

        private static string dialog(string question)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{question}: ");
            Console.ForegroundColor = ConsoleColor.Cyan;

            var val = Console.ReadLine().Trim();

            Logo(ConsoleColor.Cyan);
            return val;
        }

        private static void proxyUpdater()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var sec = 600;
            while (true)
            {
                if (stopwatch.ElapsedTicks * 10 >= sec)
                {
                    var proxies = string.Empty;
                    foreach (var proxyUrl in Urls)
                        using (var req = new HttpRequest())
                        {
                            try
                            {
                                proxies += req.Get(proxyUrl) + "\r\n";
                            }
                            catch
                            {
                                // ignore
                            }
                        }

                    scraper.SafeUpdate(proxies);
                    sec += 600;
                }

                Thread.Sleep(1000);
            }
        }

        private static void Logo(ConsoleColor color)
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

        private static void Log()
        {
            while (true)
            {
                Console.SetCursorPosition(0, pos);
                Console.WriteLine(
                    $"Success connections: {botted}          \r\n" +
                    $"Errors: {errors}         \r\n" +
                    $"Proxies: {scraper.Length}          \r\n" +
                    $"Threads: {threadsCount}            \r\n" +
                    $"Title: {title}{new string(' ', Console.WindowWidth - title.Length)}\r\n" +
                    $"Viewers: {viewers}{new string(' ', Console.WindowWidth - viewers.Length)}\r\n"
                );
                Thread.Sleep(250);
            }
        }

        private static string buildUrl(Dictionary<string, string> args)
        {
            var url = "https://s.youtube.com/api/stats/watchtime?";
            foreach (var arg in args) url += $"{arg.Key}={arg.Value}&";
            return url;
        }

        private static void Worker()
        {
            var random = new Random();

            while (true)
            {
                try
                {
                    using (var req = new HttpRequest
                    {
                        Proxy = scraper.Next()
                    })
                    {
                        HttpResponse res;

                        req.UserAgentRandomize();

                        res = req.Get($"https://www.youtube.com/watch?v={id}");

                        var sres = res.ToString();
                        var viewersTemp = string.Join("",
                            RegularExpressions.Viewers.Match(sres).Groups[1].Value.Where(c => char.IsDigit(c)));

                        if (!string.IsNullOrEmpty(viewersTemp))
                            viewers = viewersTemp;

                        title = RegularExpressions.Title.Match(sres).Groups[1].Value;

                        var url = RegularExpressions.ViewUrl.Match(sres).Groups[1].Value;
                        url = url.Replace(@"\u0026", "&").Replace("%2C", ",").Replace(@"\/", "/");

                        var query = HttpUtility.ParseQueryString(url);

                        var cl = query.Get(query.AllKeys[0]);
                        var ei = query.Get("ei");
                        var of = query.Get("of");
                        var vm = query.Get("vm");

                        var buffer = new byte[100];

                        random.NextBytes(buffer);

                        var cpn = RegularExpressions.Trash.Replace(Convert.ToBase64String(buffer), "").Substring(0, 16);

                        var st = random.Next(1000, 10000);
                        var et = st + random.Next(200, 700);

                        var rt = random.Next(10, 200);

                        var lact = random.Next(1000, 8000);
                        var rtn = rt + 300;

                        var args = new Dictionary<string, string>();

                        args["ns"] = "yt";
                        args["el"] = "detailpage";
                        args["cpn"] = cpn;
                        args["docid"] = id;
                        args["ver"] = "2";
                        args["cmt"] = et.ToString();
                        args["ei"] = ei;
                        args["fmt"] = "243";
                        args["fs"] = "0";
                        args["rt"] = rt.ToString();
                        args["of"] = of;
                        args["euri"] = "";
                        args["lact"] = lact.ToString();
                        args["live"] = "dvr";
                        args["cl"] = cl;
                        args["state"] = "playing";
                        args["vm"] = vm;
                        args["volume"] = "100";
                        args["cbr"] = "Firefox"; //TODO: parse from header
                        args["cbrver"] = "83.0"; // ^
                        args["c"] = "WEB";
                        args["cplayer"] = "UNIPLAYER";
                        args["cver"] = "2.20201210.01.00";
                        args["cos"] = "Windows";
                        args["cosver"] = "10.0";
                        args["cplatform"] = "DESKTOP";
                        args["delay"] = "5";
                        args["hl"] = "en_US";
                        args["rtn"] = rtn.ToString();
                        args["aftm"] = "140";
                        args["rti"] = rt.ToString();
                        args["muted"] = "0";
                        args["st"] = st.ToString();
                        args["et"] = et.ToString();

                        var urlToGet = buildUrl(args);

                        req.AcceptEncoding = "gzip, deflate";
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
    }
}