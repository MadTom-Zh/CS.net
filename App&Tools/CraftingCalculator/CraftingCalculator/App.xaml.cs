using MadTomDev.App;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CraftingCalculator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string logFile = MadTomDev.Common.Logger.StaticLog(e.Exception);
            MessageBox.Show(e.Exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // open dir
            //Process.Start("Explorer.exe", "/select," + logFile);

            // open log file
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(logFile)
            { UseShellExecute = true, };
            p.Start();
        }
    }
}
