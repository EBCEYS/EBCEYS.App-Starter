using System.Net;
using System.Text;
using System.Text.Json;
using EBCEYS.Container_AppStarter.Options;
using EBCEYS.ContainersEnvironment.Configuration.Models;
using EBCEYS.ContainersEnvironment.HealthChecks;
using Microsoft.AspNetCore.Http.Extensions;

namespace EBCEYS.Container_AppStarter.Middle;

internal class ConfigRequester
{
    private readonly bool _breakIfNoConfigs;
    private readonly HttpClient _client;
    private readonly string _configSaveDirectoryBase;
    private readonly string _containerTypeName;
    private readonly Uri _fileInfoUri;
    private readonly PingServiceHealthStatusInfo? _health;
    private readonly ILogger<ConfigRequester> _logger;

    private readonly Uri _serverUri;

    public ConfigRequester(ILogger<ConfigRequester> logger, PingServiceHealthStatusInfo? health = null,
        ConfigRequesterOptions? opts = null)
    {
        _logger = logger;
        _health = health;
        opts ??= ConfigRequesterOptions.CreateFromEnvironment();
        _client = new HttpClient
        {
            Timeout = opts.HttpTimeout
        };
        _serverUri = opts.ServerAddress;
        _containerTypeName = opts.ContainerTypeName;
        _configSaveDirectoryBase = new DirectoryInfo(opts.ConfigDirectory).FullName;
        _breakIfNoConfigs = opts.BreakStartIfNoConfigs;
        _fileInfoUri = FormatFileInfoUri();
    }

    private async Task<IEnumerable<ConfigurationFileInfo>?> GetConfigInfoAsync(CancellationToken token = default)
    {
        try
        {
            _logger.LogDebug("Try request file info configs from {uri}", _fileInfoUri.ToString());
            var response = await _client.GetAsync(_fileInfoUri, token);
            _logger.LogDebug("Response status code = {code}", response.StatusCode);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                _health?.SetHealthyStatus();
                var obj = await response.Content.ReadAsStringAsync(token);
                return JsonSerializer.Deserialize(obj,
                    SourceGenerationContext.Default.IEnumerableConfigurationFileInfo);
            }

            _health?.SetHealthyStatus();
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on requesting config files info!");
            _health?.SetUnhealthyStatus($"ERROR ON REQUESTING CONFIGS! {ex.Message}");
            return null;
        }
    }

    private async Task<Stream?> GetConfigurationFileAsync(string fileName, CancellationToken token = default)
    {
        var uri = FormatFileUri(fileName);

        try
        {
            _logger.LogDebug("Try request config file from {uri}", uri.ToString());
            var response = await _client.GetAsync(uri, token);
            _logger.LogDebug("Response status code = {code}", response.StatusCode);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                _health?.SetHealthyStatus();
                return await response.Content.ReadAsStreamAsync(token);
            }

            _health?.SetHealthyStatus();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on requesting config file {filename}", fileName);
            _health?.SetUnhealthyStatus($"ERROR ON REQUESTING CONFIGS! {ex.Message}");
        }

        return null;
    }

    public async Task<int> FullConfigProcessAsync(CancellationToken token = default)
    {
        _logger.LogDebug("Starting configuration update process...");
        var fileInfos = await GetConfigInfoAsync(token);
        if (fileInfos == null)
        {
            _logger.LogDebug("No config files avaliable...");
            if (_breakIfNoConfigs)
            {
                _health?.SetUnhealthyStatus("NO CONFIGURATION FILES AVALIABLE");
                return -1;
            }

            return 0;
        }

        var filesUpdated = 0;
        var existedFiles = Directory.Exists(_configSaveDirectoryBase)
            ? Directory.EnumerateFiles(_configSaveDirectoryBase, "*", SearchOption.AllDirectories)
                .Select(f => new FileInfo(f))
            : [];
        foreach (var serverFileInfo in fileInfos)
        {
            var existedFile = existedFiles.FirstOrDefault(f => f.FullName == serverFileInfo.FileSaveFullPath);
            if (existedFile is { Exists: true } &&
                existedFile.LastWriteTimeUtc >= serverFileInfo.LastWriteUTC)
            {
                _logger.LogDebug("Existed file {path} no need updates {writeUTC}", existedFile.FullName,
                    existedFile.LastWriteTimeUtc);
                continue;
            }

            await using var fileStream = await GetConfigurationFileAsync(serverFileInfo.ServerFileFullPath, token);
            if (fileStream == null)
            {
                _logger.LogWarning("No file {path}", serverFileInfo.FileSaveFullPath);
                continue;
            }

            FileInfo newFile = new(serverFileInfo.FileSaveFullPath);
            Directory.CreateDirectory(newFile.DirectoryName ?? "/");
            if (newFile.Exists) newFile.Delete();
            await using var fs = File.CreateText(newFile.FullName);
            fileStream.Seek(0, SeekOrigin.Begin);
            using StreamReader sr = new(fileStream, Encoding.UTF8, leaveOpen: true);
            while (await sr.ReadLineAsync(token) is { } line) await fs.WriteLineAsync(line);

            filesUpdated++;
        }

        _health?.SetHealthyStatus("UPDATE CONFIGURATION SUCCESSFULLY");
        return filesUpdated;
    }

    private Uri FormatFileInfoUri()
    {
        QueryBuilder qb =
            [];
        qb.Add("containerTypeName", _containerTypeName);
        qb.Add("containerSavePath", _configSaveDirectoryBase);
        UriBuilder ub = new(_serverUri)
        {
            Path = "/api/configuration/files/info",
            Query = qb.ToQueryString().ToString()
        };
        var uri = ub.Uri;
        return uri;
    }

    private Uri FormatFileUri(string filePath)
    {
        UriBuilder ub = new(_serverUri)
        {
            Path = $"/api/configuration/files/{Uri.EscapeDataString(filePath)}"
        };
        return ub.Uri;
    }
}