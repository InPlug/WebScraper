using OpenQA.Selenium;
using OpenQA.Selenium.Internal;

namespace GetDynamicWebsiteContent
{
    /// <summary>
    /// Provides a WebElement that doesn't throw StaleElementReferenceExceptions.
    /// </summary>
    public interface IStableWebElement : IWebElement, ILocatable, ITakesScreenshot, IWrapsElement, IWrapsDriver
    {
        /// <summary>
        /// Returns true, if the element is not connected to the DOM.
        /// </summary>
        /// <returns>True, if the element is not connected to the DOM.</returns>
        bool IsStale();

        /// <summary>
        /// Tries to re-find the element so that it's connected to the DOM (hopefully for a few milliseconds).
        /// </summary>
        void RegenerateElement();

        /// <summary>
        /// Returns the Locator of this element.
        /// </summary>
        /// <returns>The Locator of this element.</returns>
        string GetDescription();
    }
}
