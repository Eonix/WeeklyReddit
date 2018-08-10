namespace WeeklyReddit
{
    public class RedditPost
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string CommentsUrl { get; set; }
        public string Domain { get; set; }
        public int Score { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool Nsfw { get; set; }
        public int Comments { get; set; }
    }
}