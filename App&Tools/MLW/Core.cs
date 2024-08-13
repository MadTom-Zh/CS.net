using MadTomDev.Data;
using MLW_Succubus_Storys.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static MLW_Succubus_Storys.Classes.SuccuNode;
using CSVReader = MadTomDev.Common.CSVHelper.Reader;

namespace MLW_Succubus_Storys
{
    internal class Core
    {
        private Core() { }


        internal static WindowChooseLanguage luncherWindow;
        internal static MainWindow mainWindow;
        public static FileInfo storyPkgFile;

        #region settings

        public static Dictionary<string, string> localizeDict = new Dictionary<string, string>();
        public static void TrySetLocalTx(UIElement ui, string key)
        {
            if (!localizeDict.ContainsKey(key))
                return;
            ui.Dispatcher.Invoke(() =>
            {
                if (ui is Button)
                    ((Button)ui).Content = localizeDict[key];
                else if (ui is TextBlock)
                    ((TextBlock)ui).Text = localizeDict[key];
                else if (ui is MenuItem)
                    ((MenuItem)ui).Header = localizeDict[key];
            });
        }

        private static SettingsTxt setting;
        internal static void LoadSettings()
        {
            setting = new SettingsTxt("settings.txt");
            mainWindow.isSetting = true;
            BGMVolume = Setting_BGMVolume;
            mainWindow.sldBGMVolume.Value = BGMVolume;

            mainWindow.isSetting = false;
        }
        internal static void SettingSave()
        {
            setting.Save();
        }
        public static int Setting_BGMVolume
        {
            set => setting["BGMVolume"] = value.ToString();
            get
            {
                if (int.TryParse(setting["BGMVolume"], out int v))
                    return v;
                else
                    return 50;
            }
        }
        #endregion


        #region BGM

        private static int _BGMVolume = 50;
        internal static int BGMVolume
        {
            get => _BGMVolume;
            set
            {
                if (value < 0) value = 0;
                else if (value > 100) value = 100;
                _BGMVolume = value;
                if (bgmPlayer == null)
                    return;
                bgmPlayer.Volume = (double)value / 100;
                Setting_BGMVolume = value;
            }
        }
        private static int BGMIndexPre = -1;
        public static int BGMIndex = -1;
        internal static void BGMPlay(int bgmIndex)
        {
            if (BGMIndex == bgmIndex)
                return;
            mainWindow.Dispatcher.BeginInvoke(() =>
            {
                BGMIndex = bgmIndex;
                _BGMChangeStart();
            });
        }

        internal static void BGMPause()
        {
            if (BGMIndex < 0)
                return;
            mainWindow.Dispatcher.BeginInvoke(() =>
            {
                BGMIndex = -1;
                _BGMChangeStart();
            });
        }
        private static MediaElement bgmPlayer;
        private static bool _BGMChanging = false;
        public static event Action BGMChangingStart, BGMChangingEnd;
        private static void _BGMChangeStart()
        {
            if (_BGMChanging) return;

            BGMChangingStart?.Invoke();
            _BGMChanging = true;
            if (bgmPlayer == null)
            {
                bgmPlayer = new MediaElement()
                {
                    LoadedBehavior = MediaState.Manual,
                    UnloadedBehavior = MediaState.Stop,
                };
                mainWindow.mainGrid.Children.Add(bgmPlayer);
                bgmPlayer.MediaEnded += (s, e) =>
                { mainWindow.Dispatcher.Invoke(() => { bgmPlayer.Position = new TimeSpan(); }); };
            }

            if (BGMIndexPre < 0)
            {
                // in
                _BGMIn(BGMIndex);
            }
            else if (BGMIndex < 0)
            {
                // out
                _BGMOut();
            }
            else
            {
                // change
                _BGMOut();
                _BGMIn(BGMIndex);
            }

            BGMIndexPre = BGMIndex;
            _BGMChanging = false;
            BGMChangingEnd?.Invoke();
        }
        private static void _BGMOut()
        {
            if (bgmPlayer.Source != null)
            {
                double _BGMVolumeChangingMtp = 1;
                while (true)
                {
                    _BGMVolumeChangingMtp -= 0.05;
                    bgmPlayer.Volume = _BGMVolumeChangingMtp * BGMVolume / 100;
                    Task.Delay(15).Wait();
                    if (_BGMVolumeChangingMtp <= 0)
                        break;
                }
                bgmPlayer.Source = null;
            }
        }
        private static void _BGMIn(int bgmIdx)
        {
            bgmPlayer.Volume = 0;
            switch (bgmIdx)
            {
                case 0:
                    bgmPlayer.Source = new Uri(Path.Combine("BGMs", "bgm1.mp3"), UriKind.RelativeOrAbsolute);
                    break;
                case 1:
                    bgmPlayer.Source = new Uri(Path.Combine("BGMs", "bgm2.mp3"), UriKind.RelativeOrAbsolute);
                    break;
                case 2:
                    bgmPlayer.Source = new Uri(Path.Combine("BGMs", "bgmEA.mp3"), UriKind.RelativeOrAbsolute);
                    break;
                case 3:
                    bgmPlayer.Source = new Uri(Path.Combine("BGMs", "bgmEB.mp3"), UriKind.RelativeOrAbsolute);
                    break;
                case 4:
                    bgmPlayer.Source = new Uri(Path.Combine("BGMs", "bgmSucEA.mp3"), UriKind.RelativeOrAbsolute);
                    break;
                case 5:
                    bgmPlayer.Source = new Uri(Path.Combine("BGMs", "bgmSucEB.mp3"), UriKind.RelativeOrAbsolute);
                    break;
                default:
                    bgmPlayer.Source = null;
                    break;
            }
            if (bgmPlayer.Source != null)
            {
                bgmPlayer.Volume = 0;
                bgmPlayer.Play();
                double _BGMVolumeChangingMtp = 0;
                while (true)
                {
                    _BGMVolumeChangingMtp += 0.05;
                    bgmPlayer.Volume = _BGMVolumeChangingMtp * BGMVolume / 100;
                    Task.Delay(15).Wait();
                    if (_BGMVolumeChangingMtp >= 1)
                        break;
                }
            }
        }


        #endregion


        #region Images

        public static Dictionary<string, BitmapImage> imageDict = new Dictionary<string, BitmapImage>();
        public static void LoadAllImages()
        {
            DirectoryInfo di = new DirectoryInfo("Images");
            foreach (FileInfo fi in di.GetFiles("*.png"))
            {
                imageDict.Add(fi.Name.Substring(0, fi.Name.Length - 4),
                    new BitmapImage(new Uri(fi.FullName, UriKind.Absolute)));
            }
            foreach (FileInfo fi in di.GetFiles("*.jpg"))
            {
                imageDict.Add(fi.Name.Substring(0, fi.Name.Length - 4),
                    new BitmapImage(new Uri(fi.FullName, UriKind.Absolute)));
            }
        }
        #endregion


        #region CSV to structure

        public static string warningInfo;
        public static List<SuccuNode> allSuccubus = new List<SuccuNode>();

        public static void LoadCSV()
        {
            //  0           1             2                  3       4           5     6     7     
            //  SuccubusNo  SuccubusName  HeartLevel/Ending  Choice  ChoiceNext  DlgS  DlgJ  font
            //  0           Warning                                              xxxx
            //  1           Berith        I-1
            //  .................................................................xxxxx  xxxx xxxxx
            //                                               1-1-A   I-2
            //                            EA

            string[] row = null;
            string no = null, n = null, hl = null, choice = null, choiceNext = null,
                dlgS = null, dlgJ = null, dlgArgs = null;
            int testInt;
            SuccuNode curSuccuNode = null;
            SuccuNode.StoryNode storyNode = null;
            SuccuNode.ChoiceNode choiceNode = null;
            SuccuNode.EndingNode endingNode = null;
            using (CSVReader csvReader = new CSVReader(storyPkgFile.FullName))
            {
                do
                {
                    row = csvReader.ReadRow();
                    if (row == null || row.Length == 0)
                        continue;

                    no = row[0].Trim();
                    n = row[1].Trim();
                    hl = row[2].Trim();
                    choice = row[3].Trim();
                    choiceNext = row[4].Trim();
                    dlgS = row[5].Trim();
                    if (no.Length > 0)
                    {
                        if (!int.TryParse(no, out testInt))
                            continue;
                        if (no == "0")
                            continue;
                        if (n == "localization")
                        {
                            do
                            {
                                row = csvReader.ReadRow();
                                if (row == null || row.Length == 0)
                                    continue;
                                hl = row[2].Trim();
                                dlgS = row[5].Trim();
                                if (hl.Length > 0)
                                {
                                    if (!localizeDict.ContainsKey(hl))
                                        localizeDict.Add(hl, dlgS);
                                }
                            }
                            while (!csvReader.IsEoF);
                            break;
                        }

                        curSuccuNode = new SuccuNode()
                        { succuName = n, succuNameLocalized = dlgS, };
                        allSuccubus.Add(curSuccuNode);

                        if (hl.Length > 0)
                            AddStoryNode(hl);
                    }
                    else if (hl.Length > 0)
                    {
                        if (hl == "EA")
                        {
                            AddEndingNode(true);
                            AppendDlg(dlgS, null, null);
                        }
                        else if (hl == "EB")
                        {
                            AddEndingNode(false);
                            AppendDlg(dlgS, null, null);
                        }
                        else
                        {
                            // dlgS must be null or empty, or it's ending
                            if (string.IsNullOrEmpty(dlgS))
                                AddStoryNode(hl);
                        }
                    }
                    else if (choice.Length > 0)
                    {
                        AddChoiceNode(choice, choiceNext, dlgS);
                    }
                    else
                    {
                        dlgJ = row[6].Trim();
                        dlgArgs = row[7].Trim();

                        if (string.IsNullOrEmpty(dlgS)
                            && string.IsNullOrEmpty(dlgJ))
                            continue;

                        AppendDlg(dlgS, dlgJ, dlgArgs);
                    }
                }
                while (!csvReader.IsEoF);
            }

            // link nodes, re-construct choices
            // 选择项目全部在故事节点中，方便连接；随后在做选择项的结构划分；
            foreach (SuccuNode succuNode in allSuccubus)
            {
                foreach (StoryNode styNode in succuNode.storyLv1)
                    LinkNext(succuNode, styNode);
                foreach (StoryNode styNode in succuNode.storyLv2)
                    LinkNext(succuNode, styNode);
                foreach (StoryNode styNode in succuNode.storyLv3)
                    LinkNext(succuNode, styNode);
            }

            LoadCSV_recipes();
            LoadCSV_linkImage();


            #region sub functions

            void AddStoryNode(string sName)
            {
                choiceNode = null;
                endingNode = null;
                storyNode = new SuccuNode.StoryNode()
                { name = row[2].Trim(), };
                if (sName.StartsWith("I-"))
                    curSuccuNode.storyLv1.Add(storyNode);
                else if (sName.StartsWith("II-"))
                    curSuccuNode.storyLv2.Add(storyNode);
                else // if(sName.StartsWith("III-"))
                    curSuccuNode.storyLv3.Add(storyNode);
            }
            void AppendDlg(string dlgS, string dlgJ, string font)
            {
                if (endingNode != null)
                {
                    endingNode.msgs.Add(dlgS);
                }
                else if (choiceNode != null)
                {
                    choiceNode.chatHistory.chatMsgs.Add(new SuccuNode.ChatHistory.ChatMsg()
                    {
                        fromSucOrPlayer = dlgS.Length > 0,
                        msg = dlgS.Length > 0 ? dlgS : dlgJ,
                        args = font,
                    });
                }
                else
                {
                    storyNode.chatHistory.chatMsgs.Add(new SuccuNode.ChatHistory.ChatMsg()
                    {
                        fromSucOrPlayer = dlgS.Length > 0,
                        msg = dlgS.Length > 0 ? dlgS : dlgJ,
                        args = font,
                    });
                }
            }
            void AddChoiceNode(string cName, string cNextName, string cText)
            {
                choiceNode = new ChoiceNode()
                { text = cText, name = cName, };
                if (!string.IsNullOrWhiteSpace(cNextName))
                    choiceNode.nextNodeName = cNextName;
                storyNode.choices.Add(choiceNode);
            }
            void AddEndingNode(bool isEAorEB)
            {
                endingNode = new EndingNode()
                { isGoodOrBad = isEAorEB, };
                if (isEAorEB)
                    curSuccuNode.endingA = endingNode;
                else
                    curSuccuNode.endingB = endingNode;
            }

            void LinkNext(SuccuNode succuNode, StoryNode storyNode)
            {
                ChoiceNode choiceNode, choiceNodeParent;
                for (int i = storyNode.choices.Count - 1; i > 0; --i)
                {
                    choiceNode = storyNode.choices[i];
                    LinkNext_StoryOrEnding(succuNode, choiceNode);
                    for (int j = i - 1; j >= 0; --j)
                    {
                        choiceNodeParent = storyNode.choices[j];
                        if (choiceNodeParent.nextNodeName == "")
                        {
                            if (choiceNodeParent.name != null
                                && choiceNode.name.StartsWith(choiceNodeParent.name)
                                && choiceNode.name.IndexOf('-', choiceNodeParent.name.Length + 1) < 0)
                            {
                                choiceNodeParent.nextChoices.Add(choiceNode);
                                storyNode.choices.Remove(choiceNode);
                                LinkNext_StoryOrEnding(succuNode, choiceNode);
                                break;
                            }
                        }
                        else
                        {
                            LinkNext_StoryOrEnding(succuNode, choiceNodeParent);
                        }
                    }

                }
            }
            void LinkNext_StoryOrEnding(SuccuNode succuNode, ChoiceNode choiceNode)
            {
                if (choiceNode.nextNodeName == null)
                    return;

                if (choiceNode.nextNodeName == "EA")
                {
                    choiceNode.nextEnding = succuNode.endingA;
                }
                else if (choiceNode.nextNodeName == "EB")
                {
                    choiceNode.nextEnding = succuNode.endingB;
                }
                else
                {
                    StoryNode foundStoryNode = LinkNext_FindStory(succuNode, choiceNode.nextNodeName);
                    if (foundStoryNode != null)
                    {
                        choiceNode.nextStory = foundStoryNode;
                    }
                }
                choiceNode.nextNodeName = null;
            }
            StoryNode LinkNext_FindStory(SuccuNode succuNode, string storyName)
            {
                if (string.IsNullOrWhiteSpace(storyName))
                    return null;

                foreach (StoryNode s in succuNode.storyLv1)
                    if (s.name == storyName)
                        return s;
                foreach (StoryNode s in succuNode.storyLv2)
                    if (s.name == storyName)
                        return s;
                foreach (StoryNode s in succuNode.storyLv3)
                    if (s.name == storyName)
                        return s;

                return null;
            }

            #endregion
        }

        private static void LoadCSV_recipes()
        {
            FileInfo recipeFile = new FileInfo("succubiRecipe.csv");
            if (!recipeFile.Exists)
                return;

            using (CSVReader csvReader = new CSVReader(recipeFile.FullName))
            {
                string[] row;
                int testInt;
                SuccuNode? sNode;
                do
                {
                    row = csvReader.ReadRow();

                    if (row == null || row.Length < 5)
                        continue;
                    if (!int.TryParse(row[0], out testInt))
                        continue;

                    sNode = allSuccubus.Find(s => s.succuName == row[1]);
                    if (sNode != null)
                    {
                        sNode.mtl1Name = row[2];
                        sNode.mtl2Name = row[3];
                        sNode.mtl3Name = row[4];
                    }
                }
                while (!csvReader.IsEoF);
            }
        }
        private static void LoadCSV_linkImage()
        {
            // succu icon, meterials, endings
            string key;
            foreach (SuccuNode succuNode in allSuccubus)
            {
                key = "Icon" + succuNode.succuName;
                if (imageDict.ContainsKey(key))
                    succuNode.succuIcon = imageDict[key];

                key = "Mt" + succuNode.mtl1Name;
                if (imageDict.ContainsKey(key))
                    succuNode.mtl1Icon = imageDict[key];
                key = "Mt" + succuNode.mtl2Name;
                if (imageDict.ContainsKey(key))
                    succuNode.mtl2Icon = imageDict[key];
                key = "Mt" + succuNode.mtl3Name;
                if (imageDict.ContainsKey(key))
                    succuNode.mtl3Icon = imageDict[key];

                key = "EA_" + succuNode.succuName;
                if (imageDict.ContainsKey(key))
                    succuNode.endingA.image = imageDict[key];
                key = "EB_" + succuNode.succuName;
                if (imageDict.ContainsKey(key))
                    succuNode.endingB.image = imageDict[key];
            }
        }

        #endregion



        public static void Exit()
        {
            setting?.Save();
            luncherWindow?.Close();
            try
            {
                mainWindow?.Close();
            }
            catch (Exception) { }
        }

    }
}
