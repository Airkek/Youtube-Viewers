using Leaf.xNet;
using Youtube_Viewers.Core.Enums;
using Youtube_Viewers.Core.Objects;

namespace Youtube_Viewers.Core
{
    public class ViewersCore
    {
        public readonly string Stream;
        public readonly string StreamUrl;

        private readonly HttpRequest Request = new HttpRequest();

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

        public WorkerResponse DoWork()
        {
            Request.ClearAllHeaders();
            Request.Cookies.Clear();
            Request.UserAgentRandomize();

            return new WorkerResponse(string.Empty, int.MinValue, WorkerResponseStatus.Failed);
        }
        
        
    }
}