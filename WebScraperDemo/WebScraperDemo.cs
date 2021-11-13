using GetDynamicWebsiteContent;
using NetEti.ApplicationControl;
using NetEti.Globals;
using NetEti.WebTools;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
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
    /// TODO: ersetzen Sie diesen Kommentar durch Ihren eigenen Kommentar.
    /// </summary>
    /// <remarks>
    /// Autor: Erik Nagel
    ///
    /// 30.01.2020 Erik Nagel: erstellt
    /// </remarks>
    public class CheckCovid19 : INodeChecker, IDisposable
    {
        #region INodeChecker Implementation

        const string JohnsHopkinsUrl = "https://coronavirus.jhu.edu/map.html";
        const string RKIUrl = "https://experience.arcgis.com/experience/478220a4c454480e823b17327b2bf1d4/page/page_1";

        /// <summary>
        /// Kann aufgerufen werden, wenn sich der Verarbeitungs-Fortschritt
        /// des Checkers geändert hat, muss aber zumindest aber einmal zum
        /// Schluss der Verarbeitung aufgerufen werden.
        /// </summary>
        public event CommonProgressChangedEventHandler NodeProgressChanged;

        /// <summary>
        /// Rückgabe-Objekt des Checkers
        /// </summary>
        public object ReturnObject
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
        public bool? Run(object checkerParameters, TreeParameters treeParameters, TreeEvent source)
        {
            this._driverPath = treeParameters.CheckerDllDirectory;
            // Directory.SetCurrentDirectory(this._driverPath);
            this.OnNodeProgressChanged(this.GetType().Name, 100, 0, ItemsTypes.items);
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
            this.OnNodeProgressChanged(this.GetType().Name, 100, 100, ItemsTypes.items); // erforderlich!
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

        private IInfoController _publisher;
        private string _driverPath;
        private Logger _covidLogger;
        private string _paraString;
        private object _returnObject = null;
        private CheckCovid19_ReturnObject _checkCovid19_ReturnObject;
        private string _covid19InfoFile;
        private Logger _logger;

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
                string line;
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
            this._checkCovid19_ReturnObject.RecordCount = records.Count;
            this._checkCovid19_ReturnObject.SubResults.SubResults.Clear();
            if (!this.recordsToResult(records))
            {
                rtn = false;
            }
            this._checkCovid19_ReturnObject.LogicalResult = rtn;
            this._returnObject = this._checkCovid19_ReturnObject;
            return rtn;
        }

        private void LogWebsites()
        {
            this.LogJohnsHopkins();
            this.OnNodeProgressChanged(this.GetType().Name, 100, 40, ItemsTypes.items);
            this._covidLogger.Flush();
            this.LogRKI();
            this.OnNodeProgressChanged(this.GetType().Name, 100, 85, ItemsTypes.items);
            this._covidLogger.Flush();
        }

        private void LogRKI()
        {
            using (ChromeScraper chromeScraper = new ChromeScraper(RKIUrl, this._driverPath))
            {
                this.Publish("LogRKI() Start");

                // würde hier noch nichts finden: int countIFrames = chromeScraper.WebDriver.FindElements(By.TagName("iframe")).Count;
                By by1 = By.XPath("//iframe[contains(@src,'')]"); // allgemeine Suche
                StableWebElement stableInnerFrame = chromeScraper.WaitForStableWebElement(by1, LocatorCondition.IsVisible); // dient auch zum Warten, bis überhaupt iFrames geladen sind
                // hier wäre das ok (insgesamt ein iFrame): int countIFrames = chromeScraper.WebDriver.FindElements(By.TagName("iframe")).Count;

                // hier keine weitere Suche nötig: StableWebElement stableInnerFrame = chromeScraper.WaitForStableWebElement(by1, LocatorCondition.IsVisible);
                // "Coronavirus COVID-19 Global Cases by Johns Hopkins CSSE"

                int germanyCases;
                int germanyDeaths;

                this.Publish("vor driver.SwitchTo().Frame(stableInnerFrame)");
                chromeScraper.WebDriver.SwitchTo().Frame(stableInnerFrame);
                this.Publish("nach driver.SwitchTo().Frame(stableInnerFrame)");

                // By by2 = By.XPath("//div[@id='ember72']"); // findet zwar das Element, aber der Text könnte noch leer sein.
                By by2 = By.XPath("//div[@id='ember72' and .//*[contains(text(), 'COVID-19-F')]]"); // liefert "COVID-19-Fälle\r\n1.028.089\r\naus total 1.028.089"
                StableWebElement stableGermanyCasesElement = chromeScraper.WaitForStableWebElement(by2, LocatorCondition.IsVisible);
                string tmpString1 = stableGermanyCasesElement.Text.Replace(Environment.NewLine, "|");
                this.Publish(tmpString1);

                string numString1 = new Regex(@".*?\|(.*)\|.*", RegexOptions.IgnoreCase).Matches(tmpString1)[0].Groups[1].Value;

                if (int.TryParse(numString1.Replace(".", ""), out germanyCases))
                {
                    this.LogCovidData(String.Format($"RKI_Deutschland Erkrankungen: {germanyCases:N0}"));
                }
                else
                {
                    this.LogCovidData(String.Format($"RKI_Deutschland Erkrankungen: - konnte nicht ermittelt werden -"));
                }

                // By by3 = By.XPath("//div[@id='ember86']"); // findet zwar das Element, aber der Text könnte noch leer sein.
                By by3 = By.XPath("//div[@id='ember86' and .//*[contains(text(), 'COVID-19-T')]]"); // liefert "COVID-19-Todesfälle\r\n15.965\r\naus total 15.965"
                StableWebElement stableGermanyDeathsElement = chromeScraper.WaitForStableWebElement(by3, LocatorCondition.IsVisible);
                string tmpString2 = stableGermanyDeathsElement.Text.Replace(Environment.NewLine, "|");
                this.Publish(tmpString2);

                string numString2 = new Regex(@".*?\|(.*)\|.*", RegexOptions.IgnoreCase).Matches(tmpString2)[0].Groups[1].Value;

                if (int.TryParse(numString2.Replace(".", ""), out germanyDeaths))
                {
                    this.LogCovidData(String.Format($"RKI_Deutschland Tote: {germanyDeaths:N0}"));
                }
                else
                {
                    this.LogCovidData(String.Format($"RKI_Deutschland Tote: - konnte nicht ermittelt werden -"));
                }

                this.Publish("LogRKI() Ende");
            }
        }

        private void LogJohnsHopkins()
        {
            using (ChromeScraper chromeScraper = new ChromeScraper(JohnsHopkinsUrl, this._driverPath))
            {
                this.Publish("LogJohnsHopkins() Start");

                By by1 = By.XPath("//iframe[contains(@title,'Global Cases')]"); // konkretes iFrame
                StableWebElement stableInnerFrame = chromeScraper.WaitForStableWebElement(by1, LocatorCondition.IsVisible);
                // "Coronavirus COVID-19 Global Cases by Johns Hopkins CSSE"

                int germanyCases = 0;
                int germanyDeaths = 0;

                this.Publish("vor driver.SwitchTo().Frame(stableInnerFrame)");
                chromeScraper.WebDriver.SwitchTo().Frame(stableInnerFrame);
                this.Publish("nach driver.SwitchTo().Frame(stableInnerFrame)");

                By by2 = By.XPath("//div/h5[.//span[contains(text(),'Germany')]]"); // liefert h5 mit enthaltenem Element mit Text 'Germany'
                StableWebElement stableGermanyElement = chromeScraper.WaitForStableWebElement(by2, LocatorCondition.IsVisible);

                this.Publish(stableGermanyElement.TagName + " vor Click()");
                stableGermanyElement.Click();
                this.Publish(stableGermanyElement.TagName + " nach Click()");

                string tmpString = stableGermanyElement.Text;
                this.Publish(tmpString);
                if (int.TryParse(tmpString.Replace(" Germany", "").Replace(".", ""), out germanyCases))
                {
                    this.LogCovidData(String.Format($"JHU_Deutschland Erkrankungen: {germanyCases:N0}"));
                }
                else
                {
                    this.LogCovidData(String.Format($"JHU_Deutschland Erkrankungen: - konnte nicht ermittelt werden -"));
                }

                // By by3 = By.XPath("//div[@id='ember100']"); // findet zwar das Element, aber der Text könnte noch leer sein.
                /* vor Website-Änderung von JohnsHopkins am 01.12.2020:
                By by3 = By.XPath("//div[@id='ember100' and .//*[contains(text(), 'Global Deaths')]]"); // liefert 'Global Deaths\r\n15.640'.
                StableWebElement stableGermanyDeathsElement = chromeScraper.WaitForStableWebElement(by3, LocatorCondition.IsVisible);
                string tmpString2 = stableGermanyDeathsElement.Text;
                this.Publish(tmpString2);
                if (int.TryParse(tmpString2.Replace("Global Deaths" + Environment.NewLine, "").Replace(".", ""), out germanyDeaths))
                {
                    this.LogCovidData(String.Format($"JHU_Deutschland Tote: {germanyDeaths:N0}"));
                }
                else
                {
                    this.LogCovidData(String.Format($"JHU_Deutschland Tote: - konnte nicht ermittelt werden -"));
                }
                */

                // nach Website-Änderung von JohnsHopkins am 01.12.2020:
                By by3 = By.XPath("//div[@id='ember106' and .//*[contains(text(), 'Germany')]]"); // liefert '16.694 deaths\r\nGermany'.
                StableWebElement stableGermanyDeathsElement = chromeScraper.WaitForStableWebElement(by3, LocatorCondition.IsVisible);
                string tmpString2 = stableGermanyDeathsElement.Text;
                this.Publish(tmpString2);
                if (int.TryParse(tmpString2.Replace(" deaths " + Environment.NewLine + "Germany", "").Replace(".", ""), out germanyDeaths))
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

        private void evaluateParameters(string paraString)
        {
            string[] para = paraString.Split('|');
            string covid19InfoFile = String.IsNullOrEmpty(para[0].Trim()) ? @"Covid19_Archive.txt" : para[0].Trim();
            if (!String.IsNullOrEmpty(covid19InfoFile) && covid19InfoFile != this._covid19InfoFile)
            {
                this._covid19InfoFile = covid19InfoFile;
            }
            string comment = para.Length > 1 ? para[1].Trim() : "";

            this._checkCovid19_ReturnObject = new CheckCovid19_ReturnObject()
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
            this._covidLogger = new Logger(this._covid19InfoFile, loggingRegexFilter, false);
            InfoType[] loggerInfos = new InfoType[] { InfoType.Milestone };
            this._publisher.RegisterInfoReceiver(this._covidLogger, loggerInfos);
        }

        private void OnNodeProgressChanged(string itemsName, int countAll, int countSucceeded, ItemsTypes itemsType)
        {
            NodeProgressChanged?.Invoke(null, new CommonProgressChangedEventArgs(itemsName, countAll, countSucceeded, itemsType, null));
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
                    this._checkCovid19_ReturnObject.SubResults.SubResults.Add(new CheckCovid19_ReturnObject.SubResult()
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

        private void Publish(string message)
        {
            InfoController.Say("CheckCovid19 " + message);
        }

        private void LogCovidData(string message)
        {
            this._publisher.Publish(this, "CheckCovid19 " + message, InfoType.Milestone);
        }

        private string syntax(string errorMessage)
        {
            return (
                errorMessage
                + Environment.NewLine
                + "Parameter: Pfad zur Datei mit den Covid19-Infos|Beschreibung"
                + Environment.NewLine
                + @"Beispiel: Covid19_Archive.txt|Holt Johns Hopkins- und RKI-Zahlen für Deutschland."
             );
        }

    }
}
