using System;

using System.IO;
using System.Management;
using System.Security.Principal;

namespace MadTomDev.Common.Variables
{
    public class IOPath
    {
        private static string _LogDir;
        /// <summary>
        /// 路径为 我的文档 下的 MadTomDevLogs；
        /// * 注意 不会自动生成文件夹；
        /// </summary>
        public static string LogDir
        {
            get
            {
                if (_LogDir == null)
                {
                    string result = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    _LogDir = Path.Combine(result, "MadTomDevLogs");
                    if (!Directory.Exists(_LogDir))
                        Directory.CreateDirectory(_LogDir);
                }
                return _LogDir;
            }
        }

        private static string _SettingDir;
        /// <summary>
        /// 路径为 我的文档 下的 MadTomDev；
        /// * 注意 不会自动生成文件夹；
        /// </summary>
        public static string SettingDir
        {
            get
            {
                if (_SettingDir == null)
                {
                    string result = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    _SettingDir = Path.Combine(result, "MadTomDev");
                    if (!Directory.Exists(_SettingDir))
                        Directory.CreateDirectory(_SettingDir);
                }
                return _SettingDir;
            }
        }
    }

    public class PC
    {
        public static string GetComputerName()
        {
            //System.Environment.MachineName from a console or WinForms app.
            //HttpContext.Current.Server.MachineName from a web app
            //System.Net.Dns.GetHostName() to get the FQDN
            return Environment.MachineName;
        }
        public static string GetCurrentUserName()
        {
            return WindowsIdentity.GetCurrent().Name.ToString();
        }
        public static string GetProcessorID()
        {
            try
            {
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                string strCpuID = null;
                foreach (ManagementObject mo in moc)
                {
                    strCpuID = mo.Properties["ProcessorId"].Value.ToString();
                    mo.Dispose();
                    //break;
                }
                return strCpuID;
            }
            catch
            {
                return "";
            }
        }
    }
}
