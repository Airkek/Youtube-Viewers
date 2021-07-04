using System.IO;
using Leaf.xNet;
using Newtonsoft.Json;

namespace Youtube_Viewers.Config
{
    public class ConfigScheme
    {
        [JsonIgnore]
        public const string Filename = "YTBot-Config.json";

        [JsonProperty("threads_count")] public int Threads = 300;
        [JsonProperty("proxy_type")] public ProxyType ProxyType = ProxyType.Socks4;
        [JsonProperty("scrape_proxies_from_url")] public bool ParseProxies = true;
        [JsonProperty("scrape_timeout")] public int ScrapeTimeout = 600;

        [JsonProperty("scrape_urls")] public string[] ScrapeUrls = {
            "https://raw.githubusercontent.com/clarketm/proxy-list/master/proxy-list-raw.txt",
            "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks4.txt",
            "https://api.proxyscrape.com/?request=getproxies&proxytype=socks4&timeout=9000&ssl=yes",
            "https://www.proxy-list.download/api/v1/get?type=socks4"
        };
        
        public static ConfigScheme Read()
        {
            ConfigScheme cfg = null;

            if (File.Exists(Filename))
            {
                cfg = JsonConvert.DeserializeObject<ConfigScheme>(File.ReadAllText(Filename));
                cfg.Save();
            }

            return cfg;
        }

        public void Save()
        {
            File.WriteAllText(Filename, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}