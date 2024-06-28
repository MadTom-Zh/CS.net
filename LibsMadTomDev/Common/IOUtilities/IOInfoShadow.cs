using MadTomDev.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.Data
{
    public class IOInfoShadow
    {
        public IOInfoShadow()
        {
        }

        public static bool TryNewIOInfoShadow(string fileOrDir, out IOInfoShadow newIO)
        {
            newIO = null;
            try
            {
                newIO = new IOInfoShadow(fileOrDir);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IOInfoShadow(string fileOrDir)
        {
            if (File.Exists(fileOrDir))
                Init(new FileInfo(fileOrDir));
            else if (Directory.Exists(fileOrDir))
                Init(new DirectoryInfo(fileOrDir));
            else
                throw new FileNotFoundException("Can't find file or folder.", fileOrDir);
        }
        public IOInfoShadow(FileInfo fileInfo)
        {
            Init(fileInfo);
        }
        private void Init(FileInfo fileInfo)
        {
            fullName = fileInfo.FullName;
            name = fileInfo.Name;
            extension = fileInfo.Extension;
            length = fileInfo.Length;
            directoryName = fileInfo.DirectoryName;
            attributes = new AttributesShadow(fileInfo.Attributes);
            creationTime = fileInfo.CreationTime;
            lastWriteTime = fileInfo.LastWriteTime;
            lastAccessTime = fileInfo.LastAccessTime;
            wasExists = fileInfo.Exists;
        }
        public IOInfoShadow(DirectoryInfo dirInfo, Exception dirError = null)
        {
            Init(dirInfo, dirError);
        }
        public void Init(DirectoryInfo dirInfo, Exception dirError = null)
        {
            fullName = dirInfo.FullName;
            name = dirInfo.Name;
            if (name.StartsWith("\\\\"))
                name = name.Substring(name.LastIndexOf("\\") + 1);
            extension = dirInfo.Extension;
            length = 0;
            if (dirInfo.Parent != null)
                directoryName = dirInfo.Parent.FullName;
            attributes = new AttributesShadow(dirInfo.Attributes);
            creationTime = dirInfo.CreationTime;
            lastWriteTime = dirInfo.LastWriteTime;
            lastAccessTime = dirInfo.LastAccessTime;
            this.dirError = dirError;
            wasExists = dirInfo.Exists;
        }

        public string fullName;
        public string name;
        public string extension;
        public long length;
        public AttributesShadow attributes;
        public string directoryName;
        public DateTime creationTime;
        public DateTime lastWriteTime;
        public DateTime lastAccessTime;
        public Exception dirError;
        public bool wasExists;
        public bool wasFile
        {
            get => attributes != null && attributes.directory == false;
        }

        public class AttributesShadow
        {
            public AttributesShadow()
            {
            }
            public AttributesShadow(FileAttributes attributes)
            {
                readOnly = attributes.HasFlag(FileAttributes.ReadOnly);
                hidden = attributes.HasFlag(FileAttributes.Hidden);
                archive = attributes.HasFlag(FileAttributes.Archive);
                compressed = attributes.HasFlag(FileAttributes.Compressed);
                device = attributes.HasFlag(FileAttributes.Device);
                directory = attributes.HasFlag(FileAttributes.Directory);
                encrypted = attributes.HasFlag(FileAttributes.Encrypted);
                integrityStream = attributes.HasFlag(FileAttributes.IntegrityStream);
                normal = attributes.HasFlag(FileAttributes.Normal);
                noScrubData = attributes.HasFlag(FileAttributes.NoScrubData);
                notContentIndexed = attributes.HasFlag(FileAttributes.NotContentIndexed);
                offline = attributes.HasFlag(FileAttributes.Offline);
                reparsePoint = attributes.HasFlag(FileAttributes.ReparsePoint);
                sparseFile = attributes.HasFlag(FileAttributes.SparseFile);
                system = attributes.HasFlag(FileAttributes.System);
                temporary = attributes.HasFlag(FileAttributes.Temporary);
            }

            public override int GetHashCode()
            {
                return ToShortString().GetHashCode();
            }
            public override bool Equals(object obj)
            {
                if (obj is not AttributesShadow)
                {
                    return false;
                }
                return (AttributesShadow)obj == this;
            }
            public static bool operator ==(AttributesShadow a, AttributesShadow b)
            {
                return !(a != b);
            }
            public static bool operator !=(AttributesShadow a, AttributesShadow b)
            {
                if (a is null && b is null)
                    return false;
                if ((a is null && !(b is null))
                    || (!(a is null) && b is null))
                    return true;

                return a.archive != b.archive
                    || a.readOnly != b.readOnly
                    || a.hidden != b.hidden
                    || a.compressed != b.compressed
                    || a.device != b.device
                    || a.directory != b.directory
                    || a.encrypted != b.encrypted
                    || a.integrityStream != b.integrityStream
                    || a.normal != b.normal
                    || a.noScrubData != b.noScrubData
                    || a.notContentIndexed != b.notContentIndexed
                    || a.offline != b.offline
                    || a.reparsePoint != b.reparsePoint
                    || a.sparseFile != b.sparseFile
                    || a.system != b.system
                    || a.temporary != b.temporary;
            }

            public bool readOnly;
            public bool hidden;
            public bool archive;
            public bool compressed;
            public bool device;
            public bool directory;
            public bool encrypted;
            public bool integrityStream;
            public bool normal;
            public bool noScrubData;
            public bool notContentIndexed;
            public bool offline;
            public bool reparsePoint;
            public bool sparseFile;
            public bool system;
            public bool temporary;

            internal AttributesShadow Clone()
            {
                return new AttributesShadow()
                {
                    readOnly = this.readOnly,
                    hidden = this.hidden,
                    archive = this.archive,
                    compressed = this.compressed,
                    device = this.device,
                    directory = this.directory,
                    encrypted = this.encrypted,
                    integrityStream = this.integrityStream,
                    normal = this.normal,
                    noScrubData = this.noScrubData,
                    notContentIndexed = this.notContentIndexed,
                    offline = this.offline,
                    reparsePoint = this.reparsePoint,
                    sparseFile = this.sparseFile,
                    system = this.system,
                    temporary = this.temporary,
                };
            }

            public string ToShortString()
            {
                StringBuilder result = new StringBuilder();

                if (archive)
                    result.Append("A");
                if (compressed)
                    result.Append("C");
                if (device)
                    result.Append("Dev");
                if (directory)
                    result.Append("Dir");
                if (encrypted)
                    result.Append("E");
                if (hidden)
                    result.Append("H");
                if (integrityStream)
                    result.Append("I");
                if (normal)
                    result.Append("Nor");
                if (noScrubData)
                    result.Append("Nsd");
                if (notContentIndexed)
                    result.Append("Nci");
                if (offline)
                    result.Append("O");
                if (readOnly)
                    result.Append("Ro");
                if (reparsePoint)
                    result.Append("Rp");
                if (sparseFile)
                    result.Append("Sf");
                if (system)
                    result.Append("Sys");
                if (temporary)
                    result.Append("T");

                return result.ToString();
            }
            public string ToShortString7()
            {
                StringBuilder result = new StringBuilder();

                if (archive) result.Append("A");
                else result.Append("-");
                if (compressed) result.Append("C");
                else result.Append("-");
                if (directory) result.Append("D");
                else result.Append("-");
                if (encrypted) result.Append("E");
                else result.Append("-");
                if (hidden) result.Append("H");
                else result.Append("-");
                if (readOnly) result.Append("R");
                else result.Append("-");
                if (system) result.Append("S");
                else result.Append("-");

                return result.ToString();
            }
        }

        public IOInfoShadow Clone()
        {
            return new IOInfoShadow()
            {
                attributes = this.attributes.Clone(),
                creationTime = this.creationTime,
                directoryName = this.directoryName,
                dirError = this.dirError,
                extension = this.extension,
                fullName = this.fullName,
                lastAccessTime = this.lastAccessTime,
                lastWriteTime = this.lastWriteTime,
                length = this.length,
                name = this.name,
                wasExists = this.wasExists,
            };
        }

        public void UpdateFrom(IOInfoShadow newIO)
        {
            attributes = newIO.attributes.Clone();
            creationTime = newIO.creationTime;
            directoryName = newIO.directoryName;
            dirError = newIO.dirError;
            extension = newIO.extension;
            fullName = newIO.fullName;
            lastAccessTime = newIO.lastAccessTime;
            lastWriteTime = newIO.lastWriteTime;
            length = newIO.length;
            name = newIO.name;
            wasExists = newIO.wasExists;
        }
    }
    public class DriveInfoShadow
    {
        public DriveInfoShadow()
        {
        }
        public DriveInfoShadow(DriveInfo driveInfo)
        {
            isReady = driveInfo.IsReady;
            if (isReady)
            {
                availableFreeSpace = driveInfo.AvailableFreeSpace;
                name = driveInfo.Name;
                totalSize = driveInfo.TotalSize;
                driveFormat = driveInfo.DriveFormat;
                driveType = driveInfo.DriveType;
                volumeLabel = driveInfo.VolumeLabel;
                totalFreeSpace = driveInfo.TotalFreeSpace;
            }
        }

        /// <summary>
        /// such as C:\
        /// </summary>
        public string name;
        /// <summary>
        /// such as OS, my movie
        /// </summary>
        public string volumeLabel;
        /// <summary>
        /// such as NTFS, FAT32
        /// </summary>
        public string driveFormat;
        public DriveType driveType;
        public bool isReady = false;
        public long totalSize;
        public long totalFreeSpace;
        public long availableFreeSpace;

        public override string ToString()
        {
            StringBuilder strBdr = new StringBuilder();
            strBdr.Append(name);
            strBdr.Append("("); strBdr.Append(volumeLabel); strBdr.Append(")");
            strBdr.Append(" "); strBdr.Append(driveType);
            strBdr.Append(" in "); strBdr.Append(driveFormat);
            strBdr.Append(" Space:"); strBdr.Append(availableFreeSpace);
            strBdr.Append("/"); strBdr.Append(totalSize);

            return strBdr.ToString();
        }
    }


    public class AdvancedDriveInfoHelper
    {
        private AdvancedDriveInfoHelper() { }
        private static AdvancedDriveInfoHelper instance = null;
        public static AdvancedDriveInfoHelper GetInstance()
        {
            if (instance == null)
            {
                instance = new AdvancedDriveInfoHelper();
                instance.Refresh();
            }
            return instance;
        }


        #region device detector, watching USB drives plug in/out
        private DriveDetector driveDetector = null;
        private bool _IsWatchingRemovableDrives = false;
        public bool IsWatchingRemovableDrives
        {
            get { return _IsWatchingRemovableDrives; }
            set
            {
                if (_IsWatchingRemovableDrives == value) return;

                _IsWatchingRemovableDrives = value;
                if (_IsWatchingRemovableDrives)
                {
                    if (driveDetector == null)
                    {
                        driveDetector = DriveDetector.GetInstance();
                    }
                    DriveDetectorAddHandlers();
                }
                else
                {
                    if (driveDetector != null)
                    {
                        DriveDetectorRemoveHandlers();
                        driveDetector.Dispose();
                        driveDetector = null;
                    }
                }
            }
        }

        public class DevicePlugedArgs : EventArgs
        {
            public AdvDriveInfo plugedDeviceAdvInfo;
        }
        public delegate void DevicePlugedDelegate(AdvancedDriveInfoHelper sender, AdvDriveInfo adi);
        public event DevicePlugedDelegate DevicePlugedIn;
        public event DevicePlugedDelegate DevicePlugedOut;
        private void DriveDetector_DrivePluged(char driveLetter, bool plugedInOrOut)
        {
            AdvDriveInfo adi;
            if (plugedInOrOut)
            {
                if (Contains(driveLetter))
                    RemoveAtLetter(driveLetter);
                adi = AddUpdateAtLetter(driveLetter);
                DevicePlugedIn?.Invoke(this, adi);
            }
            else
            {
                adi = null;
                if (Contains(driveLetter))
                    adi = RemoveAtLetter(driveLetter);
                DevicePlugedOut?.Invoke(this, adi);
            }
        }
        private void DriveDetectorAddHandlers()
        {
            driveDetector.DrivePluged -= DriveDetector_DrivePluged;
            driveDetector.DrivePluged += DriveDetector_DrivePluged;
        }


        private void DriveDetectorRemoveHandlers()
        {
            driveDetector.DrivePluged -= DriveDetector_DrivePluged;
        }

        #endregion

        public const char DeviceVolumeLetterUnknow = '?';

        public readonly List<AdvDriveInfo> AllDrivers = new List<AdvDriveInfo>();
        public void Refresh()
        {
            AllDrivers.Clear();
            AllDrivers.AddRange(Get_AdvDriveInfoList());
            _Dictionary_DeviceId_VolumeLetter = null;
            _Dictionary_VolumeLetter_DeviceId = null;
        }
        public bool Contains(char volumeLetter)
        {
            foreach (AdvDriveInfo adi in AllDrivers)
            {
                if (adi.VolumeLetter == volumeLetter)
                    return true;
            }
            return false;
        }
        public bool Contains(Guid deviceId)
        {
            AdvDriveInfo found
                = AllDrivers.Find((AdvDriveInfo adi) => adi.DeviceId == deviceId);
            if (found == null) return false;
            else return true;
        }
        public AdvDriveInfo GetAdvDriveInfo(char volumeLetter)
        {
            foreach (AdvDriveInfo adi in AllDrivers)
            {
                if (adi.VolumeLetter == volumeLetter)
                    return adi;
            }
            return null;
        }
        public AdvDriveInfo GetAdvDriveInfo_byDeviceId(Guid deviceId)
        {
            foreach (AdvDriveInfo adi in AllDrivers)
            {
                if (adi.DeviceId == deviceId)
                    return adi;
            }
            return null;
        }

        public char GetDeviceVolumeLetter(Guid deviceId)
        {
            AdvDriveInfo found
                = AllDrivers.Find((AdvDriveInfo adi) => adi.DeviceId == deviceId);
            if (found != null) return found.VolumeLetter;
            else return DeviceVolumeLetterUnknow;
        }
        public Guid GetDeviceId(char deviceLetter, bool tryRefresh = true)
        {
            AdvDriveInfo found
                = AllDrivers.Find((AdvDriveInfo adi) => adi.VolumeLetter == deviceLetter);
            if (found != null) return found.DeviceId;
            else
            {
                if (tryRefresh)
                {
                    Refresh();
                    return GetDeviceId(deviceLetter, false);
                }
                else
                    return Guid.Empty;
            }
        }
        public Dictionary<Guid, char> Dictionary_DeviceId_VolumeLetter
        {
            get
            {
                _TryReLoadDIdLV();
                return _Dictionary_DeviceId_VolumeLetter;
            }
        }
        public Dictionary<char, Guid> Dictionary_VolumeLetter_DeviceId
        {
            get
            {
                _TryReLoadDIdLV();
                return _Dictionary_VolumeLetter_DeviceId;
            }
        }

        #region private methods
        private AdvDriveInfo AddUpdateAtLetter(char vLetter)
        {
            AdvDriveInfo result
                = new AdvDriveInfo(
                    new DriveInfoShadow(new DriveInfo(vLetter.ToString())),
                    AdvancedDriveInfoHelper.GetPatitionInfoList().Find(a => a.volumeLetter == vLetter),
                    AdvancedDriveInfoHelper.GetPhysicalDiskInforList().Find(a => a.volumeLetter == vLetter),
                    AdvancedDriveInfoHelper.GetLogicDiskInfoList().Find(a => a.volumeLetter == vLetter),
                    AdvancedDriveInfoHelper.GetVolumeInfo(vLetter),
                    AdvancedDriveInfoHelper.Get_DeviceIds(new char[] { vLetter })[0]);
            AdvDriveInfo oriADI
                = AllDrivers.Find((AdvDriveInfo adi) => adi.DeviceId == result.DeviceId);
            if (oriADI != null)
                AllDrivers.Remove(oriADI);
            AllDrivers.Add(result);
            return result;
        }
        private AdvDriveInfo RemoveAtLetter(char vLetter)
        {
            foreach (AdvDriveInfo adi in AllDrivers)
            {
                if (adi.VolumeLetter == vLetter)
                {
                    AllDrivers.Remove(adi);
                    return adi;
                }
            }
            return null;
        }

        private Dictionary<Guid, char> _Dictionary_DeviceId_VolumeLetter = null;
        private Dictionary<char, Guid> _Dictionary_VolumeLetter_DeviceId = null;
        private void _TryReLoadDIdLV()
        {
            if (_Dictionary_DeviceId_VolumeLetter == null || _Dictionary_VolumeLetter_DeviceId == null)
            {
                _Dictionary_DeviceId_VolumeLetter = new Dictionary<Guid, char>();
                _Dictionary_VolumeLetter_DeviceId = new Dictionary<char, Guid>();
                foreach (AdvDriveInfo adi in AllDrivers)
                {
                    _Dictionary_DeviceId_VolumeLetter.Add(adi.DeviceId, adi.VolumeLetter);
                    _Dictionary_VolumeLetter_DeviceId.Add(adi.VolumeLetter, adi.DeviceId);
                }
            }
        }
        #endregion


        #region static methods
        public static DriveInfoShadow Get_DevInfo(string devName)
        {
            return new DriveInfoShadow(new DriveInfo(devName));
        }
        public static List<DriveInfoShadow> Get_ReadyDevInfoList()
        {
            List<DriveInfoShadow> result = new List<DriveInfoShadow>();
            foreach (DriveInfo di in DriveInfo.GetDrives())
            {
                if (di.IsReady)
                {
                    result.Add(new DriveInfoShadow(di));
                }
            }
            return result;
        }


        public class PartitionInformation
        {
            public char volumeLetter;
            public int diskIndex = -1;
            public int partitionIndex = -1;
        }
        public static List<PartitionInformation> GetPatitionInfoList()
        {
            List<PartitionInformation> result = new List<PartitionInformation>();

            //using (ManagementObjectSearcher ms = new ManagementObjectSearcher($"Select * from Win32_LogicalDiskToPartition where Dependent like \"\"%{volumeLetter}:%\"\""))
            using (ManagementObjectSearcher ms = new ManagementObjectSearcher($"Select * from Win32_LogicalDiskToPartition"))
            {
                object testObj, testObj1;
                string testStr;
                int idxStr;
                PartitionInformation newPI;
                foreach (ManagementObject mo in ms.Get())
                {
                    testObj = mo["Antecedent"];
                    testObj1 = mo["Dependent"];
                    if (testObj != null && testObj1 != null)
                    {
                        newPI = new PartitionInformation();
                        // \\DESKTOP-H0M15OQ\root\cimv2:Win32_DiskPartition.DeviceID="Disk #0, Partition #2"
                        // \\DESKTOP-H0M15OQ\root\cimv2:Win32_LogicalDisk.DeviceID="C:"
                        testStr = testObj1.ToString();
                        idxStr = testStr.LastIndexOf("DeviceID=");
                        if (idxStr < 0)
                            continue;

                        testStr = testStr.Substring(idxStr + 10);
                        newPI.volumeLetter = testStr[0];

                        testStr = testObj.ToString();
                        idxStr = testStr.IndexOf("Disk #");
                        if (idxStr < 0)
                            continue;

                        testStr = testStr.Substring(idxStr + 6);
                        idxStr = testStr.IndexOf(",");
                        if (idxStr < 0)
                            continue;
                        newPI.diskIndex = int.Parse(testStr.Substring(0, idxStr));

                        testStr = testStr.Substring(idxStr);
                        idxStr = testStr.IndexOf("Partition #");
                        if (idxStr < 0)
                            continue;
                        testStr = testStr.Substring(idxStr + 11);
                        newPI.partitionIndex = int.Parse(testStr.Substring(0, testStr.Length - 1));

                        result.Add(newPI);
                    }
                    else continue;
                    //break;
                }
            }
            return result;
        }


        public struct LogicDiskInformation
        {
            public char volumeLetter;
            public string description;
            public LogicDiskDriveTypes driveType;
            public LogicDiskMediaTypes mediaType;
            public string volumeSerialNumberHex;
        }
        public enum LogicDiskDriveTypes
        {
            Unknow = 0,
            RemovableDisk = 2,
            LocalDisk = 3,
            NetworkDrive = 4,
            CompactDisc = 5,
            RAMDisk = 6,
        }
        public enum LogicDiskMediaTypes
        {
            /// <summary>
            /// Format is unknown
            /// </summary>
            Unknow = 0,
            /// <summary>
            /// 5 1/4-Inch Floppy Disk - 1.2 MB - 512 bytes/sector
            /// </summary>
            FloppyDisk5Inch1_2MB512bps = 1,
            /// <summary>
            /// 3 1/2-Inch Floppy Disk - 1.44 MB -512 bytes/sector
            /// </summary>
            FloppyDisk3Inch1_44MB512bps = 2,
            /// <summary>
            /// 3 1/2-Inch Floppy Disk - 2.88 MB - 512 bytes/sector
            /// </summary>
            FloppyDisk3Inch2_88MB512bps = 3,
            /// <summary>
            /// 3 1/2-Inch Floppy Disk - 20.8 MB - 512 bytes/sector
            /// </summary>
            FloppyDisk3Inch20_8MB512bps = 4,
            /// <summary>
            /// 3 1/2-Inch Floppy Disk - 720 KB - 512 bytes/sector
            /// </summary>
            FloppyDisk3Inch720KB512bps = 5,
            /// <summary>
            /// 5 1/4-Inch Floppy Disk - 360 KB - 512 bytes/sector
            /// </summary>
            FloppyDisk5Inch360KB512bps = 6,
            /// <summary>
            /// 5 1/4-Inch Floppy Disk - 320 KB - 512 bytes/sector
            /// </summary>
            FloppyDisk5Inch320KB512bps = 7,
            /// <summary>
            /// 5 1/4-Inch Floppy Disk - 320 KB - 1024 bytes/sector
            /// </summary>
            FloppyDisk5Inch320KB1024bps = 8,
            /// <summary>
            /// 5 1/4-Inch Floppy Disk - 180 KB - 512 bytes/sector
            /// </summary>
            FloppyDisk5Inch180KB512bps = 9,
            /// <summary>
            /// 5 1/4-Inch Floppy Disk - 160 KB - 512 bytes/sector
            /// </summary>
            FloppyDisk5Inch160KB512bps = 10,
            /// <summary>
            /// Removable media other than floppy (11)
            /// </summary>
            RemovableNonFloppy = 11,
            /// <summary>
            /// Fixed hard disk media
            /// </summary>
            FixedHardDisk = 12,
            /// <summary>
            /// 3 1/2-Inch Floppy Disk - 120 MB - 512 bytes/sector
            /// </summary>
            FloppyDisk3Inch120MB512bps = 13,
            /// <summary>
            /// 3 1/2-Inch Floppy Disk - 640 KB - 512 bytes/sector
            /// </summary>
            FloppyDisk3Inch640KB512bps = 14,
            /// <summary>
            /// 5 1/4-Inch Floppy Disk - 640 KB - 512 bytes/sector
            /// </summary>
            FloppyDisk5Inch640KB512bps = 15,
            /// <summary>
            /// 5 1/4-Inch Floppy Disk - 720 KB - 512 bytes/sector
            /// </summary>
            FloppyDisk5Inch720KB512bps = 16,
            /// <summary>
            /// 3 1/2-Inch Floppy Disk - 1.2 MB - 512 bytes/sector
            /// </summary>
            FloppyDisk3Inch1_2MB512bps = 17,
            /// <summary>
            /// 3 1/2-Inch Floppy Disk - 1.23 MB - 1024 bytes/sector
            /// </summary>
            FloppyDisk3Inch1_23MB1024bps = 18,
            /// <summary>
            /// 5 1/4-Inch Floppy Disk - 1.23 MB - 1024 bytes/sector
            /// </summary>
            FloppyDisk5Inch1_23MB1024bps = 19,
            /// <summary>
            /// 3 1/2-Inch Floppy Disk - 128 MB - 512 bytes/sector
            /// </summary>
            FloppyDisk3Inch128MB512bps = 20,
            /// <summary>
            /// 3 1/2-Inch Floppy Disk - 230 MB - 512 bytes/sector
            /// </summary>
            FloppyDisk3Inch230MB512bps = 21,
            /// <summary>
            /// 8-Inch Floppy Disk - 256 KB - 128 bytes/sector
            /// </summary>
            FloppyDisk8Inch256KB128bps = 22,
        }
        public static List<LogicDiskInformation> GetLogicDiskInfoList()
        {
            List<LogicDiskInformation> result = new List<LogicDiskInformation>();

            //using (ManagementObjectSearcher ms = new ManagementObjectSearcher($"Select * from Win32_LogicalDisk where DeviceID=\"{volumeLetter}:\""))
            using (ManagementObjectSearcher ms = new ManagementObjectSearcher($"Select * from Win32_LogicalDisk"))
            {
                object testObj;
                LogicDiskInformation newLDI;
                foreach (ManagementObject mo in ms.Get())
                {
                    testObj = mo["DeviceID"];
                    newLDI.volumeLetter = ((string)testObj)[0];
                    testObj = mo["Description"];
                    newLDI.description = (string)testObj;
                    testObj = mo["VolumeSerialNumber"];
                    newLDI.volumeSerialNumberHex = (string)testObj;
                    testObj = mo["DriveType"];
                    newLDI.driveType = testObj != null ? (LogicDiskDriveTypes)(uint)testObj : LogicDiskDriveTypes.Unknow;
                    testObj = mo["MediaType"];
                    newLDI.mediaType = testObj != null ? (LogicDiskMediaTypes)(uint)testObj : LogicDiskMediaTypes.Unknow;
                    result.Add(newLDI);
                }
            }
            return result;
        }


        public struct VolumeInformation
        {
            public string label;
            public UInt32 VolumeSerialNumber;
            public UInt32 MaximumComponentLength;
            public UInt32 FileSystemFlags;
            public string fileSystem;
        }
        public static VolumeInformation GetVolumeInfo(char volumeLetter)
        {
            string volumeName = volumeLetter + ":\\";
            uint serial_number = 0;
            uint max_component_length = 0;
            StringBuilder sb_volume_name = new StringBuilder(256);
            UInt32 file_system_flags = new UInt32();
            StringBuilder sb_file_system_name = new StringBuilder(256);

            if (GetVolumeInformation(volumeName, sb_volume_name,
                (UInt32)sb_volume_name.Capacity, ref serial_number,
                ref max_component_length, ref file_system_flags,
                sb_file_system_name,
                (UInt32)sb_file_system_name.Capacity) == 0)
            {
                return new VolumeInformation();
            }
            else
            {
                return new VolumeInformation()
                {
                    label = sb_volume_name.ToString(),
                    VolumeSerialNumber = serial_number,
                    MaximumComponentLength = max_component_length,
                    fileSystem = sb_file_system_name.ToString(),
                    FileSystemFlags = file_system_flags,
                };
            }
        }

        public struct PhysicalDiskInformation
        {
            public char volumeLetter;
            public PhysicalDiskMediaTypes mediaType;
        }
        public enum PhysicalDiskMediaTypes
        { Unknow = 0, Unspecified = 1, HDD = 3, SSD = 4, SCM = 5, }
        public static List<PhysicalDiskInformation> GetPhysicalDiskInforList()
        {
            List<PhysicalDiskInformation> result = new List<PhysicalDiskInformation>();
            ManagementScope scope = new ManagementScope(@"\\.\root\microsoft\windows\storage");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM MSFT_PhysicalDisk");
            string type = "";
            scope.Connect();
            searcher.Scope = scope;

            foreach (ManagementObject queryObj in searcher.Get())
            {
                switch (Convert.ToInt16(queryObj["MediaType"]))
                {
                    case 1:
                        type = "Unspecified";
                        break;

                    case 3:
                        type = "HDD";
                        break;

                    case 4:
                        type = "SSD";
                        break;

                    case 5:
                        type = "SCM";
                        break;

                    default:
                        type = "Unspecified";
                        break;
                }
            }
            searcher.Dispose();
            return result;
        }

        public static Dictionary<char, Guid> Get_DeviceIdDirectory()
        {
            Dictionary<char, Guid> result = new Dictionary<char, Guid>();

            using (ManagementObjectSearcher ms = new ManagementObjectSearcher("Select * from Win32_Volume"))
            {
                string vlStr, vdId;
                Guid dId;
                foreach (ManagementObject mo in ms.Get())
                {
                    vlStr = (string)mo["DriveLetter"];
                    if (vlStr == null) continue;
                    vdId = (string)mo["DeviceID"];
                    if (vdId == null || vdId.Length == 0) continue;
                    vdId = vdId.Substring(vdId.IndexOf('{') + 1);
                    vdId = vdId.Substring(0, vdId.IndexOf('}'));
                    dId = Guid.Parse(vdId);
                    result.Add(vlStr[0], dId);
                }
            }
            return result;
        }
        public static string aaa()
        {
            //var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskPartition");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskPartition");

            StringBuilder strBdr = new StringBuilder();
            foreach (ManagementObject queryObj in searcher.Get())
            {
                strBdr.AppendLine("-------------------------------------------------------");
                foreach (PropertyData pd in queryObj.Properties)
                    strBdr.AppendLine(pd.Name + ":" + pd.Value);
                //try { strBdr.AppendLine("AdditionalAvailability:" + (uint)queryObj["AdditionalAvailability"]); } catch (Exception) { }
                //try { strBdr.AppendLine("Availability:" + (uint)queryObj["Availability"]); } catch (Exception) { }
                //try
                //{ strBdr.AppendLine("PowerManagementCapabilities:" + ((uint[])queryObj["PowerManagementCapabilities"])[0]); }
                //catch (Exception) { }
                //try
                //{ strBdr.AppendLine("IdentifyingDescriptions:" + ((uint[])queryObj["IdentifyingDescriptions"])[0]); }
                //catch (Exception) { }
                //try
                //{ strBdr.AppendLine("MaxQuiesceTime:" + (ulong)queryObj["MaxQuiesceTime"]); }
                //catch (Exception) { }
                //try
                //{ strBdr.AppendLine("OtherIdentifyingInfo:" + (ulong)queryObj["OtherIdentifyingInfo"]); }
                //catch (Exception) { }
                //try
                //{ strBdr.AppendLine("StatusInfo:" + (uint)queryObj["StatusInfo"]); }
                //catch (Exception) { }
                //try { strBdr.AppendLine("PowerOnHours:" + (ulong)queryObj["PowerOnHours"]); } catch (Exception) { }
                //try { strBdr.AppendLine("TotalPowerOnHours:" + (ulong)queryObj["TotalPowerOnHours"]); } catch (Exception) { }
                //try
                //{
                //    strBdr.AppendLine("Access:" + (uint)queryObj["Access"]);
                //}
                //catch (Exception) { }
                //try
                //{
                //    strBdr.AppendLine("BlockSize:" + (ulong)queryObj["BlockSize"]);
                //}
                //catch (Exception) { }
                //try
                //{
                //    strBdr.AppendLine("Bootable:" + (bool)queryObj["Bootable"]);
                //}
                //catch (Exception) { }
                //try
                //{
                //    strBdr.AppendLine("BootPartition:" + (bool)queryObj["BootPartition"]);
                //}
                //catch (Exception) { }
                //try
                //{
                //    strBdr.AppendLine("Caption:" + (string)queryObj["Caption"]);
                //}
                //catch (Exception) { }
                //try { strBdr.AppendLine("ConfigManagerErrorCode:" + (uint)queryObj["ConfigManagerErrorCode"]); } catch (Exception) { }
                //try { strBdr.AppendLine("ConfigManagerUserConfig:" + (bool)queryObj["ConfigManagerUserConfig"]); } catch (Exception) { }
                //try { strBdr.AppendLine("CreationClassName:" + (string)queryObj["CreationClassName"]); } catch (Exception) { }
                //try { strBdr.AppendLine("Description:" + (string)queryObj["Description"]); } catch (Exception) { }
                //try { strBdr.AppendLine("DeviceID:" + (string)queryObj["DeviceID"]); } catch (Exception) { }
                //try { strBdr.AppendLine("DiskIndex:" + (uint)queryObj["DiskIndex"]); } catch (Exception) { }
                //try { strBdr.AppendLine("ErrorCleared:" + (bool)queryObj["ErrorCleared"]); } catch (Exception) { }
                //try { strBdr.AppendLine("ErrorDescription:" + (string)queryObj["ErrorDescription"]); } catch (Exception) { }
                //try { strBdr.AppendLine("ErrorMethodology:" + (string)queryObj["ErrorMethodology"]); } catch (Exception) { }
                //try { strBdr.AppendLine("HiddenSectors:" + (uint)queryObj["HiddenSectors"]); } catch (Exception) { }
                //try { strBdr.AppendLine("Index:" + (uint)queryObj["Index"]); } catch (Exception) { }
                //try { strBdr.AppendLine("InstallDate:" + ((DateTime)queryObj["InstallDate"]).ToString()); } catch (Exception) { }
                //try { strBdr.AppendLine("LastErrorCode:" + (uint)queryObj["LastErrorCode"]); } catch (Exception) { }
                //try { strBdr.AppendLine("Name:" + (string)queryObj["Name"]); } catch (Exception) { }
                //try { strBdr.AppendLine("NumberOfBlocks:" + (ulong)queryObj["NumberOfBlocks"]); } catch (Exception) { }
                //try { strBdr.AppendLine("PNPDeviceID:" + (string)queryObj["PNPDeviceID"]); } catch (Exception) { }
                //try { strBdr.AppendLine("PowerManagementSupported:" + (bool)queryObj["PowerManagementSupported"]); } catch (Exception) { }
                //try { strBdr.AppendLine("PrimaryPartition:" + (bool)queryObj["PrimaryPartition"]); } catch (Exception) { }
                //try { strBdr.AppendLine("Purpose:" + (string)queryObj["Purpose"]); } catch (Exception) { }
                //try { strBdr.AppendLine("RewritePartition:" + (bool)queryObj["RewritePartition"]); } catch (Exception) { }
                //try { strBdr.AppendLine("Size:" + (ulong)queryObj["Size"]); } catch (Exception) { }
                //try { strBdr.AppendLine("StartingOffset:" + (ulong)queryObj["StartingOffset"]); } catch (Exception) { }
                //try { strBdr.AppendLine("Status:" + (string)queryObj["Status"]); } catch (Exception) { }
                //try { strBdr.AppendLine("SystemCreationClassName:" + (string)queryObj["SystemCreationClassName"]); } catch (Exception) { }
                //try { strBdr.AppendLine("SystemName:" + (string)queryObj["SystemName"]); } catch (Exception) { }
                //try { strBdr.AppendLine("Type:" + (string)queryObj["Type"]); } catch (Exception) { }
            }
            strBdr.AppendLine();
            strBdr.AppendLine();

            ManagementObjectCollection drives = new ManagementObjectSearcher("Select * from Win32_LogicalDiskToPartition").Get();
            foreach (ManagementObject drive in drives)
            {
                strBdr.AppendLine("-------------------------------------------");
                foreach (PropertyData pd in drive.Properties)
                {
                    strBdr.AppendLine(pd.Name + ":" + pd.Value);
                }
            }
            strBdr.AppendLine();
            strBdr.AppendLine();



            ManagementObjectCollection disks = new ManagementObjectSearcher("Select * from Win32_LogicalDisk").Get();
            foreach (ManagementObject disk in disks)
            {
                strBdr.AppendLine("-------------------------------------------");
                foreach (PropertyData pd in disk.Properties)
                {
                    strBdr.AppendLine(pd.Name + ":" + pd.Value);
                }
            }
            strBdr.AppendLine();
            strBdr.AppendLine();



            ManagementObjectCollection vols = new ManagementObjectSearcher("Select * from Win32_Volume").Get();
            foreach (ManagementObject vol in vols)
            {
                strBdr.AppendLine("-------------------------------------------");
                foreach (PropertyData pd in vol.Properties)
                {
                    strBdr.AppendLine(pd.Name + ":" + pd.Value);
                }
            }
            strBdr.AppendLine();
            strBdr.AppendLine();


            return strBdr.ToString();
        }

        public static Guid[] Get_DeviceIds(params char[] volumeLetters)
        {
            Dictionary<char, Guid> idDict = Get_DeviceIdDirectory();
            List<Guid> result = new List<Guid>();
            foreach (char l in volumeLetters)
            {
                if (idDict.ContainsKey(l)) result.Add(idDict[l]);
                else result.Add(Guid.Empty);
            }
            return result.ToArray();
        }

        public static AdvDriveInfo Get_AdvDriveInfo_byVolLetter(char driveVolumnLetter)
        {
            List<DriveInfoShadow> devs = Get_ReadyDevInfoList();
            DriveInfoShadow curDriveInfo;

            List<PartitionInformation> PIList = GetPatitionInfoList();
            List<LogicDiskInformation> LDIList = GetLogicDiskInfoList();
            List<PhysicalDiskInformation> PDIList = GetPhysicalDiskInforList();
            for (int i = devs.Count - 1; i >= 0; i--)
            {
                curDriveInfo = devs[i];
                if (driveVolumnLetter == curDriveInfo.name[0])
                {
                    char vLetter = driveVolumnLetter;
                    return new AdvDriveInfo(
                        curDriveInfo,
                        PIList.Find(a => a.volumeLetter == vLetter),
                        PDIList.Find(a => a.volumeLetter == vLetter),
                        LDIList.Find(a => a.volumeLetter == vLetter),
                        GetVolumeInfo(vLetter),
                        Get_DeviceIds(new char[] { vLetter })[0]);
                }
            }
            return null;
        }
        public static AdvDriveInfo Get_AdvDriveInfo_byDevId(Guid deviceId)
        {
            List<AdvDriveInfo> advDIs = Get_AdvDriveInfoList();
            foreach (AdvDriveInfo adi in advDIs)
            {
                if (deviceId == adi.DeviceId)
                    return adi;
            }
            return null;
        }
        public static List<AdvDriveInfo> Get_AdvDriveInfoList()
        {
            List<AdvDriveInfo> result = new List<AdvDriveInfo>();
            List<DriveInfoShadow> devs = Get_ReadyDevInfoList();


            List<char> letters = new List<char>();
            for (int i = 0; i < devs.Count; i++)
                letters.Add(devs[i].name[0]);
            char[] lettersArray = letters.ToArray();
            Guid[] dIds = Get_DeviceIds(lettersArray);

            DriveInfoShadow di;
            char vChar;
            List<PartitionInformation> PIList = GetPatitionInfoList();
            List<LogicDiskInformation> LDIList = GetLogicDiskInfoList();
            List<PhysicalDiskInformation> PDIList = GetPhysicalDiskInforList();
            for (int i = 0; i < devs.Count; i++)
            {
                di = devs[i];
                vChar = di.name[0];
                result.Add(new AdvDriveInfo(
                    di,
                    PIList.Find(a => a.volumeLetter == vChar),
                    PDIList.Find(a => a.volumeLetter == vChar),
                    LDIList.Find(a => a.volumeLetter == vChar),
                    GetVolumeInfo(vChar),
                    dIds[i]));
            }
            return result;
        }

        #endregion


        #region class and sturct defines
        public class AdvDriveInfo
        {
            public AdvDriveInfo(
                DriveInfoShadow driveInfo,
                PartitionInformation patitionInfo,
                PhysicalDiskInformation phyDiskInfo,
                LogicDiskInformation logicDiskInfo,
                VolumeInformation volumeInfo,
                Guid devId)
            {
                _driveInfo = driveInfo;
                _PartitionInformation = patitionInfo;
                _LogicDiskInfo = logicDiskInfo;
                _VolumeInformation = volumeInfo;
                _PhysicalDiskInformation = phyDiskInfo;
                _DeviceId = devId;
            }

            public char VolumeLetter { get { return _driveInfo.name[0]; } }

            private DriveInfoShadow _driveInfo;
            public DriveInfoShadow driveInfo
            {
                get { return _driveInfo; }
            }
            private Guid _DeviceId = Guid.Empty;
            public Guid DeviceId { get => _DeviceId; }
            private PartitionInformation _PartitionInformation;
            public PartitionInformation PartitionInformation { get => _PartitionInformation; }
            private LogicDiskInformation _LogicDiskInfo;
            public LogicDiskInformation LogicDiskInfo { get => _LogicDiskInfo; }
            private VolumeInformation _VolumeInformation;
            public VolumeInformation VolumeInformation { get => _VolumeInformation; }
            private PhysicalDiskInformation _PhysicalDiskInformation;
            public PhysicalDiskInformation PhysicalDiskInformation { get => _PhysicalDiskInformation; }
        }

        #endregion

        #region "kernel32.dll"
        [DllImport("kernel32.dll")]
        private static extern long GetVolumeInformation(
            string PathName,
            StringBuilder VolumeNameBuffer,
            UInt32 VolumeNameSize,
            ref UInt32 VolumeSerialNumber,
            ref UInt32 MaximumComponentLength,
            ref UInt32 FileSystemFlags,
            StringBuilder FileSystemNameBuffer,
            UInt32 FileSystemNameSize);

        #endregion
    }
}
