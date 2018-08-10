using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentScheduler;
using Microsoft.Extensions.Configuration;
using WeeklyReddit.Services;

namespace WeeklyReddit
{
    public static class Program
    {
        private static bool runOnStart;

        public static void Main(string[] args)
        {
            if (args.Contains("-run"))
                runOnStart = true;

            AppHost.RunAndBlock(Start);

            JobManager.Stop();
        }

        private static void Start()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(Environment.CurrentDirectory, "appsettings.json"));
            var configuration = builder.Build();

            JobManager.JobException += x => Log($"An unhandled exception occurred.{Environment.NewLine}{x.Exception}");
            JobManager.UseUtcTime();
            
            var registry = new Registry();
            registry.Schedule(() =>
                    GenerateNewsletterAsync(configuration).ConfigureAwait(false).GetAwaiter().GetResult()).ToRunEvery(0)
                .Weeks()
                .On(DayOfWeek.Friday).At(12, 0);

            JobManager.Initialize(registry);
            JobManager.Start();
            Log("Weekly Reddit Started!");

            if (runOnStart)
                GenerateNewsletterAsync(configuration).Wait();
        }

        private static void Log(string message)
        {
            Console.WriteLine($"{DateTime.UtcNow}: {message}");
        }
        
        private static async Task GenerateNewsletterAsync(IConfiguration configuration)
        {
            Log("Generating newsletter.");
            const string title = "Weekly Newsletter of Reddit";

            var redditOptions = new RedditOptions
            {
                Username = configuration["reddit:username"],
                Password = configuration["reddit:password"],
                ClientId = configuration["reddit:clientId"],
                ClientSecret = configuration["reddit:clientSecret"]
            };

            Log("Fetching content...");
            using (var redditClient = await RedditClient.CreateAsync(redditOptions))
            {
                var trendings = await redditClient.GetTrendings();
                var subreddits = await redditClient.GetSubredditsTopPosts();

                var formatterOptions = new FormatterOptions
                {
                    Subreddits = subreddits,
                    Title = title,
                    IssueDate = DateTime.Today,
                    Trendings = trendings
                };

                Log("Generating email...");
                var html = DataFormatter.GenerateHtml(formatterOptions);

                var emailOptions = new EmailOptions
                {
                    Password = configuration["smtpSettings:password"],
                    Username = configuration["smtpSettings:username"],
                    SmtpServer = configuration["smtpSettings:server"],
                    SmtpPort = Convert.ToInt32(configuration["smtpSettings:port"]),
                };

                Log("Sending email...");
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

            Log("Newsletter sent!");
        }
    }
}
