using MadTomDev.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static MadTomDev.App.Classes.Things;

namespace MadTomDev.App.Classes
{
    public class ImageIO
    {
        //private static readonly BitmapImage _AppIcon = new BitmapImage(new Uri("pack://application:,,,/icon.ico"));
        //public static BitmapImage AppIcon { get => _AppIcon; }
        private static readonly BitmapImage _Image_Unknow = new BitmapImage(new Uri("pack://application:,,,/Images/QMark_48.png"));
        public static BitmapImage Image_Unknow { get => _Image_Unknow; }
        private static readonly BitmapImage _Image_MouseCursorGreen = new BitmapImage(new Uri("pack://application:,,,/Images/mouse-cursor-pointer.png"));
        public static BitmapImage Image_MouseCursorGreen { get => _Image_MouseCursorGreen; }
        private static readonly BitmapImage _Image_XmlFile = new BitmapImage(new Uri("pack://application:,,,/Images/xmlFile.png"));
        public static BitmapImage Image_XmlFile { get => _Image_XmlFile; }

        public static void SetSceneCover(ImageSource? img)
        {
            string sceneDir = SceneMgr.Instance.selectedTreeViewNode.GetDirFullName();
            string imgFile = Path.Combine(sceneDir, SceneMgr.FILENAME_SCENECOVER);
            if (img == null)
            {
                if (File.Exists(imgFile))
                {
                    Data.Utilities.MSVBFileOperation.Delete(new string[] { imgFile }, out Exception err);
                    if (err != null)
                    {
                        throw err;
                    }
                }
                return;
            }
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)img));
            using (FileStream fs = new FileStream(imgFile, FileMode.Create))
            {
                encoder.Save(fs);
                fs.Flush();
            }
        }
        public static BitmapImage? GetSceneCover(string sceneDir)
        {
            string imgFile = Path.Combine(sceneDir, SceneMgr.FILENAME_SCENECOVER);
            if (File.Exists(imgFile))
            {
                return LoadImgNoLock(imgFile);
            }
            return null;
        }
        public static BitmapImage? GetSceneCover()
        {
            string sceneDir = SceneMgr.Instance.selectedTreeViewNode.GetDirFullName();
            return GetSceneCover(sceneDir);
        }
        private static BitmapImage LoadImgNoLock(string imgFile)
        {
            BitmapImage result;
            using (FileStream fs = new FileStream(imgFile, FileMode.Open))
            {
                result = new BitmapImage();
                result.BeginInit();
                result.StreamSource = fs;
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.EndInit();
            }
            return result;
        }

        public static void PutIn(string sceneDir, Guid imgId, string? imgName, ImageSource img)
        {
            string imgDir = Path.Combine(sceneDir, SceneMgr.DIRNAME_IMAGES);
            if (!Directory.Exists(imgDir))
            {
                Directory.CreateDirectory(imgDir);
            }
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img.ToBitmapImage()));
            string fileName = $"{imgName}.{imgId}.png";
            fileName = Data.Utilities.FilePath.CorrectorName(fileName, out HashSet<char> missing);
            using (FileStream fs = new FileStream(
                Path.Combine(imgDir, fileName),
                FileMode.Create))
            {
                encoder.Save(fs);
                fs.Flush();
            }
            if (imgDict.ContainsKey(imgId))
            {
                imgDict[imgId] = img.ToBitmapImage();
            }
            else
            {
                imgDict.Add(imgId, img.ToBitmapImage());
            }
        }
        public static void PutIn(Things.ThingBase? t, string? nameAs, ImageSource img)
        {
            if (t == null)
            {
                return;
            }
            string? sceneDir = t.GetSceneDir();
            if (string.IsNullOrWhiteSpace(sceneDir))
            {
                return;
            }
            ImageIO.PutIn(
                sceneDir,
                t.id,
                nameAs != null ? nameAs : t.name,
                img);
        }

        private static Dictionary<Guid, BitmapImage> imgDict = new Dictionary<Guid, BitmapImage>();
        public static BitmapImage? GetOut(string? sceneDir, Guid imgId)
        {
            if (imgDict.ContainsKey(imgId))
            {
                return imgDict[imgId];
            }
            if (string.IsNullOrWhiteSpace(sceneDir))
            {
                return null;
            }
            DirectoryInfo imgDir = new DirectoryInfo(Path.Combine(sceneDir, SceneMgr.DIRNAME_IMAGES));
            if (!imgDir.Exists)
            {
                return null;
            }
            FileInfo[] imgFiles = imgDir.GetFiles($"*{imgId}*");
            if (imgFiles.Length > 0)
            {
                return LoadImgNoLock(imgFiles[0].FullName);
            }
            return null;
        }
        public static BitmapImage? GetOut(Things.ThingBase? thing)
        {
            if (thing == null || thing.Container == null || thing.Container.Scene == null)
            {
                return null;
            }
            BitmapImage ? result = GetOut(thing.GetSceneDir(), thing.id);
            int i = thing.InheritedList.Count;
            ThingBase parent;
            if (result == null &&  i > 0)
            {
                for (--i; i>=0;--i)
                {
                    parent = thing.InheritedList[i];
                    result = GetOut(parent.GetSceneDir(), parent.id);
                    if (result != null)
                    {
                        break;
                    }
                }
            }
            return result;
        }

        public static void Remove(string? sceneDir, Guid imgId)
        {
            if (imgDict.ContainsKey(imgId))
            {
                imgDict.Remove(imgId);
            }
            if (string.IsNullOrWhiteSpace(sceneDir))
            {
                return;
            }
            DirectoryInfo imgDir = new DirectoryInfo(Path.Combine(sceneDir, SceneMgr.DIRNAME_IMAGES));
            if (!imgDir.Exists)
            {
                return;
            }
            FileInfo[] imgFiles = imgDir.GetFiles($"*{imgId}*");
            if (imgFiles.Length > 0)
            {
                Data.Utilities.MSVBFileOperation.Delete(new string[] { imgFiles[0].FullName }, out Exception err);
                if (err != null)
                {
                    throw err;
                }
            }
        }
        public static void Remove(Things.ThingBase t)
        {
            if (t != null)
            {
                Remove(t.GetSceneDir(), t.id);
            }
        }
        public static bool TryChangeName(string sceneDir, Guid imgId, string newImgName, out Exception? err)
        {
            err = null;
            DirectoryInfo imgDir = new DirectoryInfo(Path.Combine(sceneDir, SceneMgr.DIRNAME_IMAGES));
            FileInfo[] imgFiles = imgDir.GetFiles($"*{imgId}*");
            if (imgFiles.Length > 0)
            {
                try
                {
                    string newName = $"{newImgName}.{imgId}.png";
                    newName = Data.Utilities.FilePath.CorrectorName(newName, out HashSet<char> missing);
                    string? dirName = imgFiles[0].DirectoryName;
                    if (dirName == null)
                    {
                        return false;
                    }
                    File.Move(imgFiles[0].FullName, Path.Combine(dirName, newName));
                    return true;
                }
                catch (Exception e1)
                {
                    err = e1;
                }
            }
            return false;
        }
    }
}
