using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace NetEti.WebTools
{
    /// <summary>
    /// Lädt den zur aktuell installierten chrome-Browser-Version passenden Treiber (chromedriver.exe).
    /// Kopiert die passende "chromedriver.exe" von der google-Webseite, wenn die passende Version
    /// noch nicht heruntergeladen wurde.
    /// Author: Niels Swimberghe, vielen Dank dafür.
    /// Habe seine Lösung nur auf .Net-Framework und ein paar Belange der Vishnu-Verarbeitung
    /// angepasst, aber ansonsten unverändert übernommen.
    /// https://swimburger.net/blog/dotnet/download-the-right-chromedriver-version-and-keep-it-up-to-date-on-windows-linux-macos-using-csharp-dotnet
    /// </summary>
    public class ChromeDriverInstaller
    {
        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://chromedriver.storage.googleapis.com/")
        };

        /// <summary>
        /// Prüft die aktuell installierte chrome-Browser-Version und holt den dazu passenden
        /// Treiber "chromedriver.exe", wenn dieser noch nicht vorhanden ist.
        /// </summary>
        /// <returns>Version der aktuell lokal installierten Treibers "chromedriver.exe".</returns>
        public Task Install() => Install(null, null, false);

        /// <summary>
        /// Prüft die aktuell installierte chrome-Browser-Version und holt den dazu passenden
        /// Treiber "chromedriver.exe", wenn dieser noch nicht vorhanden ist.
        /// </summary>
        /// <param name="chromeVersion">Version des aktuell installierten chrome-Browsers oder null.</param>
        /// <returns>Version der aktuell lokal installierten Treibers "chromedriver.exe".</returns>
        public Task Install(string? chromeVersion) => Install(chromeVersion, null, false);

        /// <summary>
        /// Prüft die aktuell installierte chrome-Browser-Version und holt den dazu passenden
        /// Treiber "chromedriver.exe", wenn dieser noch nicht vorhanden ist.
        /// </summary>
        /// <param name="forceDownload">Bei True wird der Treiber auch dann heruntergeladen, wenn
        /// dieser lokal schon vorhanden ist; Default: false.</param>
        /// <returns>Version der aktuell lokal installierten Treibers "chromedriver.exe".</returns>
        public Task Install(bool forceDownload) => Install(null, null, forceDownload);

        /// <summary>
        /// Prüft die aktuell installierte chrome-Browser-Version und holt den dazu passenden
        /// Treiber "chromedriver.exe", wenn dieser noch nicht vorhanden ist.
        /// </summary>
        /// <param name="chromeVersion">Version des aktuell installierten chrome-Browsers oder null.</param>
        /// <param name="forceDownload">Bei True wird der Treiber auch dann heruntergeladen, wenn
        /// dieser lokal schon vorhanden ist; Default: false.</param>
        /// <returns>Version der aktuell lokal installierten Treibers "chromedriver.exe".</returns>
        public Task Install(string? chromeVersion, bool forceDownload) => Install(chromeVersion, null, forceDownload);

        /// <summary>
        /// Prüft die aktuell installierte chrome-Browser-Version und holt den dazu passenden
        /// Treiber "chromedriver.exe", wenn dieser noch nicht vorhanden ist.
        /// </summary>
        /// <param name="chromeVersion">Version des aktuell installierten chrome-Browsers oder null.</param>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        /// <returns>Version der aktuell lokal installierten Treibers "chromedriver.exe".</returns>
        public Task Install(string? chromeVersion, string? driverPath) => Install(chromeVersion, driverPath, false);

        /// <summary>
        /// Prüft die aktuell installierte chrome-Browser-Version und holt den dazu passenden
        /// Treiber "chromedriver.exe", wenn dieser noch nicht vorhanden ist.
        /// </summary>
        /// <param name="chromeVersion">Version des aktuell installierten chrome-Browsers oder null.</param>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        /// <param name="forceDownload">Bei True wird der Treiber auch dann heruntergeladen, wenn
        /// dieser lokal schon vorhanden ist; Default: false.</param>
        /// <returns>Version der aktuell lokal installierten Treibers "chromedriver.exe".</returns>
        public async Task Install(string? chromeVersion, string? driverPath, bool forceDownload)
        {
            // Instructions from https://chromedriver.chromium.org/downloads/version-selection
            //   First, find out which version of Chrome you are using. Let's say you have Chrome 72.0.3626.81.
            if (chromeVersion == null)
            {
                chromeVersion = await GetChromeVersion();
            }
            // Because google didn't issue a downloadble driver for chrome 115 (see following log-extracts),
            // ChromeDriverInstaller had to be extended.
            // Log:
            //     selenium-manager.exe --browser chrome --clear-cache --clear-metadata --trace
            //     ...
            //     DEBUG   The version of chrome is 115.0.5790.110
            //     ...
            //     DEBUG   Detected browser: chrome 115
            //     ...
            //     WARN    Error getting version of chromedriver 115. Retrying with chromedriver 114 (attempt 1/5)
            //     DEBUG   Reading chromedriver version from https://chromedriver.storage.googleapis.com/LATEST_RELEASE_114
            //     TRACE   Writing metadata to C:\Users\micro\.cache\selenium\selenium-manager.json
            //     DEBUG   Required driver: chromedriver 114.0.5735.90
            //     ...
            //     TRACE   Downloading https://chromedriver.storage.googleapis.com/114.0.5735.90/chromedriver_win32.zip to temporal folder "C:\\Users\\micro\\AppData\\Local\\Temp\\selenium-managerO9hQVc"
            //     ...
            //     INFO    C:\Users\micro\.cache\selenium\chromedriver\win32\114.0.5735.90\chromedriver.exe

            // BaseAddress = new Uri("https://chromedriver.storage.googleapis.com/")
            // Take the Chrome version number, remove the last part.
            string? chromeDriverVersion = null;
            string? previousChromeVersion;
            int attempts = 0;
            while (chromeDriverVersion == null && attempts++ < 5)
            {
                previousChromeVersion = chromeVersion;
                if (chromeVersion?.Contains('.') == true)
                {
                    chromeVersion = chromeVersion?.Substring(0, chromeVersion.LastIndexOf('.'));
                }
                else
                {
                    chromeVersion = (Convert.ToInt32(previousChromeVersion) - 1).ToString();
                }
                chromeDriverVersion = await GetLatestAppropriateChromeDriverVersion(chromeVersion);
            }
            if (String.IsNullOrEmpty(chromeDriverVersion))
            {
                throw new Exception($"ChromeDriver version not found for Chrome version {chromeVersion}");
            }

            string zipName;
            string driverName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                zipName = "chromedriver_win32.zip";
                driverName = "chromedriver.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                zipName = "chromedriver_linux64.zip";
                driverName = "chromedriver";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                zipName = "chromedriver_mac64.zip";
                driverName = "chromedriver";
            }
            else
            {
                throw new PlatformNotSupportedException("Your operating system is not supported.");
            }
            string? targetPath = driverPath;
            if (String.IsNullOrEmpty(targetPath))
            {
                targetPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            }
            targetPath = Path.Combine(targetPath, driverName);
            if (!forceDownload && File.Exists(targetPath))
            {
                string? existingChromeDriverVersion = null;
                string? error = null;
                Process? process = null;
                try
                {
                    process = Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = targetPath,
                            Arguments = "--version",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        }
                    );
                    if (process != null)
                    {
                        existingChromeDriverVersion = await process.StandardOutput.ReadToEndAsync();
                        error = await process.StandardError.ReadToEndAsync();
                        process.WaitForExit();
                    }
                }
                finally
                {
                    process?.Dispose();
                }
                // expected output is something like "ChromeDriver 88.0.4324.96 (68dba2d8a0b149a1d3afac56fa74648032bcf46b-refs/branch-heads/4324@{#1784})"
                // the following line will extract the version number and leave the rest
                existingChromeDriverVersion = existingChromeDriverVersion?.Split(' ')[1];
                if (chromeDriverVersion == existingChromeDriverVersion)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception($"Failed to execute {driverName} --version");
                }
            }

            //   Use the URL created in the last step to retrieve a small file containing the version of ChromeDriver to use. For example, the above URL will get your a file containing "72.0.3626.69". (The actual number may change in the future, of course.)
            //   Use the version number retrieved from the previous step to construct the URL to download ChromeDriver. With version 72.0.3626.69, the URL would be "https://chromedriver.storage.googleapis.com/index.html?path=72.0.3626.69/".
            var driverZipResponse = await httpClient.GetAsync($"{chromeDriverVersion}/{zipName}");
            if (!driverZipResponse.IsSuccessStatusCode)
            {
                throw new Exception($"ChromeDriver download request failed with status code: {driverZipResponse.StatusCode}, reason phrase: {driverZipResponse.ReasonPhrase}");
            }

            this.killChromedriverZombies();

            // this reads the zipfile as a stream, opens the archive, 
            // and extracts the chromedriver executable to the targetPath without saving any intermediate files to disk
            using (var zipFileStream = await driverZipResponse.Content.ReadAsStreamAsync())
            using (var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read))
            using (var chromeDriverWriter = new FileStream(targetPath, FileMode.Create))
            {
                ZipArchiveEntry? entry = zipArchive.GetEntry(driverName);
                Stream? chromeDriverStream = null;
                try
                {
                    chromeDriverStream = entry?.Open();
                    if (chromeDriverStream != null)
                    {
                        await chromeDriverStream.CopyToAsync(chromeDriverWriter);
                    }
                }
                finally
                {
                    chromeDriverStream?.Dispose();
                }
            }

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
                            Arguments = String.Format($" +x {targetPath}"),
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

        private static async Task<string?> GetLatestAppropriateChromeDriverVersion(string? chromeVersion)
        {
            // Append the result to URL "https://chromedriver.storage.googleapis.com/LATEST_RELEASE_". 
            // For example, with Chrome version 72.0.3626.81, you'd get a URL "https://chromedriver.storage.googleapis.com/LATEST_RELEASE_72.0.3626".
            var chromeDriverVersionResponse = await httpClient.GetAsync($"LATEST_RELEASE_{chromeVersion}");
            if (!chromeDriverVersionResponse.IsSuccessStatusCode)
            {
                if (chromeDriverVersionResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    // throw new Exception($"ChromeDriver version not found for Chrome version {chromeVersion}");
                    return null;
                }
                else
                {
                    throw new Exception($"ChromeDriver version request failed with status code: {chromeDriverVersionResponse.StatusCode}, reason phrase: {chromeDriverVersionResponse.ReasonPhrase}");
                }
            }
            var chromeDriverVersion = await chromeDriverVersionResponse.Content.ReadAsStringAsync();
            return chromeDriverVersion;
        }

        /// <summary>
        /// Holt die Version des aktuell installierten chrome-Browsers aus der Registry.
        /// </summary>
        /// <returns>Version der aktuell lokal installierten Treibers "chromedriver.exe".</returns>
        public async Task<string?> GetChromeVersion()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string? chromePath
                    = (string?)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe", null, null);
                if (chromePath == null)
                {
                    throw new Exception("Google Chrome not found in registry");
                }

                FileVersionInfo? fileVersionInfo = FileVersionInfo.GetVersionInfo(chromePath);
                return fileVersionInfo?.FileVersion;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    string? output = null;
                    string? error = null;
                    Process? process = null;
                    try
                    {
                        process = Process.Start(
                            new ProcessStartInfo
                            {
                                FileName = "google-chrome",
                                Arguments = "--product-version",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                            }
                        );
                        if (process != null)
                        {
                            output = await process.StandardOutput.ReadToEndAsync();
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
                        throw new Exception(error);
                    }

                    return output;
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred trying to execute 'google-chrome --product-version'", ex);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                try
                {
                    string? output = null;
                    string? error = null;
                    Process? process = null;
                    try
                    {
                        process = Process.Start(
                            new ProcessStartInfo
                            {
                                FileName = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                                Arguments = "--version",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                            }
                        );
                        if (process != null)
                        {
                            output = await process.StandardOutput.ReadToEndAsync();
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
                        throw new Exception(error);
                    }
                    output = output?.Replace("Google Chrome ", "");
                    return output;
                }
                catch (Exception ex)
                {
                    throw new Exception($"An error occurred trying to execute '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome --version'", ex);
                }
            }
            else
            {
                throw new PlatformNotSupportedException("Your operating system is not supported.");
            }
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
            /*
            Process process2 = Process.Start(
                new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = "/F /IM chrome.exe /T",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            );
            */
            /*
            Process process3 = Process.Start(
                new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = "/F /IM conhost.exe /T",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            );
            */
        }

    }
}