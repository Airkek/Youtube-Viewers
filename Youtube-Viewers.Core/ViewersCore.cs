using System;
using System.Linq;
using System.Text;
using System.Web;
using Leaf.xNet;
using Youtube_Viewers.Core.Enums;
using Youtube_Viewers.Core.Objects;
using Youtube_Viewers.Core.Utils;
using HttpRequest = Leaf.xNet.HttpRequest;

namespace Youtube_Viewers.Core
{
    public class ViewersCore
    {
        public readonly string Stream;
        public readonly string StreamUrl;

        private readonly HttpRequest Request = new HttpRequest
        {
            Cookies = new CookieStorage()
        };
        
        private readonly Random Random = new Random();

        public ProxyClient Proxy
        {
            get => Request.Proxy;
            set => Request.Proxy = value;
        }
        
        public ViewersCore(string streamId)
        {
            Stream = streamId;
            StreamUrl = $"https://www.youtube.com/watch?v={streamId}";
        }

        public ViewersCore(string streamId, ProxyClient proxy) : this(streamId)
        {
            Proxy = proxy;
        }

        public VideoStats Stats = new VideoStats(string.Empty, 0);

        /// <summary>
        /// Метод будет вызываться в потоках боттера, очевидно будут накручивать просмотры
        /// </summary>
        /// <returns>WorkerResponseStatus</returns>
        public WorkerResponseStatus IncreaseViews()
        {
            Request.ClearAllHeaders();
            Request.Cookies.Clear();
            Request.UserAgentRandomize();

            try
            {
                var urls = GetUrls();

                foreach (var url in urls)
                    Request.Get(url); 
            }
            catch
            {
                return WorkerResponseStatus.Failed;
            }

            return WorkerResponseStatus.Botted;
        }

        /// <summary>
        /// Метод для получения данных для сборки url
        /// </summary>
        /// <returns>string[]
        /// {
        ///     playbackUrl,
        ///     watchtimeUrl
        /// }</returns>
        private string[] GetUrls()
        {
            var response = Request.Get(StreamUrl).ToString();

            var viewers = string.Join(string.Empty, RegularExpressions.Viewers.Match(response).Groups[1].Value.Where(char.IsDigit));
            var title = RegularExpressions.Title.Match(response).Groups[1].Value;

            if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrEmpty(viewers))
                Stats = new VideoStats(title, int.Parse(viewers));
            
            var start = DateTime.UtcNow;
            
            var originalWatchtimeUrl = RegularExpressions.WatchtimeUrl.Match(response).Groups[1].Value;
            originalWatchtimeUrl = originalWatchtimeUrl.Replace(@"\u0026", "&").Replace("%2C", ",").Replace(@"\/", "/");
            originalWatchtimeUrl = originalWatchtimeUrl.Split('?')[1];
            
            var query = HttpUtility.ParseQueryString(originalWatchtimeUrl);

            var cl = query.Get("cl");
            var ei = query.Get("ei");
            var of = query.Get("of");
            var vm = query.Get("vm");
            
            var cpn = GenerateCPN();
            var et = GenerateCmt(start);
            var lio = GenerateLio(start);

            return BuildUrls(cl, ei, of, vm, cpn, et, lio);
        }

        /// <summary>
        /// Метод для сборки необходимых для работы url
        /// </summary>
        /// <returns>string[]
        /// {
        ///     playbackUrl,
        ///     watchtimeUrl
        /// }</returns>
        private string[] BuildUrls(string cl, string ei, string of, string vm, string cpn, double et, double lio)
        {
            var reqParams = new RequestParams()
            {
                ["ns"] = "yt",
                ["el"] = "detailpage",
                ["cpn"] = cpn,
                ["docid"] = Stream,
                ["ver"] = "2",
                ["cmt"] = et.ToString(),
                ["ei"] = ei,
                ["fmt"] = "243",
                ["fs"] = "0",
                ["rt"] = Random.Next(10, 200).ToString(),
                ["of"] = of,
                ["euri"] = "",
                ["lact"] = Random.Next(1000, 8000).ToString(),
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
                ["rtn"] = Random.Next(200, 500).ToString(),
                ["aftm"] = "140",
                ["rti"] = Random.Next(10, 200).ToString(),
                ["muted"] = "0",
                ["st"] = Random.Next(1000, 10000).ToString(),
                ["et"] = et.ToString(),
                ["lio"] = lio.ToString()
            };

            var sb = new StringBuilder();

            foreach (var param in reqParams)
            {
                sb.Append(param.Key);
                sb.Append('=');
                sb.Append(param.Value);
                sb.Append('&');
            }

            var query = sb.ToString();

            return new[]
            {
                "https://s.youtube.com/api/stats/playback?" + query,
                "https://s.youtube.com/api/stats/watchtime?" + query,
            };
        }
        
        /// <summary>
        /// Генерирует значение Cmt
        /// </summary>
        /// <param name="date">Datetime.UtcNow</param>
        /// <returns>Cmt</returns>
        public static double GenerateCmt(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var start = date.ToUniversalTime() - origin;
            var now = DateTime.UtcNow.ToUniversalTime() - origin;
            var value = (now.TotalSeconds - start.TotalSeconds).ToString("#.000");
            return double.Parse(value);
        }

        /// <summary>
        /// Генерирует значение Lio
        /// </summary>
        /// <param name="date">Datetime.UtcNow</param>
        /// <returns>Lio</returns>
        public static double GenerateLio(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var start = date.ToUniversalTime() - origin;
            var value = start.TotalSeconds.ToString("#.000");
            return double.Parse(value);
        }
        
        /// <summary>
        /// Генерирует значение CPN
        /// </summary>
        /// <returns>CPN</returns>
        public string GenerateCPN()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
            return new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}