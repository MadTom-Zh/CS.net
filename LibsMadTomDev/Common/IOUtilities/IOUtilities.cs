using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.InteropServices;
using IWshRuntimeLibrary;  // COM  --  Windows Script Host Object Model
using System.Threading;
using File = System.IO.File;
using System.Windows;
using System.Collections.Specialized;
using System.DirectoryServices;

namespace MadTomDev.Data
{
    public class Utilities
    {
        public static void FileMultipleCopy(string sourceFileFullName, params string[] destinationFileFullNames)
        {
            if (sourceFileFullName == null || sourceFileFullName.Length <= 0) throw new ArgumentException("Source file must be specified.", "sourceFilePath");

            if (destinationFileFullNames == null || destinationFileFullNames.Length == 0) throw new ArgumentException("Destination file(s) must be specified.", "destinationPaths");

            Parallel.ForEach(destinationFileFullNames, new ParallelOptions(),
                             destinationFileFullName =>
                             {
                                 using (var source = new FileStream(sourceFileFullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                                 using (var destination = new FileStream(destinationFileFullName, FileMode.Create))
                                 {
                                     var buffer = new byte[1024];
                                     int read;

                                     while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
                                     {
                                         destination.Write(buffer, 0, read);
                                     }
                                 }

                             });
        }

        public static void DirMultipleCopy(string sourceDirFullName, params string[] destinationDirFullNames)
        {
            if (sourceDirFullName == null || sourceDirFullName.Length <= 0) throw new ArgumentException("Source Dir must be specified.", "sourceDirPath");
            if (destinationDirFullNames == null || destinationDirFullNames.Length == 0) throw new ArgumentException("Destination Dir(s) must be specified.", "destinationDirPaths");

            foreach (string dDirPath in destinationDirFullNames)
            {
                if (Directory.Exists(dDirPath) == false) Directory.CreateDirectory(dDirPath);
            }

            string sourceFileName;
            string[] dFilePathes = new string[destinationDirFullNames.Length];
            DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceDirFullName);
            foreach (FileInfo fileInfo in sourceDirInfo.GetFiles())
            {
                sourceFileName = fileInfo.Name;
                for (int i = destinationDirFullNames.Length - 1; i >= 0; i--)
                {
                    dFilePathes[i] = destinationDirFullNames[i] + Path.DirectorySeparatorChar + sourceFileName;
                }
                FileMultipleCopy(fileInfo.FullName, dFilePathes);
            }

            string sourceDirName;
            string[] dDirPathes = new string[destinationDirFullNames.Length];
            foreach (DirectoryInfo dirInfo in sourceDirInfo.GetDirectories())
            {
                sourceDirName = dirInfo.Name;
                for (int i = destinationDirFullNames.Length - 1; i >= 0; i--)
                {
                    dDirPathes[i] = destinationDirFullNames[i] + Path.DirectorySeparatorChar + sourceDirName;
                }
                DirMultipleCopy(dirInfo.FullName, dDirPathes);
            }
        }


        /// <summary>
        /// delete the dir with all contains, even read-only items
        /// </summary>
        /// <param name="target_dir"></param>
        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                System.IO.File.SetAttributes(file, FileAttributes.Normal);
                System.IO.File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            try
            {
                Directory.Delete(target_dir, true);
            }
            catch (IOException)
            {
                Directory.Delete(target_dir, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(target_dir, true);
            }
        }


        public static class CSharpWapper
        {
            public static string RecycleBinName = "$Recycle.Bin.MTDev";
            public static Exception Recycle(string filesOrDir)
            {
                string recycDir = Path.Combine(FilePath.GetRoot(filesOrDir), RecycleBinName);
                if (!Directory.Exists(recycDir))
                    Directory.CreateDirectory(recycDir);

                Exception err = Move(filesOrDir, recycDir, ExistHandleMethods.Rename, ExistHandleMethods.Rename, out string newFullName);
                RecycleLog(recycDir, ref filesOrDir, ref newFullName, ref err);
                return err;
            }
            private static void RecycleLog(string recycleDir, ref string sur, ref string tar, ref Exception err)
            {
                using (StreamWriter sw = File.AppendText(Path.Combine(recycleDir, "_RecycleHistory.txt")))
                {
                    sw.WriteLine();
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    sw.WriteLine($"From {sur}");
                    sw.WriteLine($"To   {tar}");
                    if (err != null)
                        sw.WriteLine($"Err {err.ToString()}");
                }
            }
            public static Exception Delete(string filesOrDir)
            {
                try
                {
                    if (File.Exists(filesOrDir))
                        File.Delete(filesOrDir);
                    else if (Directory.Exists(filesOrDir))
                        Directory.Delete(filesOrDir, true);
                    else
                        return new IOException("File or Directory not found: [" + filesOrDir + "]");
                }
                catch (Exception err)
                {
                    return err;
                }
                return null;
            }

            public enum ExistHandleMethods
            { Skip, OverwriteCombine, Rename, }

            public static Exception Copy(string filesOrDir, string toDir,
                ExistHandleMethods dirExistHandleMethod, ExistHandleMethods fileExistHandleMethod)
            {
                return _CopyOrMove(true, filesOrDir, toDir, dirExistHandleMethod, fileExistHandleMethod, out string newFullName);
            }
            public static Exception Copy(string filesOrDir, string toDir,
                ExistHandleMethods dirExistHandleMethod, ExistHandleMethods fileExistHandleMethod, out string newFullName)
            {
                return _CopyOrMove(true, filesOrDir, toDir, dirExistHandleMethod, fileExistHandleMethod, out newFullName);
            }
            public static Exception Move(string filesOrDir, string toDir,
                ExistHandleMethods dirExistHandleMethod, ExistHandleMethods fileExistHandleMethod)
            {
                return _CopyOrMove(false, filesOrDir, toDir, dirExistHandleMethod, fileExistHandleMethod, out string newFullName);
            }
            public static Exception Move(string filesOrDir, string toDir,
                ExistHandleMethods dirExistHandleMethod, ExistHandleMethods fileExistHandleMethod, out string newFullName)
            {
                return _CopyOrMove(false, filesOrDir, toDir, dirExistHandleMethod, fileExistHandleMethod, out newFullName);
            }
            public static Exception DirectoyCopy(string source, string target,
                ExistHandleMethods dirExistHandleMethod, ExistHandleMethods fileExistHandleMethod)
            {
                return CopyOrMoveDir_loop(true, source, target, dirExistHandleMethod, fileExistHandleMethod, out string newFullName);
            }
            public static Exception DirectoyCopy(string source, string target,
                ExistHandleMethods dirExistHandleMethod, ExistHandleMethods fileExistHandleMethod, out string newFullName)
            {
                return CopyOrMoveDir_loop(true, source, target, dirExistHandleMethod, fileExistHandleMethod, out newFullName);
            }
            /// <summary>
            /// 复制文件夹，如果目标存在，则报错；
            /// </summary>
            /// <param name="source">源文件夹，必须为已存在的文件夹；</param>
            /// <param name="target">目标文件夹，必须为不存在的文件夹；</param>
            /// <returns></returns>
            public static Exception DirectoyCopy(string source, string target)
            {
                return CopyOrMoveDir_loop(true, source, target);
            }
            public static Exception DirectoyMove(string source, string target,
                ExistHandleMethods dirExistHandleMethod, ExistHandleMethods fileExistHandleMethod)
            {
                return CopyOrMoveDir_loop(false, source, target, dirExistHandleMethod, fileExistHandleMethod, out string newFullName);
            }
            public static Exception DirectoyMove(string source, string target,
                ExistHandleMethods dirExistHandleMethod, ExistHandleMethods fileExistHandleMethod, out string newFullName)
            {
                return CopyOrMoveDir_loop(false, source, target, dirExistHandleMethod, fileExistHandleMethod, out newFullName);
            }
            /// <summary>
            /// 移动文件夹，如果目标存在，则报错；
            /// </summary>
            /// <param name="source">源文件夹，必须为已存在的文件夹；</param>
            /// <param name="target">目标文件夹，必须为不存在的文件夹；</param>
            /// <returns></returns>
            public static Exception DirectoyMove(string source, string target)
            {
                return CopyOrMoveDir_loop(false, source, target);
            }
            private static Exception _CopyOrMove(bool isCopyOrMove, string fileOrDir, string toDir,
                ExistHandleMethods dirExistHandleMethod, ExistHandleMethods fileExistHandleMethod,
                out string newFullName)
            {
                newFullName = null;
                try
                {
                    Exception err;
                    newFullName = Path.Combine(toDir, Path.GetFileName(fileOrDir));
                    bool? surIsFileOrDir = null;
                    if (File.Exists(fileOrDir)) surIsFileOrDir = true;
                    else if (Directory.Exists(fileOrDir)) surIsFileOrDir = false;
                    bool? desIsFileOrDir = null;
                    if (File.Exists(newFullName)) desIsFileOrDir = true;
                    else if (Directory.Exists(newFullName)) desIsFileOrDir = false;

                    if (surIsFileOrDir == true)
                    {
                        // source is file
                        if (desIsFileOrDir == null)
                        {
                            if (isCopyOrMove)
                                File.Copy(fileOrDir, newFullName);
                            else
                                File.Move(fileOrDir, newFullName);
                        }
                        else if (desIsFileOrDir == true)
                        {
                            // target exists, is file
                            switch (fileExistHandleMethod)
                            {
                                case ExistHandleMethods.Skip:
                                    break;
                                case ExistHandleMethods.OverwriteCombine:
                                    if (isCopyOrMove)
                                        File.Copy(fileOrDir, newFullName, true);
                                    else
                                    {
                                        File.Copy(fileOrDir, newFullName, true);
                                        File.Delete(fileOrDir);
                                    }
                                    break;
                                case ExistHandleMethods.Rename:
                                    newFullName = AutoNewFullName(newFullName);
                                    if (isCopyOrMove)
                                    {
                                        File.Copy(fileOrDir, newFullName, false);
                                    }
                                    else
                                    {
                                        File.Copy(fileOrDir, newFullName, false);
                                        File.Delete(fileOrDir);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            // target exists, is dir
                            switch (fileExistHandleMethod)
                            {
                                case ExistHandleMethods.Skip:
                                    break;
                                case ExistHandleMethods.OverwriteCombine:
                                    MSVBFileOperation.Delete(new string[] { newFullName }, out err, false);
                                    if (err != null)
                                        break;
                                    if (isCopyOrMove)
                                        File.Copy(fileOrDir, newFullName);
                                    else
                                        File.Move(fileOrDir, newFullName);
                                    break;
                                case ExistHandleMethods.Rename:
                                    newFullName = AutoNewFullName(newFullName);
                                    if (isCopyOrMove)
                                    {
                                        File.Copy(fileOrDir, newFullName, false);
                                    }
                                    else
                                    {
                                        File.Copy(fileOrDir, newFullName, false);
                                        File.Delete(fileOrDir);
                                    }
                                    break;
                            }
                        }
                    }
                    else if (surIsFileOrDir == false)
                    {
                        // source is dir
                        if (desIsFileOrDir == null)
                        {
                            if (isCopyOrMove)
                                err = DirectoyCopy(fileOrDir, newFullName);
                            else
                                err = DirectoyMove(fileOrDir, newFullName);
                            if (err != null)
                                return err;
                        }
                        else if (desIsFileOrDir == true)
                        {
                            // target exists, is file
                            switch (fileExistHandleMethod)
                            {
                                case ExistHandleMethods.Skip:
                                    break;
                                case ExistHandleMethods.OverwriteCombine:
                                    MSVBFileOperation.Delete(new string[] { newFullName }, out err, false);
                                    if (err != null)
                                        return err;
                                    if (isCopyOrMove)
                                        err = DirectoyCopy(fileOrDir, newFullName);
                                    else
                                        err = DirectoyMove(fileOrDir, newFullName);
                                    if (err != null)
                                        return err;
                                    break;
                                case ExistHandleMethods.Rename:
                                    newFullName = AutoNewFullName(newFullName);
                                    if (isCopyOrMove)
                                        err = DirectoyCopy(fileOrDir, newFullName);
                                    else
                                        err = DirectoyMove(fileOrDir, newFullName);
                                    if (err != null)
                                        return err;
                                    break;
                            }
                        }
                        else
                        {
                            // target exists, is dir
                            switch (dirExistHandleMethod)
                            {
                                case ExistHandleMethods.Skip:
                                    break;
                                case ExistHandleMethods.OverwriteCombine:
                                    err = CopyOrMoveDir_loop(isCopyOrMove, fileOrDir, newFullName,
                                        dirExistHandleMethod, fileExistHandleMethod, out newFullName);
                                    if (err != null)
                                        return err;
                                    break;
                                case ExistHandleMethods.Rename:
                                    newFullName = AutoNewFullName(newFullName);
                                    if (isCopyOrMove)
                                        err = DirectoyCopy(fileOrDir, newFullName);
                                    else
                                        err = DirectoyMove(fileOrDir, newFullName);
                                    if (err != null)
                                        return err;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        // source not exists
                        return new IOException("File or Directory not found: [" + fileOrDir + "]");
                    }

                }
                catch (Exception err)
                {
                    return err;
                }
                return null;
            }

            /// <summary>
            /// 复制、移动文件夹，使用给定的存在条件，来处理已存在的文件、文件夹
            /// </summary>
            /// <param name="isCopyOrMove"></param>
            /// <param name="sourceDir">源文件夹，必须是已存在的</param>
            /// <param name="targetDir">(合并)目标文件夹，必须是已存在的</param>
            /// <param name="dirExistHandleMethod"></param>
            /// <param name="fileExistHandleMethod"></param>
            /// <returns></returns>
            private static Exception CopyOrMoveDir_loop(bool isCopyOrMove, string sourceDir, string targetDir,
                ExistHandleMethods dirExistHandleMethod, ExistHandleMethods fileExistHandleMethod, out string newFullName)
            {
                newFullName = null;
                if (dirExistHandleMethod == ExistHandleMethods.Skip)
                    return null;

                try
                {
                    switch (dirExistHandleMethod)
                    {
                        case ExistHandleMethods.OverwriteCombine:
                            {
                                DirectoryInfo sourceDI = new DirectoryInfo(sourceDir);
                                Exception err;
                                string tarSubDir;
                                foreach (FileInfo fi in sourceDI.GetFiles())
                                {
                                    err = _CopyOrMove(isCopyOrMove, fi.FullName, targetDir,
                                        dirExistHandleMethod, fileExistHandleMethod, out newFullName);
                                    if (err != null)
                                        return err;
                                }
                                foreach (DirectoryInfo surSubDI in sourceDI.GetDirectories())
                                {
                                    err = null;
                                    tarSubDir = Path.Combine(targetDir, surSubDI.Name);
                                    if (Directory.Exists(tarSubDir))
                                    {
                                        err = CopyOrMoveDir_loop(isCopyOrMove, surSubDI.FullName, tarSubDir,
                                            dirExistHandleMethod, fileExistHandleMethod, out newFullName);
                                    }
                                    else
                                    {
                                        newFullName = Path.Combine(targetDir, surSubDI.Name);
                                        err = CopyOrMoveDir_loop(isCopyOrMove, surSubDI.FullName, tarSubDir);
                                    }

                                    if (err != null)
                                        return err;
                                }
                                break;
                            }
                        case ExistHandleMethods.Rename:
                            {
                                targetDir = AutoNewFullName(targetDir);
                                newFullName = Path.Combine(targetDir, Path.GetFileName(sourceDir));
                                return CopyOrMoveDir_loop(isCopyOrMove, sourceDir, targetDir);
                            }
                    }
                }
                catch (Exception err)
                {
                    return err;
                }
                return null;
            }
            /// <summary>
            /// 复制、一定文件夹，如果目标存在，则报错；
            /// </summary>
            /// <param name="isCopyOrMove"></param>
            /// <param name="sourceDir">源文件夹；</param>
            /// <param name="targetDir">目标文件夹，必须为不存在的文件夹；</param>
            /// <returns></returns>
            private static Exception CopyOrMoveDir_loop(bool isCopyOrMove, string sourceDir, string targetDir)
            {
                try
                {
                    if (isCopyOrMove)
                    {
                        DirectoryInfo sourceDI = new DirectoryInfo(sourceDir);
                        Exception err;
                        DirectoryInfo tarDI = Directory.CreateDirectory(targetDir);
                        tarDI.SetAccessControl(sourceDI.GetAccessControl());
                        foreach (FileInfo fi in sourceDI.GetFiles())
                        {
                            File.Copy(fi.FullName, Path.Combine(targetDir, fi.Name));
                        }
                        foreach (DirectoryInfo di in sourceDI.GetDirectories())
                        {
                            err = CopyOrMoveDir_loop(true, di.FullName, Path.Combine(targetDir, di.Name));
                            if (err != null)
                                return err;
                        }
                    }
                    else
                    {
                        Directory.Move(sourceDir, targetDir);
                    }
                }
                catch (Exception err)
                {
                    return err;
                }
                return null;
            }

            public static string AutoNewFullName(string fullPath, List<string>? potentialFullNameList = null)
            {
                string newFullName = fullPath;
                string result = Path.GetFileName(fullPath);

                int counter;
                if (File.Exists(newFullName))
                {
                    string path, prefix, suffix = null;
                    path = Path.GetDirectoryName(newFullName) + Path.DirectorySeparatorChar.ToString();
                    prefix = fullPath.Substring(path.Length);
                    if (prefix.Contains("."))
                    {
                        suffix = prefix.Substring(prefix.LastIndexOf("."));
                        prefix = prefix.Substring(0, prefix.Length - suffix.Length);
                    }
                    counter = TryGetExistCounter(ref prefix);
                    do
                    {
                        newFullName = path + prefix + " (" + (++counter).ToString() + ")" + suffix;
                        if (potentialFullNameList != null && potentialFullNameList.Contains(newFullName))
                            continue;
                    }
                    while (File.Exists(newFullName));
                }
                else if (Directory.Exists(newFullName))
                {
                    string dirName = Path.GetFileName(newFullName);
                    string dirParent = Path.GetDirectoryName(newFullName);
                    counter = TryGetExistCounter(ref dirName);
                    do
                    {
                        newFullName = dirParent + Path.DirectorySeparatorChar + dirName + " (" + (++counter).ToString() + ")";
                        if (potentialFullNameList != null && potentialFullNameList.Contains(newFullName))
                            continue;
                    }
                    while (Directory.Exists(newFullName));
                }
                return newFullName;

                int TryGetExistCounter(ref string prefix)
                {
                    if (prefix.Contains('('))
                    {
                        int blIdx = prefix.LastIndexOf('(');
                        int brIdx = prefix.LastIndexOf(')');
                        if (blIdx + 1 < brIdx && int.TryParse(prefix.Substring(blIdx + 1, brIdx - blIdx - 1), out int c))
                        {
                            prefix = prefix.Substring(0, blIdx).TrimEnd();
                            return c;
                        }
                        else return 0;
                    }
                    else return 0;
                }
            }
            public static string AutoNewName(string fullPath, List<string>? potentialFullNameList = null)
            {
                return Path.GetFileName(AutoNewFullName(fullPath, potentialFullNameList));
            }

            /// <summary>
            /// 以当前名称为准，找到最新的自动名称；
            /// 如果没有自动名称，则返回null；
            /// </summary>
            /// <param name="fullPath"></param>
            /// <returns></returns>
            public static string FindLastAutoNewFullName(string fullPath)
            {
                string dir = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dir))
                    return null;

                string fileName = Path.GetFileName(fullPath);
                string fileExt = Path.GetExtension(fileName);
                int filePreLength = fileName.Length - fileExt.Length;
                string fileNamePre = fileName.Substring(0, filePreLength);


                int no = -1, test;
                string targetFullName = null;
                if (File.Exists(fullPath))
                    foreach (FileInfo f in new DirectoryInfo(dir).GetFiles(
                        $"{fileNamePre}*"))
                        Test(f.FullName, f.Name);
                else if (Directory.Exists(fullPath))
                    foreach (DirectoryInfo d in new DirectoryInfo(dir).GetDirectories(
                        $"{fileName}*"))
                        TestDir(d.FullName);

                if (no < 0)
                    return null;
                else
                    return targetFullName;

                string GetNoText(string newFileName, ref string oriFileNamePre, ref string oriFileExt)
                {
                    if (!newFileName.EndsWith(oriFileExt))
                        return null;
                    string txt;
                    if (newFileName.Length > fileExt.Length)
                        txt = newFileName.Substring(0, newFileName.Length - fileExt.Length);
                    else
                        return null;
                    if (txt.Length > oriFileNamePre.Length)
                        txt = txt.Substring(oriFileNamePre.Length);
                    else
                        return null;

                    int iS = txt.IndexOf("(");
                    if (iS < 0) return null;
                    iS += 1;
                    int iE = txt.IndexOf(")");
                    if (iS < 0) return null;
                    if (iE > iS)
                        return txt.Substring(iS, iE - iS);
                    else
                        return null;
                }
                void Test(string newFullName, string newName)
                {
                    if (int.TryParse(GetNoText(newName, ref fileNamePre, ref fileExt), out test))
                    {
                        if (no < test)
                        {
                            no = test;
                            targetFullName = newFullName;
                        }
                    }
                }
                void TestDir(string newFullName)
                {
                    if (newFullName.Length <= fullPath.Length)
                        return;
                    string txt = newFullName.Substring(fullPath.Length);
                    int iS = txt.IndexOf("(");
                    if (iS < 0) return;
                    iS += 1;
                    int iE = txt.IndexOf(")");
                    if (iS < 0) return;
                    if (iE > iS && int.TryParse(txt.Substring(iS, iE - iS), out test))
                    {
                        if (no < test)
                        {
                            no = test;
                            targetFullName = newFullName;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// not yet tested
        /// </summary>
        public class CSharpIOHelper
        {
            public CSharpIOHelper()
            {
                TransferingReport += CSharpIOHelper_TransferingReport;
            }


            public enum ProgressReportTypes
            { RealTime, Interval, }
            public ProgressReportTypes ProgressReportType = ProgressReportTypes.Interval;
            public float ProgressReportIntervalSec = 1f;

            /// <summary>
            /// whether overwrite exist files;
            /// true - overwrite;
            /// false - throw as a exception;
            /// </summary>
            public bool IsOverwrite { private set; get; } = false;

            /// <summary>
            /// whether overwrite dir attributes
            /// true - set exact attributes as source dir;
            /// false - just create a dir;
            /// </summary>
            public bool IsOverwriteDirAttributes { private set; get; } = false;

            /// <summary>
            /// whether stop the whole operation, when one error raised;
            /// true - stop;
            /// false - skip and continue;
            /// </summary>
            public bool IsErrorStop { private set; get; } = false;


            public OperationTypes CurOperation { private set; get; } = OperationTypes.None;
            public OperationStates CurOperatState { private set; get; } = OperationStates.Idle;
            public FileSystemInfo CurOperatingItem;

            List<OperationInfo> transferingList = new List<OperationInfo>();
            List<OperatingErrorInfo> errorList = new List<OperatingErrorInfo>();

            public enum OperationTypes
            { None, Copy, Move, Delete, }
            public enum OperationStates
            {
                Idle, Busy, Canceling,
                //Paused,
            }

            private class OperationInfo
            {
                public FileSystemInfo source;
                public string target;
                public OperationTypes operationType;
            }
            public class OperatingErrorInfo
            {
                public FileSystemInfo ioItem;
                public OperationTypes operation;
                public Exception error;
            }

            public delegate void OperatingStartStopDelegate(bool isStartOrStop, bool isCanceled, List<OperatingErrorInfo> errorList);
            public event OperatingStartStopDelegate OperatingStartStop;
            public delegate void OperatingErrorDelegate(OperatingErrorInfo error);
            public event OperatingErrorDelegate OperatingError;
            private DateTime OperatingCurFileSystemItemPreTime = DateTime.MinValue;
            public delegate void OperatingCurFileSystemItemDelegate(FileSystemInfo ioItem, OperationTypes operation);
            public event OperatingCurFileSystemItemDelegate OperatingCurFileSystemItem;


            private delegate void TransferingReportDelegate(bool isStartOrStop, Exception err);
            private event TransferingReportDelegate TransferingReport;
            private void ReScanSources(string[] sources, string target, bool targetIsParentDir = true)
            {
                transferingList.Clear();
                if (!targetIsParentDir)
                {
                    if (Directory.Exists(sources[0]))
                    {
                        ReScanSources_Loop(new DirectoryInfo(sources[0]), Path.GetDirectoryName(target));
                    }
                    else
                    {
                        transferingList.Add(new OperationInfo()
                        {
                            source = new FileInfo(sources[0]),
                            target = target,
                            operationType = CurOperation,
                        });
                    }
                }
                else
                {
                    foreach (string source in sources)
                    {
                        if (Directory.Exists(source))
                        {
                            ReScanSources_Loop(new DirectoryInfo(source), target);
                        }
                        else
                        {
                            FileInfo sourceFI = new FileInfo(source);
                            transferingList.Add(new OperationInfo()
                            {
                                source = sourceFI,
                                target = target + Path.DirectorySeparatorChar + sourceFI.Name,
                                operationType = CurOperation,
                            });
                        }
                    }
                }
            }
            private void ReScanSources_Loop(DirectoryInfo dir, string targetParentDir)
            {
                transferingList.Add(new OperationInfo()
                {
                    source = dir,
                    target = targetParentDir + Path.DirectorySeparatorChar + dir.Name,
                    operationType = OperationTypes.Copy,
                });

                foreach (FileInfo fi in dir.GetFiles())
                {
                    transferingList.Add(new OperationInfo()
                    {
                        source = fi,
                        target = targetParentDir + Path.DirectorySeparatorChar + fi.Name,
                        operationType = CurOperation,
                    });
                }
                foreach (DirectoryInfo di in dir.GetDirectories())
                {
                    ReScanSources_Loop(di, targetParentDir + Path.DirectorySeparatorChar + dir.Name);
                }

                switch (CurOperation)
                {
                    case OperationTypes.Move:
                    case OperationTypes.Delete:
                        {
                            transferingList.Add(new OperationInfo()
                            {
                                source = dir,
                                //target = targetParentDir + Path.DirectorySeparatorChar + dir.Name,
                                operationType = OperationTypes.Delete,
                            });
                            break;
                        }
                }
            }
            private void TransferLoopAsync()
            {
                if (cancelFlag)
                {
                    CurOperatState = OperationStates.Idle;
                    OperatingStartStop?.Invoke(false, true, errorList);
                    return;
                }
                if (transferingList.Count == 0)
                {
                    OperatingStartStop?.Invoke(false, false, errorList);
                    return;
                }

                OperationInfo curIOItem = transferingList[0];
                transferingList.RemoveAt(0);
                CurOperatingItem = curIOItem.source;
                bool sourceIsDir = curIOItem.source.Attributes.HasFlag(FileAttributes.Directory);
                try
                {
                    OperatingCurFileSystemItem?.Invoke(curIOItem.source, CurOperation);
                    TransferingReport.Invoke(true, null);
                    switch (curIOItem.operationType)
                    {
                        case OperationTypes.Copy:
                            {
                                if (sourceIsDir)
                                {
                                    if (IsOverwrite)
                                    {
                                        if (File.Exists(curIOItem.target))
                                            File.Delete(curIOItem.target);
                                    }

                                    if (!Directory.Exists(curIOItem.target))
                                        Directory.CreateDirectory(curIOItem.target);
                                    if (IsOverwriteDirAttributes)
                                    {
                                        DirectoryInfo tarDir = new DirectoryInfo(curIOItem.target);
                                        tarDir.Attributes = curIOItem.source.Attributes;
                                    }
                                }
                                else
                                {
                                    if (IsOverwrite)
                                    {
                                        if (File.Exists(curIOItem.target))
                                            File.Delete(curIOItem.target);
                                        // else if(Directory.Exists(    ))
                                        // do not delete dir
                                    }
                                    ((FileInfo)curIOItem.source).CopyTo(curIOItem.target);
                                }
                                break;
                            }
                        case OperationTypes.Move:
                            {
                                // only file
                                if (IsOverwrite)
                                {
                                    if (File.Exists(curIOItem.target))
                                        File.Delete(curIOItem.target);
                                }
                                    ((FileInfo)curIOItem.source).MoveTo(curIOItem.target);
                                break;
                            }
                        case OperationTypes.Delete:
                            {
                                if (sourceIsDir)
                                    ((DirectoryInfo)curIOItem.source).Delete();
                                else
                                    ((FileInfo)curIOItem.source).Delete();
                                break;
                            }
                    }
                    TransferingReport.Invoke(false, null);
                }
                catch (Exception err)
                {
                    TransferingReport.Invoke(false, err);
                }
            }

            private void CSharpIOHelper_TransferingReport(bool isStartOrStop, Exception err)
            {
                if (isStartOrStop)
                {
                    switch (ProgressReportType)
                    {
                        case ProgressReportTypes.RealTime:
                            {
                                OperatingCurFileSystemItem?.Invoke(CurOperatingItem, CurOperation);
                                break;
                            }
                        case ProgressReportTypes.Interval:
                            {
                                if (OperatingCurFileSystemItem != null
                                    && (DateTime.Now - OperatingCurFileSystemItemPreTime).TotalSeconds
                                        >= ProgressReportIntervalSec)
                                {
                                    OperatingCurFileSystemItem.Invoke(CurOperatingItem, CurOperation);
                                    OperatingCurFileSystemItemPreTime = DateTime.Now;
                                }
                                break;
                            }
                    }
                }

                if (err != null)
                {
                    OperatingErrorInfo errInfo = new OperatingErrorInfo()
                    {
                        ioItem = CurOperatingItem,
                        operation = CurOperation,
                        error = err,
                    };
                    errorList.Add(errInfo);
                    OperatingError?.Invoke(errInfo);

                    if (IsErrorStop)
                    {
                        //transferingList.Clear();
                        CurOperatState = OperationStates.Idle;
                        OperatingStartStop.Invoke(false, false, errorList);
                        return;
                    }
                }
                if (!isStartOrStop && transferingList.Count > 0)
                {
                    TransferLoopAsync();
                }
            }



            private void CheckBusy()
            {
                if (CurOperatState != OperationStates.Idle)
                    throw new Exception("I'm busy or paused, try again later.");
            }
            public void CopyAsync(string source, string target)
            {
                CheckBusy();
                cancelFlag = false;
                errorList.Clear();
                CurOperation = OperationTypes.Copy;
                CurOperatState = OperationStates.Busy;
                ThreadPool.QueueUserWorkItem(new WaitCallback((s) =>
                {
                    OperatingStartStop?.Invoke(true, false, errorList);
                    ReScanSources(new string[] { source }, target, false);
                    TransferLoopAsync();
                }));
            }
            public void CopyAsync(string[] sources, string targetDir)
            {
                CheckBusy();
                cancelFlag = false;
                errorList.Clear();
                CurOperation = OperationTypes.Copy;
                CurOperatState = OperationStates.Busy;
                ThreadPool.QueueUserWorkItem(new WaitCallback((s) =>
                {
                    OperatingStartStop?.Invoke(true, false, errorList);
                    ReScanSources(sources, targetDir);
                    TransferLoopAsync();
                }));
            }
            public void MoveAsync(string source, string target)
            {
                CheckBusy();
                cancelFlag = false;
                errorList.Clear();
                CurOperation = OperationTypes.Move;
                CurOperatState = OperationStates.Busy;
                ThreadPool.QueueUserWorkItem(new WaitCallback((s) =>
                {
                    OperatingStartStop?.Invoke(true, false, errorList);
                    ReScanSources(new string[] { source }, target, false);
                    TransferLoopAsync();
                }));
            }
            public void MoveAsync(string[] sources, string targetDir)
            {
                CheckBusy();
                cancelFlag = false;
                errorList.Clear();
                CurOperation = OperationTypes.Move;
                CurOperatState = OperationStates.Busy;
                ThreadPool.QueueUserWorkItem(new WaitCallback((s) =>
                {
                    OperatingStartStop?.Invoke(true, false, errorList);
                    ReScanSources(sources, targetDir);
                    TransferLoopAsync();
                }));
            }
            public void DeleteAsync(string source)
            {
                CheckBusy();
                cancelFlag = false;
                errorList.Clear();
                CurOperation = OperationTypes.Delete;
                CurOperatState = OperationStates.Busy;
                ThreadPool.QueueUserWorkItem(new WaitCallback((s) =>
                {
                    OperatingStartStop?.Invoke(true, false, errorList);
                    ReScanSources(new string[] { source }, null);
                    TransferLoopAsync();
                }));
            }
            public void DeleteAsync(string[] sources)
            {
                CheckBusy();
                cancelFlag = false;
                errorList.Clear();
                CurOperation = OperationTypes.Delete;
                CurOperatState = OperationStates.Busy;
                ThreadPool.QueueUserWorkItem(new WaitCallback((s) =>
                {
                    OperatingStartStop?.Invoke(true, false, errorList);
                    ReScanSources(sources, null);
                    TransferLoopAsync();
                }));
            }

            //public void PauseResume()
            //{
            //    if (transferThread != null)
            //    {
            //        if (transferThread.ThreadState == ThreadState.Suspended
            //            || transferThread.ThreadState == ThreadState.SuspendRequested)
            //        {
            //            transferThread.Resume();
            //            CurOperatState = OperationStates.Busy;
            //        }
            //        else if (transferThread.ThreadState == ThreadState.Running
            //            || transferThread.ThreadState == ThreadState.Background)
            //        {
            //            transferThread.Suspend();
            //            CurOperatState = OperationStates.Paused;
            //        }
            //    }
            //}

            private bool cancelFlag = false;
            public void CancelAsync()
            {
                cancelFlag = true;
                CurOperatState = OperationStates.Canceling;
            }
        }

        /// <summary>
        /// ** not very reliable for chinese-named files n' dirs
        /// </summary>
        public static class Shell32SHFileOperation
        {
            /// <summary>
            /// 文件操作代理，该类提供类似于Windows的文件操作体验
            /// 原文来自 https://www.cnblogs.com/lxblog/archive/2012/11/13/2768096.html
            /// </summary>
            #region 【内部类型定义】
            private struct SHFILEOPSTRUCT
            {
                public IntPtr hwnd;         //父窗口句柄 
                public wFunc wFunc;         //要执行的动作 
                public string pFrom;        //源文件路径，可以是多个文件，以结尾符号"\0"结束
                public string pTo;          //目标路径，可以是路径或文件名 
                public FILEOP_FLAGS fFlags;             //标志，附加选项 
                public bool fAnyOperationsAborted;      //是否可被中断 
                public IntPtr hNameMappings;            //文件映射名字，可在其它 Shell 函数中使用 
                public string lpszProgressTitle;        // 只在 FOF_SIMPLEPROGRESS 时，指定对话框的标题。
            }

            private enum wFunc
            {
                FO_MOVE = 0x0001,   //移动文件
                FO_COPY = 0x0002,   //复制文件
                FO_DELETE = 0x0003, //删除文件，只是用pFrom
                FO_RENAME = 0x0004  //文件重命名
            }

            private enum FILEOP_FLAGS
            {
                FOF_MULTIDESTFILES = 0x0001,    //pTo 指定了多个目标文件，而不是单个目录
                FOF_CONFIRMMOUSE = 0x0002,
                FOF_SILENT = 0x0044,            // 不显示一个进度对话框
                FOF_RENAMEONCOLLISION = 0x0008, // 碰到有抵触的名字时，自动分配前缀
                FOF_NOCONFIRMATION = 0x10,      // 不对用户显示提示
                FOF_WANTMAPPINGHANDLE = 0x0020, // 填充 hNameMappings 字段，必须使用 SHFreeNameMappings 释放
                FOF_ALLOWUNDO = 0x40,           // 允许撤销
                FOF_FILESONLY = 0x0080,         // 使用 *.* 时, 只对文件操作
                FOF_SIMPLEPROGRESS = 0x0100,    // 简单进度条，意味者不显示文件名。
                FOF_NOCONFIRMMKDIR = 0x0200,    // 建新目录时不需要用户确定
                FOF_NOERRORUI = 0x0400,         // 不显示出错用户界面
                FOF_NOCOPYSECURITYATTRIBS = 0x0800,     // 不复制 NT 文件的安全属性
                FOF_NORECURSION = 0x1000        // 不递归目录
            }
            #endregion 【内部类型定义】

            #region 【DllImport】

            [DllImport("shell32.dll")]
            private static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

            [DllImport("shell32.dll")]
            private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint flags);


            #endregion 【DllImport】

            #region 【删除文件操作】

            /// <summary>
            /// 删除一组文件。
            /// </summary>
            /// <param name="fileNames">字符串数组，表示一组文件名</param>
            /// <param name="showDialog">指示是否显示确认对话框，true-显示确认删除对话框，false-不显示确认删除对话框</param>
            /// <param name="showProgress">指示是否显示进度对话框，true-显示，false-不显示。该参数当指定永久删除文件时有效</param>
            /// <param name="errorMsg">反馈错误消息的字符串</param>
            /// <returns></returns>
            public static bool DeleteFiles(string[] fileNames, out Exception err)
            {
                err = null;
                try
                {
                    string fName = "";
                    foreach (string str in fileNames)
                    {
                        fName += GetFullName(str) + "\0";     //组件文件组字符串
                    }

                    //  文件名， 是否放入回收站， 是否显示确认对话框， 是否显示进度对话框，返回错误信息
                    return ToDelete(fName, true, false, true, ref err) == 0;
                }
                catch (Exception ex)
                {
                    err = ex;
                    return false;
                }
            }
            #endregion 【删除文件操作】

            #region 【移动文件操作】

            /// <summary>
            /// 移动一组文件到指定的路径下
            /// </summary>
            /// <param name="sourceFileNames">要移动的文件名数组</param>
            /// <param name="destinationDirPath">移动到的目的路径</param>
            /// <param name="autoRename">指示当文件名重复时，是否自动为新文件加上后缀名</param>
            /// <param name="errorMsg">反馈错误消息的字符串</param>
            /// <returns></returns>
            public static bool MoveFiles(string[] sourceFileNames, string destinationDirPath, bool autoRename, out Exception err)
            {
                err = null;
                try
                {
                    string sfName = "";
                    foreach (string str in sourceFileNames)
                    {
                        sfName += GetFullName(str) + "\0";   //组件文件组字符串
                    }
                    string dfName = GetFullName(destinationDirPath);

                    return ToMoveOrCopy(wFunc.FO_MOVE, sfName, dfName, false, true, autoRename, ref err) == 0;
                }
                catch (Exception ex)
                {
                    err = ex;
                    return false;
                }
            }
            #endregion 【移动文件操作】

            #region 【复制文件操作】

            /// <summary>
            /// 复制一组文件到指定的路径
            /// </summary>
            /// <param name="sourceFileNames">要复制的文件名数组</param>
            /// <param name="destinationPath">if it's a dir, copy these to the dir;
            /// is source is only one file, and destination is not exist, then copy this file to the new name in destinationPath</param>
            /// <param name="autoRename">指示当文件名重复时，是否自动为新文件加上后缀名</param>
            /// <returns></returns>
            public static bool CopyFiles(string[] sourceFileNames, string destinationPath, bool autoRename, out Exception err)
            {
                err = null;
                try
                {
                    string sfName = "";
                    foreach (string str in sourceFileNames)
                    {
                        sfName += GetFullName(str) + "\0";     //组件文件组字符串
                    }
                    string dfName = GetFullName(destinationPath);

                    return ToMoveOrCopy(wFunc.FO_COPY, sfName, dfName, false, true, autoRename, ref err) == 0;
                }
                catch (Exception ex)
                {
                    err = ex;
                    return false;
                }
            }
            #endregion 【复制文件操作】

            #region 【重命名】

            /// <summary>
            /// 利用Microsoft.VisualBasic.FileSystem.ReName()方法实现
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="newName"></param>
            public static bool ReName(string fullPath, string newName, out Exception err)
            {
                err = null;
                try
                {
                    if (File.Exists(fullPath))
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(fullPath, newName);
                    }
                    else
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.RenameDirectory(fullPath, newName);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    err = ex;
                    return false;
                }
            }

            #endregion 【重命名】

            #region 获取文件属性信息

            public static SHFILEINFO GetFileInfo(string file)
            {
                SHFILEINFO shfi;
                if (IntPtr.Zero != SHGetFileInfo(
                    file,
                    FILE_ATTRIBUTE_NORMAL,
                    out shfi,
                    (uint)Marshal.SizeOf(typeof(SHFILEINFO)),
                    SHGFI_USEFILEATTRIBUTES | SHGFI_TYPENAME))
                {
                    return shfi;
                }
                return new SHFILEINFO();
            }
            public static string GetFileTypeDescription(string fileNameOrExtension)
            {
                SHFILEINFO shfi;
                if (IntPtr.Zero != SHGetFileInfo(
                    fileNameOrExtension,
                    FILE_ATTRIBUTE_NORMAL,
                    out shfi,
                    (uint)Marshal.SizeOf(typeof(SHFILEINFO)),
                    SHGFI_USEFILEATTRIBUTES | SHGFI_TYPENAME))
                {
                    return shfi.szTypeName;
                }
                return null;
            }


            [StructLayout(LayoutKind.Sequential)]
            public struct SHFILEINFO
            {
                public IntPtr hIcon;
                public int iIcon;
                public uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
                public string szTypeName;
            }

            private const uint FILE_ATTRIBUTE_READONLY = 0x00000001;
            private const uint FILE_ATTRIBUTE_HIDDEN = 0x00000002;
            private const uint FILE_ATTRIBUTE_SYSTEM = 0x00000004;
            private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
            private const uint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
            private const uint FILE_ATTRIBUTE_DEVICE = 0x00000040;
            private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
            private const uint FILE_ATTRIBUTE_TEMPORARY = 0x00000100;
            private const uint FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200;
            private const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400;
            private const uint FILE_ATTRIBUTE_COMPRESSED = 0x00000800;
            private const uint FILE_ATTRIBUTE_OFFLINE = 0x00001000;
            private const uint FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000;
            private const uint FILE_ATTRIBUTE_ENCRYPTED = 0x00004000;
            private const uint FILE_ATTRIBUTE_VIRTUAL = 0x00010000;

            private const uint SHGFI_ICON = 0x000000100;     // get icon
            private const uint SHGFI_DISPLAYNAME = 0x000000200;     // get display name
            private const uint SHGFI_TYPENAME = 0x000000400;     // get type name
            private const uint SHGFI_ATTRIBUTES = 0x000000800;     // get attributes
            private const uint SHGFI_ICONLOCATION = 0x000001000;     // get icon location
            private const uint SHGFI_EXETYPE = 0x000002000;     // return exe type
            private const uint SHGFI_SYSICONINDEX = 0x000004000;     // get system icon index
            private const uint SHGFI_LINKOVERLAY = 0x000008000;     // put a link overlay on icon
            private const uint SHGFI_SELECTED = 0x000010000;     // show icon in selected state
            private const uint SHGFI_ATTR_SPECIFIED = 0x000020000;     // get only specified attributes
            private const uint SHGFI_LARGEICON = 0x000000000;     // get large icon
            private const uint SHGFI_SMALLICON = 0x000000001;     // get small icon
            private const uint SHGFI_OPENICON = 0x000000002;     // get open icon
            private const uint SHGFI_SHELLICONSIZE = 0x000000004;     // get shell size icon
            private const uint SHGFI_PIDL = 0x000000008;     // pszPath is a pidl
            private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;     // use passed dwFileAttribute

            #endregion


            #region private methods
            /// <summary>
            /// 删除单个或多个文件
            /// </summary>
            /// <param name="fileName">删除的文件名，如果是多个文件，文件名之间以字符串结尾符'\0'隔开</param>
            /// <param name="toRecycle">指示是将文件放入回收站还是永久删除，true-放入回收站，false-永久删除</param>
            /// <param name="showDialog">指示是否显示确认对话框，true-显示确认删除对话框，false-不显示确认删除对话框</param>
            /// <param name="showProgress">指示是否显示进度对话框，true-显示，false-不显示。该参数当指定永久删除文件时有效</param>
            /// <param name="errorMsg">反馈错误消息的字符串</param>
            /// <returns>操作执行结果标识，删除文件成功返回0，否则，返回错误代码</returns>
            private static int ToDelete(string fileName, bool toRecycle, bool showDialog, bool showProgress, ref Exception err)
            {
                SHFILEOPSTRUCT lpFileOp = new SHFILEOPSTRUCT();
                lpFileOp.wFunc = wFunc.FO_DELETE;
                lpFileOp.pFrom = fileName + "\0";       //将文件名以结尾字符"\0"结束

                lpFileOp.fFlags = FILEOP_FLAGS.FOF_NOERRORUI;
                if (toRecycle)
                    lpFileOp.fFlags |= FILEOP_FLAGS.FOF_ALLOWUNDO;  //设定删除到回收站
                if (!showDialog)
                    lpFileOp.fFlags |= FILEOP_FLAGS.FOF_NOCONFIRMATION;     //设定不显示提示对话框
                if (!showProgress)
                    lpFileOp.fFlags |= FILEOP_FLAGS.FOF_SILENT;     //设定不显示进度对话框

                lpFileOp.fAnyOperationsAborted = true;

                int n = SHFileOperation(ref lpFileOp);
                if (n == 0)
                    return 0;

                string tmp = GetErrorString(n);

                //.av 文件正常删除了但也提示 402 错误，不知道为什么。屏蔽之。
                if ((fileName.ToLower().EndsWith(".av") && n.ToString("X") == "402"))
                    return 0;

                err = new Exception(string.Format("{0}({1})", tmp, GetShowableNames(fileName)));

                return n;
            }

            /// <summary>
            /// 移动或复制一个或多个文件到指定路径下
            /// </summary>
            /// <param name="flag">操作类型，是移动操作还是复制操作</param>
            /// <param name="sourceFileName">要移动或复制的文件名，如果是多个文件，文件名之间以字符串结尾符'\0'隔开</param>
            /// <param name="destinationFileName">移动到的目的位置</param>
            /// <param name="showDialog">指示是否显示确认对话框，true-显示确认对话框，false-不显示确认对话框</param>
            /// <param name="showProgress">指示是否显示进度对话框</param>
            /// <param name="autoRename">指示当文件名重复时，是否自动为新文件加上后缀名</param>
            /// <param name="errorMsg">反馈错误消息的字符串</param>
            /// <returns>返回移动操作是否成功的标识，成功返回0，失败返回错误代码</returns>
            private static int ToMoveOrCopy(wFunc flag, string sourceFileName, string destinationFileName, bool showDialog, bool showProgress, bool autoRename, ref Exception err)
            {
                SHFILEOPSTRUCT lpFileOp = new SHFILEOPSTRUCT();
                lpFileOp.wFunc = flag;
                lpFileOp.pFrom = sourceFileName + "\0";         //将文件名以结尾字符"\0\0"结束
                lpFileOp.pTo = destinationFileName + "\0\0";

                lpFileOp.fFlags = FILEOP_FLAGS.FOF_NOERRORUI;
                lpFileOp.fFlags |= FILEOP_FLAGS.FOF_NOCONFIRMMKDIR; //指定在需要时可以直接创建路径
                if (!showDialog)
                    lpFileOp.fFlags |= FILEOP_FLAGS.FOF_NOCONFIRMATION;     //设定不显示提示对话框
                if (!showProgress)
                    lpFileOp.fFlags |= FILEOP_FLAGS.FOF_SILENT;     //设定不显示进度对话框
                if (autoRename)
                    lpFileOp.fFlags |= FILEOP_FLAGS.FOF_RENAMEONCOLLISION;  //自动为重名文件添加名称后缀

                lpFileOp.fAnyOperationsAborted = true;

                int n = SHFileOperation(ref lpFileOp);
                if (n == 0)
                    return 0;

                string tmp = GetErrorString(n);

                err = new Exception(string.Format("{0}({1})", tmp, GetShowableNames(sourceFileName)));

                return n;
            }

            /// <summary>
            /// 获取一个文件的全名
            /// </summary>
            /// <param name="fileName">文件名</param>
            /// <returns>返回生成文件的完整路径名</returns>
            private static string GetFullName(string fileName)
            {
                FileInfo fi = new FileInfo(fileName);
                return fi.FullName;
            }
            private static string GetShowableNames(string fName)
            {
                while (fName.EndsWith("\0"))
                    fName = fName.Substring(0, fName.Length - 1);
                return fName.Replace("\0", " | ");
            }

            /// <summary>
            /// 解释错误代码
            /// </summary>
            /// <param name="n">代码号</param>
            /// <returns>返回关于错误代码的文字描述</returns>
            private static string GetErrorString(int n)
            {
                if (n == 0) return string.Empty;

                switch (n)
                {
                    case 2:
                        return "Can't find the file.";
                    case 7:
                        return "存储控制块被销毁。您是否选择的“取消”操作？";
                    case 113:
                        return "File already exists.";
                    case 115:
                        return "Renaming file must with same parent folder, and don't use relative path.";
                    case 117:
                        return "I/O控制错误";
                    case 123:
                        return "Duplicated names.";
                    case 116:
                        return "The source is a root directory, which cannot be moved or renamed.";
                    case 118:
                        return "Security settings denied access to the source.";
                    case 124:
                        return "The path in the source or destination or both was invalid.";
                    case 65536:
                        return "An unspecified error occurred on the destination.";
                    case 1026:
                        return "Source not exists.";
                    case 1223:
                        return "Canceled.";
                    default:
                        return "未识别的错误代码：" + n;
                }
            }
            #endregion

        }

        public static class MSVBFileOperation
        {
            public enum ExistingFileOperations
            { Rename, Overwrite, Skip, Ask, }
            public enum ExistingDirOperations
            { Rename, Combine, Skip, Ask, }
            public static void Copy(string[] items, string destinationDir, out Exception err,
                ExistingDirOperations existingDirOperation = ExistingDirOperations.Ask,
                ExistingFileOperations existingFileOperation = ExistingFileOperations.Ask)
            {
                err = null;
                if (items == null)
                    return;

                try
                {
                    string targetItemFullName;
                    bool sourceIsFile, existsFile, existsDir;
                    foreach (string i in items)
                    {
                        sourceIsFile = System.IO.File.Exists(i);
                        targetItemFullName = destinationDir + "\\" + i.Substring(i.LastIndexOf("\\") + 1);
                        existsFile = System.IO.File.Exists(targetItemFullName);
                        existsDir = Directory.Exists(targetItemFullName);

                        if (existsFile)
                        {
                            switch (existingFileOperation)
                            {
                                case ExistingFileOperations.Skip:
                                    continue;
                                case ExistingFileOperations.Ask:
                                    break;
                                case ExistingFileOperations.Overwrite:
                                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(targetItemFullName);
                                    break;
                                case ExistingFileOperations.Rename:
                                    targetItemFullName = destinationDir + "\\" + AutoNewName(targetItemFullName, true);
                                    break;
                            }
                            if (sourceIsFile)
                                Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(i, targetItemFullName,
                                    Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                    Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                            else
                                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(i, targetItemFullName,
                                    Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                    Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                        }
                        else if (existsDir)
                        {
                            switch (existingDirOperation)
                            {
                                case ExistingDirOperations.Skip:
                                    continue;
                                case ExistingDirOperations.Ask:
                                    if (sourceIsFile)
                                        Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(i, targetItemFullName,
                                            Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                            Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                                    else
                                        Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(i, targetItemFullName,
                                            Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                            Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                                    break;
                                case ExistingDirOperations.Combine:
                                    if (sourceIsFile)
                                    {
                                        targetItemFullName = destinationDir + "\\" + AutoNewName(targetItemFullName, false);
                                        Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(i, targetItemFullName,
                                            Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                            Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                                    }
                                    else
                                    {
                                        CombineDir(i, targetItemFullName, true, out err, existingFileOperation);
                                        if (err != null) return;
                                    }
                                    break;
                                case ExistingDirOperations.Rename:
                                    targetItemFullName = destinationDir + "\\" + AutoNewName(targetItemFullName, false);
                                    if (sourceIsFile)
                                        Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(i, targetItemFullName,
                                            Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                            Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                                    else
                                        Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(i, targetItemFullName,
                                            Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                            Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                                    break;
                            }
                        }
                        else
                        {
                            if (sourceIsFile)
                                Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(i, targetItemFullName,
                                    Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                    Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                            else
                                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(i, targetItemFullName,
                                    Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                    Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                        }
                    }
                }
                catch (Exception e)
                { err = e; }
            }

            private static void CombineDir(string dir1, string dir2, bool isCopyOrMove, out Exception err,
                ExistingFileOperations existingFileOperation)
            {
                err = null;
                DirectoryInfo sourceDir = new DirectoryInfo(dir1);
                if (!sourceDir.Exists) return;
                if (!Directory.Exists(dir2)) return;

                string[] subFiles = sourceDir.GetFiles().Select(a => a.FullName).ToArray();
                if (isCopyOrMove)
                    Copy(subFiles, dir2, out err, ExistingDirOperations.Combine, existingFileOperation);
                else
                    Move(subFiles, dir2, out err, ExistingDirOperations.Combine, existingFileOperation);

                if (err != null) return;

                string targetSubDir;
                foreach (DirectoryInfo subDir in sourceDir.GetDirectories())
                {
                    targetSubDir = dir2 + "\\" + subDir.Name;
                    if (Directory.Exists(targetSubDir))
                    {
                        CombineDir(subDir.FullName, targetSubDir, isCopyOrMove, out err, existingFileOperation);
                        if (err != null) return;
                    }
                    else
                    {
                        if (isCopyOrMove)
                            Copy(new string[] { subDir.FullName }, dir2, out err,
                                ExistingDirOperations.Combine, ExistingFileOperations.Rename);
                        else
                            Move(new string[] { subDir.FullName }, dir2, out err,
                                ExistingDirOperations.Combine, ExistingFileOperations.Rename);
                        if (err != null) return;
                    }
                }

                if (!isCopyOrMove)
                    Directory.Delete(dir1, true);
            }

            public static void Move(string[] items, string destinationDir, out Exception err,
                ExistingDirOperations existingDirOperation = ExistingDirOperations.Ask,
                ExistingFileOperations existingFileOperation = ExistingFileOperations.Ask)
            {
                err = null;
                if (items == null)
                    return;

                try
                {
                    string targetItemFullName;
                    bool sourceIsFile, existsFile, existsDir;
                    foreach (string i in items)
                    {
                        sourceIsFile = File.Exists(i);
                        targetItemFullName = destinationDir + "\\" + i.Substring(i.LastIndexOf("\\") + 1);
                        existsFile = File.Exists(targetItemFullName);
                        existsDir = Directory.Exists(targetItemFullName);

                        if (existsFile)
                        {
                            switch (existingFileOperation)
                            {
                                case ExistingFileOperations.Skip:
                                    continue;
                                case ExistingFileOperations.Ask:
                                    break;
                                case ExistingFileOperations.Overwrite:
                                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(targetItemFullName);
                                    break;
                                case ExistingFileOperations.Rename:
                                    targetItemFullName = AutoNewFullName(targetItemFullName, true);
                                    break;
                            }
                            if (sourceIsFile)
                                Microsoft.VisualBasic.FileIO.FileSystem.MoveFile(i, targetItemFullName,
                                    Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                    Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                            else
                                Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(i, targetItemFullName,
                                    Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                    Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                        }
                        else if (existsDir)
                        {
                            switch (existingDirOperation)
                            {
                                case ExistingDirOperations.Skip:
                                    continue;
                                case ExistingDirOperations.Ask:
                                    if (sourceIsFile)
                                        Microsoft.VisualBasic.FileIO.FileSystem.MoveFile(i, targetItemFullName,
                                            Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                            Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                                    else
                                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(i, targetItemFullName,
                                            Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                            Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                                    break;
                                case ExistingDirOperations.Combine:
                                    if (sourceIsFile)
                                    {
                                        targetItemFullName = destinationDir + "\\" + AutoNewName(targetItemFullName, false);
                                        Microsoft.VisualBasic.FileIO.FileSystem.MoveFile(i, targetItemFullName,
                                            Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                            Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                                    }
                                    else
                                    {
                                        CombineDir(i, targetItemFullName, false, out err, existingFileOperation);
                                        if (err != null) return;
                                    }
                                    break;
                                case ExistingDirOperations.Rename:
                                    targetItemFullName = destinationDir + "\\" + AutoNewName(targetItemFullName, false);
                                    if (sourceIsFile)
                                        Microsoft.VisualBasic.FileIO.FileSystem.MoveFile(i, targetItemFullName,
                                            Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                            Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                                    else
                                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(i, targetItemFullName,
                                            Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                            Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                                    break;
                            }
                        }
                        else
                        {
                            if (sourceIsFile)
                                Microsoft.VisualBasic.FileIO.FileSystem.MoveFile(i, targetItemFullName,
                                    Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                    Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                            else
                                Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(i, targetItemFullName,
                                    Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                    Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                        }
                    }
                }
                catch (Exception e)
                { err = e; }
            }

            public static void Rename(string item, string newName, out Exception err, bool autoNewName = true)
            {
                err = null;
                try
                {
                    string targetFullName = item.Substring(0, item.LastIndexOf("\\") + 1) + newName;
                    bool existFile = System.IO.File.Exists(targetFullName);
                    if (System.IO.File.Exists(item))
                    {
                        if (autoNewName)
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(item,
                                AutoNewName(targetFullName, existFile));
                        }
                        else
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(item, newName);
                        }
                    }
                    else
                    {
                        if (autoNewName)
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.RenameDirectory(item,
                                AutoNewName(targetFullName, existFile));
                        }
                        else
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.RenameDirectory(item, newName);
                        }
                    }
                }
                catch (Exception e)
                { err = e; }
            }
            /// <summary>
            /// 检查目标文件、文件夹是否存在，如果存在，则自动返回带有标号的新文件名、文件夹名；
            /// </summary>
            /// <param name="fullPath"></param>
            /// <param name="isExistingFileOrDir">测试路径是文件，或文件夹</param>
            /// <returns></returns>
            public static string AutoNewName(string fullPath, bool? isExistingFileOrDir)
            {
                while (fullPath.Contains("\\\\"))
                    fullPath = fullPath.Replace("\\\\", "\\");
                if (fullPath.EndsWith("\\"))
                    fullPath = fullPath.Remove(fullPath.Length - 1, 1);

                if (fullPath.StartsWith("\\"))
                    fullPath = "\\" + fullPath;

                int lastIdxOfS = fullPath.LastIndexOf("\\");
                string newName = fullPath.Substring(lastIdxOfS + 1);
                if (isExistingFileOrDir != null)
                {
                    string dirName = fullPath.Substring(0, lastIdxOfS);
                    int counter = 1;
                    if (isExistingFileOrDir == true)
                    {
                        string prefix, suffix = null;
                        if (newName.Contains("."))
                        {
                            int lastIdxOfDot = newName.LastIndexOf(".");
                            prefix = newName.Substring(0, lastIdxOfDot);
                            suffix = newName.Substring(lastIdxOfDot);
                        }
                        else
                        {
                            prefix = newName;
                        }
                        StringBuilder nameBdr = new StringBuilder();
                        nameBdr.Append(fullPath);
                        while (File.Exists(nameBdr.ToString()))
                        {
                            nameBdr.Clear();
                            nameBdr.Append(dirName);
                            nameBdr.Append(Path.DirectorySeparatorChar);
                            nameBdr.Append(prefix);
                            nameBdr.Append(" (");
                            nameBdr.Append(counter++);
                            nameBdr.Append(")");
                            nameBdr.Append(suffix);
                        }
                        newName = nameBdr.ToString().Substring(lastIdxOfS + 1);
                    }
                    else
                    {
                        string tmp = fullPath;
                        while (Directory.Exists(tmp))
                        {
                            tmp = fullPath + " (" + (counter++).ToString() + ")";
                        }
                        newName = tmp.Substring(lastIdxOfS + 1);
                    }
                }
                return newName;
            }
            public static string AutoNewFullName(string fullPath, bool? isExistingFileOrDir)
            {
                while (fullPath.Contains("\\\\"))
                    fullPath = fullPath.Replace("\\\\", "\\");
                if (fullPath.EndsWith("\\"))
                    fullPath = fullPath.Remove(fullPath.Length - 1, 1);

                if (fullPath.StartsWith("\\"))
                    fullPath = "\\" + fullPath;

                int lastIdxOfS = fullPath.LastIndexOf("\\");
                string dirName = fullPath.Substring(0, lastIdxOfS);
                string newName = AutoNewName(fullPath, isExistingFileOrDir);
                return dirName + Path.DirectorySeparatorChar + newName;
            }

            public static void Delete(string[] items, out Exception err, bool showProcessDialog = true)
            {
                err = null;
                try
                {
                    foreach (string i in items)
                    {
                        if (File.Exists(i))
                            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(i,
                                showProcessDialog ?
                                    Microsoft.VisualBasic.FileIO.UIOption.AllDialogs
                                    : Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin,
                                Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                        else
                            Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(i,
                                showProcessDialog ?
                                    Microsoft.VisualBasic.FileIO.UIOption.AllDialogs
                                    : Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin,
                                Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                    }
                }
                catch (Exception e)
                {
                    err = e;
                }
            }
        }

        public static class Checker
        {
            /// <summary>
            /// check name to see, if it can be used as a legal name;
            /// these will not pass:  c:\   aa\bb   aa/bb   as|   {abc}
            /// </summary>
            /// <param name="testName"></param>
            /// <returns></returns>
            public static bool CheckIOName(string testName)
            {
                if (CheckIOPath(testName) == false) return false;

                if (testName.Contains(Path.DirectorySeparatorChar)
                    || testName.Contains(Path.AltDirectorySeparatorChar))
                    return false;
                return true;
            }

            /// <summary>
            /// check name to see, if it can be used as a legal path;
            /// </summary>
            /// <param name="testPath"></param>
            /// <returns></returns>
            public static bool CheckIOPath(string testPath)
            {
                if (testPath == null) return false;
                testPath = testPath.Trim();
                if (testPath.Length == 0) return false;
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    if (testPath.Contains(c)) return false;
                }
                foreach (char c in Path.GetInvalidPathChars())
                {
                    if (testPath.Contains(c)) return false;
                }
                return true;
            }

            public static bool CheckIsHostPath(string testPath)
            {
                if (string.IsNullOrEmpty(testPath))
                    return false;
                return testPath.StartsWith("\\\\") && testPath.IndexOf("\\", 2) < 0;
            }
            public static bool CheckIsHostRootPath(string testPath)
            {
                if (string.IsNullOrEmpty(testPath))
                    return false;
                if (testPath.StartsWith("\\\\"))
                {
                    int idx = testPath.IndexOf("\\", 2);
                    if (idx < 0)
                        return false;
                    return testPath.IndexOf("\\", idx + 1) < 0;
                }
                return false;
            }
            public static bool CheckIsHostRootOrSubPath(string testPath)
            {
                if (string.IsNullOrEmpty(testPath))
                    return false;
                if (testPath.StartsWith("\\\\"))
                {
                    int idx = testPath.IndexOf("\\", 2);
                    return idx > 0;
                }
                return false;
            }

            public static bool CheckIsNumberStart(ref string testName, out string numStr, out int intLength)
            {
                numStr = null;
                intLength = 0;
                if (string.IsNullOrWhiteSpace(testName))
                    return false;
                int nIdx = -1, testNameLength = testName.Length;
                char testChar;
                bool haveDot = false;
                while (true)
                {
                    testChar = testName[++nIdx];
                    if (testChar == '.')
                    {
                        if (haveDot)
                        {
                            break;
                        }
                        else
                        {
                            haveDot = true;
                            continue;
                        }
                    }
                    if (nIdx >= testNameLength - 1)
                        break;

                    if ('0' <= testChar && testChar <= '9')
                    {
                        if (!haveDot)
                            ++intLength;
                    }
                    else
                    {
                        break;
                    }
                }
                if (nIdx > 0)
                {
                    numStr = testName.Substring(0, nIdx);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static class Other
        {
            public static string GetStartUp_DirPath()
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            }
            public static string GetCommonApplicationData_DirPath()
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            }
            public static bool StartUp_CheckLnkFileExists(string lnkFileName)
            {
                string fileFullname = GetStartUp_DirPath() + "\\" + lnkFileName;
                return System.IO.File.Exists(fileFullname);
            }
            public static bool StartUp_RemoveShortcut(string lnkFileName)
            {
                if (lnkFileName.ToLower().EndsWith(".lnk") == false)
                    throw new Exception("Source file name is not end with \".lnk\" (shortcut file).");

                string startupDirLinkFile
                    = Environment.GetFolderPath(Environment.SpecialFolder.Startup)
                    + "\\" + lnkFileName;

                if (File.Exists(startupDirLinkFile))
                {
                    File.Delete(startupDirLinkFile);
                    return true;
                }
                else return false;
            }
            public static bool StartUp_GenerateShortcut(string sourceExeFileFullName, string lnkFileName, string description = null)
            {
                try
                {
                    string linkFileFullName = Path.Combine(GetStartUp_DirPath(), lnkFileName);
                    if (string.IsNullOrWhiteSpace(description))
                        description = lnkFileName;

                    GenerateShortcut(sourceExeFileFullName, linkFileFullName, description);
                    return true;
                }
                catch (Exception) { return false; }
            }
            /// <summary>
            /// 创建快捷方式文件
            /// </summary>
            /// <param name="sourceFileOrDirFullName">要生成快捷方式的原始文件或文件夹的完整路径</param>
            /// <param name="lnkFileName">快捷方式文件的名称，不带路径</param>
            /// <param name="toDir">将要保存这个快捷方式的文件夹的完整路径</param>
            public static void GenerateShortcut(string sourceFileOrDirFullName, string lnkFileFullName, string description = null)
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(lnkFileFullName);
                shortcut.TargetPath = sourceFileOrDirFullName;
                shortcut.WorkingDirectory = Path.GetDirectoryName(sourceFileOrDirFullName);
                shortcut.WindowStyle = 1; // 1- normal window; ?? what's others??
                shortcut.Description = description;
                shortcut.IconLocation = sourceFileOrDirFullName;
                shortcut.Save();
            }
        }

        public static class ClipBoard
        {
            public static void SetFileDrags(bool isCopyOrCut, bool sortFileList, params string[] filesAndDirs)
            {
                //DataObject files2CB = new DataObject(DataFormats.FileDrop, filesAndDirs);
                //Clipboard.SetDataObject(files2CB, isCopyOrCut);

                Clipboard.Clear();

                DragDropEffects dropEffect = isCopyOrCut ? DragDropEffects.Copy : DragDropEffects.Move;
                DataObject data = new DataObject();
                StringCollection droplist = new StringCollection();
                if (sortFileList)
                {
                    FileNameSorter.Sort(ref filesAndDirs, true);
                }
                droplist.AddRange(filesAndDirs);
                data.SetFileDropList(droplist);
                data.SetData("Preferred Dropeffect", new MemoryStream(BitConverter.GetBytes((int)dropEffect)));
                Clipboard.SetDataObject(data);
            }
            public static string[] GetFileDrops(out DragDropEffects copyOrMove)
            {
                copyOrMove = DragDropEffects.None;
                if (!Clipboard.ContainsFileDropList())
                    return null;

                //None = 0,   
                //Copy = 5,  
                //Cut = 2
                //DataObject testObj = Clipboard.GetData(for);
                try
                {
                    object obj = Clipboard.GetData("Preferred DropEffect");
                    if (obj != null)
                    {
                        MemoryStream ms = (MemoryStream)obj;
                        BinaryReader br = new BinaryReader(ms);
                        copyOrMove = (DragDropEffects)br.ReadInt32();


                        //switch (br.ReadInt32())
                        //{
                        //    case 5:
                        //        copyOrMove = DragDropEffects.Copy;
                        //        break;
                        //    case 2:
                        //        copyOrMove = DragDropEffects.Move;
                        //        break;
                        //        //default:
                        //        //case 0:
                        //        //    copyOrMove = DragDropEffects.None;
                        //        //    break;
                        //}
                    }
                    return (string[])Clipboard.GetData(DataFormats.FileDrop);
                }
                catch (Exception)
                { return null; }
            }

            public static void Clear()
            {
                Clipboard.Clear();
            }
        }


        public static class FilePath
        {
            /// <summary>
            /// 清理路径中节点前后多余的空格，清理双重分隔符，清理前后多余分隔符（网络地址的前双分隔符会被清除）
            /// </summary>
            /// <param name="oldPath"></param>
            /// <returns></returns>
            public static string CorrectorPath(string oldPath)
            {
                string pathSeparator = Path.PathSeparator.ToString();
                string doublePS = pathSeparator + pathSeparator;
                string spacePS = " " + pathSeparator;
                string PSspace = pathSeparator + " ";

                // remove odd spaces
                while (oldPath.Contains(spacePS))
                    oldPath = oldPath.Replace(spacePS, pathSeparator);
                while (oldPath.Contains(PSspace))
                    oldPath = oldPath.Replace(PSspace, pathSeparator);

                // remove double separator
                while (oldPath.Contains(doublePS))
                    oldPath = oldPath.Replace(doublePS, pathSeparator);

                // remove start-separator and end-separator
                while (oldPath.StartsWith(pathSeparator))
                    oldPath = oldPath.Substring(1).Trim();
                while (oldPath.EndsWith(pathSeparator))
                    oldPath = oldPath.Substring(0, oldPath.Length - 1).Trim();

                return oldPath;
            }

            /// <summary>
            /// 清理文件名中的非法字符，并将出现的非法字符输出
            /// </summary>
            /// <param name="fileOrDirName">待检测的文件名</param>
            /// <param name="invalidChars">出现的非法字符</param>
            /// <returns>可用文件名（可能为空串）</returns>
            public static string CorrectorName(string fileOrDirName, out HashSet<char> invalidChars)
            {
                invalidChars = new HashSet<char>();
                string result = fileOrDirName;
                int charIdx = -1;

                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    while (true)
                    {
                        charIdx = result.IndexOf(c);
                        if (charIdx >= 0)
                        {
                            invalidChars.Add(c);
                            result = result.Remove(charIdx, 1);
                        }
                        else break;
                    }
                }
                return result;
            }

            public static string GetPathRelated(string fullPath, string basePath)
            {
                if (fullPath == null)
                    return null;
                if (string.IsNullOrWhiteSpace(fullPath))
                    return basePath;

                char? test = GetPathSeparator(fullPath);
                char splitChar = test == null ? Path.PathSeparator : (char)test;

                string[] fParts = fullPath.Split(new string[] { splitChar.ToString() }, StringSplitOptions.RemoveEmptyEntries);
                string[] bParts = basePath.Split(new string[] { splitChar.ToString() }, StringSplitOptions.RemoveEmptyEntries);

                StringBuilder strBdr = new StringBuilder();
                if (fParts.Length > 0 && bParts.Length > 0 && fParts[0] == bParts[0])
                {
                    int i = 1, ivf = fParts.Length, ivb = bParts.Length,
                               iv = Math.Max(fParts.Length, bParts.Length);
                    for (; i < iv; i++)
                    {
                        if (i < ivf && i < ivb)
                        {
                            if (fParts[i].ToLower() != bParts[i].ToLower())
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    int c = ivb - i;
                    while (c-- > 0)
                    {
                        strBdr.Insert(0, splitChar);
                        strBdr.Insert(0, "..");
                    }
                    for (; i < ivf; i++)
                    {
                        strBdr.Append(fParts[i]);
                        strBdr.Append(splitChar);
                    }
                    if (strBdr.Length > 0)
                        strBdr.Remove(strBdr.Length - 1, 1);
                    return strBdr.ToString();
                }
                else
                {
                    return fullPath;
                }
            }
            public static string GetPathAbsolute(string relatePath, string basePath)
            {
                if (relatePath == null)
                    return null;

                char? test = GetPathSeparator(basePath);
                char splitChar = test == null ? Path.PathSeparator : (char)test;

                string[] rParts = relatePath.Split(new string[] { splitChar.ToString() }, StringSplitOptions.RemoveEmptyEntries);
                string[] bParts = basePath.Split(new string[] { splitChar.ToString() }, StringSplitOptions.RemoveEmptyEntries);

                int backCount = 0;
                int ir = 0, iv = rParts.Length;
                for (; ir < iv; ir++)
                {
                    if (rParts[ir] == "..")
                        backCount++;
                    else
                        break;
                }
                StringBuilder strBdr = new StringBuilder();
                for (int i = 0, ibv = bParts.Length - backCount; i < ibv; i++)
                {
                    strBdr.Append(bParts[i]);
                    strBdr.Append(splitChar);
                }
                for (; ir < iv; ir++)
                {
                    strBdr.Append(rParts[ir]);
                    strBdr.Append(splitChar);
                }
                if (strBdr.Length > 0)
                    strBdr.Remove(strBdr.Length - 1, 1);
                return strBdr.ToString();
            }
            public static char? GetPathSeparator(string path)
            {
                if (path.Contains(Path.DirectorySeparatorChar))
                    return Path.DirectorySeparatorChar;
                else if (path.Contains(Path.AltDirectorySeparatorChar))
                    return Path.AltDirectorySeparatorChar;
                else return null;
            }


            /// <summary>
            /// 如输入地址为 \\host\c\dir ，则返回 host
            /// </summary>
            /// <param name="uncPath"></param>
            /// <returns></returns>
            public static string GetUNCHostName(string uncPath)
            {
                if (uncPath.StartsWith("\\\\"))
                {
                    int testI = uncPath.IndexOf("\\", 3);
                    if (testI < 0)
                    {
                        return uncPath.Substring(2);
                    }
                    else
                    {
                        return uncPath.Substring(2, testI - 2);
                    }
                }
                return null;
            }

            /// <summary>
            /// 获取共享路径的根目录，如 //host/C ； 
            /// </summary>
            /// <param name="uncPath"></param>
            /// <returns></returns>
            public static string GetUNCRoot(string uncPath)
            {
                if (uncPath.StartsWith("\\\\"))
                {
                    int testI = uncPath.IndexOf("\\", 3);
                    if (testI < 0)
                        return null;

                    testI = uncPath.IndexOf("\\", testI + 1);
                    if (testI > 0)
                        return uncPath.Substring(0, testI);
                    else
                        return uncPath;
                }
                return null;
            }

            /// <summary>
            /// 如输入地址为 \\host\c\dir ，则返回 c
            /// </summary>
            /// <param name="uncPath"></param>
            /// <returns></returns>
            public static string GetUNCRootName(string uncPath)
            {
                string root = GetUNCRoot(uncPath);
                if (root != null)
                    return root.Substring(root.LastIndexOf("\\") + 1);
                else
                    return null;
            }

            /// <summary>
            /// 获取路径的根，如 C: 或者 \\hsot\c
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string GetRoot(string path)
            {
                if (path.StartsWith("\\\\"))
                    return GetUNCRoot(path);
                else if (path.Length > 1 && path[1] == ':')
                    return path.Substring(0, 2);
                else
                    throw new Exception($"Unknow path:[{path}]");

                return null;
            }

            /// <summary>
            /// 例如地址为 \\host\c  ,则返回true
            /// </summary>
            /// <param name="fullName"></param>
            /// <returns></returns>
            public static bool CheckIsUngRoot(string fullName)
            {
                string root = GetUNCRoot(fullName);
                if (root != null)
                    return root == fullName;
                else
                    return false;
            }
        }
    }
}
