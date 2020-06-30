using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Util;
using Youtube_Viewers.Helpers;

namespace Youtube_Viewers
{
    class Program
    {
        static string id;
        static int threadsCount;
        static ProxyScraper scraper;
        static Random random = new Random();
        static Object locker = new Object();
        static Object loglocker = new Object();
        static int botted = 0;
        static int errors = 0;


        static string intro = @"/_/\/_/\   /________/\ /_______/\     /_____/\     /________/\ 
\ \ \ \ \  \__.::.__\/ \::: _  \ \    \:::_ \ \    \__.::.__\/ 
 \:\_\ \ \    \::\ \    \::(_)  \/_    \:\ \ \ \      \::\ \   
  \::::_\/     \::\ \    \::  _  \ \    \:\ \ \ \      \::\ \  
    \::\ \      \::\ \    \::(_)  \ \    \:\_\ \ \      \::\ \ 
     \__\/       \__\/     \_______\/     \_____\/       \__\/ ";

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(intro);
            
            scraper = new ProxyScraper();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Enter Video ID: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            id = Console.ReadLine().Trim();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Enter Threads Count: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            threadsCount = Convert.ToInt32(Console.ReadLine().Trim());

            Console.ForegroundColor = ConsoleColor.Green;

            List<Thread> threads = new List<Thread>();

            for(int i = 0; i < threadsCount; i++)
            {
                Thread t = new Thread(worker);
                t.Start();
                threads.Add(t);
            }

            Console.ReadKey();
        }

        static void log()
        {
            Console.Clear();
            Console.WriteLine(intro + $"\r\n\r\nBotted: {botted}\r\nErrors: {errors}\r\nProxies: {scraper.Proxies.Count}\r\nThreads: {threadsCount}");
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
                        int time = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;


                        lock (locker)
                        {
                            if (scraper.Time < time - 300) 
                                scraper.Update();
                            proxy = scraper.Proxies[random.Next(0, scraper.Proxies.Count - 1)];
                        }

                        req.Proxy = proxy.Http;
                        req.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 12_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.1 Mobile/15E148 Safari/604.1";

                        res = req.Get($"https://m.youtube.com/watch?v={id}");

                        url = res.ToString().Split(new string[] { "videostatsWatchtimeUrl\\\":{\\\"baseUrl\\\":\\\"" }, StringSplitOptions.None)[1];
                        url = url.Split(new string[] { "\\\"}" }, StringSplitOptions.None)[0];
                        url = url.Replace(@"\\u0026", "&").Replace("%2C", ",").Replace(@"\/", "/");

                        cl = url.Split(new string[] { "cl=" }, StringSplitOptions.None)[1].Split('&')[0];
                        ei = url.Split(new string[] { "ei=" }, StringSplitOptions.None)[1].Split('&')[0];
                        of = url.Split(new string[] { "of=" }, StringSplitOptions.None)[1].Split('&')[0];
                        vm = url.Split(new string[] { "vm=" }, StringSplitOptions.None)[1].Split('&')[0];

                        urlToGet = $"https://s.youtube.com/api/stats/watchtime?ns=yt&el=detailpage&cpn=isWmmj2C9Y2vULKF&docid={id}&ver=2&cmt=7334&ei={ei}&fmt=133&fs=0&rt=1003&of={of}&euri&lact=4418&live=dvr&cl={cl}&state=playing&vm={vm}&volume=100&c=MWEB&cver=2.20200313.03.00&cplayer=UNIPLAYER&cbrand=apple&cbr=Safari%20Mobile&cbrver=12.1.15E148&cmodel=iphone&cos=iPhone&cosver=12_2&cplatform=MOBILE&delay=5&hl=ru&cr=GB&rtn=1303&afmt=140&lio=1556394045.182&idpj=&ldpj=&rti=1003&muted=0&st=7334&et=7634";

                        req.AddHeader("Referrer", $"https://m.youtube.com/watch?v={id}");

                        req.Get(urlToGet);
                        
                        lock (loglocker)
                        {
                            botted++;
                            log();
                        }
                    }
                }
                catch (Exception) 
                {
                    lock (locker)
                    {
                        errors++;
                    }
                    
                }
            }
        }
    }
}
