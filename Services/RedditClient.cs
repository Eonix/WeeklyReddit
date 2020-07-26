using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WeeklyReddit.Services
{
    public sealed class RedditClient : IDisposable
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private static DateTime _nextRequest = DateTime.Now;
        private const string AppName = "weekly-reddit";
        private const string AppVersion = "1.0";

        private RedditClient() { }

        public static async Task<RedditClient> CreateAsync(RedditOptions options)
        {
            var client = new RedditClient();
            var accessToken = await client.Authorize(options);
            client._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            client._httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(AppName, AppVersion));

            return client;
        }

        private string GetPreview(JsonElement element)
        {
            var url = element.TryGetProperty("preview", out var preview)
                ? preview.TryGetProperty("images", out var images)
                    ? images.EnumerateArray().First().TryGetProperty("source", out var source)
                        ? source.GetProperty("url").GetString()
                        : null
                    : null
                : null;

            if (!string.IsNullOrEmpty(url))
                return WebUtility.HtmlDecode(url);

            var thumbnailUrl = element.GetProperty("thumbnail").GetString();
            return thumbnailUrl?.StartsWith("http") is true ? thumbnailUrl : null;
        }

        public async Task<IEnumerable<RedditPost>> GetTrendings()
        {
            var response = await Enqueue(() => _httpClient.GetAsync("https://oauth.reddit.com/r/trendingsubreddits/new?limit=7"));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(content);

            return document.RootElement
                    .GetProperty("data")
                    .GetProperty("children")
                    .EnumerateArray()
                    .Select(x => x.GetProperty("data"))
                    .Select(x =>
                        new RedditPost
                        {
                            Title = x.GetProperty("title").GetString(),
                            Url = x.GetProperty("url").GetString(),
                            CommentsUrl = "https://www.reddit.com" + x.GetProperty("permalink").GetString(),
                            Comments = x.GetProperty("num_comments").GetInt32(),
                            Domain = x.GetProperty("domain").GetString(),
                            Nsfw = x.GetProperty("over_18").GetBoolean(),
                            Score = x.GetProperty("score").GetInt32(),
                            ImageUrl = GetPreview(x)
                        }).ToList();
        }

        public async Task<IEnumerable<Subreddit>> GetSubredditsTopPosts()
        {
            var subreddits = new List<Subreddit>();

            foreach (var subreddit in await GetSubreddits())
            {
                var response = await Enqueue(() => _httpClient.GetAsync($"https://oauth.reddit.com/r/{subreddit}/top?t=week&limit=3"));
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content);
                
                var redditPosts = document.RootElement
                    .GetProperty("data")
                    .GetProperty("children")
                    .EnumerateArray()
                    .Select(x => x.GetProperty("data"))
                    .Select(x =>
                        new RedditPost
                        {
                            Title = x.GetProperty("title").GetString(),
                            Url = x.GetProperty("url").GetString(),
                            CommentsUrl = "https://www.reddit.com" + x.GetProperty("permalink").GetString(),
                            Comments = x.GetProperty("num_comments").GetInt32(),
                            Domain = x.GetProperty("domain").GetString(),
                            Nsfw = x.GetProperty("over_18").GetBoolean(),
                            Score = x.GetProperty("score").GetInt32(),
                            ImageUrl = GetPreview(x)
                        }).ToList();

                subreddits.Add(new Subreddit { Name = subreddit, TopPosts = redditPosts });
            }

            return subreddits;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        /// <summary>
        /// This is a naive throttling mechanism. It is not thread safe, so don't even think about
        /// calling methods that use this throttle, concurrently. Could potentially be made more
        /// robust using a semaphore lock to prevent concurrent execution.
        /// </summary>
        private static async Task<TResult> Enqueue<TResult>(Func<Task<TResult>> func)
        {
            var delta = _nextRequest - DateTime.Now;
            if (delta > TimeSpan.Zero)
                await Task.Delay(delta);

            var result = await func();
            _nextRequest = DateTime.Now.AddSeconds(1);
            return result;
        }

        private async Task<IEnumerable<string>> GetSubreddits()
        {
            string afterToken = null;
            var subreddits = new List<string>();

            do
            {
                var afterParameter = afterToken != null ? $"&after={afterToken}" : null;
                var response = await Enqueue(() => _httpClient.GetAsync(
                    $"https://oauth.reddit.com/subreddits/mine/subscriber?limit=100{afterParameter}"));
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                using var subscriptions = JsonDocument.Parse(content);


                subreddits.AddRange(
                    subscriptions.RootElement
                        .GetProperty("data")
                        .GetProperty("children")
                        .EnumerateArray()
                        .Select(x => x.GetProperty("data").GetProperty("display_name").GetString())
                );

                afterToken = subscriptions.RootElement.GetProperty("data").GetProperty("after").GetString();
            } while (afterToken != null);

            return subreddits;
        }

        private async Task<string> Authorize(RedditOptions options)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.reddit.com/api/v1/access_token");
            var bodyContent = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", options.Username),
                new KeyValuePair<string, string>("password", options.Password)
            };
            request.Content = new FormUrlEncodedContent(bodyContent);
            request.Headers.Authorization = new AuthenticationHeaderValue("basic", Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{options.ClientId}:{options.ClientSecret}")));

            var response = await Enqueue(() => _httpClient.SendAsync(request));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var auth = JsonDocument.Parse(content);

            return auth.RootElement.GetProperty("access_token").GetString();
        }
    }
}