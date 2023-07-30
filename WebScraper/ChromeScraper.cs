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
    /// 
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
            // Theoretically obsolet since Selenium.WebDriver 4.6.0.
            // But during the implicite call of the new selenium-manager.exe a
            // short flicker of a console window appears.
            // Therefore the existing implementation is retained.
            // Because google didn't issue a downloadble driver for chrome 115 (see following log-extracts),
            // ChromeDriverInstaller had to be extended anywhere (see ChromeDriverInstaller for details).
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
            
            // await Task.Run(() => { Task.Delay(1); }); // just to suppress "no await"-warning.
        }

    }
}
