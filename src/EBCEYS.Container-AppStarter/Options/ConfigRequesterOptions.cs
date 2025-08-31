using EBCEYS.Container_AppStarter.ContainerEnvironment;
using EBCEYS.ContainersEnvironment.Configuration.Environment;

namespace EBCEYS.Container_AppStarter.Options;

internal class ConfigRequesterOptions(
    Uri serverAddress,
    TimeSpan httpTimeout,
    string containerTypeName,
    string configDirectory,
    bool breakStartIfNoConfigs)
{
    public TimeSpan HttpTimeout { get; } = httpTimeout > TimeSpan.Zero
        ? httpTimeout
        : SupportedEnvironmentVariables.HttpClientTimeout.DefaultValue!.Value;

    public Uri ServerAddress { get; } = serverAddress;
    public string ContainerTypeName { get; } = containerTypeName;
    public string ConfigDirectory { get; } = configDirectory;
    public bool BreakStartIfNoConfigs { get; } = breakStartIfNoConfigs;

    public static ConfigRequesterOptions CreateFromEnvironment()
    {
        return new ConfigRequesterOptions(
            new Uri(ConfigurationEnvironment.ConfigurationServerUrl.Value!),
            SupportedEnvironmentVariables.HttpClientTimeout.Value!.Value,
            ConfigurationEnvironment.ConfigurationContainerTypeName.Value!,
            ConfigurationEnvironment.ConfigurationSaveDirectory.Value!,
            SupportedEnvironmentVariables.BreakStartIfNoConfig.Value!.Value);
    }
}