using System.Text;
using EBCEYS.ContainersEnvironment.ServiceEnvironment;

namespace EBCEYS.Container_AppStarter.ContainerEnvironment
{
    internal static class SupportedEnvironmentVariables
    {
        private const string breakIfNoConfig = "CONFIGURATION_BREAK_START_IF_NO_CONFIGS";
        private const string httpClientTimeout = "CONFIGURATION_HTTP_TIMEOUT";
        private const string configRequestPeriod = "CONFIGURATION_REQUEST_PERIOD";
        private const string configRequestRetries = "CONFIGURATION_REQUEST_RETRIES";
        private const string configRequestDelay = "CONFIGURATION_REQUEST_DELAY";
        private const string appStartDelay = "APP_STARTER_DELAY_BEFORE_START";
        private const string appStartExecFile = "APP_STARTER_EXECUTION_FILE";
        private const string appStartExecArgs = "APP_STARTER_EXECUTION_ARGS";
        private const string appWorkingDirectory = "APP_STARTER_WORKING_DIRECTORY";
        private const string restartAppOnUpdate = "APP_STARTER_RESTART_APP_ON_CONFIG_UPDATE";
        public static ServiceEnvironmentVariable<bool?> BreakStartIfNoConfig { get; } = new(breakIfNoConfig, true);
        public static ServiceEnvironmentVariable<TimeSpan?> HttpClientTimeout { get; } = new(httpClientTimeout, TimeSpan.FromSeconds(30.0));
        public static ServiceEnvironmentVariable<TimeSpan?> ConfigRequestPeriod { get; } = new(configRequestPeriod, TimeSpan.FromSeconds(10.0));
        public static ServiceEnvironmentVariable<int> ConfigRequestRetries { get; } = new(configRequestRetries, 3);
        public static ServiceEnvironmentVariable<TimeSpan?> ConfigRequestDelay { get; } = new(configRequestDelay, TimeSpan.FromSeconds(5.0));
        public static ServiceEnvironmentVariable<TimeSpan?> DelayBeforeStart { get; } = new(appStartDelay, TimeSpan.Zero);
        public static ServiceEnvironmentVariable<string?> ExecFile { get; } = new(appStartExecFile, null);
        public static ServiceEnvironmentVariable<string> ExecArgs { get; } = new(appStartExecArgs, string.Empty);
        public static ServiceEnvironmentVariable<string?> WorkingDirectory { get; } = new(appWorkingDirectory, null);
        public static ServiceEnvironmentVariable<bool?> RestartAfterUpdateConfigs { get; } = new(restartAppOnUpdate, true);
        public static IEnumerable<ServiceEnvironmentInfo> GetInfo()
        {
            return [BreakStartIfNoConfig.GetInfo(), HttpClientTimeout.GetInfo(), ConfigRequestPeriod.GetInfo(), ConfigRequestRetries.GetInfo(), ConfigRequestDelay.GetInfo(),
            DelayBeforeStart.GetInfo(), ExecFile.GetInfo(), ExecArgs.GetInfo(), WorkingDirectory.GetInfo(), RestartAfterUpdateConfigs.GetInfo()];
        }
        public static string GetHelp()
        {
            StringBuilder sb = new();
            foreach (ServiceEnvironmentInfo info in GetInfo())
            {
                sb.AppendLine(info.ToString());
            }
            return sb.ToString();
        }
    }
}
