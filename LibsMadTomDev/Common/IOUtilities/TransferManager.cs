using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

namespace MadTomDev.Data
{
    public class TransferManager
    {
        private TransferManager() { }

        #region vars, and data define
        private static TransferManager instance;
        public static TransferManager GetInstance()
        {
            if (instance == null)
            {
                instance = new TransferManager();
            }
            return instance;
        }

        private static object lockObj = new object();
        private static long taskNo;
        public static long GetNextTaskNo()
        {
            lock (lockObj)
            {
                return ++taskNo;
            }
        }

        public enum TaskTypes
        {
            Copy, Move, Delete, CreateLink,
        }
        public enum SameNameDirHandleTypes
        {
            Rename, Combine, Skip, Ask,
        }
        public enum SameNameFileHandleTypes
        {
            Rename, Overwrite, Skip, Ask,
        }
        #endregion

        // 作业流程：
        // 首先扫描源；
        // 安找设定进性操作，有疑问的保存到待处理列表；
        // 最后，还剩待处理，按设定的操作，执行重命名、覆盖、跳过，或询问（批量询问）；

        public delegate void OnExceptionDelegate(TransferManager sender, Exception err);
        public event OnExceptionDelegate OnException;
        public event Action<TransferManager, string> RecycleBinGenerated;

        /// <summary>
        /// 新增任务
        /// 删除时，只需要sourceIOs；
        /// 创建连接时候，targetIOs为空时在源位置创建，单个目录路径时，在此路径创建，对应数量的全名，则创建对应连接文件；
        /// 复制时，targetIOs只能为1个目录地址；
        /// 移动时，targetIOs可以是1目标目录地址，或对应数量的新目标全名（如批量修改文件名时）；
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="sourceIOs"></param>
        /// <param name="targetIOs"></param>
        /// <param name="SameNameDirHandleType"></param>
        /// <param name="SameNameFileHandleType"></param>
        /// <param name="optArgs"></param>
        /// <returns></returns>
        public TransferTask TransferTaskAdd(TaskTypes taskType, string[] sourceIOs, string[] targetIOs,
            SameNameDirHandleTypes sameNameDirHandleType, SameNameFileHandleTypes sameNameFileHandleType, params object[] optArgs)
        {
            TransferTask newTask = new TransferTask(this, taskType, sourceIOs, targetIOs, sameNameDirHandleType, sameNameFileHandleType, optArgs);
            newTask.EventRaised += NewTask_EventRaised;
            taskList.Add(newTask);
            return newTask;
        }

        private void NewTask_EventRaised(TransferTask sender, TransferTask.Events e)
        {
            if (e == TransferTask.Events.Canceled || e == TransferTask.Events.Completed)
            {
                if (!isPausingAll)
                    TryStartTasksAsync();
            }
        }

        private object lockerObj = new object();
        public HashSet<string> flagListDiskReading = new HashSet<string>();
        public HashSet<string> flagListDiskWriting = new HashSet<string>();

        #region flag set, remove

        private bool FlagDiskReading(string path, long taskId, bool isReadingOrNot)
        {
            return FlagDiskReadingWriting(flagListDiskReading, taskId, path, isReadingOrNot);
        }
        private bool FlagDiskWriting(string path, long taskId, bool isWritingOrNot)
        {
            return FlagDiskReadingWriting(flagListDiskWriting, taskId, path, isWritingOrNot);
        }
        private bool FlagDiskReadingWriting(HashSet<string> flagList, long taskId, string path, bool flag)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            lock (lockerObj)
            {
                string key = GetPathRoot(path);
                if (key == null)
                    return false;

                key = taskId.ToString() + "," + key;
                if (flag)
                {
                    if (flagList.Contains(key))
                        return false;

                    flagList.Add(key);
                    return true;
                }
                else
                {
                    if (flagList.Contains(key))
                    {
                        flagList.Remove(key);
                        return true;
                    }
                    return false;
                }
            }
        }
        private string GetPathRoot(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            if (path.StartsWith("\\\\"))
            {
                string root = Utilities.FilePath.GetUNCRoot(path);
                if (string.IsNullOrEmpty(root))
                    return null;
                else
                    return root;
            }
            else if (path.Length >= 2)
                return path.Substring(0, 2);
            else
                return null;
        }

        #endregion

        public bool? CheckFlagDiskReading(string path)
        {
            string key = GetPathRoot(path);
            if (key == null)
                return null;
            return flagListDiskReading.Where(a => a.EndsWith("," + key)).FirstOrDefault() != null;
        }
        public bool? CheckFlagDiskWriting(string path)
        {
            string key = GetPathRoot(path);
            if (key == null)
                return null;
            return flagListDiskWriting.Where(a => a.EndsWith("," + key)).FirstOrDefault() != null;
        }

        private bool TryStartTasksAsync_working = false;

        /// <summary>
        /// 尝试立即执行可执行的任务；
        /// * 新增的作业会被立即尝试执行，当涉及磁盘正在读写时，则等待前面任务完成，磁盘读写释放后再继续；
        /// </summary>
        /// <param name="newTask"></param>
        public Task TryStartTasksAsync()
        {
            return Task.Run(() =>
            {
                if (TryStartTasksAsync_working)
                    return;
                TryStartTasksAsync_working = true;

                if (isPausingAll)
                {
                    // 恢复暂停的所有作业
                    ResumeAll();
                }

                // 按标记，按顺序，尝试启动作业；
                bool toExec;
                bool waitTillStarted;
                for (int i = 0; i < taskList.Count; ++i)
                {
                    try
                    {
                        TransferTask curTask = taskList[i];

                        if (curTask.HasStarted && curTask.flagPause != true)
                            continue;

                        toExec = false;
                        if (curTask.TaskType == TaskTypes.Delete || CheckIfSameDiskMove(curTask))
                        {
                            // 2023-02-27 如果时同盘移动(包括入回收站)，则立即执行；
                            // 彻底删除时，也是立即执行；
                            toExec = true;
                        }
                        else if (CheckIfDiskAvailable(curTask))
                        {
                            toExec = true;
                        }

                        if (toExec)
                        {
                            taskList.RemoveAt(i--);
                            waitTillStarted = true;
                            curTask.EventRaised += (s, e) =>
                            {
                                if (e == TransferTask.Events.Start
                                    || e == TransferTask.Events.Canceled
                                    || e == TransferTask.Events.Completed)
                                {
                                    waitTillStarted = false;
                                }
                                //else if (e == TransferTask.Events.Canceled
                                //    || e == TransferTask.Events.Completed)
                                //{
                                //    waitTillStarted = false;
                                //    TryStartTasksAsync();
                                //}
                            };
                            curTask.StartAsync();
                            while (waitTillStarted)
                            {
                                Task.Delay(10).Wait();
                            }
                        }
                    }
                    catch (Exception err)
                    { OnException?.Invoke(this, err); }
                }
                TryStartTasksAsync_working = false;
            });
        }
        private bool CheckIfDiskAvailable(TransferTask task)
        {
            string testSur = task.SourceIOs[0];
            if (CheckFlagDiskReading(testSur) == true)
                return false;
            if (CheckFlagDiskWriting(testSur) == true)
                return false;

            if (task.TargetIOs != null && task.TargetIOs.Length > 0)
            {
                string testTar = task.TargetIOs[0];
                bool isSurNet = testSur.StartsWith("\\\\"),
                    isTarNet = testTar.StartsWith("\\\\"),
                    isCheck = false;
                string tarNetRoot = null;
                if (isSurNet && isTarNet)
                {
                    tarNetRoot = Utilities.FilePath.GetUNCRoot(testTar);
                    isCheck = Utilities.FilePath.GetUNCRoot(testSur) != tarNetRoot;
                }
                else if (isSurNet == isTarNet)
                {
                    isCheck = testSur[0] != testTar[0];
                }
                if (isCheck)
                {
                    if (isTarNet)
                    {
                        if (CheckFlagDiskReading(tarNetRoot) == true)
                            return false;
                        if (CheckFlagDiskWriting(tarNetRoot) == true)
                            return false;
                    }
                    else
                    {
                        if (CheckFlagDiskReading(testTar) == true)
                            return false;
                        if (CheckFlagDiskWriting(testTar) == true)
                            return false;
                    }
                }
            }
            return true;
        }
        private bool CheckIfSameDiskMove(TransferTask task)
        {
            if (task.TaskType == TaskTypes.Move)
            {
                string testS = task.SourceIOs[0],
                    testT = task.TargetIOs[0];
                return GetPathRoot(testS) == GetPathRoot(testT);
            }

            return false;
        }


        private bool isPausingAll = false;
        public void PauseAll()
        {
            isPausingAll = true;
            foreach (TransferTask t in taskList)
            {
                t.PauseAsync();
            }
        }
        public void ResumeAll()
        {
            isPausingAll = false;
            foreach (TransferTask t in taskList)
            {
                if (t.flagPause == true)
                    t.flagPause = false;
            }
            TryStartTasksAsync();
        }

        public void CancelAll()
        {
            for (int i = 0, iv = taskList.Count; i < iv; ++i)
            {
                taskList[i].CancelAsync();
            }
            taskList.Clear();
            flagListDiskReading.Clear();
            flagListDiskWriting.Clear();
        }

        public List<TransferTask> taskList = new List<TransferTask>();

        public class TransferTask
        {
            public TransferManager parent;
            public long Id { private set; get; }
            public TaskTypes TaskType { private set; get; }
            public string[] SourceIOs { private set; get; }
            public string[] TargetIOs { private set; get; }
            public SameNameDirHandleTypes SameNameDirHandleType { private set; get; }
            public SameNameFileHandleTypes SameNameFileHandleType { private set; get; }

            public bool IsDelete_Permanent = false;

            /// <summary>
            /// 创建传输作业，创建后，请使用StartAsync启动作业；
            /// </summary>
            /// <param name="taskType">作业类型，复制、移动（重命名），删除，创建连接</param>
            /// <param name="sourceIOs">要操作的多个文件或文件夹</param>
            /// <param name="targetIOs">目标，看情况，
            /// 复制-目标文件夹，移动-1目标文件夹或对应数量的目标全名，删除-任意，
            /// 创建连接-空，在源所在位置创建，目标文件夹中创建，或对应数量的目标全面</param>
            /// <param name="sameNameDirHandleType">同名文件夹处理方法</param>
            /// <param name="sameNameFileHandleType">同名文件处理方法</param>
            /// <param name="optArgs">[1] bool 是否使用永久删除</param>
            public TransferTask(TransferManager parent, TaskTypes taskType, string[] sourceIOs, string[] targetIOs,
                SameNameDirHandleTypes sameNameDirHandleType, SameNameFileHandleTypes sameNameFileHandleType, params object[] optArgs)
            {
                this.parent = parent;
                Id = TransferManager.GetNextTaskNo();
                this.TaskType = taskType;
                this.SourceIOs = sourceIOs;
                this.TargetIOs = targetIOs;
                this.SameNameDirHandleType = sameNameDirHandleType;
                this.SameNameFileHandleType = sameNameFileHandleType;

                switch (taskType)
                {
                    case TaskTypes.Delete:
                        if (optArgs != null && optArgs.Length > 0 && optArgs[0] is bool)
                        {
                            IsDelete_Permanent = (bool)optArgs[0];
                        }
                        break;
                }
            }


            #region events
            public enum Events
            {
                Scanning, Start, Completed, Paused, Resumed, Canceled,
            }
            public delegate void EventRaisedDelegate(TransferTask sender, Events e);
            public event EventRaisedDelegate EventRaised;
            public class ProgressData
            {
                public IOInfoShadow ioInfo;
                public bool isSkipped = false;
                public long fileSizeTransfered = 0;
            }
            public delegate void ProgressedDelegate(TransferTask sender, ProgressData e);
            public event ProgressedDelegate Progressed;
            public delegate void ExceptionRaisedDelegate(TransferTask sender, Exception e);
            public event ExceptionRaisedDelegate ExceptionRaised;

            #endregion


            private Stack<FileToTransfer> filesWaiting = new Stack<FileToTransfer>();
            public class FileToTransfer
            {
                public IOInfoShadow source;
                public string targetFullPath;
            }
            public long filesCountTotal = 0;
            public long filesCountTransfered = 0;
            public long filesSizeTotal = 0;
            public long filesSizeTransfered = 0;
            public long exceptionsCount = 0;

            public class RestFilesData
            {
                public IOInfoShadow io;
                public string targetFullName;
                public string newName;
                public bool needTransfer;
                public string sur, tar;
                public bool deleteSource;
                public Exception err;
                public RestFilesHandleTypes restFilesHandleType;

                public RestFilesData()
                {
                }
                public RestFilesData(RestFilesData rest)
                {
                    this.io = rest.io;
                    this.targetFullName = rest.targetFullName;
                    this.newName = rest.newName;
                    this.sur = rest.sur;
                    this.tar = rest.tar;
                    this.deleteSource = rest.deleteSource;
                    this.err = rest.err;
                    this.restFilesHandleType = rest.restFilesHandleType;
                }
            }
            public enum RestFilesHandleTypes
            { Skip, Retry, Rename, Overwrite, combine, SelfDelete, }
            List<RestFilesData> filesWithError = new List<RestFilesData>();


            public bool HasStarted { private set; get; } = false;
            public bool StartAsync()
            {
                if (HasStarted) return false;

                HasStarted = true;

                Task.Factory.StartNew(() =>
                {
                    SetFlagReading(true);

                    Scan();

                    SetFlagWriting(true);

                    Transfer();
                    HandleRests();

                    SetFlagWriting(false);
                    SetFlagReading(false);

                    if (flagCancel)
                        EventRaised?.Invoke(this, Events.Canceled);
                    else
                        EventRaised?.Invoke(this, Events.Completed);
                });

                return true;
            }
            private void SetFlagReading(bool isReading)
            {
                parent.FlagDiskReading(SourceIOs[0], Id, isReading);
            }
            private void SetFlagWriting(bool isWriting)
            {
                bool selfWriting = TargetIOs == null || TargetIOs.Length <= 0;
                if (selfWriting)
                    parent.FlagDiskWriting(SourceIOs[0], Id, isWriting);
                else
                    parent.FlagDiskWriting(TargetIOs[0], Id, isWriting);
            }


            public bool needDataCopy { private set; get; } = false;
            private void Scan()
            {
                // 从底层开始扫描，逐渐深入，将扫描到的文件压入栈；
                // 也就是说出栈的必定是最底层的内容，最后出栈的是根内容；
                EventRaised?.Invoke(this, Events.Scanning);


                #region target count check

                int targetsCount = CheckTargetsCount();
                if (TaskType == TaskTypes.Copy)
                {
                    if (targetsCount <= 0)
                    {
                        ExceptionCantCopyFiles_NoTarget();
                        flagCancel = true;
                        return;
                    }
                    else if (SourceIOs.Length != targetsCount)
                    {
                        ExceptionCantCopyFiles_SourceTargetNotInPair();
                        flagCancel = true;
                        return;
                    }
                }
                else if (TaskType == TaskTypes.Move)
                {
                    if (targetsCount > 0)
                    {
                        if (SourceIOs.Length != targetsCount)
                        {
                            ExceptionCandMoveFiles_SourceTargetNotInPair();
                            flagCancel = true;
                            return;
                        }
                    }
                    else
                    {
                        ExceptionCantMoveFiles_NoTarget();
                        flagCancel = true;
                        return;
                    }
                }
                else if (TaskType == TaskTypes.CreateLink)
                {
                    if (targetsCount > 1 && SourceIOs.Length != targetsCount)
                    {
                        ExceptionCantCreateLinks_SourceTargetNotInPair();
                        flagCancel = true;
                        return;
                    }
                }

                int CheckTargetsCount()
                {
                    if (TargetIOs != null && TargetIOs.Length > 0)
                    {
                        return TargetIOs.Length;
                    }
                    return 0;
                }

                #endregion

                CheckSetNeedTransfer();

                if (needDataCopy)
                {
                    IOInfoShadow io;
                    string s, t;
                    // 2023 1006
                    // 因为使用了栈，所有要保证原有的顺序，则这里必须使用倒叙入栈，从而使出栈的顺序和原顺序相同；
                    for (int i = SourceIOs.Length - 1; i >= 0; --i)
                    {
                        s = SourceIOs[i];
                        t = TargetIOs[i];

                        IfPauseWait();
                        if (flagCancel) return;
                        if (File.Exists(s))
                        {
                            ScanPushStack(new IOInfoShadow(new FileInfo(s)), t);
                        }
                        else if (Directory.Exists(s))
                        {
                            ScanLoop(new DirectoryInfo(s), t);
                        }
                        else
                        {
                            ExceptionFileNotFound(s, false);
                        }
                    }
                }
                else
                {
                    filesCountTotal = SourceIOs.Length;
                    filesSizeTotal = 0;
                }
            }
            private void ScanLoop(DirectoryInfo parentDir, string targetParentDir)
            {
                IfPauseWait();
                if (flagCancel) return;
                ScanPushStack(new IOInfoShadow(parentDir), targetParentDir);
                IOInfoShadow io;
                foreach (FileInfo fi in parentDir.GetFiles())
                {
                    IfPauseWait();
                    if (flagCancel) return;
                    io = new IOInfoShadow(fi);
                    ScanPushStack(io, Path.Combine(targetParentDir, io.name));
                }
                foreach (DirectoryInfo di in parentDir.GetDirectories())
                {
                    IfPauseWait();
                    if (flagCancel) return;
                    ScanLoop(di, Path.Combine(targetParentDir, di.Name));
                }
            }
            private void ScanPushStack(IOInfoShadow io, string targetFullPath)
            {
                filesWaiting.Push(new FileToTransfer()
                { source = io, targetFullPath = targetFullPath, });
                ++filesCountTotal;
                if (io.wasFile)
                {
                    filesSizeTotal += io.length;
                }
                Progressed?.Invoke(this, new ProgressData() { ioInfo = io, });
            }

            private void IfPauseWait()
            {
                if (flagPause == true)
                    EventRaised?.Invoke(this, Events.Paused);
                while (flagPause == true)
                {
                    Task.Delay(20).Wait();
                    if (flagCancel) return;
                }
                EventRaised?.Invoke(this, Events.Resumed);
            }
            private void Transfer()
            {
                IfPauseWait();
                if (flagCancel) return;

                EventRaised?.Invoke(this, Events.Start);
                IOInfoShadow io;
                Exception err;
                switch (TaskType)
                {
                    case TaskTypes.Delete: // 目标，任意
                        _Delete(SourceIOs);
                        break;
                    case TaskTypes.CreateLink: // 目标，空，创建到同目录，目标路径，或对应数量的新全名                        
                        _CreateLink(SourceIOs, TargetIOs);
                        break;
                    case TaskTypes.Copy: // 目标 文件夹
                        for (int i = 0, iv = filesWaiting.Count; i < iv; ++i)
                        {
                            IfPauseWait();
                            if (flagCancel) break;
                            _Copy(filesWaiting.Pop());
                        }
                        break;
                    case TaskTypes.Move: // 目标文件夹， 或对应数量的新全名
                        {
                            if (needDataCopy)
                            {
                                for (int i = 0, iv = filesWaiting.Count; i < iv; ++i)
                                {
                                    IfPauseWait();
                                    if (flagCancel) break;
                                    _Copy(filesWaiting.Pop(), null, true);
                                }
                            }
                            else
                            {
                                string tarFileName;
                                for (int i = 0, iv = SourceIOs.Length; i < iv; ++i)
                                {
                                    IfPauseWait();
                                    if (flagCancel) break;

                                    tarFileName = TargetIOs[i];
                                    _MoveInSamePartition(SourceIOs[i], ref tarFileName);
                                }
                            }
                        }
                        break;
                }
            }
            private void _MoveInSamePartition(string sur, ref string tar)
            {
                IfPauseWait();
                if (flagCancel) return;

                // 已存在检查，错误报告
                try
                {
                    if (File.Exists(tar))
                    {
                        if (Directory.Exists(sur))
                        {
                            ExceptionFileAlreadyExists(tar);
                        }
                        else if (File.Exists(sur))
                        {
                            try
                            {
                                switch (SameNameFileHandleType)
                                {
                                    case SameNameFileHandleTypes.Skip:
                                        break;
                                    case SameNameFileHandleTypes.Overwrite:
                                        File.Delete(tar);
                                        File.Move(sur, tar);
                                        ++filesCountTransfered;
                                        Progressed?.Invoke(this, new ProgressData() { ioInfo = new IOInfoShadow(sur), });
                                        break;
                                    case SameNameFileHandleTypes.Rename:
                                        File.Move(sur, tar);
                                        ++filesCountTransfered;
                                        Progressed?.Invoke(this, new ProgressData() { ioInfo = new IOInfoShadow(sur), });
                                        break;
                                    case SameNameFileHandleTypes.Ask:
                                        filesWithError.Add(new RestFilesData()
                                        {
                                            io = new IOInfoShadow(sur),
                                            sur = sur,
                                            tar = tar,
                                            needTransfer = false,
                                        });
                                        break;
                                }
                            }
                            catch (Exception err)
                            {
                                ExceptionWithException(err);
                            }
                        }
                        else
                        {
                            ExceptionFileNotFound(sur);
                        }
                    }
                    else if (Directory.Exists(tar))
                    {
                        if (File.Exists(sur))
                        {
                            ExceptionDirectoryAlreadyExists(tar);
                        }
                        else if (Directory.Exists(sur))
                        {
                            try
                            {
                                switch (SameNameDirHandleType)
                                {
                                    case SameNameDirHandleTypes.Skip:
                                        break;
                                    case SameNameDirHandleTypes.Combine:
                                        _MoveInSamePartition_DirCombine(sur, tar);
                                        ++filesCountTransfered;
                                        Progressed?.Invoke(this, new ProgressData() { ioInfo = new IOInfoShadow(sur), });
                                        break;
                                    case SameNameDirHandleTypes.Rename:
                                        tar = Utilities.CSharpWapper.AutoNewFullName(tar);
                                        Directory.Move(sur, tar);
                                        break;
                                    case SameNameDirHandleTypes.Ask:
                                        filesWithError.Add(new RestFilesData()
                                        {
                                            io = new IOInfoShadow(sur),
                                            sur = sur,
                                            tar = tar,
                                            needTransfer = false,
                                        });
                                        break;
                                }
                            }
                            catch (Exception err)
                            {
                                ExceptionWithException(err);
                            }
                        }
                        else
                        {
                            ExceptionFileNotFound(sur);
                        }
                    }
                    else
                    {
                        if (File.Exists(sur))
                        {
                            File.Move(sur, tar);
                        }
                        else
                        {
                            Directory.Move(sur, tar);
                        }
                        ++filesCountTransfered;
                        Progressed(this, new ProgressData() { fileSizeTransfered = 0, ioInfo = new IOInfoShadow(tar), });
                    }
                }
                catch (Exception err)
                {
                    ExceptionWithException(err);
                }
            }
            private void _MoveInSamePartition_DirCombine(string surDir, string tarDir)
            {
                IfPauseWait();
                if (flagCancel) return;

                DirectoryInfo surDi = new DirectoryInfo(surDir);
                string tarFile, targetDir;
                foreach (FileInfo fi in surDi.GetFiles())
                {
                    IfPauseWait();
                    if (flagCancel) return;

                    tarFile = Path.Combine(tarDir, fi.Name);
                    try
                    {
                        if (Directory.Exists(tarFile))
                        {
                            ExceptionDirectoryAlreadyExists(tarFile);
                        }
                        if (File.Exists(tarFile))
                        {
                            switch (SameNameFileHandleType)
                            {
                                case SameNameFileHandleTypes.Skip:
                                    break;
                                case SameNameFileHandleTypes.Overwrite:
                                    File.Delete(fi.FullName);
                                    File.Move(fi.FullName, tarFile);
                                    ++filesCountTotal; ++filesCountTransfered;
                                    Progressed(this, new ProgressData() { fileSizeTransfered = 0, ioInfo = new IOInfoShadow(fi), });
                                    break;
                                case SameNameFileHandleTypes.Rename:
                                    tarFile = Utilities.CSharpWapper.AutoNewFullName(tarFile);
                                    File.Move(fi.FullName, tarFile);
                                    ++filesCountTotal; ++filesCountTransfered;
                                    Progressed(this, new ProgressData() { fileSizeTransfered = 0, ioInfo = new IOInfoShadow(fi), });
                                    break;
                                case SameNameFileHandleTypes.Ask:
                                    filesWithError.Add(new RestFilesData()
                                    {
                                        io = new IOInfoShadow(fi),
                                        sur = surDir,
                                        tar = tarDir,
                                        needTransfer = false,
                                    });
                                    break;
                            }
                        }
                        else
                        {
                            File.Move(fi.FullName, tarFile);
                            ++filesCountTotal; ++filesCountTransfered;
                            Progressed(this, new ProgressData() { fileSizeTransfered = 0, ioInfo = new IOInfoShadow(fi), });
                        }
                    }
                    catch (Exception err)
                    {
                        ExceptionWithException(err);
                    }
                }
                foreach (DirectoryInfo di in surDi.GetDirectories())
                {
                    IfPauseWait();
                    if (flagCancel) return;

                    targetDir = Path.Combine(tarDir, di.Name);
                    try
                    {
                        if (File.Exists(targetDir))
                        {
                            ExceptionFileAlreadyExists(targetDir);
                        }
                        else if (Directory.Exists(targetDir))
                        {
                            switch (SameNameDirHandleType)
                            {
                                case SameNameDirHandleTypes.Skip:
                                    break;
                                case SameNameDirHandleTypes.Combine:
                                    _MoveInSamePartition_DirCombine(di.FullName, targetDir);
                                    ++filesCountTotal; ++filesCountTransfered;
                                    Progressed(this, new ProgressData() { fileSizeTransfered = 0, ioInfo = new IOInfoShadow(di), });
                                    break;
                                case SameNameDirHandleTypes.Rename:
                                    targetDir = Utilities.CSharpWapper.AutoNewFullName(targetDir);
                                    Directory.Move(di.FullName, targetDir);
                                    ++filesCountTotal; ++filesCountTransfered;
                                    Progressed(this, new ProgressData() { fileSizeTransfered = 0, ioInfo = new IOInfoShadow(di), });
                                    break;
                                case SameNameDirHandleTypes.Ask:
                                    filesWithError.Add(new RestFilesData()
                                    {
                                        io = new IOInfoShadow(di),
                                        sur = surDir,
                                        tar = tarDir,
                                        needTransfer = false,
                                    });
                                    break;
                            }
                        }
                        else
                        {
                            Directory.Move(di.FullName, targetDir);
                            ++filesCountTotal; ++filesCountTransfered;
                            Progressed(this, new ProgressData() { fileSizeTransfered = 0, ioInfo = new IOInfoShadow(di), });
                        }
                    }
                    catch (Exception err)
                    {
                        ExceptionWithException(err);
                    }
                }
                try
                {
                    if (!surDi.GetFileSystemInfos().Any())
                    {
                        Directory.Delete(surDi.FullName);
                    }
                }
                catch (Exception err)
                {
                    ExceptionWithException(err);
                }
            }
            private void _Delete(params string[] filesToDelete)
            {
                IOInfoShadow io;
                Exception err;
                foreach (string s in filesToDelete)
                {
                    if (IOInfoShadow.TryNewIOInfoShadow(s, out io))
                    {
                        err = null;
                        string binDir = null;
                        bool binDirExist = true;
                        if (IsDelete_Permanent)
                        {
                            try
                            {
                                if (io.wasFile)
                                {
                                    File.Delete(io.fullName);
                                }
                                else
                                {
                                    Directory.Delete(io.fullName, true);
                                }
                            }
                            catch (Exception e1)
                            {
                                err = e1;
                            }
                        }
                        else
                        {
                            //Utilities.MSVBFileOperation.Delete(new string[] { s }, out err, false);
                            binDir = Path.Combine(Path.GetPathRoot(s), Utilities.CSharpWapper.RecycleBinName);
                            binDirExist = Directory.Exists(binDir);
                            err = Utilities.CSharpWapper.Recycle(s);
                        }
                        if (err != null)
                        {
                            if (SameNameFileHandleType == SameNameFileHandleTypes.Ask)
                            {
                                filesWithError.Add(new RestFilesData()
                                {
                                    io = io,
                                    err = err,
                                    needTransfer = false,
                                    restFilesHandleType = RestFilesHandleTypes.Retry,
                                });
                            }
                            else
                            {
                                ExceptionWithException(err);
                            }
                        }
                        else
                        {
                            if (!binDirExist)
                            {
                                this.parent.RecycleBinGenerated?.Invoke(this.parent, binDir);
                            }
                            Progressed?.Invoke(this, new ProgressData() { ioInfo = io, });
                            ++filesCountTransfered;
                        }
                    }
                    else
                    {
                        ExceptionFileNotFound(s);
                    }

                    IfPauseWait();
                    if (flagCancel) break;
                }
            }
            private void _CreateLink(string[] sourceArr, string[] targetArr)
            {
                if (targetArr == null || targetArr.Length == 0)
                {
                    // all to same folder
                    foreach (string s in sourceArr)
                    {
                        IfPauseWait();
                        if (flagCancel) break;

                        _CreateLink_inner(s, s + ".lnk");
                    }
                }
                else if (targetArr.Length == 1 && Directory.Exists(targetArr[0]))
                {
                    // all to target folder
                    string s, t = targetArr[0];
                    for (int i = 0, iv = sourceArr.Length; i < iv; ++i)
                    {
                        IfPauseWait();
                        if (flagCancel) break;

                        s = SourceIOs[i];
                        _CreateLink_inner(s, Path.Combine(t, Path.GetFileName(s) + ".lnk"));
                    }
                }
                else
                {
                    // many to many
                    string s, t;
                    for (int i = 0, iv = sourceArr.Length; i < iv; ++i)
                    {
                        IfPauseWait();
                        if (flagCancel) break;

                        s = sourceArr[i];
                        t = targetArr[i];
                        _CreateLink_inner(s, t);
                    }
                }
            }
            private void _CreateLink_inner(string s, string t)
            {
                if (Directory.Exists(t))
                {
                    ExceptionDirectoryAlreadyExists(t);
                }
                else
                {
                    IOInfoShadow io;
                    if (IOInfoShadow.TryNewIOInfoShadow(s, out io))
                    {
                        try
                        {
                            if (File.Exists(t))
                            {
                                switch (SameNameFileHandleType)
                                {
                                    case SameNameFileHandleTypes.Skip:
                                        break;
                                    case SameNameFileHandleTypes.Rename:
                                        Utilities.Other.GenerateShortcut(s, Utilities.CSharpWapper.AutoNewFullName(t));
                                        ++filesCountTransfered;
                                        break;
                                    case SameNameFileHandleTypes.Overwrite:
                                        try
                                        {
                                            File.Delete(t);
                                            Utilities.Other.GenerateShortcut(s, t);
                                            ++filesCountTransfered;
                                        }
                                        catch (Exception err)
                                        {
                                            ExceptionWithException(err);
                                        }
                                        break;
                                    case SameNameFileHandleTypes.Ask:
                                        filesWithError.Add(new RestFilesData()
                                        {
                                            io = io,
                                            targetFullName = t,
                                            needTransfer = false,
                                        });
                                        break;
                                }
                            }
                            else
                            {
                                Utilities.Other.GenerateShortcut(s, t);
                                ++filesCountTransfered;
                            }
                        }
                        catch (Exception err1)
                        {
                            ExceptionWithException(err1);
                        }
                    }
                    else
                    {
                        ExceptionFileNotFound(s);
                    }
                }
            }
            private void _Copy(FileToTransfer f2t, string newName = null, bool deleteSource = false)
            {
                string tarDir = Path.GetDirectoryName(f2t.targetFullPath);
                Exception err;
                string newTarFile = f2t.targetFullPath;
                if (_TryCreateFolderToPath(ref tarDir, out err))
                {
                    if (f2t.source.wasFile)
                    {
                        // source is file
                        if (Directory.Exists(f2t.targetFullPath))
                        {
                            ExceptionDirectoryAlreadyExists(f2t.targetFullPath);
                        }
                        else
                        {
                            bool doCopy = true;
                            bool isOverWrite = false;
                            bool isSkipped = false;
                            bool doDel = true;
                            bool doReport = true;
                            if (File.Exists(f2t.targetFullPath))
                            {
                                switch (SameNameFileHandleType)
                                {
                                    case SameNameFileHandleTypes.Skip:
                                        doCopy = false;
                                        doDel = false;
                                        isSkipped = true;
                                        break;
                                    case SameNameFileHandleTypes.Overwrite:
                                        isOverWrite = true;
                                        break;
                                    case SameNameFileHandleTypes.Rename:
                                        newTarFile = Utilities.CSharpWapper.AutoNewFullName(f2t.targetFullPath);
                                        break;
                                    case SameNameFileHandleTypes.Ask:
                                        filesWithError.Add(new RestFilesData()
                                        {
                                            io = f2t.source,
                                            targetFullName = f2t.targetFullPath,
                                            newName = newName,
                                            needTransfer = true,
                                            deleteSource = deleteSource,
                                        });
                                        doCopy = false;
                                        doDel = false;
                                        doReport = false;
                                        break;
                                }
                            }
                            long fileTransfered = 0;
                            if (doCopy)
                            {
                                int bufferLength = 4096000;
                                byte[] buffer = new byte[bufferLength];
                                int readLength;
                                try
                                {
                                    using (FileStream ss = new FileStream(f2t.source.fullName, FileMode.Open))
                                    {
                                        using (FileStream ts = new FileStream(newTarFile, isOverWrite ? FileMode.Create : FileMode.CreateNew))
                                        {
                                            do
                                            {
                                                IfPauseWait();
                                                if (flagCancel) break;

                                                readLength = ss.Read(buffer, 0, bufferLength);
                                                ts.Write(buffer, 0, readLength);

                                                fileTransfered += readLength;
                                                filesSizeTransfered += readLength;
                                                if (readLength == bufferLength)
                                                {
                                                    Progressed(this, new ProgressData() { fileSizeTransfered = fileTransfered, ioInfo = f2t.source, });
                                                }
                                            }
                                            while (readLength > 0);
                                            if (flagCancel)
                                            {
                                                ts.Close();
                                            }
                                            else
                                            {
                                                ts.Flush();
                                            }
                                        }
                                        if (flagCancel)
                                        {
                                            doDel = false;
                                            File.Delete(newTarFile);
                                        }
                                        else
                                        {
                                            // 2024 0228 复制：将文件的修改时间，调整为和原文件一致；移动：创建时间也改为原时间；
                                            FileInfo newFI = new FileInfo(newTarFile);
                                            FileInfo oriFI = new FileInfo(f2t.source.fullName);
                                            newFI.LastWriteTime = oriFI.LastWriteTime;
                                            if (deleteSource)
                                            {
                                                // move
                                                newFI.CreationTime = oriFI.CreationTime;
                                            }
                                        }
                                    }
                                    if (doReport && !doDel)
                                    {
                                        ++filesCountTransfered;
                                        Progressed(this, new ProgressData() { fileSizeTransfered = fileTransfered, ioInfo = f2t.source, });
                                        doReport = false;
                                    }
                                    if (doDel && deleteSource)
                                    {
                                        File.Delete(f2t.source.fullName);
                                    }
                                    if (doReport)
                                    {
                                        ++filesCountTransfered;
                                        Progressed(this, new ProgressData()
                                        {
                                            fileSizeTransfered = fileTransfered,
                                            ioInfo = f2t.source,
                                            isSkipped = isSkipped,
                                        });
                                    }
                                }
                                catch (Exception e1)
                                {
                                    ExceptionWithException(e1);
                                }
                            }
                        }
                    }
                    else
                    {
                        // source if dir
                        if (File.Exists(f2t.targetFullPath))
                        {
                            ExceptionFileAlreadyExists(f2t.targetFullPath);
                        }
                        else
                        {
                            bool doCopy = true;
                            bool doDelNReport = true;
                            if (Directory.Exists(f2t.targetFullPath))
                            {
                                switch (SameNameDirHandleType)
                                {
                                    case SameNameDirHandleTypes.Skip:
                                    case SameNameDirHandleTypes.Combine:
                                        doCopy = false;
                                        break;
                                    case SameNameDirHandleTypes.Rename:
                                        newTarFile = Utilities.CSharpWapper.AutoNewFullName(f2t.targetFullPath);
                                        break;
                                    case SameNameDirHandleTypes.Ask:
                                        filesWithError.Add(new RestFilesData()
                                        {
                                            io = f2t.source,
                                            targetFullName = f2t.targetFullPath,
                                            newName = newName,
                                            needTransfer = true,
                                            deleteSource = deleteSource,
                                        });
                                        doCopy = false;
                                        doDelNReport = false;
                                        break;
                                }
                            }

                            if (doCopy)
                            {
                                DirectoryInfo tarDi = Directory.CreateDirectory(newTarFile);
                                tarDi.SetAccessControl(new DirectoryInfo(f2t.source.fullName).GetAccessControl());
                            }
                            if (doDelNReport)
                            {
                                if (deleteSource)
                                {
                                    try
                                    {
                                        Directory.Delete(f2t.source.fullName);
                                    }
                                    catch (Exception err1)
                                    {
                                        ExceptionWithException(new Exception("Can't delete source folder (after copying).", err1));
                                    }
                                }
                                ++filesCountTransfered;
                                Progressed(this, new ProgressData() { fileSizeTransfered = 0, ioInfo = f2t.source, });
                            }
                        }
                    }
                }
                else
                {
                    if (flagCancel) return;
                    ExceptionWithException(err);
                }
            }

            bool _TryCreateFolderToPath(ref string tarDir, out Exception err)
            {
                err = null;
                IfPauseWait();
                if (flagCancel) return false;

                if (Directory.Exists(tarDir))
                {
                    return true;
                }
                else
                {
                    // search backward
                    Stack<string> dirList = new Stack<string>();
                    dirList.Push(tarDir);

                    string subDir = Path.GetDirectoryName(tarDir);
                    while (!Directory.Exists(subDir))
                    {
                        if (File.Exists(subDir))
                        {
                            err = new IOException($"A file with name {subDir} has already existed.");
                            return false;
                        }
                        else
                        {
                            dirList.Push(subDir);
                            subDir = Path.GetDirectoryName(subDir);
                        }
                    }

                    // create dir forward
                    try
                    {
                        string d;
                        for (int i = 0, iv = dirList.Count; i < iv; ++i)
                        {
                            d = dirList.Pop();
                            Directory.CreateDirectory(d);
                        }
                        return true;
                    }
                    catch (Exception err1)
                    {
                        err = err1;
                        return false;
                    }
                }
            }

            #region exception raise, count
            private void ExceptionCantCopyFiles_NoTarget()
            {
                ExceptionRaised?.Invoke(this, new FileNotFoundException("Can't copy files, target path not set."));
                ++exceptionsCount;
            }
            private void ExceptionCantMoveFiles_NoTarget()
            {
                ExceptionRaised?.Invoke(this, new FileNotFoundException("Can't move files, target path not set."));
                ++exceptionsCount;
            }
            private void ExceptionCantCopy_MultipleTargets()
            {
                ExceptionRaised?.Invoke(this, new FileNotFoundException("Can't copy to multiple places in one go."));
                ++exceptionsCount;
            }
            private void ExceptionCantCopyFiles_SourceTargetNotInPair()
            {
                ExceptionRaised?.Invoke(this, new FileNotFoundException("Can't copy files, source-target not in pairs."));
                ++exceptionsCount;
            }
            private void ExceptionCandMoveFiles_SourceTargetNotInPair()
            {
                ExceptionRaised?.Invoke(this, new FileNotFoundException("Can't move files, source-target not in pairs."));
                ++exceptionsCount;
            }
            private void ExceptionFileNotFound(string fileOrDir, bool isCount = true)
            {
                ExceptionRaised?.Invoke(this, new FileNotFoundException("Can't find file or folder.", fileOrDir));
                if (isCount) ++exceptionsCount;
            }
            private void ExceptionDirectoryAlreadyExists(string dir)
            {
                ExceptionRaised?.Invoke(this, new IOException($"A folder with same name has already existed.{Environment.NewLine + dir}"));
                ++exceptionsCount;
            }
            private void ExceptionFileAlreadyExists(string file)
            {
                ExceptionRaised?.Invoke(this, new IOException($"A file with same name has already existed.{Environment.NewLine + file}"));
                ++exceptionsCount;
            }
            private void ExceptionCantCreateLinks_SourceTargetNotInPair()
            {
                ExceptionRaised?.Invoke(this, new FileNotFoundException("Can't create links, source-target not in pairs."));
                ++exceptionsCount;
            }
            private void ExceptionOperationNotSupported_MoveManyToMany_inOtherPartition()
            {
                ExceptionRaised?.Invoke(this, new InvalidOperationException("Can't move multiple files to another partition with different names."));
                ++exceptionsCount;
            }
            private void ExceptionCantMove_SourceTargetNotInPair()
            {
                ExceptionRaised?.Invoke(this, new FileNotFoundException("Can't move files, source-target not in pairs."));
                ++exceptionsCount;
            }
            private void ExceptionWithException(Exception err)
            {
                ExceptionRaised?.Invoke(this, err);
                ++exceptionsCount;
            }
            #endregion

            public delegate void HandleRestNeededDelegate(TransferTask sender, List<RestFilesData> filessNeedToHandle);

            /// <summary>
            /// 当启用重名询问，或出现失败时，在传输最后会发出此事件并进入暂停状态；
            /// 请检查filesWithError中的补充共的项目，并设定处理方法，随后取消暂停即可继续；
            /// </summary>
            public event HandleRestNeededDelegate HandleRestNeeded;
            private void HandleRests()
            {
                if (filesWithError.Count > 0)
                {
                    HandleRestNeeded?.Invoke(this, filesWithError);

                    flagPause = true;
                    IfPauseWait();

                    List<RestFilesData> tmpFWE = new List<RestFilesData>();
                    tmpFWE.AddRange(filesWithError);
                    filesWithError.Clear();
                    switch (TaskType)
                    {
                        case TaskTypes.Delete:// retry         skip
                            _RestDelete(tmpFWE);
                            break;
                        case TaskTypes.CreateLink: // retry  rename  skip
                            _restCreateLink(tmpFWE);
                            break;
                        case TaskTypes.Copy: // retry  rename  skip
                            _restCopy(tmpFWE);
                            break;
                        case TaskTypes.Move: // retry  rename  skip
                            _restMove(tmpFWE);
                            break;
                    }

                    if (filesWithError.Count > 0)
                        HandleRests();
                }
            }
            private void _RestDelete(List<RestFilesData> filesToHandle)
            {
                IOInfoShadow io;
                Exception err;
                RestFilesData s;
                for (int i = 0, iv = filesToHandle.Count; i < iv; ++i)
                {
                    s = filesToHandle[i];
                    if (s.restFilesHandleType != RestFilesHandleTypes.Skip)
                    {
                        if (IOInfoShadow.TryNewIOInfoShadow(s.io.fullName, out io))
                        {
                            //Utilities.MSVBFileOperation.Delete(new string[] { s.io.fullName }, out err, false);
                            string binDir = Path.Combine(Path.GetPathRoot(s.io.fullName), Utilities.CSharpWapper.RecycleBinName);
                            bool binDirExist = Directory.Exists(binDir);
                            err = Utilities.CSharpWapper.Recycle(s.io.fullName);
                            if (err != null)
                            {
                                if (SameNameFileHandleType == SameNameFileHandleTypes.Ask)
                                {
                                    filesWithError.Add(new RestFilesData()
                                    {
                                        io = io,
                                        err = err,
                                        needTransfer = false,
                                        restFilesHandleType = RestFilesHandleTypes.Skip,
                                    });
                                }
                                else
                                {
                                    ExceptionWithException(err);
                                }
                            }
                            else
                            {
                                if (!binDirExist)
                                {
                                    this.parent.RecycleBinGenerated?.Invoke(this.parent, binDir);
                                }
                                Progressed?.Invoke(this, new ProgressData() { ioInfo = io, });
                                ++filesCountTransfered;
                            }
                        }
                        else
                        {
                            ExceptionFileNotFound(s.io.fullName);
                        }

                        IfPauseWait();
                        if (flagCancel) break;
                    }
                    filesToHandle.RemoveAt(0);
                    --i; --iv;
                }
            }
            private void _restCreateLink(List<RestFilesData> filesToHandle)
            {
                RestFilesData rest;
                string s, t;
                IOInfoShadow io;
                for (int i = 0, iv = filesToHandle.Count; i < iv; ++i)
                {
                    rest = filesToHandle[i];
                    s = rest.io.fullName;
                    t = rest.targetFullName;
                    if (Directory.Exists(s))
                    {
                        ExceptionDirectoryAlreadyExists(rest.targetFullName);
                    }
                    else
                    {
                        if (IOInfoShadow.TryNewIOInfoShadow(s, out io))
                        {
                            try
                            {
                                if (File.Exists(t))
                                {
                                    switch (rest.restFilesHandleType)
                                    {
                                        default:
                                        case RestFilesHandleTypes.Skip:
                                            break;
                                        case RestFilesHandleTypes.Rename:
                                            Utilities.Other.GenerateShortcut(s, Utilities.CSharpWapper.AutoNewFullName(t));
                                            ++filesCountTransfered;
                                            break;
                                        case RestFilesHandleTypes.Overwrite:
                                            try
                                            {
                                                File.Delete(t);
                                                Utilities.Other.GenerateShortcut(s, t);
                                                ++filesCountTransfered;
                                            }
                                            catch (Exception e1)
                                            {
                                                ExceptionWithException(e1);
                                            }
                                            break;
                                        case RestFilesHandleTypes.Retry:
                                            try
                                            {
                                                Utilities.Other.GenerateShortcut(s, t);
                                                ++filesCountTransfered;
                                            }
                                            catch (Exception e2)
                                            {
                                                filesWithError.Add(new RestFilesData()
                                                {
                                                    io = io,
                                                    err = e2,
                                                    needTransfer = false,
                                                    targetFullName = t,
                                                });
                                            }
                                            break;
                                            //case RestFilesHandleTypes.SelfDelete:
                                            //    Utilities.MSVBFileOperation.Delete(new string[] { io.fullName }, out Exception e3, false);
                                            //    if (e3 != null)
                                            //    {
                                            //        filesWithError.Add(new RestFilesData()
                                            //        {
                                            //            io = io,
                                            //            err = e3,
                                            //            targetFullName = t,
                                            //        });
                                            //    }
                                            //    break;
                                    }
                                }
                                else
                                {
                                    Utilities.Other.GenerateShortcut(s, t);
                                    ++filesCountTransfered;
                                }
                            }
                            catch (Exception err1)
                            {
                                ExceptionWithException(err1);
                            }
                        }
                        else
                        {
                            ExceptionFileNotFound(s);
                        }
                    }
                }
            }
            private void _restCopy(List<RestFilesData> filesToHandle)
            {
                RestFilesData rest;
                for (int i = 0, iv = filesToHandle.Count; i < iv; ++i)
                {
                    rest = filesToHandle[0];
                    switch (rest.restFilesHandleType)
                    {
                        default:
                        case RestFilesHandleTypes.Skip:
                            break;
                        case RestFilesHandleTypes.Retry:
                            _Copy(new FileToTransfer() { source = rest.io, targetFullPath = rest.targetFullName },
                                rest.newName, rest.deleteSource);
                            break;
                        case RestFilesHandleTypes.Rename:
                            rest.targetFullName = Utilities.CSharpWapper.AutoNewFullName(rest.targetFullName);
                            _Copy(new FileToTransfer() { source = rest.io, targetFullPath = rest.targetFullName },
                                rest.newName, rest.deleteSource);
                            break;
                        case RestFilesHandleTypes.Overwrite:
                            _Delete(rest.targetFullName);
                            _Copy(new FileToTransfer() { source = rest.io, targetFullPath = rest.targetFullName },
                                rest.newName, rest.deleteSource);
                            break;
                    }
                    filesToHandle.RemoveAt(0);
                    --i; --iv;
                }
            }
            private void _restMove(List<RestFilesData> filesToHandle)
            {
                if (needDataCopy)
                {
                    _restCopy(filesToHandle);
                }
                else
                {
                    RestFilesData rest;
                    for (int i = 0, iv = filesToHandle.Count; i < iv; ++i)
                    {
                        rest = filesToHandle[0];

                        switch (rest.restFilesHandleType)
                        {
                            default:
                            case RestFilesHandleTypes.Skip:
                                break;
                            case RestFilesHandleTypes.Retry:
                                if (rest.needTransfer)
                                {
                                    _Copy(new FileToTransfer() { source = rest.io, targetFullPath = rest.targetFullName },
                                        rest.newName, rest.deleteSource);
                                }
                                else
                                {
                                    _MoveInSamePartition(rest.sur, ref rest.tar);
                                }
                                break;
                            case RestFilesHandleTypes.Rename:
                                if (rest.needTransfer)
                                {
                                    rest.newName = Utilities.CSharpWapper.AutoNewFullName(rest.tar);
                                    _Copy(new FileToTransfer() { source = rest.io, targetFullPath = rest.targetFullName },
                                        rest.newName, rest.deleteSource);
                                }
                                else
                                {
                                    rest.tar = Utilities.CSharpWapper.AutoNewFullName(rest.tar);
                                    _MoveInSamePartition(rest.sur, ref rest.tar);
                                }
                                break;
                            case RestFilesHandleTypes.SelfDelete:
                                Utilities.MSVBFileOperation.Delete(new string[] { rest.io.fullName }, out Exception e3, false);
                                if (e3 != null)
                                {
                                    filesWithError.Add(new RestFilesData(rest)
                                    {
                                        err = e3,
                                    });
                                }
                                break;
                        }
                        filesToHandle.RemoveAt(0);
                        --i; --iv;
                    }
                }
            }

            public void SetHandleData(List<RestFilesData> filessWithHandleData)
            {
                filesWithError = filessWithHandleData;
                flagPause = false;
            }

            public bool? flagPause = false;
            public void PauseAsync()
            {
                flagPause = true;
                SetFlagReading(false);
                SetFlagWriting(false);
            }
            public void ResumeAsync()
            {
                SetFlagReading(true);
                SetFlagWriting(true);
                flagPause = false;
            }

            public bool flagCancel = false;
            public void CancelAsync()
            {
                flagCancel = true;
                //SetFlagReading(false);
                //SetFlagWriting(false);
            }

            private bool CheckSetNeedTransfer_checked = false;
            public void CheckSetNeedTransfer()
            {
                if (CheckSetNeedTransfer_checked)
                    return;
                CheckSetNeedTransfer_checked = true;

                if (TaskType == TransferManager.TaskTypes.Copy)
                {
                    needDataCopy = true;
                }
                else if (TaskType == TransferManager.TaskTypes.Move)
                {
                    string testS = SourceIOs[0], testT = TargetIOs[0];
                    if (testS.StartsWith("\\\\"))
                    {
                        string testUNCRoot = Utilities.FilePath.GetUNCRoot(testS);
                        if (testS[0] != testT[0])
                        {
                            needDataCopy = true;
                        }
                        else if (testUNCRoot != null
                            && testUNCRoot != Utilities.FilePath.GetUNCRoot(testT))
                        {
                            needDataCopy = true;
                        }
                    }
                    else if (testS[0] != testT[0])
                    {
                        needDataCopy = true;
                    }
                }
            }
        }
    }
}
