using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Address = proxy.Trim();
            Http = HttpProxyClient.Parse(Address);
            Socks4 = Socks4ProxyClient.Parse(Address);
            Socks5 = Socks5ProxyClient.Parse(Address);
        }

        public static List<Proxy> GetList(List<string> list)
        {
            List<Proxy> proxies = new List<Proxy>();
            foreach(string proxy in list)
            {
                try
                {
                    proxies.Add(new Proxy(proxy));
                }
                catch { }
            }

            return proxies;
        }
    }
}
