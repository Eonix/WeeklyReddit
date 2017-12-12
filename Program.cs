using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WeeklyReddit.Services;

namespace WeeklyReddit
{
    public class Program
    {
        public static async Task Main()
        {
            try
            {
                var builder = new ConfigurationBuilder();
                builder.AddJsonFile(@"C:\dev\secrets\WeeklyReddit\appsettings.json", true); // dev settings.
                builder.AddEnvironmentVariables();

                var configuration = builder.Build();

                if (configuration.AsEnumerable().Any(pair => pair.Value == "replace-me"))
                {
                    throw new Exception("You forgot to replace one of the environment variables.");
                }

                Console.WriteLine("Weekly Reddit Started!");
                await GenerateNewsletterAsync(configuration);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Unhandled exception!");
                Console.WriteLine(exception);
            }

            Console.WriteLine("Press enter to quit.");
            Console.ReadLine();
            Console.WriteLine("Program ended.");
        }

        private static async Task GenerateNewsletterAsync(IConfiguration configuration)
        {
            const string title = "Weekly Newsletter of Reddit";

            var redditOptions = new RedditOptions
            {
                Username = configuration["reddit:username"],
                Password = configuration["reddit:password"],
                ClientId = configuration["reddit:clientId"],
                ClientSecret = configuration["reddit:clientSecret"]
            };

            using (var redditClient = await RedditClient.CreateAsync(redditOptions))
            {
                var subreddits = await redditClient.GetSubredditsTopPosts();
                var trendings = await redditClient.GetTrendings();

                var formatterOptions = new FormatterOptions
                {
                    Subreddits = subreddits,
                    Title = title,
                    IssueDate = DateTime.Today,
                    Trendings = trendings
                };

                var html = await DataFormatter.GenerateHtmlAsync(formatterOptions);

                var emailOptions = new EmailOptions
                {
                    Password = configuration["smtpSettings:password"],
                    Username = configuration["smtpSettings:username"],
                    SmtpServer = configuration["smtpSettings:server"],
                    SmtpPort = Convert.ToInt32(configuration["smtpSettings:port"]),
                };

                using (var emailClient = new EmailClient(emailOptions))
                {
                    var content = new EmailContent
                    {
                        Content = html,
                        Subject = $"{title} // {DateTime.Today.ToLongDateString()}",
                        FromName = title,
                        FromAddress = configuration["emailSettings:fromAddress"],
                        To = configuration["emailSettings:toAddress"]
                    };
                    await emailClient.SendAsync(content);
                }
            }
        }
    }
}
