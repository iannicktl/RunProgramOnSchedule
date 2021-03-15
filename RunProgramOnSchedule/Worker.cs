using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RunProgramOnSchedule
{
    public class AppConfig
    {
        public string Path { get; set; } = @"C:\RemovePrompt\Config.Json";
        public int RunInMin { get; set; } = 2;
    }

    public class TimedHostedService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<TimedHostedService> logger;
        private readonly AppConfig appConfig;

        private Timer timer;

        public TimedHostedService(ILogger<TimedHostedService> logger, AppConfig appConfig)
        {
            this.logger = logger;
            this.appConfig = appConfig;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Timed Hosted Service running.");

            timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(appConfig.RunInMin));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);

            logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);

            try
            {
                //I tried to invoke RegEdit, but each time I got a confirm prompt(UAC enabled, no elevated permissions). Instead of RegEdit I recommand "reg.exe"(which is included in Windows since XP)

                Process proc = new Process();

                try
                {
                    //string user = Environment.UserDomainName + "\\" + Environment.UserName;
                    //RegistrySecurity rs = new RegistrySecurity();

                    //rs.AddAccessRule(new RegistryAccessRule(user,
                    //            RegistryRights.WriteKey | RegistryRights.ReadKey | RegistryRights.Delete,
                    //            InheritanceFlags.None,
                    //            PropagationFlags.None,
                    //            AccessControlType.Allow));

                    //    proc.StartInfo.FileName = "reg.exe";

                    //    proc.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                    //    proc.StartInfo.CreateNoWindow = false;

                    var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    key.SetValue("ConsentPromptBehaviorAdmin", "4", RegistryValueKind.DWord);
                    key.SetValue("ConsentPromptBehaviorUser", "4", RegistryValueKind.DWord);

                    logger.LogInformation("Runned {key} at {datetime}", "ConsentPromptBehaviorAdmin & ConsentPromptBehaviorUser", DateTime.Now);

                    //proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    //proc.StartInfo.CreateNoWindow = true;
                    //    proc.StartInfo.UseShellExecute = false;

                    //    string command = "import " + appConfig.Path;
                    //    proc.StartInfo.Arguments = command;
                    //    var started = proc.Start();

                    //    proc.WaitForExit();
                }
                catch (System.Exception ex)
                {
                    logger.LogError(ex, "Bad");
                    //proc.Dispose();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Timed Hosted Service is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}