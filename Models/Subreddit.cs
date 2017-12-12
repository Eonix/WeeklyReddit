using System.Collections.Generic;

namespace WeeklyReddit
{
    public class Subreddit
    {
        public string Name { get; set; }
        public IEnumerable<RedditPost> TopPosts { get; set; }
    }
}