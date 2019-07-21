using System.Collections.Generic;
using System.Linq;
using MailBodyPack;

namespace WeeklyReddit.Services
{
    public static class DataFormatter
    {
        private static MailBodyTemplate GetCustomTemplate()
        {
            return MailBodyTemplate.GetDefaultTemplate()
                .Title(x => $"<h1 style=\"color:#333;margin: 0; padding: 20px 0 0 0;\">{x.Content}</h1>")
                .Text(x => x.Attributes?.Id == "issue-date"
                    ? $"<span style=\"color:#333;font-size:12px;\">{x.Content}</span>"
                    : x.Content)
                .SubTitle(x =>
                    x.Attributes?.Id != "section-title"
                        ? $"<h2>{x.Content}</h2>"
                        : $"<h2 style=\"color:#369;font-size:16px;margin-bottom:3px;text-transform:uppercase;\">{x.Content}</h2>")
                .LineBreak(x =>
                    x.Attributes?.Id != "hr"
                        ? "<br />"
                        : "<hr style=\"border-style:none;margin-top:0px;margin-bottom:5px;border-top-style:solid;border-top-color:#9b9b9b;\" />")
                .Link(CustomLink);
        }

        private static MailBlockFluent AddSection(this MailBlockFluent body, string sectionTitle, IEnumerable<RedditPost> posts)
        {
            if (!posts.Any())
                return body;

            var sectionHeader = MailBody.CreateBlock()
                .SubTitle($"#{sectionTitle}", new { Id = "section-title" })
                .LineBreak(new { Id = "hr" });

            var imagesBlock = MailBody.CreateBlock();
            sectionHeader.Paragraph(imagesBlock);

            foreach (var post in posts)
            {
                if (post.ThumbnailUrl != null)
                    imagesBlock.Raw($"<img src=\"{post.ThumbnailUrl}\"style=\"margin: 3px;\" />");

                var postTitle = post.Nsfw ? $"{post.Title} [NSFW]" : post.Title;
                var postLink = MailBody.CreateBlock()
                    .Link(post.CommentsUrl, postTitle, new { Id = "post-link", Hint = $"Score: {post.Score} Comments: {post.Comments}" });

                sectionHeader.Paragraph(postLink);
            }

            return body.Paragraph(sectionHeader);
        }

        public static string GenerateHtml(FormatterOptions options)
        {
            var template = GetCustomTemplate();

            var body = MailBody.CreateBody(template)
                .Title(options.Title)
                .Text($"Issue {options.IssueDate.ToLongDateString()}", new { Id = "issue-date" })
                .AddSection("trending", options.Trendings);

            foreach (var subreddit in options.Subreddits)
            {
                body.AddSection(subreddit.Name, subreddit.TopPosts);
            }

            return body.ToString();
        }

        private static string CustomLink(ActionElement actionElement)
        {
            if (actionElement.Attributes?.Id == "post-link")
                return $"<a href=\"{actionElement.Link}\" target=\"_blank\" title=\"{actionElement.Attributes?.Hint ?? string.Empty}\">{actionElement.Content}</a>";

            return $"<a href='{actionElement.Link}'>{actionElement.Content}</a>";
        }
    }
}