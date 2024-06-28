using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MadTomDev.Data
{
    public class Network
    {
        /// <summary>
        /// 还没有用过，，，
        /// </summary>
        public class Connection : IDisposable
        {
            string _networkName;

            public Connection(string networkName, NetworkCredential credentials)
            {
                _networkName = networkName;

                var netResource = new NetResource()
                {
                    Scope = ResourceScope.GlobalNetwork,
                    iResourceType = ResourceType.Disk,
                    DisplayType = ResourceDisplaytype.Share,
                    RemoteName = networkName
                };

                var userName = string.IsNullOrEmpty(credentials.Domain)
                    ? credentials.UserName
                    : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

                var result = WNetAddConnection2(
                    netResource,
                    credentials.Password,
                    userName,
                    0);

                if (result != 0)
                {
                    throw new Win32Exception(result);
                }
            }

            ~Connection()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                WNetCancelConnection2(_networkName, 0, true);
            }

            [DllImport("mpr.dll")]
            private static extern int WNetAddConnection2(NetResource netResource,
                string password, string username, int flags);

            [DllImport("mpr.dll")]
            private static extern int WNetCancelConnection2(string name, int flags,
                bool force);

            [StructLayout(LayoutKind.Sequential)]
            public class NetResource
            {
                public ResourceScope Scope;
                public ResourceType iResourceType;
                public ResourceDisplaytype DisplayType;
                public int Usage;
                public string LocalName;
                public string RemoteName;
                public string Comment;
                public string Provider;

            }
            public enum ResourceScope : int
            {
                Connected = 1,
                GlobalNetwork,
                Remembered,
                Recent,
                Context
            };

            public enum ResourceType : int
            {
                Any = 0,
                Disk = 1,
                Print = 2,
                Reserved = 8,
            }

            public enum ResourceDisplaytype : int
            {
                Generic = 0x0,
                Domain = 0x01,
                Server = 0x02,
                Share = 0x03,
                File = 0x04,
                Group = 0x05,
                Network = 0x06,
                Root = 0x07,
                Shareadmin = 0x08,
                Directory = 0x09,
                Tree = 0x0a,
                Ndscontainer = 0x0b
            }
        }


        public class Hosts
        {
            public static IEnumerable<string> GetVisibleComputers(bool workgroupOnly = false)
            {
                Func<string, IEnumerable<DirectoryEntry>> immediateChildren = key => new DirectoryEntry("WinNT:" + key)  // WinNT or LDAP
                        .Children
                        .Cast<DirectoryEntry>();
                Func<IEnumerable<DirectoryEntry>, IEnumerable<string>> qualifyAndSelect = entries =>
                    entries.Where(c => c.SchemaClassName == "Computer")
                        .Select(c => c.Name);
                return (
                    !workgroupOnly ?
                        qualifyAndSelect(immediateChildren(String.Empty)
                            .SelectMany(d => d.Children.Cast<DirectoryEntry>()))
                        :
                        qualifyAndSelect(immediateChildren("//WORKGROUP"))
                ).ToArray();

                //string key;
                //if (workgroupOnly)
                //    key = "//WORKGROUP";
                //else
                //    key = "";
                //List<string> result = new List<string>();
                //try
                //{
                //    DirectoryEntry de = new DirectoryEntry("WinNT:" + key)
                //    { AuthenticationType = AuthenticationTypes.Secure };

                //    //System.Collections.IEnumerator enumerator = de.Children.GetEnumerator();
                //    //DirectoryEntry curD;
                //    //do
                //    //{
                //    //    curD = (DirectoryEntry)enumerator.Current;
                //    //    result.Add(curD.Name + " - " + curD.SchemaClassName);
                //    //}
                //    //while (enumerator.MoveNext());

                //    //foreach (DirectoryEntry d in de.Children)
                //    //{
                //    //    result.Add(d.Name + " - " + d.SchemaClassName);
                //    //}

                //    DirectorySearcher mySearcher = new DirectorySearcher(de);
                //    DirectoryEntry curD;
                //    foreach (SearchResult resEnt in mySearcher.FindAll())
                //    {
                //        try
                //        {
                //            curD = resEnt.GetDirectoryEntry();
                //            result.Add(curD.Name.ToString() + " - " + curD.Path.ToString());
                //        }
                //        catch
                //        {
                //        }
                //    }

                //}
                //catch (Exception err)
                //{
                //    ;
                //}
                //return result;
            }
            #region External Calls
            [DllImport("Netapi32.dll", SetLastError = true)]
            static extern int NetApiBufferFree(IntPtr Buffer);
            [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
            private static extern int NetShareEnum(
                 StringBuilder ServerName,
                 int level,
                 ref IntPtr bufPtr,
                 uint prefmaxlen,
                 ref int entriesread,
                 ref int totalentries,
                 ref int resume_handle
                 );
            #endregion
            #region External Structures
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct SHARE_INFO_1
            {
                public string shi1_netname;
                public uint shi1_type;
                public string shi1_remark;
                public SHARE_INFO_1(string sharename, uint sharetype, string remark)
                {
                    this.shi1_netname = sharename;
                    this.shi1_type = sharetype;
                    this.shi1_remark = remark;
                }
                public override string ToString()
                {
                    return shi1_netname;
                }
            }
            #endregion
            const uint MAX_PREFERRED_LENGTH = 0xFFFFFFFF;
            const int NERR_Success = 0;
            private enum NetError : uint
            {
                NERR_Success = 0,
                NERR_BASE = 2100,
                NERR_UnknownDevDir = (NERR_BASE + 16),
                NERR_DuplicateShare = (NERR_BASE + 18),
                NERR_BufTooSmall = (NERR_BASE + 23),
            }
            private enum SHARE_TYPE : uint
            {
                STYPE_DISKTREE = 0,
                STYPE_PRINTQ = 1,
                STYPE_DEVICE = 2,
                STYPE_IPC = 3,
                STYPE_SPECIAL = 0x80000000,
            }
            public static SHARE_INFO_1[] EnumNetShares(string Server)
            {
                List<SHARE_INFO_1> ShareInfos = new List<SHARE_INFO_1>();
                int entriesread = 0;
                int totalentries = 0;
                int resume_handle = 0;
                int nStructSize = Marshal.SizeOf(typeof(SHARE_INFO_1));
                IntPtr bufPtr = IntPtr.Zero;
                StringBuilder server = new StringBuilder(Server);
                int ret = NetShareEnum(server, 1, ref bufPtr, MAX_PREFERRED_LENGTH, ref entriesread, ref totalentries, ref resume_handle);
                if (ret == NERR_Success)
                {
                    IntPtr currentPtr = bufPtr;
                    for (int i = 0; i < entriesread; i++)
                    {
                        SHARE_INFO_1 shi1 = (SHARE_INFO_1)Marshal.PtrToStructure(currentPtr, typeof(SHARE_INFO_1));
                        ShareInfos.Add(shi1);
                        currentPtr += nStructSize;
                    }
                    NetApiBufferFree(bufPtr);
                    return ShareInfos.ToArray();
                }
                else
                {
                    ShareInfos.Add(new SHARE_INFO_1("ERROR=" + ret.ToString(), 10, string.Empty));
                    return ShareInfos.ToArray();
                }
            }
        }
    }
}
