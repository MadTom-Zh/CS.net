using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MadTomDev.UI
{
    public class Core
    {
        private static Core instance;
        private Core() { }
        public static Core GetInstance()
        {
            if (instance == null)
            {
                instance = new Core();
            }
            return instance;
        }

        internal void Init(string[] args)
        {
            //StringBuilder strBdr = new StringBuilder();
            //foreach (string a in args)
            //    strBdr.AppendLine(a);
            //MessageBox.Show(strBdr.ToString(), "args");


            if (args.Length <= 0)
            {
                WindowHelper winHelper = new WindowHelper();
                winHelper.ShowDialog();
                return;
            }
            if (!Enum.TryParse(args[0], true, out GadgetNames gName))
            {
                MessageBox.Show($"Unknow gadget [{args[0]}]");
                return;
            }

            string arg;
            string title;
            switch (gName)
            {
                case GadgetNames.ShowMsgBox:
                    title = "[title]";
                    string content = "[content]";
                    MessageBoxImage img = MessageBoxImage.Information;
                    MessageBoxButton btn = MessageBoxButton.OK;
                    bool topMost = false;
                    for (int i = 1, iv = args.Length; i < iv; ++i)
                    {
                        arg = args[i];
                        if (arg.StartsWith("t:"))
                        {
                            title = arg.Substring(2);
                        }
                        else if (arg.StartsWith("c:"))
                        {
                            content = arg.Substring(2).Replace("\\r", Environment.NewLine);
                        }
                        else if (arg.StartsWith("i:"))
                        {
                            Enum.TryParse(arg.Substring(2), true, out img);
                        }
                        else if (arg.StartsWith("b:"))
                        {
                            Enum.TryParse(arg.Substring(2), true, out btn);
                        }
                        else if (arg.StartsWith("tm:"))
                        {
                            bool.TryParse(arg.Substring(3), out topMost);
                        }
                    }

                    if (topMost)
                    {
                        Window winTop = new Window()
                        {
                            Topmost = true,
                            Width = 1,
                            Height = 1,
                            Left = -19200,
                            Top = -10800
                        };
                        winTop.Show();
                        winTop.Hide();
                        MessageBox.Show(winTop, content, title, btn, img);
                        winTop.Close();
                    }
                    else
                    {
                        MessageBox.Show(content, title, btn, img);
                    }

                    break;
                case GadgetNames.DelayStart:
                    title = "[title]";
                    TimeSpan tsFix = TimeSpan.Zero, tsRand = TimeSpan.Zero;
                    bool showCmdWnd = true;
                    string[] cmdLines = null;

                    for (int i = 1, iv = args.Length; i < iv; ++i)
                    {
                        arg = args[i];
                        if (arg.StartsWith("t:"))
                        {
                            title = arg.Substring(2);
                        }
                        else if (arg.StartsWith("df:"))
                        {
                            TimeSpan.TryParse(arg.Substring(3), out tsFix);
                        }
                        else if (arg.StartsWith("dr:"))
                        {
                            TimeSpan.TryParse(arg.Substring(3), out tsRand);
                        }
                        else if (arg.StartsWith("scw:"))
                        {
                            bool.TryParse(arg.Substring(4), out showCmdWnd);
                        }
                        else if (arg.StartsWith("cmds:"))
                        {
                            cmdLines = Common.SimpleStringHelper.NewLineEncoder.Decode(arg.Substring(5)).Replace("\"\"", "\"")
                                .Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
                        }
                    }
                    WindowDelayStart winDS = new WindowDelayStart();
                    winDS.SetNStart(title, tsFix, tsRand, showCmdWnd, cmdLines);
                    winDS.ShowDialog();

                    break;
            }
        }


        private static string _ExeFileFullName = null;
        public static string ExeFileFullName
        {
            get
            {
                if (_ExeFileFullName == null)
                {
                    _ExeFileFullName = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        Process.GetCurrentProcess().MainModule.FileName);
                }
                return _ExeFileFullName;
            }
        }
        public enum GadgetNames
        {
            ShowMsgBox, DelayStart
        }
        public static string GetCmdMsg(string title, string content, MessageBoxImage img, MessageBoxButton btn, bool isTopMost)
        {
            StringBuilder cmdBdr = new StringBuilder();
            cmdBdr.Append($"\"{ExeFileFullName}\" ");
            cmdBdr.Append($"{GadgetNames.ShowMsgBox.ToString()} ");
            cmdBdr.Append($"\"t:{title}\" ");
            cmdBdr.Append($"\"c:{content}\" ");
            cmdBdr.Append($"i:{img.ToString()} ");
            cmdBdr.Append($"b:{btn.ToString()} ");
            cmdBdr.Append($"tm:{isTopMost.ToString()}");
            return cmdBdr.ToString();
        }

        public static string GetCmdDelayStart(string title, TimeSpan delayFix, TimeSpan delayRand, bool showCmdWnd, string cmdLines)
        {
            StringBuilder cmdBdr = new StringBuilder();
            cmdBdr.Append($"\"{ExeFileFullName}\" ");
            cmdBdr.Append($"{GadgetNames.DelayStart.ToString()} ");
            cmdBdr.Append($"\"t:{title}\" ");
            cmdBdr.Append($"\"df:{delayFix.ToString(@"ddd\.hh\:mm\:ss\.fff")}\" ");
            cmdBdr.Append($"\"dr:{delayRand.ToString(@"ddd\.hh\:mm\:ss\.fff")}\" ");
            cmdBdr.Append($"scw:{showCmdWnd} ");
            cmdBdr.Append($"\"cmds:{Common.SimpleStringHelper.NewLineEncoder.Encode(cmdLines).Replace("\"", "\"\"")}\"");
            return cmdBdr.ToString();
        }
    }
}
