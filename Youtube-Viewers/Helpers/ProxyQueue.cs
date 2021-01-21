using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Leaf.xNet;

namespace Youtube_Viewers.Helpers
{
    internal class ProxyQueue
    {
        private readonly object locker = new object();
        private ProxyClient[] plist;
        private ConcurrentQueue<ProxyClient> proxies;

        public ProxyQueue(string pr, ProxyType type)
        {
            Type = type;
            SafeUpdate(pr);
            proxies = new ConcurrentQueue<ProxyClient>(plist);
        }

        public int Count => proxies.Count;
        public int Length => plist.Length;

        public ProxyType Type { get; }

        private List<string> GetProxies(string str)
        {
            var res = new HashSet<string>();

            foreach (var proxy in MatchAndFormatProxies(str))
                try
                {
                    if (!res.Contains(proxy))
                        res.Add(proxy);
                }
                catch
                {
                    // ignored
                }

            return new List<string>(res);
        }

        private List<string> MatchAndFormatProxies(string str)
        {
            var res = new List<string>();

            var list = str.Split(new[] {"\n", "\r\n"}, StringSplitOptions.None);

            foreach (var lineStock in list)
            {
                var line = lineStock.Trim();

                try
                {
                    var formatted = FormatLine(line);
                    if (!string.IsNullOrEmpty(formatted))
                        res.Add(formatted);
                }
                catch
                {
                    // ignored
                }
            }

            return res;
        }

        private string FormatLine(string line)
        {
            var lineSplit = line.Split(':');
            if (lineSplit.Length < 2 || lineSplit.Length > 4) return string.Empty;

            var formatted = string.Empty;

            if (line.Contains("@") && lineSplit.Length == 3)
            {
                lineSplit = line.Split('@');
                var userPass = lineSplit[0];
                var address = lineSplit[1];

                var port = int.Parse(address.Split(':')[1]);

                if (port > 65535 || port < 1)
                    return string.Empty;

                formatted = $"{Type.ToString().ToLower()}://{address}:{userPass}";
            }
            else
            {
                if (lineSplit[0].Contains(".") && lineSplit[0].Split('.').Length == 4)
                {
                    var port = int.Parse(lineSplit[1]);

                    if (port > 65535 || port < 1)
                        return string.Empty;

                    formatted = $"{Type.ToString().ToLower()}://{line}";
                }
                else if (lineSplit.Length == 4 && lineSplit[2].Contains(".") && lineSplit[0].Split('.').Length == 4)
                {
                    var port = int.Parse(lineSplit[3]);

                    if (port > 65535 || port < 1)
                        return string.Empty;

                    formatted =
                        $"{Type.ToString().ToLower()}://{lineSplit[2]}:{lineSplit[3]}:{lineSplit[0]}:{lineSplit[1]}";
                }
            }

            return formatted;
        }

        public void SafeUpdate(string pr)
        {
            lock (locker)
            {
                var prxs = new List<ProxyClient>();
                GetProxies(pr).ForEach(x => prxs.Add(ProxyClient.Parse(x)));
                plist = prxs.ToArray();
            }
        }

        public ProxyClient Next()
        {
            if (proxies.Count == 0)
                lock (locker)
                {
                    if (proxies.Count == 0)
                        proxies = new ConcurrentQueue<ProxyClient>(plist);
                }

            ProxyClient res;

            if (proxies.TryDequeue(out res) && res != null)
                return res;
            throw new HttpException();
        }
    }
}