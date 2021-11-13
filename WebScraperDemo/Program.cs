using NetEti.Globals;
using System;
using System.IO;
using Vishnu.Interchange;

namespace Vishnu_UserModules
{
    class Program
    {
        static void Main(string[] args)
        {
            TreeEvent treeEvent = null;
            CheckCovid19 demoChecker = new CheckCovid19();
            demoChecker.NodeProgressChanged += SubNodeProgressChanged;
            bool? logicalResult = demoChecker.Run(@"Covid19_Archive.txt|holt Covid-19 Zahlen von Johns Hopkins und RKI",
                // new TreeParameters("MainTree", null) { CheckerDllDirectory = @"c:\Users\micro\Documents\private4\WPF\Eti\NetEti\Tests\TestJobs\CheckSpecials\Plugin" }, treeEvent);
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
