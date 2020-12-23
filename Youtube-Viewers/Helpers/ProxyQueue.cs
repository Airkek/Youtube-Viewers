using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Leaf.xNet;

namespace Youtube_Viewers.Helpers
{
    class ProxyQueue
    {
        ConcurrentQueue<ProxyClient> proxies;
        ProxyClient[] plist;

        object locker = new object();

        public int Count => proxies.Count;
        public int Length => plist.Length;

        public ProxyType Type { get; private set; }

        private List<string> GetProxies(string str)
        {
            HashSet<string> res = new HashSet<string>();
            
            foreach (string proxy in MatchAndFormatProxies(str))
            {
                try
                {
                    if (!res.Contains(proxy))
                        res.Add(proxy);
                }
                catch { }
            }
            
            return new List<string>(res);
        }

        private List<string> MatchAndFormatProxies(string str)
        {
            List<string> res = new List<string>();

            string[] list = str.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);

            foreach (string lineStock in list)
            {
                string line = lineStock.Trim();
                string[] lineSplit = line.Split(':');
                if (lineSplit.Length >= 2 || lineSplit.Length <= 4)
                {
                    string formatted = String.Empty;

                    if (line.Contains("@"))
                    {
                        lineSplit = line.Split('@');
                        string userPass = lineSplit[0];
                        string address = lineSplit[1];

                        formatted = $"{Type.ToString().ToLower()}://{address}:{userPass}";
                    }
                    else
                    {
                        if (lineSplit[0].Contains(".") && lineSplit[0].Split('.').Length == 4)
                            formatted = $"{Type.ToString().ToLower()}://{line}";
                        else if (lineSplit[2].Contains(".") && lineSplit[0].Split('.').Length == 4)
                            formatted = $"{Type.ToString().ToLower()}://{lineSplit[2]}:{lineSplit[3]}:{lineSplit[0]}:{lineSplit[1]}";
                    }

                    if (!string.IsNullOrEmpty(formatted))
                        res.Add(formatted);
                }
            }

            return res;
        }

        public void SafeUpdate(string pr)
        {
            lock (locker)
            {
                List<ProxyClient> prxs = new List<ProxyClient>();
                GetProxies(pr).ForEach(x => prxs.Add(ProxyClient.Parse(x)));
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

            if (proxies.TryDequeue(out res) && res != null)
                return res;
            else
                throw new HttpException();
        }
    }
}
