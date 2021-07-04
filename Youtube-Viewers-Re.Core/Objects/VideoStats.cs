namespace Youtube_Viewers_Re.Core.Objects
{
    public class VideoStats
    {
        public readonly int Viewers;
        public readonly string Title;
        
        internal VideoStats(string title, int viewers)
        {
            Title = title;
            Viewers = viewers;
        }
    }
}