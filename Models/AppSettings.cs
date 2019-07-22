namespace WeeklyReddit
{
    public class AppSettings
    {
        public RedditSettings Reddit { get; set; }
        public SmtpSettings SmtpSettings { get; set; }
        public EmailSettings EmailSettings { get; set; }
    }

    public class RedditSettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class SmtpSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class EmailSettings
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
    }
}