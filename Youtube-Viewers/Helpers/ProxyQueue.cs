using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Leaf.xNet;

namespace Youtube_Viewers.Helpers
{
    class ProxyQueue
    {
        ConcurrentQueue<ProxyClient> proxies;
        ProxyClient[] plist;

        static Regex Proxy_re = new Regex(@"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?):[0-9]{1,5}\b", RegexOptions.Compiled);

        object locker = new object();

        public int Count => proxies.Count;
        public int Length => plist.Length;

        public ProxyType Type { get; private set; }

        private List<string> GetProxies(string str)
        {
            HashSet<string> res = new HashSet<string>();
            
            foreach (Match proxy in Proxy_re.Matches(str))
            {
                try
                {
                    if (!res.Contains(proxy.Value))
                        res.Add(proxy.Value);
                }
                catch { }
            }
            
            return new List<string>(res);
        }

        public void SafeUpdate(string pr)
        {
            lock (locker)
            {
                List<ProxyClient> prxs = new List<ProxyClient>();
                GetProxies(pr).ForEach(x => prxs.Add(ProxyClient.Parse(Type, x)));
                plist = prxs.ToArray();
            }
        }

        public ProxyQueue(string pr, ProxyType type)
        {
            Type = type;
            SafeUpdate(pr);
            proxies = new ConcurrentQueue<ProxyClient>(plist);
        }

        public ProxyClient Next()
        {
            if (proxies.Count == 0)
            {
                lock (locker)
                {
                    if (proxies.Count == 0)
                        proxies = new ConcurrentQueue<ProxyClient>(plist);
                }
            }

            ProxyClient res;

            if (proxies.TryDequeue(out res))
                return res;
            else
                return Next();
        }
    }
}
