using NetEti.ApplicationControl;
using NetEti.WebTools;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SeleniumExtras.WaitHelpers;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Vishnu.Interchange;

namespace Vishnu_UserModules
{
    /// <summary>
    /// DemoChecker-Projekt für Vishnu.
    /// Die CheckCovid19.dll kann später in einer Vishnu-JobDescription.xml referenziert werden
    /// und von Vishnu bei der Ausführung des Jobs aus dem Plugin-Unterverzeichnis oder
    /// einem konfigurierten UserAssemblyDirectory dynamisch geladen werden.
    /// Vishnu ruft dann je nach weiteren Konfigurationen (Trigger) die öffentliche
    /// Methode "Run" des UserCheckers auf.
    /// </summary>
    /// <remarks>
    /// Autor: Erik Nagel
    ///
    /// 30.01.2020 Erik Nagel: erstellt
    /// </remarks>
    public class CheckCovid19 : INodeChecker, IDisposable
    {
        /// <summary>
        /// Maximal waiting time for a searched web-element in seconds.
        /// </summary>
        public const int DEFAULT_SEARCH_TIMEOUT_SECONDS = 60;

        #region INodeChecker Implementation

        const string JohnsHopkinsUrl = "https://coronavirus.jhu.edu/map.html";

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
        public CheckCovid19()
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
                    this._covidLogger?.Dispose();
                    this._logger?.Dispose();
                }
                this._disposed = true;
            }
        }

        /// <summary>
        /// Finalizer: wird vom GarbageCollector aufgerufen.
        /// </summary>
        ~CheckCovid19()
        {
            this.Dispose(false);
        }

        #endregion IDisposable Implementation

        private IInfoController? _publisher;
        private string? _driverPath;
        private Logger? _covidLogger;
        private string? _paraString;
        private object? _returnObject = null;
        private WebScraperDemo_ReturnObject? _checkCovid19_ReturnObject;
        private string? _covid19InfoFile;
        private Logger? _logger;

        private bool? Work(TreeEvent source)
        {
            // Hier folgt die eigentliche Checker-Verarbeitung, die einen erweiterten boolean als Rückgabe
            // dieses Checkers ermittelt und ggf. auch ein Return-Objekt mit zusätzlichen Informationen füllt.
            // Die Rückgabe kann völlig unabhängig von Results oder Environment sein; ist hier nur für
            // die Demo abhängig kodiert.
            // TODO: hier können Sie Ihre eigene Verarbeitung implementieren.
            this.ReturnObject = null;
            bool? rtn = true;
            List<string> records = new List<string>();
            try
            {
                this.LogWebsites();
                string? line;
                if (File.Exists(this._covid19InfoFile))
                {
                    using (System.IO.StreamReader file = new System.IO.StreamReader(this._covid19InfoFile))
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            records.Insert(0, line);
                        }
                        file.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                this.Publish("#CheckCovid19#: Exception: " + ex.Message);
                throw;
            }
            if (this._checkCovid19_ReturnObject != null)
            {
                this._checkCovid19_ReturnObject.RecordCount = records.Count;
                this._checkCovid19_ReturnObject.SubResults?.SubResults?.Clear();
                if (!this.recordsToResult(records))
                {
                    rtn = false;
                }
                this._checkCovid19_ReturnObject.LogicalResult = rtn;
                this._returnObject = this._checkCovid19_ReturnObject;
            }
            return rtn;
        }

        private void LogWebsites()
        {
            this.LogJohnsHopkins();
            this.OnNodeProgressChanged(85);
            this._covidLogger?.Flush();
        }

        private void LogJohnsHopkins()
        {
            using (ChromeScraper chromeScraper = new ChromeScraper(JohnsHopkinsUrl, this._driverPath, new ChromeOptions()))
            {
                this.Publish("LogJohnsHopkins() Start");

                By by1 = By.XPath("//iframe[contains(@title,'Global Cases')]"); // konkretes iFrame

                IWebElement? stableInnerFrame = null;
                try
                {
                    stableInnerFrame = chromeScraper.GetElement(ExpectedConditions.ElementIsVisible(by1), new TimeSpan(0, 0, CheckCovid19.DEFAULT_SEARCH_TIMEOUT_SECONDS));
                }
                catch (Exception ex) when (ex is NoSuchElementException || ex is WebDriverTimeoutException)
                {
                    this.Publish($"Exception occurred in SeleniumHelper.SendKeys(): element located by {by1.ToString()} could not be located within {CheckCovid19.DEFAULT_SEARCH_TIMEOUT_SECONDS} seconds.");
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
                    stableGermanyElement = chromeScraper.GetElement(ExpectedConditions.ElementIsVisible(by2), new TimeSpan(0, 0, CheckCovid19.DEFAULT_SEARCH_TIMEOUT_SECONDS));
                }
                catch (Exception ex) when (ex is NoSuchElementException || ex is WebDriverTimeoutException)
                {
                    this.Publish($"Exception occurred in SeleniumHelper.SendKeys(): element located by {by2.ToString()} could not be located within {CheckCovid19.DEFAULT_SEARCH_TIMEOUT_SECONDS} seconds.");
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

        private void evaluateParameters(string? paraString)
        {
            if (String.IsNullOrEmpty(paraString))
            {
                return; 
            }
            string[] para = paraString.Split('|');
            string covid19InfoFile = String.IsNullOrEmpty(para[0].Trim()) ? @"Covid19_Archive.txt" : para[0].Trim();
            if (!String.IsNullOrEmpty(covid19InfoFile) && covid19InfoFile != this._covid19InfoFile)
            {
                this._covid19InfoFile = covid19InfoFile;
            }
            string comment = para.Length > 1 ? para[1].Trim() : "";

            this._checkCovid19_ReturnObject = new WebScraperDemo_ReturnObject()
            {
                Covid19InfoFile = this._covid19InfoFile,
                Comment = comment
            };
        }

        private void SetupCovidLogging()
        {
            this._covidLogger?.Dispose();
            string loggingRegexFilter = ""; // Alles wird geloggt (ist der Default).
                                            //string loggingRegexFilter = @"(?:_NOPPES_)"; // Nichts wird geloggt, bzw. nur Zeilen, die "_NOPPES_" enthalten.
            if (File.Exists(this._covid19InfoFile))
            {
                this._covidLogger = new Logger(this._covid19InfoFile, loggingRegexFilter, false);
                InfoType[] loggerInfos = new InfoType[] { InfoType.Milestone };
                this._publisher?.RegisterInfoReceiver(this._covidLogger, loggerInfos);
            }
        }

        private void OnNodeProgressChanged(int progressPercentage)
        {
            NodeProgressChanged?.Invoke(null, new ProgressChangedEventArgs(progressPercentage, null));
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
                    this._checkCovid19_ReturnObject?.SubResults?.SubResults?.Add(new WebScraperDemo_ReturnObject.SubResult()
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

        private void Publish(string? message)
        {
            Console.WriteLine(message);
            // InfoController.Say("CheckCovid19 " + message);
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
                + "Parameter: Pfad zur Datei mit den Covid19-Infos|Beschreibung"
                + Environment.NewLine
                + @"Beispiel: Covid19_Archive.txt|Holt Johns Hopkins-Zahlen für Deutschland."
             );
        }

    }
}
