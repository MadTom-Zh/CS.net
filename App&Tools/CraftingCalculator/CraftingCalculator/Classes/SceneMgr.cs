using MadTomDev.App.VMs;
using MadTomDev.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MadTomDev.App.Classes
{
    public class SceneMgr
    {
        private SceneMgr()
        {
        }

        private static SceneMgr instance;
        public static SceneMgr Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SceneMgr();
                }
                return instance;
            }
        }

        public static string DIRNAME_DEPOTS = "Depots";
        public static string FILENAME_SCENEINFO = "Info.txt";
        public static string FILENAME_SCENECOVER = "Cover.png";
        public static string DIRNAME_IMAGES = "Images";
        public static string FILENAME_THINGS = "Things.csv";
        public static string FILENAME_CHANNELS = "Channels.csv";
        public static string FILENAME_RECIPES = "Recipes.csv";
        public static string FILENAME_PREFILTERLISTS = "PreFilters.txt";
        public static string FILENAME_FGAM_COLLAPSED_CHANNELS = "FGAMcollapsedChannels.txt";

        public static string DIRNAME_FGAM = "FGAM";

        public static string FLAG_PROCEDURES = "Procedures";
        public static string FLAG_ACCESSORIES = "Accessories";
        public static string FLAG_CHANNELS = "Channels";
        public static string FLAG_INPUTS = "Inputs";
        public static string FLAG_OUTPUTS = "Outputs";
        public static string FILENAME_SELECTEDLANGUAGE = "SelectedLanguage.txt";
        public static string VALUE_NULL = "null";

        public string GetSelectedSceneDirFullPath(string? subDir)
        {
            if (subDir == null)
            {
                return selectedTreeViewNode.GetDirFullName();
            }
            else
            {
                return Path.Combine(selectedTreeViewNode.GetDirFullName(), subDir);
            }
        }

        public ObservableCollection<object> treeViewSource = new ObservableCollection<object>();
        public TreeViewNodeModelScene selectedTreeViewNode;
        public void ReLoadSceneTreeRoots()
        {
            treeViewSource.Clear();
            // add loading node
            UI.VMBase.TreeViewNodeModelBase loadingNode = new UI.VMBase.TreeViewNodeModelBase(null)
            {
                Text = TreeViewNodeModelScene.TxLoading,
            };
            treeViewSource.Add(loadingNode);

            if (!Directory.Exists(DIRNAME_DEPOTS))
            {
                Directory.CreateDirectory(DIRNAME_DEPOTS);
            }
            foreach (DirectoryInfo di in new DirectoryInfo(DIRNAME_DEPOTS).GetDirectories())
            {
                treeViewSource.Add(LoadScene(di));
            }
            treeViewSource.Remove(loadingNode);
        }
        private TreeViewNodeModelScene LoadScene(DirectoryInfo di, TreeViewNodeModelScene? parent = null)
        {
            TreeViewNodeModelScene result = new TreeViewNodeModelScene(parent)
            { dirName = di.Name, };
            result.ActionExpandedChanged = TreeNodeExpandedChanged;
            string fileInfo = Path.Combine(di.FullName, FILENAME_SCENEINFO);
            if (File.Exists(fileInfo))
            {
                // load image
                BitmapImage? icon = ImageIO.GetSceneCover(di.FullName);
                if (icon == null)
                {
                    result.Icon = ImageIO.Image_Unknow;
                }
                else
                {
                    result.Icon = icon;
                }
                // load name n' description
                string? line;
                bool firstLine = true;
                StringBuilder desBdr = new StringBuilder();
                using (FileStream fs = new FileStream(fileInfo, FileMode.Open))
                using (StreamReader reader = new StreamReader(fs))
                {
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();
                        if (line != null)
                        {
                            if (firstLine)
                            {
                                result.Text = line;
                            }
                            else
                            {
                                desBdr.AppendLine(line);
                            }
                        }
                        firstLine = false;
                    }
                }
                result.Description = desBdr.ToString();
            }
            else
            {
                result.Icon = ImageIO.Image_Unknow;
                result.Text = "[No name]";
                result.Description = "[No description]";
            }
            bool hasSub = false;
            foreach (DirectoryInfo subDi in di.GetDirectories())
            {
                if (CheckIsSceneUsedDirName(subDi.Name))
                {
                    continue;
                }
                else
                {
                    hasSub = true;
                    break;
                }
            }
            if (hasSub)
            {
                result.AddLoadingLabelNode();
            }
            return result;
        }
        public void LoadSubScenes(ref TreeViewNodeModelScene parentNode)
        {
            TreeViewNodeModelScene subNode;
            bool hasSub;
            foreach (DirectoryInfo di in new DirectoryInfo(parentNode.GetDirFullName()).GetDirectories())
            {
                if (CheckIsSceneUsedDirName(di.Name))
                {
                    continue;
                }
                else
                {
                    hasSub = false;
                    foreach (object c in parentNode.Children)
                    {
                        if (c is TreeViewNodeModelScene)
                        {
                            subNode = (TreeViewNodeModelScene)c;
                            if (subNode.dirName == di.Name)
                            {
                                hasSub = true;
                                break;
                            }
                        }
                    }
                    if (!hasSub)
                    {
                        subNode = LoadScene(di, parentNode);
                        parentNode.Children.Add(subNode);
                    }
                }
            }
        }
        public bool CheckIsSceneUsedDirName(string dirName)
        {
            string name = dirName.Trim().ToLower();
            return name == DIRNAME_IMAGES.ToLower()
                || name == DIRNAME_FGAM.ToLower();
        }

        private void TreeNodeExpandedChanged(object node)
        {
            if (node is TreeViewNodeModelScene)
            {
                TreeViewNodeModelScene tNode = (TreeViewNodeModelScene)node;
                if (tNode.IsExpanded)
                {
                    if (tNode.HasLoadingLabelNode())
                    {
                        LoadSubScenes(ref tNode);
                        tNode.RemoveLoadingLabelNodes();
                    }
                }
                else
                {
                    if (tNode.Children.Count > 0 && !tNode.HasChild(selectedTreeViewNode))
                    {
                        tNode.Children.Clear();
                        tNode.AddLoadingLabelNode();
                    }
                }
            }
        }

        public List<TreeViewNodeModelScene> GetSceneChain(TreeViewNodeModelScene curScene)
        {
            List<TreeViewNodeModelScene> result = new List<TreeViewNodeModelScene>();
            result.Add(curScene);
            VMs.TreeViewNodeModelScene itemParent = (TreeViewNodeModelScene)curScene.parent;
            while (itemParent != null)
            {
                result.Insert(0, itemParent);
                itemParent = (TreeViewNodeModelScene)itemParent.parent;
            }
            return result;
        }

        public void UpdateSceneInfo(TreeViewNodeModelScene scene)
        {
            using (FileStream fs = new FileStream(Path.Combine(scene.GetDirFullName(), FILENAME_SCENEINFO), FileMode.Create))
            using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
            {
                writer.WriteLine(scene.Text);
                writer.Write(scene.Description);
                writer.Flush();
                fs.Flush();
            }
        }

        public TreeViewNodeModelScene CreateScene(TreeViewNodeModelScene? parentScene, string sceneName)
        {
            if (CheckIsSceneUsedDirName(sceneName))
            {
                throw new InvalidDataException($"Can't use [{sceneName}] as scene name.");
            }
            string dir = Data.Utilities.FilePath.CorrectorName(sceneName, out HashSet<char> missing);
            string dirParent;
            if (parentScene == null)
            {
                dirParent = DIRNAME_DEPOTS;
            }
            else
            {
                dirParent = parentScene.GetDirFullName();
            }
            if (Data.Utilities.FilePath.TryGetRename(dirParent, dir, out string changedDir))
            {
                dir = Path.Combine(dirParent, changedDir);
            }
            else
            {
                dir = Path.Combine(dirParent, dir);
            }
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, FILENAME_SCENEINFO), sceneName + Environment.NewLine);

            TreeViewNodeModelScene newScene = LoadScene(new DirectoryInfo(dir), parentScene);
            if (parentScene == null)
            {
                treeViewSource.Add(newScene);
            }
            else
            {
                parentScene.Children.Add(newScene);
            }
            return newScene;
        }

        internal void DeleteScene(TreeViewNodeModelScene sceneToDelete)
        {
            int rc = Data.Utilities.SHFileOperator.Delete(sceneToDelete.GetDirFullName());
            if (rc != 0)
            {
                throw new Exception(Data.Utilities.SHFileOperator.GetRCMeaning(rc));
            }
            if (sceneToDelete.parent == null)
            {
                treeViewSource.Remove(sceneToDelete);
            }
            else
            {
                TreeViewNodeModelScene parent = (TreeViewNodeModelScene)sceneToDelete.parent;
                parent.Children.Remove(sceneToDelete);
            }
        }

        public void ChangeSceneDirName(TreeViewNodeModelScene scene, string newDirName)
        {
            string oriFullDirName = scene.GetDirFullName();
            string? parentFullDirName = Path.GetDirectoryName(oriFullDirName);
            if (parentFullDirName == null)
            {
                throw new IOException($"Cant get directory-name of scene-dir[{oriFullDirName}].");
            }
            newDirName = Data.Utilities.FilePath.CorrectorName(newDirName, out HashSet<char> missing);
            string newFullDirName;
            if (Data.Utilities.FilePath.TryGetRename(parentFullDirName,newDirName,out string changedNewDirName))
            {
                newFullDirName = Path.Combine(parentFullDirName, changedNewDirName);
            }
            else
            {
                newFullDirName = Path.Combine(parentFullDirName, newDirName);
            }
            Directory.Move(oriFullDirName, newFullDirName);
            scene.dirName = newDirName;
        }



        public enum Relevances
        {
            Origin, Parent, Inherited,
        }

    }
}
