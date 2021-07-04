namespace Youtube_Viewers_Re.Core.Objects
{
    public class VideoStats
    {
        public readonly int Viewers;
        public readonly string Title;

        public VideoStats(string title, int viewers)
        {
            Title = title;
            Viewers = viewers;
        }
    }
}