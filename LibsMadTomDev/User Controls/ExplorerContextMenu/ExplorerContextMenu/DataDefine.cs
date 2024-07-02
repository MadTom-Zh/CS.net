using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Serialization;
using System.IO;

namespace MadTomDev.UI
{
    public enum SelectionCountTypes
    {
        None, Any0, Any1, More1, File,
        /// <summary>
        /// 1或多文件
        /// </summary>
        Files,
        Dir,
        /// <summary>
        /// 1或多文件夹
        /// </summary>
        Dirs,
        /// <summary>
        /// 1文件，且1文件夹
        /// </summary>
        FileNDir,
        /// <summary>
        /// 1或多文件，且1文件夹
        /// </summary>
        FilesNDir,
        /// <summary>
        /// 1文件，且1或多文件夹
        /// </summary>
        FileNDirs,
        /// <summary>
        /// 1或多文件，且1或多文件夹
        /// </summary>
        FilesNDirs,
        /// <summary>
        /// 1文件，或1文件夹
        /// </summary>
        FileOrDir,
        /// <summary>
        /// 1或多文件，或1文件夹
        /// </summary>
        FilesOrDir,
        /// <summary>
        /// 1文件，或1或多文件夹
        /// </summary>
        FileOrDirs,
        /// <summary>
        /// 1或多文件，或 1或多文件夹
        /// </summary>
        FilesOrDirs,
    }
    public enum SelectionFileTypes
    {
        Any, SameExt,
    }
    public class EMItemModel : INotifyPropertyChanged
    {
        public EMItemModel(bool isUserItem = true)
        {
            IsUserItem = isUserItem;
        }
        public string name;
        public bool IsUserItem { private set; get; }

        public BitmapSource Icon { set; get; }
        public string Text { set; get; }
        //{
        //    get => _Text;
        //    set
        //    {
        //        _Text = value;
        //        RaisePropertyChange("Text");
        //    }
        //}
        public string CommandText { set; get; }


        private bool _IsEnabled = true;
        public bool IsEnabled
        {
            get => _IsEnabled;
            set
            {
                _IsEnabled = value;
                RaisePropertyChange("IsEnabled");
            }
        }

        public SelectionCountTypes SelectionCountType { set; get; } = SelectionCountTypes.Any0;

        public SelectionFileTypes SelectionFileType { set; get; } = SelectionFileTypes.Any;


        public object tag;
        public Action<EMItemModel> actionClick;


        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class EMItemModelExtensions
    {

        public static string flag_separator = "separator";
        public static string flag_user = "[user]";
        public static string flag_pc = "[pc]";
        public static string flag_parent = "[parent]";
        public static string flag_parentName = "[parentName]";
        public static string flag_files = "[files]";




        /// <summary>
        /// 创建菜单分隔符，当索引不为负数时，为系统默认分隔符
        /// </summary>
        /// <param name="index">系统默认菜单中分隔符所在的索引</param>
        /// <returns></returns>
        public static System.Windows.Controls.Separator NewSeparator(int index = -1)
        {
            System.Windows.Controls.Separator result = new System.Windows.Controls.Separator()
            { Tag = index, };
            return result;
        }
        public static string GetRealCommand(string cmdTx, string parent, List<string> files)
        {
            string result = cmdTx;

            int i, j, startFlagIndex;
            string outTx, tmpTx;
            if (result.Contains("[date("))
            {
                i = 0;
                DateTime now = DateTime.Now;
                while (i >= 0 && TryFindVar(i, "[date(", ")]", out startFlagIndex, out outTx))
                {
                    Replace(startFlagIndex, outTx.Length + 8, now.ToString(outTx));
                    i = startFlagIndex + 1;
                }
            }
            if (result.Contains("[file("))
            {
                i = 0;
                while (i >= 0 && TryFindVar(i, "[file(", ")]", out startFlagIndex, out outTx))
                {
                    if (int.TryParse(outTx, out j))
                    {
                        tmpTx = files[j];
                        Replace(startFlagIndex, outTx.Length + 8, tmpTx);
                    }
                    i = startFlagIndex + 1;
                }
            }
            if (result.Contains("[nameOfFile("))
            {
                i = 0;
                while (i >= 0 && TryFindVar(i, "[nameOfFile(", ")]", out startFlagIndex, out outTx))
                {
                    if (int.TryParse(outTx, out j))
                    {
                        tmpTx = Path.GetFileName(files[j]);
                        Replace(startFlagIndex, outTx.Length + 14, tmpTx);
                    }
                    i = startFlagIndex + 1;
                }
            }
            if (result.Contains("[nameOfFilePrefix("))
            {
                i = 0;
                while (i >= 0 && TryFindVar(i, "[nameOfFilePrefix(", ")]", out startFlagIndex, out outTx))
                {
                    if (int.TryParse(outTx, out j))
                    {
                        tmpTx = Path.GetFileName(files[j]);
                        if (tmpTx.Contains('.'))
                            tmpTx = tmpTx.Substring(0, tmpTx.LastIndexOf('.'));
                        Replace(startFlagIndex, outTx.Length + 20, tmpTx);
                    }
                    i = startFlagIndex + 1;
                }
            }
            if (result.Contains("[nameOfFileSuffix("))
            {
                i = 0;
                while (i >= 0 && TryFindVar(i, "[nameOfFileSuffix(", ")]", out startFlagIndex, out outTx))
                {
                    if (int.TryParse(outTx, out j))
                    {
                        tmpTx = Path.GetFileName(files[j]);
                        if (tmpTx.Contains('.'))
                            tmpTx = tmpTx.Substring(tmpTx.LastIndexOf('.'));
                        Replace(startFlagIndex, outTx.Length + 20, tmpTx);
                    }
                    i = startFlagIndex + 1;
                }
            }

            if (result.Contains(flag_parent))
            {
                result = result.Replace(flag_parent, parent);
            }
            if (result.Contains(flag_parentName))
            {
                result = result.Replace(flag_parentName, System.IO.Path.GetFileName(parent));
            }
            if (result.Contains("[files]"))
            {
                StringBuilder strBdr = new StringBuilder();
                foreach (string f in files)
                {
                    if (f.StartsWith("\""))
                    {
                        strBdr.Append(f);
                    }
                    else
                    {
                        strBdr.Append("\"");
                        strBdr.Append(f);
                        strBdr.Append("\"");
                    }
                    strBdr.Append(" ");
                }
                if (strBdr.Length > 0)
                    strBdr.Remove(strBdr.Length - 1, 1);
                result = result.Replace("[files]", strBdr.ToString());
            }
            if (result.Contains("[pc]"))
                result = result.Replace("[pc]", Environment.MachineName);
            if (result.Contains("[user]"))
                result = result.Replace("[user]", Environment.UserName);

            return result;

            bool TryFindVar(int startIndex, string startFlag, string endFlag, out int startFlagIndex, out string var)
            {
                startFlagIndex = -1;
                var = null;
                startFlagIndex = result.IndexOf(startFlag, startIndex);
                if (startFlagIndex < 0) return false;
                int j = result.IndexOf(endFlag, startFlagIndex);
                if (j < 0) return false;

                int startFlagLength = startFlag.Length;
                var = cmdTx.Substring(
                    startFlagIndex + startFlagLength,
                    j - startFlagIndex - startFlagLength);
                return true;
            }
            void Replace(int startIndex, int length, string replace)
            {
                result
                    = result.Substring(0, startIndex)
                    + replace
                    + result.Substring(startIndex + length);
            }

        }

        public static CommandAnalysisResult AnalysisCommand(string cmd)
        {
            if (cmd != null)
                cmd = cmd.Trim();
            CommandAnalysisResult result = new CommandAnalysisResult()
            {
                cmd = cmd,
            };
            if (string.IsNullOrWhiteSpace(cmd))
            {
                result.cmdType = CommandAnalysisResult.CmdTypes.Unknow;
                return result;
            }
            if (cmd.ToLower() == flag_separator)
            {
                result.cmdType = CommandAnalysisResult.CmdTypes.Separator;
                return result;
            }
            result.hasFlagParent = cmd.Contains(flag_parent);
            result.hasFlagFiles = cmd.Contains(flag_files);
            result.hasFlagUser = cmd.Contains(flag_user);
            result.hasFlagPc = cmd.Contains(flag_pc);
            int testI1 = cmd.IndexOf("[date("), testI2;
            if (testI1 >= 0)
            {
                testI2 = cmd.IndexOf(")]", testI1 + 6);
                if (testI2 > 0)
                    result.hasFlagData = true;
                else
                    result.hasFlagData = false;
            }
            else
            {
                result.hasFlagData = false;
            }

            if (cmd.StartsWith("[new(") && cmd.EndsWith(")]"))
            {
                result.cmdType = CommandAnalysisResult.CmdTypes.New;
                result.newFileName = cmd.Substring(5, cmd.Length - 7);
                string tplFile = System.IO.Path.Combine(Setting.dirTempletes, result.newFileName);

                if (System.IO.File.Exists(tplFile) || System.IO.Directory.Exists(tplFile))
                {
                    result.newTempletFile = tplFile;
                }
                result.newFileExt = System.IO.Path.GetExtension(tplFile);

                return result;
            }
            else if (cmd.StartsWith("[newDir(") && cmd.EndsWith(")]"))
            {
                result.cmdType = CommandAnalysisResult.CmdTypes.NewDir;
                result.newDirName = cmd.Substring(8, cmd.Length - 10);
                return result;
            }

            string testS1 = cmd;
            if (testS1.StartsWith("\""))
            {
                testI1 = testS1.IndexOf("\"", 1);
                if (testI1 > 1)
                    testS1 = testS1.Substring(1, testI1 - 1);
                else
                    testS1 = null;
            }
            else
            {
                testI1 = cmd.IndexOf(' ');
                if (testI1 > 0)
                    testS1 = cmd.Substring(0, testI1);
                else
                    testS1 = cmd;
            }

            if (System.IO.File.Exists(testS1))
            {
                result.cmdType = CommandAnalysisResult.CmdTypes.Exec;
                result.execFile = testS1;
            }

            return result;
        }
        public class CommandAnalysisResult
        {
            public string? cmd = null;
            public enum CmdTypes
            {
                Unknow, Separator, Exec, New, NewDir,
            }
            public CmdTypes cmdType = CmdTypes.Unknow;
            public string? execFile = null;
            /// <summary>
            /// 如果样板文件存在，测此变量为样板文件的完整路径；否则为null；
            /// </summary>
            public string? newTempletFile = null;
            public string? newFileName = null;
            public string? newDirName = null;
            /// <summary>
            /// such as ".data"
            /// </summary>
            public string newFileExt;

            public bool hasFlagParent = false;
            public bool hasFlagFiles = false;

            public bool hasFlagUser = false;
            public bool hasFlagPc = false;
            public bool hasFlagData = false;
        }

    }
}
