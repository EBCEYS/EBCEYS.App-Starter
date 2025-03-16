using EBCEYS.Container_AppStarter.ContainerEnvironment;

namespace EBCEYS.Container_AppStarter.Options
{
    internal class AppStarterServiceOptions(TimeSpan delayBeforeStart, string? execFile, string? execArgs, string? workingDir, TimeSpan requestPeriod, int requestReties, TimeSpan retriesDelay, bool restartOnConfUpdate)
    {
        public TimeSpan DelayBeforeStart { get; } = delayBeforeStart;
        public string ExecutionFile { get; } = execFile ?? throw new Exception("No file to execute!");
        public string ExecutionArgs { get; } = execArgs ?? string.Empty;
        public string ExecFileWorkingDir { get; } = workingDir ?? Path.GetDirectoryName(execFile) ?? "/";
        public TimeSpan ConfigRequestPeriod { get; } = requestPeriod;
        public int Retries { get; } = requestReties > 1 ? requestReties : 1;
        public TimeSpan RetiesDelay { get; } = retriesDelay;
        public bool RestartAfterConfUpdates { get; } = restartOnConfUpdate;
        public static AppStarterServiceOptions CreateFromEnvironment()
        {
            string? execFile = SupportedEnvironmentVariables.ExecFile.Value;
            string argsLine = SupportedEnvironmentVariables.ExecArgs.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(execFile))
            {
                throw new Exception("No file to execute!");
            }
            if (!File.Exists(execFile))
            {
                throw new FileNotFoundException("No execution file!", execFile);
            }
            return new
                (
                SupportedEnvironmentVariables.DelayBeforeStart.Value!.Value,
                execFile,
                argsLine,
                SupportedEnvironmentVariables.WorkingDirectory.Value,
                SupportedEnvironmentVariables.ConfigRequestPeriod.Value!.Value,
                SupportedEnvironmentVariables.ConfigRequestRetries.Value,
                SupportedEnvironmentVariables.ConfigRequestDelay.Value!.Value,
                SupportedEnvironmentVariables.RestartAfterUpdateConfigs.Value!.Value
                );
        }
    }
}
