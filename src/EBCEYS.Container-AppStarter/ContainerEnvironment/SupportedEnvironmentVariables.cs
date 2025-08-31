using System.Text;
using EBCEYS.ContainersEnvironment.ServiceEnvironment;

namespace EBCEYS.Container_AppStarter.ContainerEnvironment;

internal static class SupportedEnvironmentVariables
{
    private const string BreakIfNoConfig = "CONFIGURATION_BREAK_START_IF_NO_CONFIGS";
    private const string HttpClientTimeoutName = "CONFIGURATION_HTTP_TIMEOUT";
    private const string ConfigRequestPeriodName = "CONFIGURATION_REQUEST_PERIOD";
    private const string ConfigRequestRetriesName = "CONFIGURATION_REQUEST_RETRIES";
    private const string ConfigRequestDelayName = "CONFIGURATION_REQUEST_DELAY";
    private const string AppStartDelay = "APP_STARTER_DELAY_BEFORE_START";
    private const string AppStartExecFile = "APP_STARTER_EXECUTION_FILE";
    private const string AppStartExecArgs = "APP_STARTER_EXECUTION_ARGS";
    private const string AppWorkingDirectory = "APP_STARTER_WORKING_DIRECTORY";
    private const string RestartAppOnUpdate = "APP_STARTER_RESTART_APP_ON_CONFIG_UPDATE";
    private const string EnableHealthChecks = "APP_STARTER_ENABLE_HEALTHCHECKS";
    private const string LogLevelName = "APP_STARTER_LOG_LEVEL";
    public static ServiceEnvironmentVariable<bool?> BreakStartIfNoConfig { get; } = new(BreakIfNoConfig, true);

    public static ServiceEnvironmentVariable<TimeSpan?> HttpClientTimeout { get; } =
        new(HttpClientTimeoutName, TimeSpan.FromSeconds(30.0));

    public static ServiceEnvironmentVariable<TimeSpan?> ConfigRequestPeriod { get; } =
        new(ConfigRequestPeriodName, TimeSpan.FromSeconds(10.0));

    public static ServiceEnvironmentVariable<int> ConfigRequestRetries { get; } = new(ConfigRequestRetriesName, 3);

    public static ServiceEnvironmentVariable<TimeSpan?> ConfigRequestDelay { get; } =
        new(ConfigRequestDelayName, TimeSpan.FromSeconds(5.0));

    public static ServiceEnvironmentVariable<TimeSpan?> DelayBeforeStart { get; } = new(AppStartDelay, TimeSpan.Zero);
    public static ServiceEnvironmentVariable<string?> ExecFile { get; } = new(AppStartExecFile);
    public static ServiceEnvironmentVariable<string> ExecArgs { get; } = new(AppStartExecArgs, string.Empty);
    public static ServiceEnvironmentVariable<string?> WorkingDirectory { get; } = new(AppWorkingDirectory);
    public static ServiceEnvironmentVariable<bool?> RestartAfterUpdateConfigs { get; } = new(RestartAppOnUpdate, true);
    
    public static ServiceEnvironmentVariable<LogLevel?> LogLevel { get; } = new(LogLevelName, Microsoft.Extensions.Logging.LogLevel.Debug);

    public static ServiceEnvironmentVariable<bool> EnableAppStarterHealthChecks { get; } =
        new(EnableHealthChecks, true);

    private static IEnumerable<ServiceEnvironmentInfo> GetInfo()
    {
        return
        [
            BreakStartIfNoConfig.GetInfo(), HttpClientTimeout.GetInfo(), ConfigRequestPeriod.GetInfo(),
            ConfigRequestRetries.GetInfo(), ConfigRequestDelay.GetInfo(),
            DelayBeforeStart.GetInfo(), ExecFile.GetInfo(), ExecArgs.GetInfo(), WorkingDirectory.GetInfo(),
            RestartAfterUpdateConfigs.GetInfo(), LogLevel.GetInfo()
        ];
    }

    public static string GetHelp()
    {
        StringBuilder sb = new();
        foreach (var info in GetInfo()) sb.AppendLine(info.ToString());
        return sb.ToString();
    }
}