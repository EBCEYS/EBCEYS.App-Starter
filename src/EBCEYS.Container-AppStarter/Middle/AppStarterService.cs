using System.Diagnostics;
using EBCEYS.Container_AppStarter.Options;
using EBCEYS.ContainersEnvironment.HealthChecks;

namespace EBCEYS.Container_AppStarter.Middle;

internal class AppStarterService(
    ILogger<AppStarterService> logger,
    ConfigRequester requester,
    AppStarterServiceOptions? opts = null,
    PingServiceHealthStatusInfo? health = null) : BackgroundService
{
    private readonly AppStarterServiceOptions _opts = opts ?? AppStarterServiceOptions.CreateFromEnvironment();
    private Process? _app;
    private Task? _appRunTask;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting...");
        health?.SetHealthyStatus("Starting...");
        await Task.Delay(_opts.DelayBeforeStart, stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            var filesUpdated = -1;
            for (var i = 0; i < _opts.Retries || filesUpdated >= 0; i++)
            {
                filesUpdated = await requester.FullConfigProcessAsync(stoppingToken);
                if (filesUpdated >= 0) break;
                await Task.Delay(_opts.RetiesDelay, stoppingToken);
            }

            if (_app == null && filesUpdated == -1)
            {
                logger.LogWarning("Stopping app...");
                await Task.Delay(1000, stoppingToken);
                throw new Exception($"ERROR ON GETTING CONFIGURATION EXT: {Program.ExitCodeNoConnection}");
            }

            if (_app == null || (filesUpdated > 0 && _opts.RestartAfterConfUpdates)) await RestartProcess(stoppingToken);
            await Task.Delay(_opts.ConfigRequestPeriod, stoppingToken);
            if (_app == null)
            {
                health?.SetUnhealthyStatus("NOT FOUND EXECUTION FILE");
                throw new Exception($"ERROR ON STARTING APP EXT: {Program.ExitCodeRunProcess}");
            }

            var isTaskEnded = (_appRunTask?.IsCompleted ?? true) || (_appRunTask?.IsFaulted ?? true);
            if (isTaskEnded)
            {
                health?.SetUnhealthyStatus("FAILED TO EXECUTE APP");
                throw new Exception($"ERROR ON STARTING APP EXT: {Program.ExitCodeRunProcess}");
            }
        }
    }

    private async Task RestartProcess(CancellationToken token = default)
    {
        if (_app != null)
        {
            _app.Kill(true);
            await _app.WaitForExitAsync(token);
            _app = null;
        }

        _appRunTask?.Dispose();
        _appRunTask = null;
        _appRunTask = Task.Run(async () => await RunProcess(token), token);
    }

    private async Task RunProcess(CancellationToken token = default)
    {
        ProcessStartInfo info = new(_opts.ExecutionFile)
        {
            WorkingDirectory = _opts.ExecFileWorkingDir,
            Arguments = _opts.ExecutionArgs
        };
        _app = Process.Start(info);
        if (_app == null)
        {
            health?.SetUnhealthyStatus("NOT FOUND EXECUTION FILE");
            return;
        }

        health?.SetHealthyStatus("START APP");
        await _app.WaitForExitAsync(token);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _app?.Kill(true);
        _app?.Dispose();
        _appRunTask?.Dispose();
        return base.StopAsync(cancellationToken);
    }
}