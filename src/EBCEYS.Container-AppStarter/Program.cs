using System.Text.Json.Serialization;
using EBCEYS.Container_AppStarter.ContainerEnvironment;
using EBCEYS.Container_AppStarter.Middle;
using EBCEYS.ContainersEnvironment.Configuration.Models;
using EBCEYS.ContainersEnvironment.HealthChecks.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using NLog;
using NLog.Web;

namespace EBCEYS.Container_AppStarter
{
    public class Program
    {
        private const string helloString = "\r\n#######################################################\r\n#    _               ____  _             _            #\r\n#   / \\   _ __  _ __/ ___|| |_ __ _ _ __| |_ ___ _ __ #\r\n#  / _ \\ | '_ \\| '_ \\___ \\| __/ _` | '__| __/ _ \\ '__|#\r\n# / ___ \\| |_) | |_) |__) | || (_| | |  | ||  __/ |   #\r\n#/_/   \\_\\ .__/| .__/____/ \\__\\__,_|_|   \\__\\___|_|   #\r\n#        |_|   |_|                                    #\r\n#######################################################";
        public const int exitCodeNoConnection = 10;
        public const int exitCodeRunProcess = 11;
        public static void Main(string[] args)
        {
            bool enableHealthChecks = SupportedEnvironmentVariables.EnableAppStarterHealthChecks.Value;

            if (!enableHealthChecks)
            {
                HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
                builder.Services.AddSingleton<ConfigRequester>();
                builder.Services.AddHostedService<AppStarterService>();

                builder.Configuration.AddJsonFile("appsettings.json", true, true);

                Logger logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

                builder.Logging.ClearProviders();
                builder.Logging.AddNLog(logger.Factory.Configuration);

                logger.Info(helloString);
                logger.Debug("Healthchecks enabled: {enabled}", enableHealthChecks);

                IHost app = builder.Build();

                app.Run();
            }
            else
            {
                WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
                builder.Services.AddSingleton<ConfigRequester>();
                builder.Services.AddHostedService<AppStarterService>();

                builder.Services.ConfigureHealthChecks();

                builder.Configuration.AddJsonFile("appsettings.json", true, true);

                Logger logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

                builder.Logging.ClearProviders();
                builder.Logging.AddNLog(logger.Factory.Configuration);

                logger.Info(helloString);
                logger.Debug("Healthchecks enabled: {enabled}", enableHealthChecks);

                WebApplication host = builder.Build();

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
}