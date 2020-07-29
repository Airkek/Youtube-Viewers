using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions; //RegEx
using Leaf.xNet;

namespace Youtube_Viewers.Helpers
{
    class Proxy
    {
        public string Address { get; private set; }
        public HttpProxyClient Http { get; private set; }
        public Socks4ProxyClient Socks4 { get; private set; }
        public Socks5ProxyClient Socks5 { get; private set; }

        public Proxy(string proxy)
        {
            bool Match = Regex.IsMatch(proxy.Trim(), string.Join(string.Empty, "^([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])$:\\d{2,5}"));

            if (Match == true)
            {
            Address = proxy.Trim();
            Http = HttpProxyClient.Parse(Address);
            Socks4 = Socks4ProxyClient.Parse(Address);
            Socks5 = Socks5ProxyClient.Parse(Address);
            }
        }

        public static List<Proxy> GetList(List<string> list)
        {
            List<Proxy> proxies = new List<Proxy>();
            foreach(string proxy in list)
            {
                if (proxy.Split(':').Length != 2 || proxy.Split(':')[0].Split('.').Length != 4)
                    continue;
                try
                {
                    proxies.Add(new Proxy(proxy));
                }
                catch (Exception) { }
            }

            return proxies;
        }
    }
}
