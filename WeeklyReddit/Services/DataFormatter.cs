using System.Collections.Generic;
using System.Linq;
using MailBodyPack;

namespace WeeklyReddit.Services
{
    public class DataFormatter
    {
        private static MailBodyTemplate GetCustomTemplate()
        {
            return MailBodyTemplate.GetDefaultTemplate()
                .Title(x => $"<h1 style=\"color:#333;margin: 0; padding: 20px 0 0 0;\">{x.Content}</h1>")
                .Text(x => x.Attributes?.id == "issue-date"
                    ? $"<span style=\"color:#333;font-size:12px;\">{x.Content}</span>"
                    : x.Content)
                .SubTitle(x =>
                    x.Attributes?.className != "section-title"
                        ? $"<h2>{x.Content}</h2>"
                        : $"<h2 style=\"color:#369;font-size:16px;margin-bottom:3px;text-transform:uppercase;\">{x.Content}</h2>")
                .LineBreak(x =>
                    x.Attributes?.className != "hr"
                        ? "<br />"
                        : "<hr style=\"border-style:none;margin-top:0px;margin-bottom:5px;border-top-style:solid;border-top-color:#9b9b9b;\" />")
                .Link(CustomLink);
        }

        private static MailBlockFluent GetSection(string sectionTitle, IEnumerable<RedditPost> posts)
        {
            var sectionHeader = MailBody.CreateBlock()
                .SubTitle($"#{sectionTitle}", new {className = "section-title"})
                .LineBreak(new {className = "hr"});

            if (!posts.Any())
            {
                sectionHeader.Paragraph("No posts this week. :(");
                return sectionHeader;
            }

            foreach (var post in posts)
            {
                var postLink = MailBody.CreateBlock().Link(post.Url, post.Title, new {className = "post-link"});
                
                if (post.CommentsUrl != post.Url)
                {
                    postLink.LineBreak().Link(post.CommentsUrl, "// comments", new {className = "comments-link"});
                }
                    
                sectionHeader.Paragraph(postLink);
            }

            return sectionHeader;
        }

        public static string GenerateHtml(FormatterOptions options)
        {
            var template = GetCustomTemplate();

            var body = MailBody.CreateBody(template)
                .Title(options.Title)
                .Text($"Issue {options.IssueDate.ToLongDateString()}", new { id = "issue-date" })
                .Paragraph(GetSection("trending", options.Trendings));

            foreach (var subreddit in options.Subreddits)
            {
                body.Paragraph(GetSection(subreddit.Name, subreddit.TopPosts));
            }

            return body.ToString();
            
        }

        private static string CustomLink(ActionElement actionElement)
        {
            if (actionElement.Attributes?.className == "post-link")
            {
                return $"<a href=\"{actionElement.Link}\" target=\"_blank\">{actionElement.Content}</a>";
            }

            if (actionElement.Attributes?.className == "comments-link")
            {
                return
                    $"<a style=\"font-size: 13px; text-decoration: none; color: #336699;\" href=\"{actionElement.Link}\" target=\"_blank\">{actionElement.Content}</a>";
            }

            return $"<a href='{actionElement.Link}'>{actionElement.Content}</a>";
        }
    }
}