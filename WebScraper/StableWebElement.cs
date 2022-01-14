using System;
using System.Collections.ObjectModel;
using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions.Internal;

namespace GetDynamicWebsiteContent
{

    /// <summary>
    /// Provides a WebElement that doesn't throw StaleElementReferenceExceptions.
    /// Original post "No more StaleElementReferenceException" by Cezary Piatek:
    /// https://cezarypiatek.github.io/post/no-more-staleelementreferenceexception/.
    /// Thanks a lot.
    /// </summary>
    /// <remarks>
    /// Author: Erik Nagel
    ///
    /// 01.12.2020 Erik Nagel: created from the original post "No more StaleElementReferenceException" by Cezary Piatek:
    /// https://cezarypiatek.github.io/post/no-more-staleelementreferenceexception/.
    /// </remarks>

    public class StableWebElement : IStableWebElement
    {
        #region public, protected, internal members

        /// <summary>
        /// Location of the element using various frames of reference.
        /// </summary>
        public ICoordinates Coordinates
        {
            get
            {
                return Execute(() => (element as ILocatable).Coordinates);
            }
        }

        /// <summary>
        /// True if the element is visible.
        /// </summary>
        public bool Displayed
        {
            get
            {
                return Execute(() => element.Displayed);
            }
        }

        /// <summary>
        /// True if the element is enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return Execute(() => element.Enabled);
            }
        }

        /// <summary>
        /// Element location.
        /// </summary>
        public Point Location
        {
            get
            {
                return Execute(() => element.Location);
            }
        }

        /// <summary>
        /// Element location after scrollimng the element into the screen.
        /// </summary>
        public Point LocationOnScreenOnceScrolledIntoView
        {
            get
            {
                return Execute(() => (element as ILocatable).LocationOnScreenOnceScrolledIntoView);
            }
        }

        /// <summary>
        /// True if the element is selected.
        /// </summary>
        public bool Selected
        {
            get
            {
                return Execute(() => element.Selected);
            }
        }

        /// <summary>
        /// Height and width of the element.
        /// </summary>
        public Size Size
        {
            get
            {
                return Execute(() => element.Size);
            }
        }

        /// <summary>
        /// HTML-Tag name of the element.
        /// </summary>
        public string TagName
        {
            get
            {
                return Execute(() => element.TagName);
            }
        }

        /// <summary>
        /// Contained text of the element.
        /// </summary>
        public string Text
        {
            get
            {
                return Execute(() => element.Text);
            }
        }

        /// <summary>
        /// Returns the Locator of this element.
        /// </summary>
        /// <returns>The Locator of this element.</returns>
        public string GetDescription()
        {
            var thisDescription = $"'{this.locator}'";
            return thisDescription;
        }

        /// <summary>
        /// Returns the element connected to the DOM.
        /// </summary>
        /// <returns>The element connected to the DOM.</returns>
        public IWebElement Unwrap()
        {
            if (IsStale())
            {
                RegenerateElement();
            }
            return element;
        }

        /// <summary>
        /// Gets the IWebElement wrapped by this object.
        /// </summary>
        public IWebElement WrappedElement => Unwrap();

        /// <summary>
        /// The IWebDriver wrapped by this object.
        /// </summary>
        public IWebDriver WrappedDriver
        {
            get
            {
                if (element is IWrapsDriver driverWrapper)
                {
                    return driverWrapper.WrappedDriver;
                }
                throw new NotSupportedException($"Element {this.GetDescription()} does not have information about driver");
            }
        }

        /// <summary>
        /// Constructor - takes an IWebElement-instance and it's locator.
        /// </summary>
        /// <param name="element">IWebElement-instance.</param>
        /// <param name="locator">Locator of the IWebElement-instance.</param>
        public StableWebElement(IWebElement element, By locator)
        {
            this.element = element;
            this.locator = locator;
        }

        /// <summary>
        /// Returns true, if the element is not connected to the DOM.
        /// </summary>
        /// <returns>True, if the element is not connected to the DOM.</returns>
        public bool IsStale()
        {
            try
            {
                var tagName = this.element.TagName;
                return false;
            }
            catch (StaleElementReferenceException)
            {
                return true;
            }
        }

        /// <summary>
        /// Tries to re-find the element so that it's connected to the DOM (hopefully for a few milliseconds).
        /// </summary>
        public void RegenerateElement()
        {
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    this.element = this.WrappedDriver.FindElement(locator);
                    break;
                }
                catch (StaleElementReferenceException ex)
                {
                    if (i > 2)
                    {
                        throw new NoSuchElementException("StableWebElement.RegenerateElement failed!", ex);
                    }
                }
                //catch (NoSuchElementException ex2)
                //{
                //    if (i > 2)
                //    {
                //        throw new NoSuchElementException("StableWebElement.RegenerateElement failed!", ex2);
                //    }
                //}
            }
        }

        /// <summary>
        /// Clears the element.
        /// </summary>
        public void Clear()
        {
            Execute(() => element.Clear());
        }

        /// <summary>
        /// Clicks the element.
        /// </summary>
        public void Click()
        {
            Execute(() => element.Click());
        }

        /// <summary>
        /// Finds the first element using the given method.
        /// </summary>
        /// <param name="locator">A searching method to locate the element in the DOM.</param>
        /// <returns>The found element.</returns>
        public IWebElement FindElement(By locator)
        {
            var foundElement = Execute(() => element.FindElement(locator));
            return new StableWebElement(foundElement, locator);
        }

        /// <summary>
        /// Finds all elements using the given method.
        /// </summary>
        /// <param name="locator">A searching method to locate the element in the DOM.</param>
        /// <returns>The found elements.</returns>
        public ReadOnlyCollection<IWebElement> FindElements(By locator)
        {
            return element.FindElements(locator);
        }

        /// <summary>
        /// Returns the element's attribute with the given name.
        /// </summary>
        /// <param name="attributeName">The attribute's name.</param>
        /// <returns>Element's attribute with the given name.</returns>
        public string GetAttribute(string attributeName)
        {
            return Execute(() => element.GetAttribute(attributeName));
        }

        /// <summary>
        /// Gets the value of the CSS property of this element.
        /// </summary>
        /// <param name="propertyName">The name of the CSS property.</param>
        /// <returns>The value of the CSS property.</returns>
        public string GetCssValue(string propertyName)
        {
            return Execute(() => element.GetCssValue(propertyName));
        }

        /// <summary>
        /// Gets the value of the element's property with the given name.
        /// </summary>
        /// <param name="propertyName">The property's name.</param>
        /// <returns>Element's property with the given name.</returns>
        public string GetProperty(string propertyName)
        {
            return Execute(() => element.GetProperty(propertyName));
        }

        /// <summary>
        /// Gets a screenshot object representing the image of the page on the screen.
        /// </summary>
        /// <returns>A screenshot object representing the image of the page on the screen.</returns>
        public Screenshot GetScreenshot()
        {
            return Execute(() => (ITakesScreenshot)element).GetScreenshot();
        }

        /// <summary>
        /// Simulates typing text into the element.
        /// </summary>
        /// <param name="text">The text to be written into the element.</param>
        public void SendKeys(string text)
        {
            Execute(() => element.SendKeys(text));
        }

        /// <summary>
        /// Submits this element to the webserver.
        /// </summary>
        public void Submit()
        {
            Execute(() => element.Submit());
        }

        #endregion public, protected, internal members

        #region private members

        private IWebElement element;
        private readonly By locator;

        private T Execute<T>(Func<T> function)
        {
            T result = default(T);
            Execute(() => { result = function(); });
            return result;
        }

        private void Execute(Action action)
        {
            bool success = false;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    action();
                    success = true;
                    break;
                }
                catch (StaleElementReferenceException)
                {
                    RegenerateElement();
                }
            }
            if (success == false)
            {
                throw new NoSuchElementException("StableWebElement.Execute failed. Element is no longer accessible!");
            }
        }

        string IWebElement.GetDomAttribute(string attributeName)
        {
            throw new NotImplementedException();
        }

        string IWebElement.GetDomProperty(string propertyName)
        {
            throw new NotImplementedException();
        }

        ISearchContext IWebElement.GetShadowRoot()
        {
            throw new NotImplementedException();
        }

        #endregion private members
    }
}
