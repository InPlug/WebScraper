using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NetEti.WebTools
{
    /// <summary>
    /// Loads an actual test-browser and it's corresponding driver
    /// from https://github.com/GoogleChromeLabs/chrome-for-testing.
    /// 
    /// 21.08.2023 Erik Nagel: created.
    /// 21.08.2023 Erik Nagel: revised for "Chrome for Testing".
    /// </summary>
    public class ChromeDriverInstaller
    {
        const string BrowserPath = "browser";
        const string DriverPath = "driver";

        /// <summary>
        /// Prüft die aktuell installierte chrome-Browser-Version und holt den dazu passenden
        /// Treiber "chromedriver.exe", wenn dieser noch nicht vorhanden ist.
        /// </summary>
        /// <param name="forceDownload">Bei True wird der Treiber auch dann heruntergeladen, wenn
        /// dieser lokal schon vorhanden ist; Default: false.</param>
        /// <returns>Version der aktuell lokal installierten Treibers "chromedriver.exe".</returns>
        public async Task<InstallationInfo?> Install(bool forceDownload)
        {
            // Since v115.0.5763.0 this routine is the appropriate one due to the new "chrome for testing" site,
            // see https://github.com/GoogleChromeLabs/chrome-for-testing#json-api-endpoints

            bool downloadIt = forceDownload;

            ChromeForTestingJsonApiDataContainer chromeForTestingJsonApiDataContainer = await FetchChromeDriverInfos()
                ?? throw new ApplicationException("Could not retrieve chrome-infos.");
            string? newVersion = chromeForTestingJsonApiDataContainer.channels?.Stable?.version;

            string? browserLink;
            string? driverLink;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                browserLink = chromeForTestingJsonApiDataContainer.channels?.Stable?.downloads?.chrome?
                    .ToList().Where(i => (i.platform == "win64")).First().url;
                driverLink = chromeForTestingJsonApiDataContainer?.channels?.Stable?.downloads?.chromedriver?
                    .ToList().Where(i => (i.platform == "win64")).First().url;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                browserLink = chromeForTestingJsonApiDataContainer.channels?.Stable?.downloads?.chrome?
                    .ToList().Where(i => (i.platform == "linux64")).First().url;
                driverLink = chromeForTestingJsonApiDataContainer.channels?.Stable?.downloads?.chromedriver?
                    .ToList().Where(i => (i.platform == "linux64")).First().url;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                browserLink = chromeForTestingJsonApiDataContainer.channels?.Stable?.downloads?.chrome?
                    .ToList().Where(i => (i.platform == "mac-x64")).First().url;
                driverLink = chromeForTestingJsonApiDataContainer.channels?.Stable?.downloads?.chromedriver?
                    .ToList().Where(i => (i.platform == "mac-x64")).First().url;
            }
            else
            {
                throw new PlatformNotSupportedException("Your operating system is not supported.");
            }
            if (String.IsNullOrEmpty(browserLink) || String.IsNullOrEmpty(driverLink))
            {
                throw new PlatformNotSupportedException("Your operating system is not supported.");
            }
            InstallationInfo installationInfo = new()
            {
                RealBrowserPath = Path.Combine(BrowserPath, Path.GetFileNameWithoutExtension(browserLink)),
                RealDriverPath = Path.Combine(DriverPath, Path.GetFileNameWithoutExtension(driverLink))
            };
            string chromeVersion = GetChromeVersion(installationInfo.RealBrowserPath);
            if (!downloadIt)
            {
                string chromeVersionStub = Regex.Replace(chromeVersion, @"\.[^.]*$", "");
                string newVersionStub = Regex.Replace(newVersion ?? "", @"\.[^.]*$", "");
                if (newVersionStub != chromeVersionStub)
                {
                    downloadIt = true;
                }
            }

            if (downloadIt)
            {
                if (Directory.Exists(DriverPath))
                {
                    this.killChromedriverZombies();
                    Directory.Delete(DriverPath, true);
                }
                if (Directory.Exists(BrowserPath))
                {
                    Directory.Delete(BrowserPath, true);
                }
                Directory.CreateDirectory(DriverPath);
                Directory.CreateDirectory(BrowserPath);

                await LoadChromeBrowserAndDriver(browserLink, driverLink);

                // on Linux/macOS, you need to add the executable permission (+x) to allow the execution of the chromedriver
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    string? error = null;
                    Process? process = null;
                    try
                    {
                        process = Process.Start(
                            new ProcessStartInfo
                            {
                                FileName = "chmod",
                                Arguments = String.Format($" +x {driverLink}"),
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                            }
                        );
                        if (process != null)
                        {
                            error = await process.StandardError.ReadToEndAsync();
                            process.WaitForExit();
                        }
                    }
                    finally
                    {
                        process?.Dispose();
                    }
                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception("Failed to make chromedriver executable");
                    }
                }
            }

            return installationInfo;
        }

        /// <summary>
        /// Holt die Version des aktuell installierten chrome-Browsers aus dem BrowserPath.
        /// </summary>
        /// <param name="realBrowserPath">Path to the subdirectory with the standalone test-browser.</param>
        /// <returns>Version des aktuell installierten chrome-Browsers aus dem BrowserPath.</returns>
        public string GetChromeVersion(string realBrowserPath)
        {
            if (Directory.Exists(realBrowserPath))
            {
                String? version = Directory.GetFiles(realBrowserPath, "*.manifest").FirstOrDefault();
                if (!string.IsNullOrEmpty(version))
                {
                    return Path.GetFileNameWithoutExtension(version.ToLower());
                }
            }
            return String.Empty;
        }

        private static async Task<ChromeForTestingJsonApiDataContainer?> FetchChromeDriverInfos()
        {
            Uri uri = new Uri(
                @"https://googlechromelabs.github.io/chrome-for-testing/last-known-good-versions-with-downloads.json");

            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 10);
            HttpResponseMessage response;
            string? responseJsonString = null;
            ChromeForTestingJsonApiDataContainer? chromeForTestingJsonApiDataContainer = null;
            try
            {
                response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                responseJsonString = await response.Content.ReadAsStringAsync();
                chromeForTestingJsonApiDataContainer
                    = JsonConvert.DeserializeObject<ChromeForTestingJsonApiDataContainer>(responseJsonString);
            }
            catch (HttpRequestException)
            {
                // Handle exception here
                throw;
                // return null;
            }

            return chromeForTestingJsonApiDataContainer;
        }

        private async Task<bool> LoadChromeBrowserAndDriver(
            string browserLink, string driverLink)
        {
            return await HttpDownloadAndUnzip(browserLink, BrowserPath)
                && await HttpDownloadAndUnzip(driverLink, DriverPath);
        }

        private async Task<bool> HttpDownloadAndUnzip(string requestUri, string directoryToUnzip)
        {
            using var response = await new HttpClient().GetAsync(requestUri);
            if (!response.IsSuccessStatusCode) return false;

            using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
            using var zip = new ZipArchive(streamToReadFrom);
            zip.ExtractToDirectory(directoryToUnzip);
            return true;
        }

        private void killChromedriverZombies()
        {
            Process? process = Process.Start(
                new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = "/F /IM chromedriver.exe /T",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            );
            if (process?.WaitForExit(500) != true)
            {
                process?.Kill();
                Thread.Sleep(500);
            }
        }
    }
}