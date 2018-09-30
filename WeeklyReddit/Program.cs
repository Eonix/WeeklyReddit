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
        private static string configPath;

        public static void Main(string[] args)
        {
            if (args.Contains("-r")) // Run now
                runOnStart = true;

            if (args.Contains("-c")) // Config path
                configPath = args[Array.IndexOf(args, "-c") + 1];

            AppHost.RunAndBlock(Start);

            JobManager.Stop();
        }

        private static void Start()
        {
            var builder = new ConfigurationBuilder();
            if (string.IsNullOrWhiteSpace(configPath))
                builder.AddJsonFile(Path.Combine(Environment.CurrentDirectory, "appsettings.json"));
            else
                builder.AddJsonFile(configPath);

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
            const string title = "Weekly Reddit";

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

                var subredditBatches = subreddits.Batch(50).ToList();
                for (int i = 1; i <= subredditBatches.Count; i++)
                {
                    var subredditBatch = subredditBatches[i - 1];
                    var countString = subredditBatches.Count < 2 ? string.Empty : $"({i}/{subredditBatches.Count}) ";

                    Log($"Generating email {countString}...");
                    var html = DataFormatter.GenerateHtml(new FormatterOptions
                    {
                        Subreddits = subredditBatch,
                        Title = title,
                        IssueDate = DateTime.Today,
                        Trendings = trendings
                    });

                    var emailOptions = new EmailOptions
                    {
                        Password = configuration["smtpSettings:password"],
                        Username = configuration["smtpSettings:username"],
                        SmtpServer = configuration["smtpSettings:server"],
                        SmtpPort = Convert.ToInt32(configuration["smtpSettings:port"]),
                    };

                    Log($"Sending email {countString}...");
                    using (var emailClient = new EmailClient(emailOptions))
                    {
                        await emailClient.SendAsync(new EmailContent
                        {
                            Content = html,
                            Subject = $"{title} for {redditOptions.Username} {countString}// {DateTime.Today.ToLongDateString()}",
                            FromName = title,
                            FromAddress = configuration["emailSettings:fromAddress"],
                            To = configuration["emailSettings:toAddress"]
                        });
                    }
                }
            }

            Log("Newsletter sent!");
        }
    }
}
