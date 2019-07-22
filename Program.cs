using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentScheduler;
using Newtonsoft.Json;
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
            else
                configPath = Path.Combine(Environment.CurrentDirectory, "appsettings.json");

            AppHost.RunAndBlock(Start);

            JobManager.Stop();
        }

        private static void Start()
        {
            var settings = JsonConvert.DeserializeObject<AppSettings>(configPath);

            JobManager.JobException += x => Log($"An unhandled exception occurred.{Environment.NewLine}{x.Exception}");

            var registry = new Registry();
            registry.Schedule(() =>
                    GenerateNewsletterAsync(settings).ConfigureAwait(false).GetAwaiter().GetResult()).ToRunEvery(0)
                .Weeks()
                .On(DayOfWeek.Friday).At(12, 0);

            JobManager.Initialize(registry);
            JobManager.Start();
            Log("Weekly Reddit Started!");

            if (runOnStart)
                GenerateNewsletterAsync(settings).Wait();
        }

        private static void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now}: {message}");
        }

        private static async Task GenerateNewsletterAsync(AppSettings settings)
        {
            Log("Generating newsletter.");
            const string title = "Weekly Reddit";

            var redditOptions = new RedditOptions
            {
                Username = settings.Reddit.Username,
                Password = settings.Reddit.Password,
                ClientId = settings.Reddit.ClientId,
                ClientSecret = settings.Reddit.ClientSecret
            };

            Log("Fetching content...");
            using (var redditClient = await RedditClient.CreateAsync(redditOptions))
            {
                var trendings = await redditClient.GetTrendings();
                var subreddits = await redditClient.GetSubredditsTopPosts();

                var subredditBatches = subreddits.Batch(50).ToList();
                for (int i = 0; i < subredditBatches.Count; i++)
                {
                    var subredditBatch = subredditBatches[i];
                    var countString = subredditBatches.Count < 2 ? string.Empty : $"({i + 1}/{subredditBatches.Count}) ";

                    Log($"Generating email {countString}...");
                    var html = DataFormatter.GenerateHtml(new FormatterOptions
                    {
                        Subreddits = subredditBatch,
                        Title = title,
                        IssueDate = DateTime.Today,
                        Trendings = i == 0 ? trendings : Enumerable.Empty<RedditPost>()
                    });

                    var emailOptions = new EmailOptions
                    {
                        Password = settings.SmtpSettings.Password,
                        Username = settings.SmtpSettings.Username,
                        SmtpServer = settings.SmtpSettings.Server,
                        SmtpPort = settings.SmtpSettings.Port
                    };

                    Log($"Sending email {countString}...");
                    using (var emailClient = new EmailClient(emailOptions))
                    {
                        await emailClient.SendAsync(new EmailContent
                        {
                            Content = html,
                            Subject = $"{title} for {redditOptions.Username} {countString}// {DateTime.Today.ToLongDateString()}",
                            FromName = title,
                            FromAddress = settings.EmailSettings.FromAddress,
                            To = settings.EmailSettings.ToAddress
                        });
                    }
                }
            }

            Log("Newsletter sent!");
        }
    }
}
