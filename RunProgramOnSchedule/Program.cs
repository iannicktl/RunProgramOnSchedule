using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RunProgramOnSchedule
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            var Configuration = CreateHostBuilder(args).Build();

            Configuration.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var config = hostContext.Configuration.GetSection("AppConfig").Get<AppConfig>();
                services.AddSingleton(config);
                services.AddHostedService<TimedHostedService>();
            }).UseWindowsService();
    }
}