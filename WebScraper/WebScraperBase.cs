using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading.Tasks;

namespace NetEti.WebTools
{
    ///<summary>Provides the various selenium-locator types.</summary>
    public enum LocatorType
    {
        ///<summary>default</summary>
        None,
        ///<summary>Locates elements whose class name contains the search value(compound class names are not permitted)</summary>
        ClassName,
        ///<summary>Locates elements matching a CSS selector</summary>
        CssSelector,
        ///<summary>Locates elements whose ID attribute matches the search value</summary>
        Id,
        ///<summary>Locates elements whose NAME attribute matches the search value</summary>
        Name,
        ///<summary>Locates anchor elements whose visible text matches the search value</summary>
        LinkText,
        ///<summary>Locates anchor elements whose visible text contains the search value.If multiple elements are matching, only the first one will be selected.</summary>
        PartialLinkText,
        ///<summary>Locates elements whose tag name matches the search value</summary>
        TagName,
        ///<summary>Locates elements matching an XPath expression</summary>
        Xpath
    }

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
    /// 28.08.2022 Erik Nagel: Revised for selenium 4.
    /// </remarks>
    public abstract class WebScraperBase : IDisposable
    {
        /// <summary>
        /// Default maximum seconds for selenium waits.
        /// </summary>
        public const int DEFAULT_PAGE_LOAD_TIMEOUT_SECONDS = 10;

        /// <summary>
        /// Constructor - initializes the WebDriver.
        /// </summary>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        public WebScraperBase(string driverPath) : this(null, driverPath, DEFAULT_PAGE_LOAD_TIMEOUT_SECONDS) { }

        /// <summary>
        /// Constructor - initializes the WebDriver with a given timeout for page-loading.
        /// </summary>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        /// <param name="pageLoadTimeoutSeconds">Webdriver page-load-timeout, default = 10 (seconds).</param>
        public WebScraperBase(string driverPath, int pageLoadTimeoutSeconds) : this(null, driverPath, pageLoadTimeoutSeconds) { }

        /// <summary>
        /// Constructor - initializes the WebDriver and navigates to the given url.
        /// </summary>
        /// <param name="url">The complete website url including https, etc.</param>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        public WebScraperBase(string url, string driverPath) : this(url, driverPath, DEFAULT_PAGE_LOAD_TIMEOUT_SECONDS) { }

        /// <summary>
        /// Constructor - initializes the WebDriver, sets the timeout for page-loading and navigates to the given url.
        /// </summary>
        /// <param name="url">The complete website url including https, etc.</param>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        /// <param name="pageLoadTimeoutSeconds">Webdriver page-load-timeout, default = 10 (seconds).</param>
        public WebScraperBase(string url, string driverPath, int pageLoadTimeoutSeconds)
        {
            this.SetupDriver(driverPath);
            if (!String.IsNullOrEmpty(url))
            {
                this.WebDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(pageLoadTimeoutSeconds);
                this.WebDriver.Navigate().GoToUrl(url);
            }
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
        /// Disposes an eventually previously instantiated driver and calls SetupDriverInstance(url)
        /// Sets up ImplicitWait and FluentWait. Navigates to the given url.
        /// </summary>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        public void SetupDriver(string driverPath)
        {
            this.WebDriver?.Dispose();
            Task task = this.SetupDriverInstance(driverPath);
            task.Wait();
            this.WebDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0); // no implicite waiting
        }

        /// <summary>
        /// Loads a web-page from an uri.
        /// </summary>
        /// <param name="uri">The website address (uri/url).</param>
        public void Load(Uri uri)
        {
            var absoluteUri = uri.AbsoluteUri;
            this.WebDriver.Navigate().GoToUrl(uri);
        }

        /// <summary>
        /// Tries to instantiate the concrete Driver (e.g. ChromeDriver).
        /// </summary>
        /// <param name="driverPath">Webdriver's containing directory.</param>
        /// <returns>True, if succeeded.</returns>
        public bool TrySetupDriver(string driverPath)
        {
            try
            {
                this.SetupDriver(driverPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Waits under a given condition for a web-page-element  for a given time and returns it, if sucessful.
        /// </summary>
        /// <param name="waitCondition">A function, that describes, under which circumstances a result is accepted.</param>
        /// <param name="maxWaitTime">Maximum time to wait, otherwise an exception is thrown.</param>
        /// <returns>IWebElement if succesful.</returns>
        public IWebElement GetElement(Func<IWebDriver, IWebElement> waitCondition, TimeSpan maxWaitTime)
        {
            IWebElement webElement = null;
            try
            {
                webElement = new WebDriverWait(this.WebDriver, maxWaitTime).Until(waitCondition);
            }
            catch (Exception ex) when (ex is NoSuchElementException || ex is WebDriverTimeoutException)
            {
                throw;
            }
            return webElement;
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
