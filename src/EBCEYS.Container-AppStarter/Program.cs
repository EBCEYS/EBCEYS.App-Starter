using EBCEYS.Container_AppStarter.Middle;
using EBCEYS.ContainersEnvironment.HealthChecks.Extensions;
using Microsoft.AspNetCore.Builder;
using NLog;
using NLog.Web;

namespace EBCEYS.Container_AppStarter
{
    public class Program
    {
        public const int exitCodeNoConnection = 10;
        public const int exitCodeRunProcess = 11;
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton<ConfigRequester>();
            builder.Services.AddHostedService<AppStarterService>();
            builder.Services.ConfigureHealthChecks();

            builder.Configuration.AddJsonFile("appsettings.json", true, true);

            Logger logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddNLog(logger.Factory.Configuration);

            WebApplication host = builder.Build();

            host.ConfigureHealthChecks();
            
            host.Run();
        }
    }
}