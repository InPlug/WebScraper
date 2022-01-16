using OpenQA.Selenium;
using OpenQA.Selenium.Internal;

// ACHTUNG: Selenium.WebDriver und Selenium.Support müssen auf Version 3.141.0 bleiben,
//    NICHT auf die 4er Version updaten, sonst Fehler im Vishnu-Betrieb:
//          Could not load type 'OpenQA.Selenium.Internal.IWrapsElement' from assembly 'WebDriver,
//          Version=4.0.0.0, Culture=neutral, PublicKeyToken=null'
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
