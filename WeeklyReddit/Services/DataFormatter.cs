using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WeeklyReddit.Services
{
    public class DataFormatter
    {
        public static async Task<string> GenerateHtmlAsync(FormatterOptions options)
        {
            var contentBuilder = new StringBuilder();

            contentBuilder.AppendLine(
                $"<h2 style=\"font-family:ubuntu, 'Lucida Grande', Arial, sans-serif;padding-top:20px;padding-bottom:0;padding-right:0;padding-left:0;color:#369;font-size:16px;font-weight:bold;margin-top:20px;margin-bottom:3px;margin-right:0;margin-left:0;text-transform:uppercase;\"><span style=\"color:#333\">#</span>trending</h2>");
            contentBuilder.AppendLine(
                "<hr style=\"border-style:none;margin-top:0px;margin-bottom:5px;margin-right:0;margin-left:0;border-top-width:1px;border-top-style:solid;border-top-color:#9b9b9b;\" />");

            foreach (var trending in options.Trendings)
            {
                contentBuilder.AppendLine(
                    $"<p style=\"font-family:ubuntu, Helvetica, 'Lucida Grande', Arial, sans-serif;padding-top:0;padding-bottom:0;padding-right:0;padding-left:0;color:#363636;font-size:16px;line-height:22px;margin-top:0;margin-bottom:10px;margin-right:0;margin-left:0;width:100%;\"><a href=\"{trending.Url}\" target=\"_blank\" style=\"color:#0446AB;text-decoration:underline; font-size:17px;\">{trending.Title}</a></p>");
            }

            foreach (var subreddit in options.Subreddits)
            {
                contentBuilder.AppendLine(
                    $"<h2 style=\"font-family:ubuntu, 'Lucida Grande', Arial, sans-serif;padding-top:20px;padding-bottom:0;padding-right:0;padding-left:0;color:#369;font-size:16px;font-weight:bold;margin-top:20px;margin-bottom:3px;margin-right:0;margin-left:0;text-transform:uppercase;\"><span style=\"color:#333\">#</span>{subreddit.Name}</h2>");
                contentBuilder.AppendLine(
                    "<hr style=\"border-style:none;margin-top:0px;margin-bottom:5px;margin-right:0;margin-left:0;border-top-width:1px;border-top-style:solid;border-top-color:#9b9b9b;\" />");

                if (!subreddit.TopPosts.Any())
                {
                    contentBuilder.AppendLine("<p style=\"font-family:ubuntu, Helvetica, 'Lucida Grande', Arial, sans-serif;padding-top:0;padding-bottom:0;padding-right:0;padding-left:0;color:#363636;font-size:16px;line-height:22px;margin-top:0;margin-bottom:10px;margin-right:0;margin-left:0;width:100%;\">No posts this week. :(</p>");
                }

                foreach (var post in subreddit.TopPosts)
                {
                    var commentsLink = post.CommentsUrl != post.Url
                        ? $"<br><span style=\"font-size: 13px; color: #777\"><span style=\"font-size:11px;padding-right:1px;\">//</span> <a style=\"text-decoration: none; color: #336699;\" href=\"{post.CommentsUrl}\" target=\"_blank\">comments</a></span>"
                        : null;

                    contentBuilder.AppendLine(
                        $"<p style=\"font-family:ubuntu, Helvetica, 'Lucida Grande', Arial, sans-serif;padding-top:0;padding-bottom:0;padding-right:0;padding-left:0;color:#363636;font-size:16px;line-height:22px;margin-top:0;margin-bottom:10px;margin-right:0;margin-left:0;width:100%;\"><a href=\"{post.Url}\" target=\"_blank\" style=\"color:#0446AB;text-decoration:underline; font-size:17px;\">{post.Title}</a>{commentsLink}</p>");
                }
            }

            var templateBuilder = new StringBuilder(await GetTemplateFileContentsAsync());

            templateBuilder.Replace("{Content}", contentBuilder.ToString());
            templateBuilder.Replace("{Title}", options.Title);
            templateBuilder.Replace("{IssueDate}", options.IssueDate.ToLongDateString());

            return templateBuilder.ToString();
        }

        private static async Task<string> GetTemplateFileContentsAsync()
        {
            var assembly = typeof(DataFormatter).GetTypeInfo().Assembly;
            var resourceStream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.template.html");
            using (var streamReader = new StreamReader(resourceStream))
            {
                return await streamReader.ReadToEndAsync();
            }
        }
    }
}