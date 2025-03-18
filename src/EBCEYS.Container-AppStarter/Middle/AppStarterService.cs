using System.Diagnostics;
using EBCEYS.Container_AppStarter.Options;
using EBCEYS.ContainersEnvironment.HealthChecks;

namespace EBCEYS.Container_AppStarter.Middle
{
    internal class AppStarterService(ILogger<AppStarterService> logger, ConfigRequester requester, AppStarterServiceOptions? opts = null, PingServiceHealthStatusInfo? health = null) : BackgroundService
    {
        private readonly AppStarterServiceOptions opts = opts ?? AppStarterServiceOptions.CreateFromEnvironment();
        private Process? app;
        private Task? appRunTask;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting...");
            health?.SetHealthyStatus("Starting...");
            await Task.Delay(opts.DelayBeforeStart, stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                int filesUpdated = -1;
                for (int i = 0; i < opts.Retries || filesUpdated >= 0; i++)
                {
                    filesUpdated = await requester.FullConfigProcessAsync(stoppingToken);
                    if (filesUpdated >= 0) break;
                    await Task.Delay(opts.RetiesDelay, stoppingToken);
                }
                if (app == null && filesUpdated == -1)
                {
                    logger.LogWarning("Stopping app...");
                    await Task.Delay(1000, stoppingToken);
                    throw new Exception($"ERROR ON GETTING CONFIGURATION EXT: {Program.exitCodeNoConnection}");
                }
                if (app == null || (filesUpdated > 0 && opts.RestartAfterConfUpdates))
                {
                    await RestartProcess(stoppingToken);
                }
                await Task.Delay(opts.ConfigRequestPeriod, stoppingToken);
                if (app == null)
                {
                    health?.SetUnhealthyStatus("NOT FOUND EXECUTION FILE");
                    throw new Exception($"ERROR ON STARTING APP EXT: {Program.exitCodeRunProcess}");
                }
                bool isTaskEnded = (appRunTask?.IsCompleted ?? true) || (appRunTask?.IsFaulted ?? true);
                if (isTaskEnded)
                {
                    health?.SetUnhealthyStatus("FAILED TO EXECUTE APP");
                    throw new Exception($"ERROR ON STARTING APP EXT: {Program.exitCodeRunProcess}");
                }
            }
        }

        private async Task RestartProcess(CancellationToken token = default)
        {
            if (app != null)
            {
                app.Kill(true);
                await app.WaitForExitAsync(token);
                app = null;
            }
            appRunTask?.Dispose();
            appRunTask = null;
            appRunTask = Task.Run(async () => await RunProcess(), token);
        }

        private async Task RunProcess(CancellationToken token = default)
        {
            ProcessStartInfo info = new(opts.ExecutionFile)
            {
                WorkingDirectory = opts.ExecFileWorkingDir,
                Arguments = opts.ExecutionArgs
            };
            app = Process.Start(info);
            if (app == null)
            {
                health?.SetUnhealthyStatus("NOT FOUND EXECUTION FILE");
                return;
            }
            health?.SetHealthyStatus("START APP");
            await app.WaitForExitAsync(token);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            app?.Kill(true);
            app?.Dispose();
            appRunTask?.Dispose();
            return base.StopAsync(cancellationToken);
        }
    }
}
