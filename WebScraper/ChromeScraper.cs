using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;

// ACHTUNG: Selenium.WebDriver und Selenium.Support müssen auf Version 3.141.0 bleiben,
//    NICHT auf die 4er Version updaten, sonst Fehler im Vishnu-Betrieb:
//          Could not load type 'OpenQA.Selenium.Internal.IWrapsElement' from assembly 'WebDriver,
//          Version=4.0.0.0, Culture=neutral, PublicKeyToken=null'
namespace NetEti.WebTools
{

    /// <summary>
    /// Starts a webbrowser and fetches values from the completed website.
    /// Waits for the DOM to be fully loaded if necessary.
    /// This is the implementation for Chrome-browsers.
    /// Uses Selenium.
    /// </summary>
    /// <remarks>
    /// Author: Erik Nagel
    ///
    /// 28.11.2020 Erik Nagel: created.
    /// </remarks>
    public class ChromeScraper : WebScraperBase
    {
        /// <summary>
        /// Constructor - takes a website url and starts the WebDriver at the current directory.
        /// </summary>
        /// <param name="url">The complete website url including https, etc.</param>
        public ChromeScraper(string url) : this(url, Directory.GetCurrentDirectory()) { }

        /// <summary>
        /// Constructor - takes a website url and starts the WebDriver at the given driverPath.
        /// </summary>
        /// <param name="url">The complete website url including https, etc.</param>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        public ChromeScraper(string url, string driverPath) : this(url, driverPath, 60) { }

        /// <summary>
        /// Constructor - takes a website url and starts the WebDriver at the given driverPath.
        /// </summary>
        /// <param name="url">The complete website url including https, etc.</param>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        /// <param name="timeout">Webdriver search-for-stable-element-timeout, default = 60 (seconds).</param>
        public ChromeScraper(string url, string driverPath, int timeout) : base(url, driverPath, timeout) { }

        /// <summary>
        /// Instantiates the concrete Driver (chromedriver.exe here).
        /// </summary>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        protected override async Task SetupDriverInstance(string driverPath)
        {
            ChromeDriverInstaller chromeDriverInstaller = new ChromeDriverInstaller();

            // not necessary, but added for logging purposes
            var chromeVersion = await chromeDriverInstaller.GetChromeVersion();
            // Console.WriteLine($"Chrome version {chromeVersion} detected");

            await chromeDriverInstaller.Install(chromeVersion, driverPath);
            // Console.WriteLine("ChromeDriver installed");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService(driverPath);
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;

            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments(new List<string>() {
                 "headless", "no-sandbox", "disable-gpu" /* reicht nicht: "log-level=3"*, sinnlos: "--silent" "log-level=OFF" */
            });

            this.WebDriver = new ChromeDriver(service, chromeOptions);

            //ChromeOptions chromeOptions = new ChromeOptions()
            //{
            //    // BinaryLocation = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe"
            //    BinaryLocation = driverPath
            //};
            // this.WebDriver = new ChromeDriver(driverPath, chromeOptions);
        }
    }
}

