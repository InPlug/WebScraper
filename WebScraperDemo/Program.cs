using NetEti.Globals;
using System;
using System.IO;
using Vishnu.Interchange;

// ACHTUNG: Selenium.WebDriver und Selenium.Support müssen auf Version 3.141.0 bleiben,
//    NICHT auf die 4er Version updaten, sonst Fehler im Vishnu-Betrieb:
//          Could not load type 'OpenQA.Selenium.Internal.IWrapsElement' from assembly 'WebDriver,
//          Version=4.0.0.0, Culture=neutral, PublicKeyToken=null'
namespace Vishnu_UserModules
{
    class Program
    {
        static void Main(string[] args)
        {
            TreeEvent treeEvent = null;
            CheckCovid19 demoChecker = new CheckCovid19();
            demoChecker.NodeProgressChanged += SubNodeProgressChanged;
            bool? logicalResult = demoChecker.Run(@"Covid19_Archive.txt|holt Covid-19 Zahlen von Johns Hopkins",
                new TreeParameters("MainTree", null) { CheckerDllDirectory = Directory.GetCurrentDirectory() }, treeEvent);
            string logicalResultString;
            switch (logicalResult)
            {
                case true: logicalResultString = "true"; break;
                case false: logicalResultString = "false"; break;
                default: logicalResultString = "null"; break;
            }
            Console.WriteLine("logical result: {0}, Result: {1}",
                logicalResultString, demoChecker.ReturnObject.ToString());
            demoChecker.Dispose();
            Console.ReadLine();
        }

        // Wird vom UserChecker bei Veränderung des Verarbeitungsfortschritts aufgerufen.
        // Wann und wie oft der Aufruf erfolgen soll, wird im UserChecker festgelegt.
        static void SubNodeProgressChanged(object sender, CommonProgressChangedEventArgs args)
        {
            Console.WriteLine("{0} of {1}", args.CountSucceeded, args.CountAll);
        }
    }
}
