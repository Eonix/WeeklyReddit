using System;
using System.Collections.Generic;

namespace WeeklyReddit
{
    public class FormatterOptions
    {
        public string Title { get; set; }
        public DateTime IssueDate { get; set; }
        public IEnumerable<Subreddit> Subreddits { get; set; }
        public IEnumerable<RedditPost> Trendings { get; set; }
    }
}