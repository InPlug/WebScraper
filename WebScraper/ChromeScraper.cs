using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NetEti.WebTools
{
    /// <summary>
    /// Starts a webbrowser and fetches values from the completed website.
    /// Waits for the DOM to be fully loaded if necessary.
    /// This is the implementation for Chrome-browsers.
    /// Uses Selenium 4.
    /// Hint: despite some warnings on the internet, usage of SeleniumExtras.WaitHelpers 3.11.0 is safe.
    /// The Implementation is static clean code, open source and doesn't seem to need any further support.
    /// </summary>
    /// <remarks>
    /// Author: Erik Nagel
    ///
    /// 28.11.2020 Erik Nagel: created.
    /// 28.08.2022 Erik Nagel: revised for selenium 4.
    /// </remarks>
    public class ChromeScraper: WebScraperBase
    {
        /// <summary>
        /// Constructor - takes a website url and starts the WebDriver at the current directory.
        /// </summary>
        /// <param name="url">The complete website url including https, etc.</param>
        /// <param name="chromeOptions">Specific ChromeOptions or null (default: null).</param>
        /// <param name="quiet">True (default): no browser-window will be opened and no messages issued.</param>
        public ChromeScraper(string? url, ChromeOptions? chromeOptions=null, bool quiet=true)
            : this(url, Directory.GetCurrentDirectory(), chromeOptions, quiet) { }

        /// <summary>
        /// Constructor - takes a website url and starts the WebDriver at the given driverPath.
        /// </summary>
        /// <param name="url">The complete website url including https, etc.</param>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        /// <param name="chromeOptions">Specific ChromeOptions or null (default: null).</param>
        /// <param name="quiet">True (default): no browser-window will be opened and no messages issued.</param>
        public ChromeScraper(string? url, string? driverPath, ChromeOptions? chromeOptions=null, bool quiet=true)
            : base(url, driverPath, (DriverOptions?)chromeOptions, quiet) { }

        /// <summary>
        /// Constructor - takes a website url and starts the WebDriver at the given driverPath.
        /// </summary>
        /// <param name="url">The complete website url including https, etc.</param>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        /// <param name="pageLoadTimeoutSeconds">Webdriver page-load-timeout, default = 10 (seconds).</param>
        /// <param name="chromeOptions">Specific ChromeOptions or null (default: null).</param>
        /// <param name="quiet">True (default): no browser-window will be opened and no messages issued.</param>
        public ChromeScraper(string? url, string driverPath, int pageLoadTimeoutSeconds, ChromeOptions? chromeOptions=null,  bool quiet=true)
            : base(url, driverPath, pageLoadTimeoutSeconds, (DriverOptions?)chromeOptions, quiet) { }

        /// <summary>
        /// Instantiates the concrete Driver (chromedriver.exe here).
        /// </summary>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        /// <param name="options">Specific DriverOptions or null (default: null).</param>
        protected override async Task SetupDriverInstance(string? driverPath, DriverOptions? options)
        {
            await InstallChromeDriver(driverPath);

            ChromeDriverService service = ChromeDriverService.CreateDefaultService(driverPath);
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;
            ChromeOptions chromeOptions = (ChromeOptions?)options ?? new ChromeOptions();
            if (this.Quiet)
            {
                chromeOptions.AddArguments(new List<string>() {
                 "headless", "no-sandbox", "disable-gpu" /* unsatisfactory: "log-level=3"*, senseless: "--silent" "log-level=OFF" */
                });
            }
            else
            {
                chromeOptions.AddArguments(new List<string>() {
                 "--start-maximized", "--ignore-certificate-errors", "--disable-popup-blocking", "--incognito"
            });
            }
            this.WebDriver = new ChromeDriver(service, chromeOptions);
        }

        /// <summary>
        /// Loads a web-page from a locally saved html-file.
        /// </summary>
        /// <param name="htmlFileName">Path to a local html file.</param>
        public void Load(string htmlFileName)
        {
            Uri uri = new System.Uri(Path.GetFullPath(htmlFileName));
            this.Load(uri);
        }

        /// <summary>
        /// Loads a web-page from a string-list.
        /// </summary>
        /// <param name="htmlFileLines">A string list with valid html.</param>
        public void Load(List<string> htmlFileLines)
        {
            string htmlFileName = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "index.html"));
            File.WriteAllLines(htmlFileName, htmlFileLines);
            Uri uri = new System.Uri(htmlFileName);
            this.Load(uri);
        }

        private static bool _isChromeDriverInstalled = false;

        private static async Task InstallChromeDriver(string? workingDirectory = null)
        {
            if (_isChromeDriverInstalled)
            {
                return;
            }

            if (String.IsNullOrEmpty(workingDirectory))
            {
                workingDirectory = Directory.GetCurrentDirectory();
            }

            ChromeDriverInstaller chromeDriverInstaller = new ChromeDriverInstaller();

            // not necessary, but added for logging purposes
            var chromeVersion = await chromeDriverInstaller.GetChromeVersion();
            // Console.WriteLine($"Chrome version {chromeVersion} detected");

            await chromeDriverInstaller.Install(chromeVersion, workingDirectory);
            // Console.WriteLine("ChromeDriver installed");

            _isChromeDriverInstalled = true;
        }

    }
}
