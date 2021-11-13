using System.Collections.Generic;
using System.IO;
using OpenQA.Selenium.Chrome;

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
        /// <param name="driverPath">The full path to the webdriver.</param>
        public ChromeScraper(string url, string driverPath) : base(url, driverPath) { }

        /// <summary>
        /// Instantiates the concrete Driver (ChromeDriver here).
        /// </summary>
        /// <param name="driverPath">The full path to the webdriver.</param>
        protected override void SetupDriverInstance(string driverPath)
        {
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

