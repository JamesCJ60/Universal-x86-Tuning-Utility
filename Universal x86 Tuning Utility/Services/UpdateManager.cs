using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Octokit;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using ProductHeaderValue = Octokit.ProductHeaderValue;

public class UpdateManager
{
    private readonly string _owner;
    private readonly string _repo;
    private readonly string _currentVersion;
    private readonly string _downloadPath;
    public string _newVersion;
    public UpdateManager(string owner, string repo, string currentVersion, string downloadPath)
    {
        _owner = owner;
        _repo = repo;
        _currentVersion = currentVersion;
        _downloadPath = downloadPath;
    }

    public async Task<bool> IsUpdateAvailable()
    {
        try
        {
            var client = new GitHubClient(new ProductHeaderValue(_repo));
            var releases = await client.Repository.Release.GetAll(_owner, _repo);

            var latestRelease = releases[0];
            var latestVersion = new Version(latestRelease.TagName);
            _newVersion = latestVersion.ToString();
            var currentVersion = new Version(_currentVersion);

            return latestVersion.CompareTo(currentVersion) > 0;
        }
        catch (Exception ex)
        {
            // log error
        }

        return false;
    }


    public async Task<bool> DownloadAndInstallUpdate()
    {
        try
        {
            var client = new GitHubClient(new ProductHeaderValue(_repo));
            var releases = await client.Repository.Release.GetAll(_owner, _repo);

            var latestRelease = releases[0];
            var latestVersion = latestRelease.TagName;

            if (latestVersion != _currentVersion)
            {
                var assets = latestRelease.Assets;
                var downloadUrl = assets[0].BrowserDownloadUrl;

                var fileName = Path.GetFileName(downloadUrl);
                var filePath = Path.Combine(_downloadPath, fileName);

                using (var wc = new System.Net.WebClient())
                {
                    await wc.DownloadFileTaskAsync(downloadUrl, filePath);
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            // log error
        }

        return false;
    }
}