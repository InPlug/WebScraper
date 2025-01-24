using System.ComponentModel;
using Vishnu.Interchange;
using Vishnu_UserModules;

namespace WebScraperDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TreeEvent treeEvent = TreeEvent.UndefinedTreeEvent;
            WebScraperDemoChecker demoChecker = new WebScraperDemoChecker();
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
                logicalResultString, demoChecker.ReturnObject?.ToString());
            demoChecker.Dispose();
            Console.ReadLine();
        }

        // Wird vom UserChecker bei Veränderung des Verarbeitungsfortschritts aufgerufen.
        // Wann und wie oft der Aufruf erfolgen soll, wird im UserChecker festgelegt.
        static void SubNodeProgressChanged(object? sender, ProgressChangedEventArgs args)
        {
            Console.WriteLine("{0}", args.ProgressPercentage);
        }
    }
}