using NetEti.ApplicationControl;
using NetEti.WebTools;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Vishnu.Interchange;

namespace Vishnu_UserModules
{
    /// <summary>
    /// DemoChecker-Projekt für Vishnu.
    /// Die WebScraperDemoChecker.dll kann später in einer Vishnu-JobDescription.xml referenziert
    /// werden und von Vishnu bei der Ausführung des Jobs aus dem Plugin-Unterverzeichnis oder
    /// einem konfigurierten UserAssemblyDirectory dynamisch geladen werden.
    /// Vishnu ruft dann je nach weiteren Konfigurationen (Trigger) die öffentliche
    /// Methode "Run" des UserCheckers auf.
    /// </summary>
    /// <remarks>
    /// Autor: Erik Nagel
    ///
    /// 30.01.2020 Erik Nagel: erstellt
    /// 24.01.2025 Erik Nagel: überarbeitet.
    /// </remarks>
    public class WebScraperDemoChecker : INodeChecker, IDisposable
    {
        /// <summary>
        /// Maximal waiting time for a searched web-element in seconds.
        /// </summary>
        public const int DEFAULT_SEARCH_TIMEOUT_SECONDS = 60;

        #region INodeChecker Implementation

        const string WebScraperDemoUrl = "https://coronavirus.jhu.edu/map.html";

        /// <summary>
        /// Kann aufgerufen werden, wenn sich der Verarbeitungs-Fortschritt
        /// des Checkers geändert hat, muss aber zumindest aber einmal zum
        /// Schluss der Verarbeitung aufgerufen werden.
        /// </summary>
        public event ProgressChangedEventHandler? NodeProgressChanged;

        /// <summary>
        /// Rückgabe-Objekt des Checkers
        /// </summary>
        public object? ReturnObject
        {
            get
            {
                return this._returnObject;
            }
            set
            {
                this._returnObject = value;
            }
        }

        /// <summary>
        /// Hier wird der Arbeitsprozess ausgeführt (oder beobachtet).
        /// </summary>
        /// <param name="checkerParameters">Ihre Aufrufparameter aus der JobDescription.xml oder null.</param>
        /// <param name="treeParameters">Für den gesamten Tree gültige Parameter oder null (z.Zt. unbenutzt).</param>
        /// <param name="source">Auslösendes TreeEvent (kann null sein).</param>
        /// <returns>True, False oder null</returns>
        public bool? Run(object? checkerParameters, TreeParameters treeParameters, TreeEvent source)
        {
            this._driverPath = treeParameters?.CheckerDllDirectory;
            // Directory.SetCurrentDirectory(this._driverPath);
            this.OnNodeProgressChanged(0);
            if (this._paraString != (checkerParameters ?? "").ToString())
            {
                this._paraString = (checkerParameters ?? "").ToString();
                this.evaluateParameters(this._paraString);
            }
            this._returnObject = null; // optional: in UserChecker_ReturnObject IDisposable implementieren und hier aufrufen.
            this.SetupCovidLogging();
            //--- Aufruf der Checker-Business-Logik ----------
            bool? returnCode = this.Work(source);
            //------------------------------------------------
            this.OnNodeProgressChanged(100); // erforderlich!
            return returnCode; // 
        }

        #endregion INodeChecker Implementation

        /// <summary>
        /// Constructor - fetches the InfoController for Loggings.
        /// </summary>
        public WebScraperDemoChecker()
        {
            this._publisher = InfoController.GetInfoController();
            // Globales Logging installieren
            this._logger = new Logger();
            InfoType[] loggerInfos = InfoTypes.Collection2InfoTypeArray(InfoTypes.All);
            this._publisher.RegisterInfoReceiver(this._logger, loggerInfos);
        }

        #region IDisposable Implementation

        private bool _disposed = false;

        /// <summary>
        /// Öffentliche Methode zum Aufräumen.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Abschlussarbeiten.
        /// </summary>
        /// <param name="disposing">False, wenn vom eigenen Destruktor aufgerufen.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    // Aufräumarbeiten durchführen und dann beenden.
                    this._webScraperDemo_Logger?.Dispose();
                    this._logger?.Dispose();
                }
                this._disposed = true;
            }
        }

        /// <summary>
        /// Finalizer: wird vom GarbageCollector aufgerufen.
        /// </summary>
        ~WebScraperDemoChecker()
        {
            this.Dispose(false);
        }

        #endregion IDisposable Implementation

        private IInfoController? _publisher;
        private string? _driverPath;
        private Logger? _webScraperDemo_Logger;
        private string? _paraString;
        private object? _returnObject = null;
        private WebScraperDemoChecker_ReturnObject? _webScraperDemoChecker_ReturnObject;
        private string? _webScraperDemo_InfoFile;
        private Logger? _logger;

        private bool? Work(TreeEvent source)
        {
            // Hier folgt die eigentliche Checker-Verarbeitung, die einen erweiterten boolean als Rückgabe
            // dieses Checkers ermittelt und ggf. auch ein Return-Objekt mit zusätzlichen Informationen füllt.
            // Die Rückgabe kann völlig unabhängig von Results oder Environment sein; ist hier nur für
            // die Demo abhängig kodiert.
            // TODO: hier können Sie Ihre eigene Verarbeitung implementieren.
            this.OnNodeProgressChanged(30);
            string url = "https://www.tagesschau.de/wirtschaft/boersenkurse/basf-aktie-basf11/";
            this.ReturnObject = new ShareChecker_ReturnObject()
            {
                FullName = "BASF SE NAMENS-AKTIEN O.N. ISIN DE000BASF111 | WKN BASF11",
                ShortName = "BASF",
                Url = url,
                StartCount = 100.00m,
                StartValue = 91.00m,
                CurrentCount = 100.00m,
                Compensation = 1000.00m,
                Timestamp = DateTime.Now
            };
            try
            {
                return this.EvaluateShareSite(url);
            }
            catch (Exception ex)
            {
                InfoController.Say("BASF Exception: " + ex.Message);
                throw;
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                // InfoController.Say("BASF_Checker: Ende");
            }
        }

        private bool? EvaluateShareSite(string url)
        {
            bool? rtn = null;
            using (ChromeScraper chromeScraper = new ChromeScraper(url, 10))
            {
                // InfoController.Say("BASF_Checker: Start");
                this.OnNodeProgressChanged(70);

                decimal currentValue;

                By by2 = By.XPath("//div[@class='VWDcomp WE021']//span[@class='price']");

                IWebElement? stableGermanyCasesElement = null;
                try
                {
                    stableGermanyCasesElement = chromeScraper.GetElement(ExpectedConditions.ElementIsVisible(by2), new TimeSpan(0, 0, DEFAULT_SEARCH_TIMEOUT_SECONDS));
                }
                catch (Exception ex) when (ex is NoSuchElementException || ex is WebDriverTimeoutException)
                {
                    this.Publish($"Exception occurred in SeleniumHelper.SendKeys(): element located by {by2.ToString()} could not be located within {DEFAULT_SEARCH_TIMEOUT_SECONDS} seconds.");
                }

                string? tmpString1
                    = stableGermanyCasesElement?.Text.Replace(Environment.NewLine, "|").Replace("€", "").Trim() + "|";
                // InfoController.Say(tmpString1);

                this.OnNodeProgressChanged(80);

                string numString1 = new Regex(@"(.*)\|.*", RegexOptions.IgnoreCase)
                    .Matches(tmpString1)[0].Groups[1].Value;

                if (decimal.TryParse(numString1.Replace(".", ""), out currentValue))
                {
                    ShareChecker_ReturnObject? shareChecker_ReturnObject = (ShareChecker_ReturnObject?)this.ReturnObject;
                    if (shareChecker_ReturnObject != null)
                    {
                        shareChecker_ReturnObject.CurrentValue = currentValue;
                        shareChecker_ReturnObject.GainsLosses
                            = currentValue * shareChecker_ReturnObject.CurrentCount
                                - shareChecker_ReturnObject.StartValue
                                * shareChecker_ReturnObject.StartCount
                                + shareChecker_ReturnObject.Compensation;
                        rtn = shareChecker_ReturnObject.GainsLosses >= 0;
                    }
                }
                else
                {
                    throw new ApplicationException("BASF_Checker: der aktuelle Kurs konnte nicht ermittelt werden.");
                }
                this.OnNodeProgressChanged(90);
            }
            return rtn;
        }

        private void evaluateParameters(string? paraString)
        {
            if (String.IsNullOrEmpty(paraString))
            {
                return; 
            }
            string[] para = paraString.Split('|');
            string covid19InfoFile = String.IsNullOrEmpty(para[0].Trim()) ? @"DemoChecker_Archive.txt" : para[0].Trim();
            if (!String.IsNullOrEmpty(covid19InfoFile) && covid19InfoFile != this._webScraperDemo_InfoFile)
            {
                this._webScraperDemo_InfoFile = covid19InfoFile;
            }
            string comment = para.Length > 1 ? para[1].Trim() : "";

            this._webScraperDemoChecker_ReturnObject = new WebScraperDemoChecker_ReturnObject()
            {
                Covid19InfoFile = this._webScraperDemo_InfoFile,
                Comment = comment
            };
        }

        private void SetupCovidLogging()
        {
            this._webScraperDemo_Logger?.Dispose();
            string loggingRegexFilter = ""; // Alles wird geloggt (ist der Default).
                                            //string loggingRegexFilter = @"(?:_NOPPES_)"; // Nichts wird geloggt, bzw. nur Zeilen, die "_NOPPES_" enthalten.
            if (!String.IsNullOrEmpty(this._webScraperDemo_InfoFile))
            {
                this._webScraperDemo_Logger = new Logger(this._webScraperDemo_InfoFile, loggingRegexFilter, false);
                InfoType[] loggerInfos = new InfoType[] { InfoType.Milestone };
                this._publisher?.RegisterInfoReceiver(this._webScraperDemo_Logger, loggerInfos);
            }
        }

        private void OnNodeProgressChanged(int progressPercentage)
        {
            NodeProgressChanged?.Invoke(null, new ProgressChangedEventArgs(progressPercentage, null));
        }

        private void Publish(string? message)
        {
            Console.WriteLine(message);
            // InfoController.Say("DemoChecker " + message);
        }

        /*
        // Deaktiviert, da die Johns Hopkins-Seite in dieser Form nicht mehr existiert.
        private void LogWebsites()
        {
            this.LogWebsite();
            this.OnNodeProgressChanged(85);
            this._webScraperDemo_Logger?.Flush();
        }

        private void LogWebsite()
        {
            using (ChromeScraper chromeScraper = new ChromeScraper(WebScraperDemoUrl, 10, new ChromeOptions()))
            {
                this.Publish("LogJohnsHopkins() Start");

                By by1 = By.XPath("//iframe[contains(@title,'Global Cases')]"); // konkretes iFrame

                IWebElement? stableInnerFrame = null;
                try
                {
                    stableInnerFrame = chromeScraper.GetElement(ExpectedConditions.ElementIsVisible(by1), new TimeSpan(0, 0, WebScraperDemoChecker.DEFAULT_SEARCH_TIMEOUT_SECONDS));
                }
                catch (Exception ex) when (ex is NoSuchElementException || ex is WebDriverTimeoutException)
                {
                    this.Publish($"Exception occurred in SeleniumHelper.SendKeys(): element located by {by1.ToString()} could not be located within {WebScraperDemoChecker.DEFAULT_SEARCH_TIMEOUT_SECONDS} seconds.");
                }

                // "Coronavirus COVID-19 Global Cases by Johns Hopkins CSSE"

                int germanyCases = 0;
                int germanyDeaths = 0;

                this.Publish("vor driver.SwitchTo().Frame(stableInnerFrame)");
                chromeScraper.WebDriver?.SwitchTo().Frame(stableInnerFrame);
                this.Publish("nach driver.SwitchTo().Frame(stableInnerFrame)");

                By by2 = By.XPath("//div[@class='external-html' and .//*[contains(text(), 'Germany')]]"); // liefert "Germany\r\n28-Day: 1.003.634 | 8.395\r\nTotals: 7.504.637 | 113.939"

                IWebElement? stableGermanyElement = null;
                try
                {
                    stableGermanyElement = chromeScraper.GetElement(ExpectedConditions.ElementIsVisible(by2), new TimeSpan(0, 0, WebScraperDemoChecker.DEFAULT_SEARCH_TIMEOUT_SECONDS));
                }
                catch (Exception ex) when (ex is NoSuchElementException || ex is WebDriverTimeoutException)
                {
                    this.Publish($"Exception occurred in SeleniumHelper.SendKeys(): element located by {by2.ToString()} could not be located within {WebScraperDemoChecker.DEFAULT_SEARCH_TIMEOUT_SECONDS} seconds.");
                }

                this.Publish(stableGermanyElement?.TagName + " vor Click()");
                stableGermanyElement?.Click();
                this.Publish(stableGermanyElement?.TagName + " nach Click()");

                string? tmpString = stableGermanyElement?.Text;
                this.Publish(tmpString);

                By bySub1 = By.XPath("p[3]"); // liefert "Totals: 7.531.905 | 113.981"
                IWebElement? stableGermanyTotalsElement = (stableGermanyElement?.FindElement(bySub1));

                string? tmpString2 = stableGermanyTotalsElement?.Text; // "Totals: 7.531.905 | 113.981"
                if (tmpString2 == null)
                {
                    throw new ApplicationException($"Web-Element {bySub1} wurde nicht gefunden.");
                }
                this.Publish(tmpString2);

                MatchCollection matchCollection2 = new Regex(@".*?([\d\.]+).*?").Matches(tmpString2);
                string totalsInfectedString = "";
                string totalsDeadString = "";
                if (matchCollection2?.Count > 0 && matchCollection2[0].Groups?.Count > 1)
                {
                    totalsInfectedString = matchCollection2[0].Groups[1].Value;
                }
                if (matchCollection2?.Count > 1 && matchCollection2[1].Groups?.Count > 1)
                {
                    totalsDeadString = matchCollection2[1].Groups[1].Value;
                }
                this.Publish(String.Format($"infected: {totalsInfectedString}, dead: {totalsDeadString}"));

                if (int.TryParse(totalsInfectedString.Replace(".", ""), out germanyCases))
                {
                    this.LogCovidData(String.Format($"JHU_Deutschland Erkrankungen: {germanyCases:N0}"));
                }
                else
                {
                    this.LogCovidData(String.Format($"JHU_Deutschland Erkrankungen: - konnte nicht ermittelt werden -"));
                }
                if (int.TryParse(totalsDeadString.Replace(".", ""), out germanyDeaths))
                {
                    this.LogCovidData(String.Format($"JHU_Deutschland Tote: {germanyDeaths:N0}"));
                }
                else
                {
                    this.LogCovidData(String.Format($"JHU_Deutschland Tote: - konnte nicht ermittelt werden -"));
                }

                this.Publish("LogJohnsHopkins() Ende");
            }
        }

        private bool recordsToResult(List<string> records)
        {
            bool rtn = true;
            int recordsCount = records != null ? records.Count : 0;
            if (recordsCount == 0)
            {
                rtn = false;
            }
            if (records != null)
            {
                foreach (string record in records)
                {
                    bool recordRtn = true;
                    this._webScraperDemoChecker_ReturnObject?.SubResults?.SubResults?.Add(new WebScraperDemoChecker_ReturnObject.SubResult()
                    {
                        LogicalResult = recordRtn,
                        ResultRecord = record
                    });
                    if (!recordRtn)
                    {
                        rtn = false;
                    }
                }
            }
            return rtn;
        }

        private void LogCovidData(string message)
        {
            this._publisher?.Publish(this, "CheckCovid19 " + message, InfoType.Milestone);
        }

        private string syntax(string errorMessage)
        {
            return (
                errorMessage
                + Environment.NewLine
                + "Parameter: Pfad zur Datei mit den DemoChecker-Infos|Beschreibung"
                + Environment.NewLine
                + @"Beispiel: DemoChecker_Archive.txt|Holt Johns Hopkins-Zahlen für Deutschland."
             );
        }

        */

    }
}
