using Youtube_Viewers.Core.Enums;

namespace Youtube_Viewers.Core.Objects
{
    public class WorkerResponse
    {
        public readonly WorkerResponseStatus Status;
        public readonly int Viewers;
        public readonly string Title;

        internal WorkerResponse(string title, int viewers, WorkerResponseStatus status)
        {
            Title = title;
            Viewers = viewers;
            Status = status;
        }
    }
}