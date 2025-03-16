using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text.Json;
using EBCEYS.Container_AppStarter.ContainerEnvironment;
using EBCEYS.Container_AppStarter.Options;
using EBCEYS.ContainersEnvironment.Configuration.Models;
using EBCEYS.ContainersEnvironment.HealthChecks;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Http.Extensions;
using NLog;

namespace EBCEYS.Container_AppStarter.Middle
{
    internal class ConfigRequester
    {
        private readonly HttpClient client;
        private readonly ILogger<ConfigRequester> logger;
        private readonly PingServiceHealthStatusInfo health;

        private readonly Uri serverUri;
        private readonly Uri fileInfoUri;
        private readonly string containerTypeName;
        private readonly string configSaveDirectoryBase;
        private readonly bool breakIfNoConfigs;

        public ConfigRequester(ILogger<ConfigRequester> logger, PingServiceHealthStatusInfo health, ConfigRequesterOptions? opts = null)
        {
            this.logger = logger;
            this.health = health;
            opts ??= ConfigRequesterOptions.CreateFromEnvironment();
            client = new()
            {
                Timeout = opts.HttpTimeout
            };
            serverUri = opts.ServerAddress;
            containerTypeName = opts.ContainerTypeName;
            configSaveDirectoryBase = new DirectoryInfo(opts.ConfigDirectory).FullName;
            breakIfNoConfigs = opts.BreakStartIfNoConfigs;
            fileInfoUri = FormatFileInfoUri();
        }

        public async Task<IEnumerable<ConfigurationFileInfo>?> GetConfigInfoAsync(CancellationToken token = default)
        {
            try
            {
                logger.LogDebug("Try request file info configs from {uri}", fileInfoUri.ToString());
                HttpResponseMessage response = await client.GetAsync(fileInfoUri, token);
                logger.LogDebug("Response status code = {code}", response.StatusCode);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    health.SetHealthyStatus();
                    string obj = await response.Content.ReadAsStringAsync(token);
                    return JsonSerializer.Deserialize(obj, SourceGenerationContext.Default.IEnumerableConfigurationFileInfo);
                }
                health.SetHealthyStatus();
                return [];
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on requesting config files info!");
                health.SetUnhealthyStatus($"ERROR ON REQUESTING CONFIGS! {ex.Message}");
                return null;
            }
        }

        public async Task<Stream?> GetConfigurationFileAsync(string fileName, CancellationToken token = default)
        {
            Uri uri = FormatFileUri(fileName);

            try
            {
                logger.LogDebug("Try request config file from {uri}", uri.ToString());
                HttpResponseMessage response = await client.GetAsync(uri, token);
                logger.LogDebug("Response status code = {code}", response.StatusCode);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    health.SetHealthyStatus();
                    return await response.Content.ReadAsStreamAsync(token);
                }
                health.SetHealthyStatus();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on requesting config file {filename}", fileName);
                health.SetUnhealthyStatus($"ERROR ON REQUESTING CONFIGS! {ex.Message}");
            }
            return null;
        }

        public async Task<int> FullConfigProcessAsync(CancellationToken token = default)
        {
            logger.LogDebug("Starting configuration update process...");
            IEnumerable<ConfigurationFileInfo>? fileInfos = await GetConfigInfoAsync(token);
            if (fileInfos == null)
            {
                logger.LogDebug("No config files avaliable...");
                if (breakIfNoConfigs)
                {
                    health.SetUnhealthyStatus("NO CONFIGURATION FILES AVALIABLE");
                    return -1;
                }
                return 0;
            }
            int filesUpdated = 0;
            IEnumerable<FileInfo> existedFiles = Directory.Exists(configSaveDirectoryBase) ? Directory.EnumerateFiles(configSaveDirectoryBase, "*", SearchOption.AllDirectories).Select(f => new FileInfo(f)) : [];
            foreach (ConfigurationFileInfo serverFileInfo in fileInfos)
            {
                FileInfo? existedFile = existedFiles.FirstOrDefault(f => f.FullName == serverFileInfo.FileSaveFullPath);
                if (existedFile != null && existedFile.Exists && existedFile.LastWriteTimeUtc >= serverFileInfo.LastWriteUTC)
                {
                    logger.LogDebug("Existed file {path} no need updates {writeUTC}", existedFile.FullName, existedFile.LastWriteTimeUtc);
                    continue;
                }
                await using Stream? fileStream = await GetConfigurationFileAsync(serverFileInfo.ServerFileFullPath, token);
                if (fileStream == null)
                {
                    logger.LogWarning("No file {path}", serverFileInfo.FileSaveFullPath);
                    continue;
                }
                FileInfo newFile = new(serverFileInfo.FileSaveFullPath);
                DirectoryInfo newDir = Directory.CreateDirectory(newFile.DirectoryName ?? "/");
                if (newFile.Exists)
                {
                    newFile.Delete();
                }
                await using StreamWriter fs = File.CreateText(newFile.FullName);
                fileStream.Seek(0, SeekOrigin.Begin);
                using StreamReader sr = new(fileStream, encoding: System.Text.Encoding.UTF8, leaveOpen: true);
                string? line;
                while((line = await sr.ReadLineAsync(token)) != null)
                {
                    await fs.WriteLineAsync(line);
                }
                
                filesUpdated++;
            }
            health.SetHealthyStatus("UPDATE CONFIGURATION SUCCESSFULLY");
            return filesUpdated;
        }

        private Uri FormatFileInfoUri()
        {
            QueryBuilder qb =
                            [];
            qb.Add("containerTypeName", containerTypeName);
            qb.Add("containerSavePath", configSaveDirectoryBase);
            UriBuilder ub = new(serverUri)
            {
                Path = "/api/configuration/files/info",
                Query = qb.ToQueryString().ToString()
            };
            Uri uri = ub.Uri;
            return uri;
        }

        private Uri FormatFileUri(string filePath)
        {
            UriBuilder ub = new(serverUri)
            {
                Path = $"/api/configuration/files/{Uri.EscapeDataString(filePath)}"
            };
            return ub.Uri;
        }
    }
}
