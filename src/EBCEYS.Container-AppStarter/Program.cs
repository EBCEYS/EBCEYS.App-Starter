using System.Text.Json.Serialization;
using EBCEYS.Container_AppStarter.ContainerEnvironment;
using EBCEYS.Container_AppStarter.Middle;
using EBCEYS.ContainersEnvironment.Configuration.Models;
using EBCEYS.ContainersEnvironment.HealthChecks.Extensions;
using Microsoft.AspNetCore.Builder;

namespace EBCEYS.Container_AppStarter;

public class Program
{
    private const string HelloString =
        "\r\n#######################################################\r\n#    _               ____  _             _            #\r\n#   / \\   _ __  _ __/ ___|| |_ __ _ _ __| |_ ___ _ __ #\r\n#  / _ \\ | '_ \\| '_ \\___ \\| __/ _` | '__| __/ _ \\ '__|#\r\n# / ___ \\| |_) | |_) |__) | || (_| | |  | ||  __/ |   #\r\n#/_/   \\_\\ .__/| .__/____/ \\__\\__,_|_|   \\__\\___|_|   #\r\n#        |_|   |_|                                    #\r\n#######################################################";

    public const int ExitCodeNoConnection = 10;
    public const int ExitCodeRunProcess = 11;

    public static void Main(string[] args)
    {
        var enableHealthChecks = SupportedEnvironmentVariables.EnableAppStarterHealthChecks.Value;

        if (!enableHealthChecks)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(SupportedEnvironmentVariables.LogLevel.Value!.Value);
            
            builder.Services.AddSingleton<ConfigRequester>();
            builder.Services.AddHostedService<AppStarterService>();

            var app = builder.Build();

            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            
            logger.LogInformation("Starting application");
            logger.LogInformation(HelloString);

            app.Run();
        }
        else
        {
            var builder = WebApplication.CreateSlimBuilder(args);
            
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(SupportedEnvironmentVariables.LogLevel.Value!.Value);
            
            builder.Services.AddSingleton<ConfigRequester>();
            builder.Services.AddHostedService<AppStarterService>();

            builder.Services.ConfigureHealthChecks();

            var host = builder.Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            
            logger.LogInformation("Starting application");
            logger.LogInformation(HelloString);
            
            host.ConfigureHealthChecks();

            host.Run();
        }
    }
}

[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ConfigurationFileInfo))]
[JsonSerializable(typeof(IEnumerable<ConfigurationFileInfo>))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}