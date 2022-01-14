using GetDynamicWebsiteContent;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading.Tasks;

namespace NetEti.WebTools
{
    /// <summary>
    /// Determines whether the existence or the visibility of an element is to be checked.
    /// </summary>
    public enum LocatorCondition
    {
        /// <summary>Checks whether the element exists.</summary>
        Exists,

        /// <summary>Checks whether the element is visible.</summary>
        IsVisible,

        /// <summary>Checks whether the element is clickable.</summary>
        IsClickable
    }

    /// <summary>
    /// Base Class for WebScapers like ChromeScraper or others.
    /// </summary>
    /// <remarks>
    /// Author: Erik Nagel
    ///
    /// 28.11.2020 Erik Nagel: created.
    /// </remarks>
    public abstract class WebScraperBase : IDisposable
    {

        /// <summary>
        /// Constructor - takes a website url and starts the WebDriver.
        /// </summary>
        /// <param name="url">The complete website url including https, etc.</param>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        public WebScraperBase(string url, string driverPath) : this(url, driverPath, 60) { }

        /// <summary>
        /// Constructor - takes a website url and starts the WebDriver.
        /// </summary>
        /// <param name="url">The complete website url including https, etc.</param>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        /// <param name="timeout">Webdriver search-for-stable-element-timeout, default = 60 (seconds).</param>
        public WebScraperBase(string url, string driverPath, int timeout)
        {
            this.SetupDriver(url, driverPath, timeout);
        }

        /// <summary>
        /// Browser controlling instance of IWebDriver.
        /// </summary>
        public IWebDriver WebDriver { get; set; }

        /// <summary>
        /// Instantiates the concrete Driver (e.g. ChromeDriver), DefaultWait and FluentWait.
        /// Must be overwritten.
        /// </summary>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        protected abstract Task SetupDriverInstance(string driverPath);

        /// <summary>
        /// Sets the default waiting time for every retrieval operation
        /// Default: 0 seconds.
        /// </summary>
        protected WebDriverWait ImplicitWait { get; set; }

        /// <summary>
        /// Sets the waiting time for a specific retrieval operation
        /// Default: 30 seconds.
        /// </summary>
        protected DefaultWait<IWebDriver> FluentWait { get; set; }

        /// <summary>
        /// Disposes an eventually previously instantiated driver and calls SetupDriverInstance(url)
        /// Sets up ImplicitWait and FluentWait. Navigates to the given url.
        /// </summary>
        /// <param name="url">The complete website url including https, etc.</param>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        /// <param name="timeout">Webdriver search-for-stable-element-timeout, default = 60 (seconds).</param>
        public void SetupDriver(string url, string driverPath, int timeout)
        {
            this.WebDriver?.Dispose();
            // Hier wird auf das Beenden der asynchronen Methode SetupDriverInstance gewartet.
            Task task = this.SetupDriverInstance(driverPath);
            task.Wait();

            this.ImplicitWait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(0));

            this.FluentWait = new DefaultWait<IWebDriver>(this.WebDriver);
            this.FluentWait.Timeout = TimeSpan.FromSeconds(timeout);
            this.FluentWait.PollingInterval = TimeSpan.FromMilliseconds(250);
            this.FluentWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            this.FluentWait.Message = "Element to be searched not found";

            this.WebDriver.Navigate().GoToUrl(url);
        }

        /// <summary>
        /// Tries to instantiate the concrete Driver (e.g. ChromeDriver), DefaultWait and FluentWait.
        /// </summary>
        /// <param name="url">True, if succeeded.</param>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        /// <param name="timeout">Webdriver search-for-stable-element-timeout, default = 60 (seconds).</param>
        /// <returns>True, if succeeded.</returns>
        public bool TrySetupDriver(string url, string driverPath, int timeout)
        {
            try
            {
                this.SetupDriver(url, driverPath, timeout);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Waits by Selenium fluent wait until a StableWebElement is found or a timeout occurs.
        /// </summary>
        /// <param name="locator">The search pattern to locate a specific DOM-element.</param>
        /// <param name="condition">Search either for visible or for existing elements.</param>
        /// <returns>Found DOM-element as StableWebElement.</returns>
        public StableWebElement WaitForStableWebElement(By locator, LocatorCondition condition)
        {
            switch (condition)
            {
                case LocatorCondition.Exists:
                    return new StableWebElement(this.FluentWait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator)), locator);
                case LocatorCondition.IsVisible:
                    return new StableWebElement(this.FluentWait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator)), locator);
                case LocatorCondition.IsClickable:
                    return new StableWebElement(this.FluentWait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator)), locator);
                default:
                    throw new ArgumentException("WebScraperBase: unknown LocatorCondition.");
            }
        }

        #region IDisposable Member

        private bool _disposed = false;

        /// <summary>
        /// Pubic clean up method.
        /// </summary>
        public void Dispose()
        {
            this.DisposeIfNecessary(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This is for calls to Dispose() for all user elements that are disposable.
        /// Should be overwritten in descendant classes, if necessary.
        /// </summary>
        protected virtual void Cleanup() 
        {
            this.WebDriver?.Dispose();
        }

        /// <summary>
        /// This is for calls to Dispose() for all user elements that are disposable.
        /// Should be overwritten in descendant classes, if necessary.
        /// </summary>
        /// <param name="disposing">If true, this method was called by the public Dispose method;
        /// if false, this method was called by internal destructor.</param>
        private void DisposeIfNecessary(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this.Cleanup();
                }
                this._disposed = true;
            }
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~WebScraperBase()
        {
            this.DisposeIfNecessary(false);
        }

        #endregion IDisposable Member

    }
}
