﻿using System;
using System.Threading.Tasks;
using FluentScheduler;
using Microsoft.Extensions.Configuration;
using WeeklyReddit.Services;

namespace WeeklyReddit
{
  public class Program
  {
    public static void Main()
    {
      try
      {
        var builder = new ConfigurationBuilder();
        builder.AddJsonFile(@"C:\dev\secrets\WeeklyReddit\appsettings.json", true); // dev settings.
        builder.AddEnvironmentVariables();

        var configuration = builder.Build();

        JobManager.JobException += x => Log($"An unhandled exception occurred.{Environment.NewLine}{x.Exception}");
        JobManager.UseUtcTime();

        Log("Weekly Reddit Started!");

        var registry = new Registry();
        registry.Schedule(() =>
            GenerateNewsletterAsync(configuration).ConfigureAwait(false).GetAwaiter().GetResult()).ToRunEvery(0)
          .Weeks()
          .On(DayOfWeek.Friday).At(12, 0);

        JobManager.Initialize(registry);
        JobManager.Start();
      }
      catch (Exception exception)
      {
        Log("Unhandled exception!");
        Log(exception.ToString());
      }

      Log("Press enter to quit.");
      Console.ReadLine();

      JobManager.Stop();
      Log("Program ended.");
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
