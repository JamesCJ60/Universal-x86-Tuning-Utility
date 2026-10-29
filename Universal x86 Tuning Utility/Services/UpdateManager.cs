using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Octokit;

public class UpdateManager
{
    internal const string InstallerName = "Universal.x86.Tuning.Utility.msi";
    private static readonly HttpClient DownloadClient = new() { Timeout = TimeSpan.FromMinutes(15) };
    private readonly string _owner;
    private readonly string _repo;
    private readonly string _currentVersion;
    private readonly string _downloadPath;
    private readonly bool _includePreReleases;
    private Release? _selectedRelease;

    public string NewVersion => _selectedRelease?.TagName ?? string.Empty;

    public UpdateManager(string owner, string repo, string currentVersion, string downloadPath, bool includePreReleases = false)
    {
        _owner = owner;
        _repo = repo;
        _currentVersion = currentVersion;
        _downloadPath = downloadPath;
        _includePreReleases = includePreReleases;
    }

    public async Task<bool> IsUpdateAvailable()
    {
        _selectedRelease = null;
        var client = new GitHubClient(new ProductHeaderValue(_repo));
        var releases = await client.Repository.Release.GetAll(_owner, _repo).ConfigureAwait(false);
        _selectedRelease = SelectRelease(releases, _currentVersion, _includePreReleases);
        return _selectedRelease != null;
    }

    internal static Release? SelectRelease(IEnumerable<Release> releases, string currentVersion, bool includePreReleases)
    {
        var installed = ReleaseVersion.Parse(currentVersion)
            ?? throw new InvalidOperationException("The application version is invalid.");

        return releases
            .Where(release => !release.Draft)
            .Select(release => (Release: release, Version: ReleaseVersion.Parse(release.TagName)))
            .Where(candidate => candidate.Version != null &&
                (includePreReleases || (!candidate.Release.Prerelease && !candidate.Version.IsPreRelease)) &&
                candidate.Version.CompareTo(installed) > 0 &&
                candidate.Release.Assets.Any(IsInstaller))
            .OrderByDescending(candidate => candidate.Version)
            .ThenBy(candidate => candidate.Release.Prerelease)
            .ThenByDescending(candidate => candidate.Release.PublishedAt)
            .Select(candidate => candidate.Release)
            .FirstOrDefault();
    }

    private static bool IsInstaller(ReleaseAsset asset) =>
        string.Equals(asset.Name, InstallerName, StringComparison.OrdinalIgnoreCase);

    public async Task<bool> DownloadAndInstallUpdate()
    {
        if (_selectedRelease == null && !await IsUpdateAvailable().ConfigureAwait(false))
            return false;

        var asset = _selectedRelease!.Assets.First(IsInstaller);
        var filePath = await DownloadInstallerAsync(asset, DownloadClient).ConfigureAwait(false);
        using var installer = Process.Start(CreateInstallerStartInfo(filePath));
        if (installer == null)
            throw new InvalidOperationException("Windows Installer could not be started.");

        return true;
    }

    internal static ProcessStartInfo CreateInstallerStartInfo(string filePath) => new()
    {
        FileName = Path.Combine(Environment.SystemDirectory, "msiexec.exe"),
        Arguments = $"/i \"{filePath}\" /norestart",
        UseShellExecute = true
    };

    internal async Task<string> DownloadInstallerAsync(ReleaseAsset asset, HttpClient client)
    {
        var downloadUri = new Uri(asset.BrowserDownloadUrl, UriKind.Absolute);
        if (downloadUri.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(downloadUri.Host, "github.com", StringComparison.OrdinalIgnoreCase) ||
            !downloadUri.AbsolutePath.StartsWith($"/{_owner}/{_repo}/releases/download/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The installer URL is not a GitHub release asset for this application.");

        var directory = Path.Combine(_downloadPath, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, InstallerName);
        var partialPath = filePath + ".partial";

        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(15));
            using var response = await client.GetAsync(downloadUri, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using (var stream = await response.Content.ReadAsStreamAsync(timeout.Token).ConfigureAwait(false))
            await using (var file = new FileStream(partialPath, System.IO.FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
            {
                await stream.CopyToAsync(file, timeout.Token).ConfigureAwait(false);
                if (file.Length == 0 || (asset.Size > 0 && file.Length != asset.Size))
                    throw new InvalidDataException("The installer download is incomplete.");
            }

            File.Move(partialPath, filePath);
            return filePath;
        }
        finally
        {
            if (File.Exists(partialPath))
                File.Delete(partialPath);
        }
    }

    internal sealed record ReleaseVersion(Version Number, string PreRelease) : IComparable<ReleaseVersion>
    {
        public bool IsPreRelease => PreRelease.Length > 0;

        public static ReleaseVersion? Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var match = Regex.Match(value.Trim(), @"^[vV]?(\d+\.\d+(?:\.\d+){0,2})(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$");
            if (!match.Success || !Version.TryParse(match.Groups[1].Value, out var number))
                return null;

            return new ReleaseVersion(new Version(number.Major, number.Minor, Math.Max(0, number.Build), Math.Max(0, number.Revision)), match.Groups[2].Value);
        }

        public int CompareTo(ReleaseVersion? other)
        {
            if (other == null)
                return 1;

            var result = Number.CompareTo(other.Number);
            if (result != 0)
                return result;
            if (!IsPreRelease || !other.IsPreRelease)
                return IsPreRelease == other.IsPreRelease ? 0 : IsPreRelease ? -1 : 1;

            var left = PreRelease.Split('.');
            var right = other.PreRelease.Split('.');
            for (var index = 0; index < Math.Min(left.Length, right.Length); index++)
            {
                var leftNumeric = left[index].All(char.IsAsciiDigit);
                var rightNumeric = right[index].All(char.IsAsciiDigit);
                if (leftNumeric && rightNumeric)
                {
                    var leftNumber = left[index].TrimStart('0');
                    var rightNumber = right[index].TrimStart('0');
                    result = leftNumber.Length.CompareTo(rightNumber.Length);
                    if (result == 0)
                        result = string.CompareOrdinal(leftNumber, rightNumber);
                }
                else
                    result = leftNumeric != rightNumeric ? leftNumeric ? -1 : 1 : string.CompareOrdinal(left[index], right[index]);

                if (result != 0)
                    return result;
            }

            return left.Length.CompareTo(right.Length);
        }
    }
}
