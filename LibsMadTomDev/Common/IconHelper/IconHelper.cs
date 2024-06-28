using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MadTomDev.Common
{
    public class IconHelper
    {
        public class FileSystem
        {
            private FileSystem() { }
            private static FileSystem instance = null;
            public static FileSystem Instance
            {
                get
                {
                    if (instance == null)
                        instance = new FileSystem();
                    return instance;
                }
            }

            // for "exe,url,lnk", use full file path as key
            private static string flag_dir = "[dir]";
            private static string flag_file = "[file]";
            private Dictionary<string, BitmapSource> iconCache_small = new Dictionary<string, BitmapSource>();
            private Dictionary<string, BitmapSource> iconCache_big = new Dictionary<string, BitmapSource>();

            public bool HaveIcon(string path, bool smallIcon, bool isDirectory)
            {
                return HaveIcon(path, smallIcon, isDirectory, out string missing);
            }
            public bool HaveIcon(string path, bool smallIcon, bool isDirectory, out string ext)
            {
                ext = GetExt(path, isDirectory);
                if (smallIcon)
                {
                    return iconCache_small.ContainsKey(ext);
                }
                else
                {
                    return iconCache_big.ContainsKey(ext);
                }
            }
            public static string GetExt(string path, bool isDirectory)
            {
                string ext;
                if (isDirectory)
                {
                    if (path.Length <= 3 && path[1] == ':')
                        ext = path;
                    else
                        ext = flag_dir;
                }
                else
                {
                    ext = Path.GetExtension(path).ToLower();
                    if (string.IsNullOrWhiteSpace(ext))
                        ext = flag_file;
                    else if (ext == ".exe" || ext == ".url" || ext == ".lnk")
                        ext = path;
                }
                return ext;
            }

            public BitmapSource GetDirIcon(bool isSmallIcon)
            {
                if (isSmallIcon)
                {
                    if (iconCache_small.ContainsKey(flag_dir))
                        return iconCache_small[flag_dir];
                }
                else
                {
                    if (iconCache_big.ContainsKey(flag_dir))
                        return iconCache_big[flag_dir];
                }

                DirectoryInfo[] dirs = new DirectoryInfo("C:\\").GetDirectories();
                return GetIcon(dirs[0].FullName, isSmallIcon, true);
            }

            private ImmutableHashSet<string> loadingExts = ImmutableHashSet.Create<string>();
            public BitmapSource GetIcon(string path, bool smallIcon, bool isDirectory)
            {
                return GetIcon(path, smallIcon, isDirectory, out string missing);
            }
            public BitmapSource GetIcon(string path, bool smallIcon, bool isDirectory, out string ext)
            {
                ext = GetExt(path, isDirectory);

                Dictionary<string, BitmapSource> imgCache;

                if (smallIcon)
                {
                    imgCache = iconCache_small;
                }
                else
                {
                    imgCache = iconCache_big;
                }
                if (imgCache.ContainsKey(ext))
                    return imgCache[ext];

                string aExt;
                if (ext.StartsWith("."))
                {
                    aExt = ext;
                }
                else
                {
                    if (ext.EndsWith("exe"))
                        aExt = ".exe";
                    else
                        aExt = ext;
                }
                while (loadingExts.Contains(aExt))
                {
                    Task.Delay(5).Wait();
                }

                if (imgCache.ContainsKey(ext))
                    return imgCache[ext];

                loadingExts.Add(aExt);

                // SHGFI_USEFILEATTRIBUTES takes the file name and attributes into account if it doesn't exist
                uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
                if (smallIcon)
                    flags |= SHGFI_SMALLICON;

                uint attributes = FILE_ATTRIBUTE_NORMAL;
                if (isDirectory)
                    attributes |= FILE_ATTRIBUTE_DIRECTORY;

                SHFILEINFO shfi;
                if (0 != SHGetFileInfo(
                            path,
                            attributes,
                            out shfi,
                            (uint)Marshal.SizeOf(typeof(SHFILEINFO)),
                            flags))
                {
                    if (shfi.hIcon != IntPtr.Zero)
                    {
                        BitmapSource result = Imaging.CreateBitmapSourceFromHIcon(
                                        shfi.hIcon,
                                        Int32Rect.Empty,
                                        BitmapSizeOptions.FromEmptyOptions());
                        if (result != null)
                        {
                            result.Freeze();

                            // 可能是因为并行的原因，有时候会存在这个key的图片；
                            lock (imgCache)
                            {
                                if (!imgCache.ContainsKey(ext))
                                    imgCache.Add(ext, result);
                            }
                        }
                        loadingExts.Remove(aExt);
                        return result;
                    }
                }
                loadingExts.Remove(aExt);
                return null;
            }
                       

            public delegate void IconGotDelegate(FileSystem sender, string path, string ext, bool smallIcon, bool isDirectory, BitmapSource icon);
            public event IconGotDelegate IconGot;
            public Task GetIconAsync(string path, bool smallIcon, bool isDirectory)
            {
                return Task.Factory.StartNew(() =>
                {
                    BitmapSource icon = GetIcon(path, smallIcon, isDirectory, out string ext);
                    IconGot?.Invoke(this, path, ext, smallIcon, isDirectory, icon);
                });
            }

            #region dll import

            [StructLayout(LayoutKind.Sequential)]
            private struct SHFILEINFO
            {
                public IntPtr hIcon;
                public int iIcon;
                public uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
                public string szTypeName;
            }

            [DllImport("shell32")]
            private static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint flags);

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
        }

        /// <summary>
        /// 初始化的实例默认会将Shell32中所有图片的句柄读出来；
        /// GetIcon方法可返回需要的图标，当然必须提前知道图标的索引位置，如果这个图标在缓存中存在则返回这个缓存的图标；
        /// LoadAllIcons方法将加载所有图标到缓存；
        /// * 关于图标的索引位置，请使用SystemResource的systemIcons展示窗口查询；
        /// </summary>
        public class Shell32Icons
        {
            [DllImport("Shell32.dll")]
            private extern static int ExtractIconEx(string libName, int iconIndex, IntPtr[] largeIcon, IntPtr[] smallIcon, int nIcons);

            public IntPtr[] largeIconIndexes;
            public IntPtr[] smallIconIndexes;
            public Dictionary<IntPtr, BitmapSource> IconCacheSmall = new Dictionary<IntPtr, BitmapSource>();
            public Dictionary<IntPtr, BitmapSource> IconCacheLarge = new Dictionary<IntPtr, BitmapSource>();
            private Shell32Icons()
            {
                ReloadIndex();
            }
            public void ReloadIndex()
            {
                largeIconIndexes = new IntPtr[1000];
                smallIconIndexes = new IntPtr[1000];

                ExtractIconEx("Shell32.dll", 0, largeIconIndexes, smallIconIndexes, 1000);

            }
            public void LoadAllIcons()
            {
                IconCacheSmall.Clear();
                IconCacheLarge.Clear();
                IntPtr ip;
                for (int i = 0; i < smallIconIndexes.Length; i++)
                {
                    ip = smallIconIndexes[i];
                    try
                    {
                        IconCacheSmall.Add(
                            ip,
                            Imaging.CreateBitmapSourceFromHIcon(
                                ip,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions())
                        );
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
                for (int i = 0; i < largeIconIndexes.Length; i++)
                {
                    ip = largeIconIndexes[i];
                    try
                    {
                        IconCacheLarge.Add(
                            ip,
                            Imaging.CreateBitmapSourceFromHIcon(
                                ip,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions())
                        );
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }

            private bool GetIcon_loading = false;
            public BitmapSource GetIcon(int index, bool isLargeIconOrSmall = true)
            {
                IntPtr key;
                Dictionary<IntPtr, BitmapSource> iconCache;
                if (isLargeIconOrSmall)
                {
                    key = largeIconIndexes[index];
                    iconCache = IconCacheLarge;
                }
                else
                {
                    key = smallIconIndexes[index];
                    iconCache = IconCacheSmall;
                }

                if (iconCache.ContainsKey(key))
                {
                    return iconCache[key];
                }
                else
                {
                    try
                    {
                        while (GetIcon_loading)
                        {
                            Task.Delay(10).Wait();
                        }
                        if (iconCache.ContainsKey(key))
                        {
                            return iconCache[key];
                        }
                        GetIcon_loading = true;
                        BitmapSource result = Imaging.CreateBitmapSourceFromHIcon(
                            key,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        iconCache.Add(key, result);
                        GetIcon_loading = false;
                        result.Freeze();
                        return result;
                    }
                    catch (Exception err)
                    {
                        return null;
                    }
                    finally
                    {
                        GetIcon_loading = false;
                    }
                }
            }


            private static Shell32Icons _instance = null;
            public static Shell32Icons Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new Shell32Icons();
                    }
                    return _instance;
                }
            }


            private Dictionary<int, BitmapSource> CustomIconCacheSmall = new Dictionary<int, BitmapSource>();
            private Dictionary<int, BitmapSource> CustomIconCacheLarge = new Dictionary<int, BitmapSource>();
            /// <summary>
            /// icon from scratch; 0-multipleDirs
            /// </summary>
            /// <param name="index"></param>
            /// <param name="isLargeIconOrSmall"></param>
            /// <returns></returns>
            public BitmapSource GetCustomIcon(int index, bool isLargeIconOrSmall = true)
            {
                if (index >= 0 && index <= 0)
                {
                    #region init icon if needed

                    if (!CustomIconCacheSmall.ContainsKey(index))
                    {
                        // source from image 3(directory)
                        //BitmapSource dirImageLarge = GetIcon(3, true).Clone();
                        //WriteableBitmap multiDirImageLarge = new WriteableBitmap(
                        //    dirImageLarge.PixelWidth, dirImageLarge.PixelHeight,
                        //    dirImageLarge.DpiX, dirImageLarge.DpiY,
                        //    PixelFormats.Bgra32, null);

                        //int stride = dirImageLarge.PixelWidth * 4;
                        //byte[] imgData = new byte[stride * dirImageLarge.PixelHeight];
                        //dirImageLarge.CopyPixels(imgData, stride, 0);

                        //Int32Rect rect = new Int32Rect(4, 0, dirImageLarge.PixelWidth - 4, dirImageLarge.PixelHeight - 4);
                        //multiDirImageLarge.WritePixels(rect, imgData, stride, stride * 4);


                        //rect.X = 0; rect.Y = 0;
                        //rect.Width += 4; rect.Height += 4;
                        //multiDirImageLarge.WritePixels(rect, imgData, stride, 0);

                        //rect.X = 0; rect.Y = 4;
                        //rect.Width -= 4; rect.Height -= 4;
                        //multiDirImageLarge.WritePixels(rect, imgData, stride, 16);

                        //multiDirImageLarge.Freeze();

                        CustomIconCacheLarge.Add(index, MakeStackImage(GetIcon(3, true).Clone(), 4));
                        CustomIconCacheSmall.Add(index, MakeStackImage(GetIcon(3, false).Clone(), 2));

                    }

                    BitmapSource MakeStackImage(BitmapSource oImg, int widthOffset)
                    {
                        System.Drawing.Bitmap oBitmat = UI.QuickGraphics.BitmapSourceToBitmap(oImg);
                        System.Drawing.Bitmap tBitmat = new System.Drawing.Bitmap(oBitmat.Width, oBitmat.Height);
                        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(tBitmat))
                        {
                            g.DrawImage(oBitmat, new System.Drawing.Point(widthOffset, -widthOffset));
                            g.DrawImage(oBitmat, new System.Drawing.Point(0, 0));
                            g.DrawImage(oBitmat, new System.Drawing.Point(-widthOffset, widthOffset));
                        }
                        BitmapSource resultImg = UI.QuickGraphics.BitmapToBitmapSource(tBitmat);
                        resultImg.Freeze();
                        return resultImg;
                    }

                    #endregion

                    if (isLargeIconOrSmall)
                        return CustomIconCacheLarge[index];
                    else
                        return CustomIconCacheSmall[index];
                }
                return null;
            }
        }

        public class SystemIcons
        {
            private SystemIcons()
            {
                Type siType = typeof(System.Drawing.SystemIcons);
                Type iconType = typeof(System.Drawing.Icon);
                System.Drawing.Icon icon;
                BitmapSource bitmapSource;
                foreach (PropertyInfo pi in siType.GetProperties())
                {
                    if (pi.PropertyType == iconType)
                    {
                        icon = (System.Drawing.Icon)pi.GetValue(null);
                        dictIcon.Add(pi.Name, icon);
                        bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                                icon.Handle,
                                Int32Rect.Empty, //new Int32Rect(0, 0, 32, 32),
                                BitmapSizeOptions.FromWidthAndHeight(32, 32));
                        bitmapSource.Freeze();
                        dictIconImage32.Add(pi.Name, bitmapSource);
                        bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                                icon.Handle,
                                Int32Rect.Empty, //new Int32Rect(0, 0, 16, 16),
                                BitmapSizeOptions.FromWidthAndHeight(16, 16));
                        bitmapSource.Freeze();
                        dictIconImage16.Add(pi.Name, bitmapSource);
                    }
                }
            }
            private static SystemIcons instance = null;
            public static SystemIcons Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new SystemIcons();
                    }
                    return instance;
                }
            }
            public Dictionary<string, System.Drawing.Icon> dictIcon = new Dictionary<string, System.Drawing.Icon>();
            public Dictionary<string, BitmapSource> dictIconImage32 = new Dictionary<string, BitmapSource>();
            public Dictionary<string, BitmapSource> dictIconImage16 = new Dictionary<string, BitmapSource>();

            public BitmapSource this[string key, bool isLargeIcon]
            {
                get
                {
                    if (isLargeIcon)
                    {
                        if (dictIconImage32.ContainsKey(key))
                            return dictIconImage32[key];
                        else
                            return null;
                    }
                    else
                    {
                        if (dictIconImage16.ContainsKey(key))
                            return dictIconImage16[key];
                        else
                            return null;
                    }
                }
            }
        }
    }
}
