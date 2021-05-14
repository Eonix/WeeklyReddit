using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WeeklyReddit
{
    public static class Program
    {
        public static async Task Main(string[] args)
            => await CreateHostBuilder(args).Build().RunAsync();

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureServices((_, x) => x.AddHostedService<Worker>());
    }
}
