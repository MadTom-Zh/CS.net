using MadTomDev.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Point = System.Drawing.Point;

namespace MadTomDev.App.Classes
{
    internal class PresetLib
    {
        public static void SetPreset01(MainWindow mainWindow)
        {
            Point posiOffset = new Point(8, 3);
            mainWindow.CreateOrChangeBlock(1 + posiOffset.X, 0 + posiOffset.Y, 2);
            mainWindow.CreateOrChangeBlock(2 + posiOffset.X, 0 + posiOffset.Y, 2);
            mainWindow.CreateOrChangeBlock(3 + posiOffset.X, 0 + posiOffset.Y, 3);
            mainWindow.CreateOrChangeBlock(1 + posiOffset.X, 1 + posiOffset.Y, 2);
            mainWindow.CreateOrChangeBlock(2 + posiOffset.X, 1 + posiOffset.Y, 0);
            mainWindow.CreateOrChangeBlock(3 + posiOffset.X, 1 + posiOffset.Y, 3);
            mainWindow.CreateOrChangeBlock(0 + posiOffset.X, 2 + posiOffset.Y, 1);
            mainWindow.CreateOrChangeBlock(1 + posiOffset.X, 2 + posiOffset.Y, 4);
            mainWindow.CreateOrChangeBlock(2 + posiOffset.X, 2 + posiOffset.Y, 0);
            mainWindow.CreateOrChangeBlock(3 + posiOffset.X, 2 + posiOffset.Y, 4);
            mainWindow.CreateOrChangeBlock(4 + posiOffset.X, 2 + posiOffset.Y, 1);
            mainWindow.CreateOrChangeBlock(0 + posiOffset.X, 3 + posiOffset.Y, 0);
            mainWindow.CreateOrChangeBlock(1 + posiOffset.X, 3 + posiOffset.Y, 0);
            mainWindow.CreateOrChangeBlock(2 + posiOffset.X, 3 + posiOffset.Y, 0);
            mainWindow.CreateOrChangeBlock(3 + posiOffset.X, 3 + posiOffset.Y, 0);
            mainWindow.CreateOrChangeBlock(4 + posiOffset.X, 3 + posiOffset.Y, 2);
            mainWindow.CreateOrChangeBlock(0 + posiOffset.X, 4 + posiOffset.Y, 0);
            mainWindow.CreateOrChangeBlock(1 + posiOffset.X, 4 + posiOffset.Y, 0);
            mainWindow.CreateOrChangeBlock(2 + posiOffset.X, 4 + posiOffset.Y, 4);
            mainWindow.CreateOrChangeBlock(3 + posiOffset.X, 4 + posiOffset.Y, 2);
            mainWindow.CreateOrChangeBlock(4 + posiOffset.X, 4 + posiOffset.Y, 2);
            mainWindow.CreateOrChangeBlock(0 + posiOffset.X, 5 + posiOffset.Y, 0);
            mainWindow.CreateOrChangeBlock(1 + posiOffset.X, 5 + posiOffset.Y, 0);
            mainWindow.CreateOrChangeBlock(2 + posiOffset.X, 5 + posiOffset.Y, 2);
            mainWindow.CreateOrChangeBlock(3 + posiOffset.X, 5 + posiOffset.Y, 1);
            mainWindow.CreateOrChangeBlock(0 + posiOffset.X, 6 + posiOffset.Y, 1);
            mainWindow.CreateOrChangeBlock(1 + posiOffset.X, 6 + posiOffset.Y, 2);
            mainWindow.CreateOrChangeBlock(2 + posiOffset.X, 6 + posiOffset.Y, 0);
            mainWindow.CreateOrChangeBlock(3 + posiOffset.X, 6 + posiOffset.Y, 2);
            mainWindow.CreateOrChangeBlock(1 + posiOffset.X, 7 + posiOffset.Y, 2);
            mainWindow.CreateOrChangeBlock(2 + posiOffset.X, 7 + posiOffset.Y, 2);
            mainWindow.CreateOrChangeBlock(3 + posiOffset.X, 7 + posiOffset.Y, 3);
        }
        public static void SetPreset02(MainWindow mainWindow)
        {
            Point posiOffset = new Point(8, 3);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 0, posiOffset.Y + 6, 0);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 1, posiOffset.Y + 6, 0);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 2, posiOffset.Y + 6, 0);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 3, posiOffset.Y + 6, 0);

            mainWindow.CreateOrChangeBlock(posiOffset.X + 0, posiOffset.Y + 7, 1);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 1, posiOffset.Y + 7, 1);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 2, posiOffset.Y + 7, 1);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 3, posiOffset.Y + 7, 1);

            mainWindow.CreateOrChangeBlock(posiOffset.X + 4, posiOffset.Y + 0, 0);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 4, posiOffset.Y + 1, 0);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 4, posiOffset.Y + 2, 0);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 4, posiOffset.Y + 3, 0);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 4, posiOffset.Y + 4, 0);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 4, posiOffset.Y + 5, 0);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 4, posiOffset.Y + 6, 0);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 4, posiOffset.Y + 7, 3);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 4, posiOffset.Y + 8, 1);

            mainWindow.CreateOrChangeBlock(posiOffset.X + 5, posiOffset.Y + 0, 2);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 5, posiOffset.Y + 1, 2);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 5, posiOffset.Y + 2, 1);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 5, posiOffset.Y + 3, 1);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 5, posiOffset.Y + 4, 3);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 5, posiOffset.Y + 5, 4);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 5, posiOffset.Y + 6, 0);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 5, posiOffset.Y + 7, 0);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 5, posiOffset.Y + 8, 1);

            mainWindow.CreateOrChangeBlock(posiOffset.X + 6, posiOffset.Y + 0, 0);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 6, posiOffset.Y + 1, 2);

            mainWindow.CreateOrChangeBlock(posiOffset.X + 6, posiOffset.Y + 3, 1);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 6, posiOffset.Y + 4, 2);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 6, posiOffset.Y + 5, 0);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 6, posiOffset.Y + 6, 3);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 6, posiOffset.Y + 7, 2);
            mainWindow.CreateOrChangeBlock(posiOffset.X + 6, posiOffset.Y + 8, 1);
        }
        public static void SetPreset03(MainWindow mw)
        {
            Point p = new Point(6, 1);
            _Create(mw, p, 2, 0, 1);
        }
        public static void SetPreset(MainWindow mw, PresetData data)
        {
            mw.SetBoardOffset(new Point());
            foreach (Point p in data.data.Keys)
            {
                _Create(mw, data.offset, p.X, p.Y, data.data[p]);
            }
        }
        private static void _Create(MainWindow mw, Point offset, int x, int y, int signal)
        {
            mw.CreateOrChangeBlock(offset.X + x, offset.Y + y, signal);
        }


        private static string PresetsDirName = "Presets";

        public class PresetData
        {
            public string Name { private set; get; }
            public Point offset = new Point();
            public Dictionary<Point, int> data = new Dictionary<Point, int>();

            public PresetData(string name)
            {
                this.Name = name;
            }
        }

        public static List<PresetData> LoadPresets()
        {
            List<PresetData> presetDataList = new List<PresetData>();
            if (!Directory.Exists(PresetsDirName))
            {
                return presetDataList;
            }
            foreach (FileInfo pf in new DirectoryInfo(PresetsDirName).GetFiles("*.txt"))
            {
                try
                {
                    string fileName = pf.Name;
                    PresetData newPData = new PresetData(fileName.Substring(0, fileName.Length - 4));
                    string[] allLines = File.ReadAllLines(pf.FullName);
                    Point? testPt = allLines[0].TryGetDPoint();
                    if (testPt == null)
                    {
                        continue;
                    }
                    newPData.offset = (Point)testPt;

                    for (int i = 1, iv = allLines.Length; i < iv; ++i)
                    {
                        string[] parts = allLines[i].Split(',');
                        Point key = new Point(int.Parse(parts[0]), int.Parse(parts[1]));
                        newPData.data.Add(key, int.Parse(parts[2]));
                    }
                    presetDataList.Add(newPData);
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Error in file [{pf.Name}]."
                        + Environment.NewLine + Environment.NewLine
                        + err.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return presetDataList;
        }
        public static PresetData? SavePreset(string name, Point offset, BlockMatrix bMatrix)
        {
            if (!Directory.Exists(PresetsDirName))
            {
                Directory.CreateDirectory(PresetsDirName);
            }
            try
            {
                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PresetsDirName, name + ".txt");
                PresetData result = new PresetData(name);
                using (FileStream fs = new FileStream(file, FileMode.CreateNew))
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.WriteLine(offset.X + "," + offset.Y);
                    result.offset = offset;
                    foreach (BlockMatrix.OneBlockData b in bMatrix.allBlocksList)
                    {
                        int sInt = b.StatuInt;
                        sw.WriteLine(b.position.X + "," + b.position.Y + "," + sInt.ToString());
                        result.data.Add(b.position, sInt);
                    }
                }
                return result;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        public static bool DeletePreset(string? name)
        {
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PresetsDirName, name + ".txt");
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                    return true;
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return false;
        }
    }
}
