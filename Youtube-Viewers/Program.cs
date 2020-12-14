using System;
using System.Collections.Generic;
using System.Threading;
using Leaf.xNet;
using Youtube_Viewers.Helpers;
using static Youtube_Viewers.Helpers.UsedProxyType;
using System.Windows.Forms;

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

        static string viewers = "Parsing...";
        static string title = "Parsing...";

        static object locker = new object();

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
            Console.Title = $"YTBot | {gitRepo}";
            Logo(ConsoleColor.Cyan);

            id = dialog("Enter Video ID");
            threadsCount = Convert.ToInt32(dialog("Enter Threads Count"));

            while (true)
            {
                Logo(ConsoleColor.Cyan);

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
                Console.Write("Path to proxy list");

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Proxy list|*.txt";
                ofd.ShowDialog();

                scraper = new ProxyScraper(ofd.FileName);
            }

            else
                scraper = new ProxyScraper();

            Logo(ConsoleColor.Green);

            List<Thread> threads = new List<Thread>();

            Thread logWorker = new Thread(Log);
            logWorker.Start();
            threads.Add(logWorker);

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
                Console.WriteLine($"\r\nBotted: {botted}\r\nErrors: {errors}\r\nProxies: {scraper.Proxies.Count}\r\nThreads: {threadsCount}\r\nTitle: {title}          \r\nViewers: {viewers}          \r\n");
                Thread.Sleep(250);
            }
        }

        static string buildUrl(Dictionary<string, string> args)
        {
            string url = "https://s.youtube.com/api/stats/watchtime?";
            foreach(KeyValuePair<string, string> arg in args)
            {
                url += $"{arg.Key}={arg.Value}&";
            }
            return url;
        }

        static void Worker()
        {
            Random random = new Random();

            while (true)
            {
                try
                {
                    using (HttpRequest req = new HttpRequest())
                    {
                        HttpResponse res;
                        Proxy proxy;

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
                        
                        req.UserAgentRandomize();

                        res = req.Get($"https://www.youtube.com/watch?v={id}");


                        string viewersTemp = res.ToString().Split(new[] { "\"viewCount\":{\"videoViewCountRenderer\":{\"viewCount\":{\"runs\":[{\"text\":\"" }, StringSplitOptions.None)[1].Split(':')[1].Split(new[] { "\"}]}," }, StringSplitOptions.None)[0].Trim();

                        try
                        {
                            int.Parse(viewersTemp.Trim());
                            viewers = viewersTemp;
                        }
                        catch { }

                        title = res.ToString().Split(new[] { "\"title\":{\"runs\":[{\"text\":\"" }, StringSplitOptions.None)[1].Split(new[] { "\"}]}," }, StringSplitOptions.None)[0].Trim();

                        string url = res.ToString().Split(new[] { "videostatsWatchtimeUrl\":{\"baseUrl\":\"" }, StringSplitOptions.None)[1].Split('"')[0];
                        url = url.Replace(@"\u0026", "&").Replace("%2C", ",").Replace(@"\/", "/");

                        string cl = url.Split(new string[] { "cl=" }, StringSplitOptions.None)[1].Split('&')[0];
                        string ei = url.Split(new string[] { "ei=" }, StringSplitOptions.None)[1].Split('&')[0];
                        string of = url.Split(new string[] { "of=" }, StringSplitOptions.None)[1].Split('&')[0];
                        string vm = url.Split(new string[] { "vm=" }, StringSplitOptions.None)[1].Split('&')[0];

                        byte[] buffer = new byte[100];

                        random.NextBytes(buffer);

                        string cpn = Convert.ToBase64String(buffer).Replace("=", "").Replace("/", "").Replace("+", "").Substring(0, 16);

                        int st = random.Next(1000, 10000);
                        int et = st + random.Next(200, 700);

                        int rt = random.Next(10, 200);

                        int lact = random.Next(1000, 8000);
                        int rtn = rt + 300;

                        Dictionary<string, string> args = new Dictionary<string, string>();

                        args["ns"] = "yt";
                        args["el"] = "detailpage";
                        args["cpn"] = cpn.ToString();
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
                        args["hl"] = "ru_RU";
                        args["rtn"] = rtn.ToString();
                        args["aftm"] = "140";
                        args["lio"] = "1556394045.182";
                        args["idpj"] = "-5";
                        args["ldpj"] = "-8";
                        args["rti"] = rt.ToString();
                        args["muted"] = "0";
                        args["st"] = st.ToString();
                        args["et"] = et.ToString();

                        string urlToGet = buildUrl(args);

                        req.AddHeader("Referrer", $"https://www.youtube.com/watch?v={id}");
                        req.AddHeader("Host", "www.youtube.com");
                        req.AddHeader("Proxy-Connection", "keep-alive");
                        req.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                        req.AddHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                        req.AddHeader("Accept-Encoding", "gzip, deflate");

                        res = req.Get(urlToGet);
                        Interlocked.Increment(ref botted);
                    }
                }
                catch
                {
                    Interlocked.Increment(ref errors);
                }
            }
        }
    }
}
