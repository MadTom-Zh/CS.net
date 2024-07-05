using MadTomDev.App.Classes;
using MadTomDev.App.Ctrls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;
using Console = MadTomDev.App.Classes.Console;
using Control = System.Windows.Controls.Control;
using Controls = System.Windows.Controls;
using Cursors = System.Windows.Input.Cursors;

using MadTomDev.App.Translators;
using System.Composition.Hosting;
using System.Reflection;
using System.Composition;
using MadTomDev.Common;

namespace MadTomDev.App
{
    public class Core
    {
        private Core() { }
        private static Core _Instance = new Core();
        public static Core Instance
        {
            get => _Instance;
        }
        public Settings setting = new Settings();
        public QuickInputProfiles quickInputProfiles = new QuickInputProfiles();

        public List<Console> consoleList = new List<Console>();
        internal MainWindow mainWindow;

        #region logger
        private Logger _Logger;
        public Logger Logger
        {
            get
            {
                if (_Logger == null)
                {
                    _Logger = new Logger()
                    { BaseDir = "Logs", };
                }
                return _Logger;
            }
        }
        public bool LoggerEnabled
        {
            get => setting.isLog;
            set
            {
                if (setting.isLog != value)
                {
                    setting.isLog = value;
                    setting.Save();
                }
            }
        }
        public void TryLog(string msg)
        {
            if (setting.isLog)
            {
                Logger.Log(msg);
            }
        }
        public void TryLog(Exception err)
        {
            if (setting.isLog)
            {
                Logger.Log(err);
            }
        }

        #endregion


        #region com list

        public string[] ComNameListAll
        {
            get => SerialPort.GetPortNames();
        }
        public string[] ComNameListFree
        {
            get
            {
                List<string> result = new List<string>();
                Console test;
                foreach (string pn in SerialPort.GetPortNames())
                {
                    if (consoleList.Find(a => a.comName == pn) == null)
                        result.Add(pn);
                }
                return result.ToArray();
            }
        }
        public string[] ComNameListInUse
        {
            get
            {
                return consoleList.Select(a => a.comName).ToArray();
            }
        }

        #endregion


        #region consoles

        internal bool CheckComInUse(string comName)
        {
            return consoleList.Find(a => a.comName == comName) != null;
        }
        internal Console? GetConsole(string comName, string cfgName)
        {
            return consoleList.Find(a => a.comName == comName && a.cfg.Name == cfgName);
        }
        internal Console? GetConsole_Com(string comName)
        {
            return consoleList.Find(a => a.comName == comName);
        }
        internal Console NewConsole(string comName, DGI_ComCfg cfg, out TabItemHeader tabHeader, out PanelConsole panelConsole)
        {
            if (GetConsole(comName, cfg.Name) != null)
                throw new Exception($"Console using [{comName}] with config [{cfg.Name}] was already created.");

            Console newConsole = new Console(comName, cfg);
            consoleList.Add(newConsole);
            tabHeader = newConsole.tabHeader;
            panelConsole = newConsole.tabPanel;
            return newConsole;
        }

        internal async void CloseConsole(Console console)
        {
            console.tabHeader.Cursor = Cursors.Wait;
            console.tabHeader.IsEnabled = false;
            console.tabPanel.IsEnabled = false;
            await Task.Delay(1);
            mainWindow.CloseConsoleTab(console.cfg.Name);
            consoleList.Remove(console);
            console.Dispose();
            ShowStatues($"Console closed.", StatuesLevels.WarningOrange);
        }

        #endregion


        #region statues msg & animation
        public enum StatuesLevels
        {
            InfoGreen, WarningOrange, ErrorRed,
        }
        //private Storyboard mainWindowStatuesStoryboard;

        private static SolidColorBrush ShowStatues_brush = new SolidColorBrush(Colors.Green);
        private static Storyboard ShowStatues_storyboard = null;
        internal void ShowStatues(string msg, StatuesLevels level)
        {
            if (mainWindow == null)
            {
                return;
            }
            mainWindow.Dispatcher.Invoke(() =>
            {
                mainWindow.tb_statues.Text = msg;

                mainWindow.bdr_statues.Background = ShowStatues_brush;
                switch (level)
                {
                    default:
                    case StatuesLevels.InfoGreen:
                        ShowStatues_brush.Color = Colors.Green;
                        break;
                    case StatuesLevels.WarningOrange:
                        ShowStatues_brush.Color = Colors.Orange;
                        break;
                    case StatuesLevels.ErrorRed:
                        ShowStatues_brush.Color = Colors.Red;
                        break;
                }


                if (ShowStatues_storyboard == null)
                {
                    ShowStatues_storyboard = new Storyboard();
                    DoubleAnimation anima = new DoubleAnimation()
                    {
                        Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                        From = 1d,
                        To = 0d,
                    };
                    Storyboard.SetTarget(anima, mainWindow.bdr_statues);
                    Storyboard.SetTargetProperty(anima, new PropertyPath(Control.OpacityProperty));
                    ShowStatues_storyboard.Children.Add(anima);
                }
                ShowStatues_storyboard.Begin();
            });
        }


        #endregion


        #region translators

        public static string TranslatorsDirName = "Translators";
        public class TranslatorInfo
        {
            public FileInfo file;
            public MetaData? metaData;
            public bool isDuplicated = false;
            public string GetMetaDataInfoString()
            {
                StringBuilder result = new StringBuilder();
                if (metaData != null)
                {
                    result.Append("V: ");
                    result.AppendLine(metaData.Version.ToString());
                    result.Append("UL: ");
                    result.AppendLine(metaData.UserLanguage);
                    result.Append("ML: ");
                    result.Append(metaData.MachineLanguage);
                }
                return result.ToString();
            }
        }
        internal List<TranslatorInfo> ReloadTranslatorInfoList()
        {
            List<TranslatorInfo> result = new List<TranslatorInfo>();
            if (Directory.Exists(TranslatorsDirName))
            {
                TranslatorInfo newInfo;
                ContainerConfiguration cfg;
                MetaDataContainer mdContainer = new MetaDataContainer();
                foreach (FileInfo dllFile in new DirectoryInfo(TranslatorsDirName).GetFiles("*.dll"))
                {
                    cfg = new ContainerConfiguration()
                        .WithAssembly(Assembly.LoadFile(dllFile.FullName));
                    using (var host = cfg.CreateContainer())
                    {
                        host.SatisfyImports(mdContainer);
                    }

                    newInfo = new TranslatorInfo()
                    { file = dllFile, metaData = mdContainer.Container?.FirstOrDefault()?.Metadata, };
                    result.Add(newInfo);
                }
                // check duplicate
                TranslatorInfo a, b;
                for (int i = result.Count - 1, j; i > 0; --i)
                {
                    b = result[i];
                    for (j = i - 1; j >= 0; --j)
                    {
                        a = result[j];
                        if (b.metaData.UserLanguage == a.metaData.UserLanguage
                            && b.metaData.MachineLanguage == a.metaData.MachineLanguage)
                        {
                            b.isDuplicated = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(TranslatorsDirName);
            }
            return result;
        }

        internal List<ITranslator> ReloadTranslators(IEnumerable<TranslatorInfo> selectedTranslatorInfoList)
        {
            List<ITranslator> result = null;
            IEnumerable<Assembly> assembiles
                = selectedTranslatorInfoList.Select(t => t.file)
                    .Select(f => Assembly.LoadFile(f.FullName));
            var configuration = new ContainerConfiguration()
                .WithAssemblies(assembiles);
            using (var container = configuration.CreateContainer())
            {
                result = container.GetExports<ITranslator>().ToList();
            }
            if (result == null)
            {
                result = new List<ITranslator>();
            }
            return result;
        }

        #endregion
    }
}
