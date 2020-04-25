using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TimeZoneConverter;
using WeeklyReddit.Services;

namespace WeeklyReddit
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Weekly Reddit Started!");

            if (_configuration.GetValue<bool>("now"))
                await GenerateNewsletterAsync();

            var lastSentDate = DateTimeOffset.MinValue;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var targetTimeZone = TZConvert.GetTimeZoneInfo("Romance Standard Time");
                    var convertedTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now.Date.AddHours(16), targetTimeZone, TimeZoneInfo.Local);
                    var now = DateTimeOffset.Now;

                    if (lastSentDate != now.Date && convertedTime.DayOfWeek == DayOfWeek.Friday && convertedTime.Hour == now.Hour)
                    {
                        await GenerateNewsletterAsync();
                        lastSentDate = now.Date;
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "An error occurred.");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task GenerateNewsletterAsync()
        {
            var appSettings = _configuration.Get<AppSettings>();
            _logger.LogInformation("Generating newsletter.");
            const string title = "Weekly Reddit";

            var redditOptions = new RedditOptions
            {
                Username = appSettings.Reddit.Username,
                Password = appSettings.Reddit.Password,
                ClientId = appSettings.Reddit.ClientId,
                ClientSecret = appSettings.Reddit.ClientSecret
            };

            _logger.LogInformation("Fetching content...");
            using var redditClient = await RedditClient.CreateAsync(redditOptions);
            var trendings = await redditClient.GetTrendings();
            var subreddits = await redditClient.GetSubredditsTopPosts();

            var subredditBatches = subreddits.Batch(25).ToList();
            for (int i = 0; i < subredditBatches.Count; i++)
            {
                var subredditBatch = subredditBatches[i];
                var countString = subredditBatches.Count < 2 ? string.Empty : $"({i + 1}/{subredditBatches.Count}) ";

                _logger.LogInformation($"Generating email {countString}...");
                var html = DataFormatter.GenerateHtml(new FormatterOptions
                {
                    Subreddits = subredditBatch,
                    Title = title,
                    IssueDate = DateTime.Today,
                    Trendings = i == 0 ? trendings : Enumerable.Empty<RedditPost>()
                });

                var emailOptions = new EmailOptions
                {
                    Password = appSettings.SmtpSettings.Password,
                    Username = appSettings.SmtpSettings.Username,
                    SmtpServer = appSettings.SmtpSettings.Server,
                    SmtpPort = appSettings.SmtpSettings.Port
                };

                _logger.LogInformation($"Sending email {countString}...");
                using var emailClient = new EmailClient(emailOptions);
                await emailClient.SendAsync(new EmailContent
                {
                    Content = html,
                    Subject = $"{title} for {redditOptions.Username} {countString}// {DateTime.Today.ToLongDateString()}",
                    FromName = title,
                    FromAddress = appSettings.EmailSettings.FromAddress,
                    To = appSettings.EmailSettings.ToAddress
                });
            }

            _logger.LogInformation("Newsletter sent!");
        }
    }
}
