using MadTomDev.App;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace SimpleTaskScheduler2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            RemoveNotifyIcon();
            AddNotifyIcon();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            RemoveNotifyIcon();
        }


        private static NotifyIcon notifyIcon;
        private void RemoveNotifyIcon()
        {
            if (notifyIcon != null)
            {
                notifyIcon.MouseUp -= NotifyIcon_MouseUp;
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                notifyIcon = null;
            }
        }
        private void AddNotifyIcon()
        {
            if (notifyIcon == null)
            {
                if (File.Exists("simepleTaskScheduler.ico"))
                {
                    notifyIcon = new NotifyIcon()
                    {
                        Icon = new System.Drawing.Icon("simepleTaskScheduler.ico"),
                        Text = "Simple Task Scheduler 2",
                        Visible = true,
                    };

                    //ContextMenuStrip cMenu = new ContextMenuStrip();
                    //MenuItem mItemClose = new MenuItem();
                    //mItemClose.t
                    //notifyIcon.ContextMenuStrip = ;

                    notifyIcon.MouseUp += NotifyIcon_MouseUp;
                }
                else
                {
                    System.Windows.MessageBox.Show("File simepleTaskScheduler.ico not found, can't show NotifyIcon.");
                }
            }
        }

        private void NotifyIcon_MouseUp(object? sender, MouseEventArgs e)
        {
            MainWindow win = Core.GetInstance().mainWindow;
            if (win != null)
            {
                win.Visibility = Visibility.Visible;
                win.Show();
                if (win.WindowState == WindowState.Minimized)
                    win.WindowState = WindowState.Normal;
                win.Activate();
            }
        }
    }
}
