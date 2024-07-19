using MadTomDev.App.Classes;
using MadTomDev.App.VMs;
using MadTomDev.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static MadTomDev.App.Classes.Recipes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace MadTomDev.App
{
    public class Core
    {
        private Core() { }
        private static Core? instance = null;
        public static Core Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Core();
                }
                return instance;
            }
        }

        public readonly uint mouseDoubleClickInterval = Common.MouseNKeyboardHelper.MouseDoubleClickTime;

        internal MainWindow mainWindow;
        public SceneMgr sceneMgr = SceneMgr.Instance;



        public enum WorkModes
        {
            None, Maintaince, Calculation,
        }
        public WorkModes WorkMode { private set; get; } = WorkModes.None;

        internal void InitSceneSelection()
        {
            WorkMode = WorkModes.None;
        }
        public string? SceneFullPath { private set; get; }
        public string? SceneName { private set; get; }


        #region maintains related

        public Things? thingsParent, thingsCurrent;
        public Recipes? recipesParent, recipesCurrent;
        public Channels? channelsParent, channelsCurrent;
        public DataGridItemModelThing? selectedMaintainThing;
        public DataGridItemModelRecipe? selectedMaintainRecipe;
        public DataGridItemModelChannel? selectedMaintainChannel;
        public ObservableCollection<DataGridItemModelThing> maintainThings = new ObservableCollection<DataGridItemModelThing>();
        public ObservableCollection<DataGridItemModelRecipe> maintainRecipes = new ObservableCollection<DataGridItemModelRecipe>();
        public ObservableCollection<DataGridItemModelChannel> maintainChannels = new ObservableCollection<DataGridItemModelChannel>();
        public void ReInitMaintaince(TreeViewNodeModelScene scene)
        {
            WorkMode = WorkModes.Maintaince;
            SceneName = scene.Text;
            List<TreeViewNodeModelScene> nodeChain = sceneMgr.GetSceneChain(scene);
            thingsParent = null; thingsCurrent = null;
            recipesParent = null; recipesCurrent = null;
            channelsParent = null; channelsCurrent = null;
            selectedMaintainThing = null;
            selectedMaintainRecipe = null;
            selectedMaintainChannel = null;
            maintainThings.Clear();
            maintainRecipes.Clear();
            maintainChannels.Clear();
            Things curThings;
            Recipes curRecipes;
            Channels curChannels;
            TreeViewNodeModelScene curScene;
            StringBuilder strBdr = new StringBuilder();
            // load things first, and load scene path
            for (int i = 0, iv = nodeChain.Count - 1; i <= iv; ++i)
            {
                curScene = nodeChain[i];
                strBdr.Append(curScene.Text);
                strBdr.Append(" => ");
                if (i == iv)
                {
                    thingsCurrent = new Things(curScene);
                }
                else
                {
                    curThings = new Things(curScene);
                    if (thingsParent == null)
                    {
                        thingsParent = curThings;
                    }
                    else
                    {
                        curThings.Inherit(thingsParent);
                        thingsParent = curThings;
                    }
                }
            }
            if (strBdr.Length >= 4)
            {
                strBdr.Remove(strBdr.Length - 4, 4);
            }
            SceneFullPath = strBdr.ToString();

            // load channels
            for (int i = 0, iv = nodeChain.Count - 1; i <= iv; ++i)
            {
                curScene = nodeChain[i];
                if (i == iv)
                {
                    channelsCurrent = new Channels(curScene);
                }
                else
                {
                    curChannels = new Channels(curScene);
                    if (channelsParent == null)
                    {
                        channelsParent = curChannels;
                    }
                    else
                    {
                        curChannels.Inherit(channelsParent);
                        channelsParent = curChannels;
                    }
                }
            }


            // load recipes (after load things)
            for (int i = 0, iv = nodeChain.Count - 1; i <= iv; ++i)
            {
                curScene = nodeChain[i];
                if (i == iv)
                {
                    recipesCurrent = new Recipes(curScene);
                }
                else
                {
                    curRecipes = new Recipes(curScene);
                    if (recipesParent == null)
                    {
                        recipesParent = curRecipes;
                    }
                    else
                    {
                        curRecipes.Inherit(recipesParent);
                        recipesParent = curRecipes;
                    }
                }
            }

            maintainThings.Clear();
            if (thingsParent != null)
            {
                List<Things.Thing> tmpCurThings = new List<Things.Thing>();
                if (thingsCurrent != null)
                {
                    tmpCurThings.AddRange(thingsCurrent.list);
                }
                Things.Thing? foundCurThing;
                foreach (Things.Thing tp in thingsParent.list)
                {
                    if (tp.isExcluded == true)
                    {
                        continue;
                    }
                    foundCurThing = tmpCurThings.Find(a => a.parentID == tp.id);
                    if (foundCurThing != null)
                    {
                        // 加入继承物品
                        maintainThings.Add(new DataGridItemModelThing(tp, foundCurThing));
                        tmpCurThings.Remove(foundCurThing);
                    }
                    else
                    {
                        // 直接加入父级物品
                        maintainThings.Add(new DataGridItemModelThing(tp, null));
                    }
                }
                if (tmpCurThings.Count > 0)
                {
                    // 加入其他当前物品
                    foreach (Things.Thing t in tmpCurThings)
                    {
                        maintainThings.Add(new DataGridItemModelThing(null, t));
                    }
                }
                tmpCurThings.Clear();
            }
            else
            {
                // 直接加入当前所有物品
                if (thingsCurrent != null)
                {
                    foreach (Things.Thing t in thingsCurrent.list)
                    {
                        maintainThings.Add(new DataGridItemModelThing(null, t));
                    }
                }
            }

            maintainChannels.Clear();
            if (channelsParent != null)
            {
                List<Channels.Channel> tmpCurChannels = new List<Channels.Channel>();
                if (channelsCurrent != null)
                {
                    tmpCurChannels.AddRange(channelsCurrent.list);
                }
                Channels.Channel? foundCurChannel;
                foreach (Channels.Channel cp in channelsParent.list)
                {
                    if (cp.isExcluded == true)
                    {
                        continue;
                    }
                    foundCurChannel = tmpCurChannels.Find(a => a.parentID == cp.id);
                    if (foundCurChannel != null)
                    {
                        // 加入继承物品
                        maintainChannels.Add(new DataGridItemModelChannel(cp, foundCurChannel));
                        tmpCurChannels.Remove(foundCurChannel);
                    }
                    else
                    {
                        // 直接加入父级物品
                        maintainChannels.Add(new DataGridItemModelChannel(cp, null));
                    }
                }
                if (tmpCurChannels.Count > 0)
                {
                    // 加入其他当前物品
                    foreach (Channels.Channel c in tmpCurChannels)
                    {
                        maintainChannels.Add(new DataGridItemModelChannel(null, c));
                    }
                }
                tmpCurChannels.Clear();
            }
            else
            {
                // 直接加入当前所有物品
                if (channelsCurrent != null)
                {
                    foreach (Channels.Channel c in channelsCurrent.list)
                    {
                        maintainChannels.Add(new DataGridItemModelChannel(null, c));
                    }
                }
            }

            maintainRecipes.Clear();
            if (recipesParent != null)
            {
                List<Recipes.Recipe> tmpCurRecipes = new List<Recipes.Recipe>();
                if (recipesCurrent != null)
                {
                    tmpCurRecipes.AddRange(recipesCurrent.list);
                }
                Recipes.Recipe? foundCurRecipe;
                foreach (Recipes.Recipe rp in recipesParent.list)
                {
                    if (rp.isExcluded == true)
                    {
                        continue;
                    }
                    foundCurRecipe = tmpCurRecipes.Find(a => a.parentID == rp.id);
                    if (foundCurRecipe != null)
                    {
                        // 加入继承物品
                        maintainRecipes.Add(new DataGridItemModelRecipe(rp, foundCurRecipe));
                        tmpCurRecipes.Remove(foundCurRecipe);
                    }
                    else
                    {
                        // 直接加入父级物品
                        maintainRecipes.Add(new DataGridItemModelRecipe(rp, null));
                    }
                }
                if (tmpCurRecipes.Count > 0)
                {
                    // 加入其他当前物品
                    foreach (Recipes.Recipe r in tmpCurRecipes)
                    {
                        maintainRecipes.Add(new DataGridItemModelRecipe(null, r));
                    }
                }
                tmpCurRecipes.Clear();
            }
            else
            {
                // 直接加入当前所有物品
                if (recipesCurrent != null)
                {
                    foreach (Recipes.Recipe r in recipesCurrent.list)
                    {
                        maintainRecipes.Add(new DataGridItemModelRecipe(null, r));
                    }
                }
            }
        }

        #region thing, find, delete, create, update

        /// <summary>
        /// 在当前场景下寻找物品
        /// </summary>
        /// <param name="id">物品的id</param>
        /// <param name="inherited">维护模式时，true-完整继承的物品(复制体，亦可能和原生相同)，false-仅当前物品，或父物品；计算模式时，无影响，必定是完整继承物品(非复制体)</param>
        /// <returns></returns>
        public Things.Thing? FindThing(Guid id, bool inherited = false)
        {
            switch (WorkMode)
            {
                case WorkModes.Maintaince:
                    Things.Thing? testT = null;
                    if (inherited)
                    {
                        if (thingsCurrent != null)
                        {
                            testT = thingsCurrent.list.Find(a => a.id == id);
                        }
                        if (testT != null)
                        {
                            if (thingsParent != null)
                            {
                                Things.Thing? testTP = thingsParent.list.Find(a => a.id == testT.parentID);
                                testT.Clone().Inherit(testTP);
                            }
                            return testT.Clone();
                        }
                        else
                        {
                            testT = thingsParent.list.Find(a => a.id == id);
                            return testT == null ? null : testT.Clone();
                        }
                    }
                    else
                    {
                        if (thingsCurrent != null)
                        {
                            testT = thingsCurrent.list.Find(a => a.id == id);
                        }
                        if (testT == null && thingsParent != null)
                        {
                            testT = thingsParent.list.Find(a => a.id == id);
                        }
                        return testT;
                    }
                case WorkModes.Calculation:
                    if (thingsFinal == null)
                    {
                        return null;
                    }
                    else
                    {
                        return thingsFinal.list.Find(a => a.id == id);
                    }
                case WorkModes.None:
                default:
                    return null;
            }
        }

        internal Channels.Channel? FindChannel(Guid channelId)
        {
            switch (WorkMode)
            {
                case WorkModes.None:
                    return null;
                case WorkModes.Calculation:
                    {
                        if (channelsFinal != null)
                        {
                            foreach (Channels.Channel c in channelsFinal.list)
                            {
                                if (c.id == channelId)
                                {
                                    return c;
                                }
                            }
                        }
                    }
                    break;
                case WorkModes.Maintaince:
                    {
                        if (channelsCurrent != null)
                        {
                            foreach (Channels.Channel c in channelsCurrent.list)
                            {
                                if (c.id == channelId)
                                {
                                    return c;
                                }
                            }
                        }
                        if (channelsParent != null)
                        {
                            foreach (Channels.Channel c in channelsParent.list)
                            {
                                if (c.id == channelId)
                                {
                                    return c;
                                }
                            }
                        }
                    }
                    break;
            }
            return null;
        }
        internal Recipes.Recipe? FindRecipe(Guid recipeId)
        {
            switch (WorkMode)
            {
                case WorkModes.None:
                    return null;
                    case WorkModes.Calculation:
                    {
                        if (recipesFinal != null)
                        {
                            foreach (Recipes.Recipe r in recipesFinal.list)
                            {
                                if (r.id == recipeId)
                                {
                                    return r;
                                }
                            }
                        }
                    }
                    break;
                case WorkModes.Maintaince:
                    {
                        if (recipesCurrent != null)
                        {
                            foreach (Recipes.Recipe r in recipesCurrent.list)
                            {
                                if (r.id == recipeId)
                                {
                                    return r;
                                }
                            }
                        }
                        if (recipesParent != null)
                        {
                            foreach (Recipes.Recipe r in recipesParent.list)
                            {
                                if (r.id == recipeId)
                                {
                                    return r;
                                }
                            }
                        }
                    }
                    break;
            }
            return null;
        }
        public bool DeleteSelectedThingCheck(out string? errMsg, out int errLevel)
        {
            errLevel = -1;
            errMsg = null;
            if (thingsCurrent == null || thingsCurrent.list.Count == 0)
            {
                errMsg = "No thing to delete.";
                errLevel = 0;
                return false;
            }
            if (selectedMaintainThing == null)
            {
                errMsg = "Select a thing first.";
                errLevel = 1;
                return false;
            }
            if (selectedMaintainThing.data == null)
            {
                errMsg = "Can not delete things from parent.";
                errLevel = 1;
                return false;
            }
            return true;
        }

        internal bool CheckSelectedThingUsedByCannelOrRecipe(out Guid channelId, out Guid recipeId)
        {
            channelId = Guid.Empty;
            recipeId = Guid.Empty;
            if (thingsCurrent == null || thingsCurrent.list.Count == 0)
            {
                return false;
            }
            if (selectedMaintainThing == null)
            {
                return false;
            }
            Guid tId = selectedMaintainThing.Id;
            if (channelsCurrent != null && _CheckInChannels(channelsCurrent.list, out channelId))
            {
                return true;
            }
            if (channelsParent != null && _CheckInChannels(channelsParent.list, out channelId))
            {
                return true;
            }
            if (recipesCurrent != null && _CheckInRecipes(recipesCurrent.list, out recipeId))
            {
                return true;
            }
            if (recipesParent != null && _CheckInRecipes(recipesParent.list, out recipeId))
            {
                return true;
            }
            return false;

            bool _CheckInChannels(List<Channels.Channel> channelList, out Guid cId)
            {
                cId = Guid.Empty;
                foreach (Channels.Channel c in channelList)
                {
                    if (c.contentList.Find(a => a.contentId == tId).contentId != Guid.Empty)
                    {
                        cId = c.id;
                        return true;
                    }
                }
                return false;
            }
            bool _CheckInRecipes(List<Recipes.Recipe> recipeList, out Guid rId)
            {
                rId = Guid.Empty;
                foreach (Recipes.Recipe r in recipeList)
                {
                    if (r.processor != null && r.processor == tId)
                    {
                        rId = r.id;
                        return true;
                    }
                    if (r.accessories.Find(a => a.thing != null && a.thing.id == tId) != null)
                    {
                        rId = r.id;
                        return true;
                    }
                    if (r.inputs.Find(a => a.thing != null && a.thing.id == tId) != null)
                    {
                        rId = r.id;
                        return true;
                    }
                    if (r.outputs.Find(a => a.thing != null && a.thing.id == tId) != null)
                    {
                        rId = r.id;
                        return true;
                    }
                }
                return false;
            }
        }

        public void DeleteSelectedThing()
        {
            if (thingsCurrent == null || selectedMaintainThing == null || selectedMaintainThing.data == null)
            {
                throw new NullReferenceException("thingsCurrent is null, or no selected thing data.");
            }
            // delete from cur thing list, save
            thingsCurrent.list.Remove(selectedMaintainThing.data);
            thingsCurrent.Save();

            // delete from VM, refresh
            if (selectedMaintainThing.dataParent == null)
            {
                maintainThings.Remove(selectedMaintainThing);
            }
            else
            {
                selectedMaintainThing.data = null;
                selectedMaintainThing.Update();
            }
        }
        public void CreateNewOrUpdateThing(Things.Thing newData, ImageSource? newImg, out DataGridItemModelThing? newVM)
        {
            if (thingsCurrent == null)
            {
                throw new Exception("(Impossible) thingsCurrent is null.");
            }

            newVM = null;
            newData.id = Guid.NewGuid();
            string? newImgNameAs = null;
            Things.Thing? thingToSaveImg = newData;

            if (selectedMaintainThing == null)
            {
                // create
                thingsCurrent.list.Add(newData);

                newVM = new DataGridItemModelThing(null, newData);
                if (newImg != null)
                {
                    newVM.Image = newImg.ToBitmapImage();
                }
                maintainThings.Add(newVM);
            }
            else if (selectedMaintainThing.dataParent != null && selectedMaintainThing.data == null)
            {
                // inherite
                newData.parentID = selectedMaintainThing.dataParent.id;
                if (newData.name == selectedMaintainThing.dataParent.name)
                {
                    newData.name = null;
                }
                // isExcluded
                if (newData.unit == selectedMaintainThing.dataParent.unit)
                {
                    newData.unit = null;
                }
                if (newData.description == selectedMaintainThing.dataParent.description)
                {
                    newData.description = null;
                }
                if (newImg != null)
                {
                    selectedMaintainThing.Image = newImg.ToBitmapImage();
                    if (newData != null && newData.name != null)
                    {
                        newImgNameAs = newData.name;
                    }
                    else if (selectedMaintainThing.dataParent != null && selectedMaintainThing.dataParent.name != null)
                    {
                        newImgNameAs = selectedMaintainThing.dataParent.name;
                    }
                }
                selectedMaintainThing.data = newData;
                thingsCurrent.list.Add(newData);
            }
            else if (selectedMaintainThing.data != null)
            {
                // update
                Things.Thing curThing = selectedMaintainThing.data;
                curThing.DataFrom(newData);
                thingToSaveImg = curThing;
                if (selectedMaintainThing.dataParent != null)
                {
                    Things.Simplify(ref curThing, selectedMaintainThing.dataParent);
                }
                //else // dataParent == null
                //{
                //    // no change
                //}

            }
            thingsCurrent.Save();

            if (newImg != null)
            {
                ImageIO.Remove(thingToSaveImg);
                ImageIO.PutIn(thingToSaveImg, newImgNameAs, newImg);
            }
            if (newVM == null)
            {
                selectedMaintainThing?.Update();
            }
        }

        #endregion



        #region channel, get content list, delete, create, update


        internal List<Channels.Channel.ContentItem> GetNewChannelContentList(Guid channelId)
        {
            List<Channels.Channel.ContentItem> result = new List<Channels.Channel.ContentItem>();
            Channels.Channel? c = null, cp = null;
            switch (WorkMode)
            {
                case WorkModes.Maintaince:
                    // condition inherited, or origin
                    if (channelsCurrent != null)
                    {
                        c = channelsCurrent.list.Find(a => a.id == channelId);
                    }
                    if (channelsParent != null && c != null && c.parentID != null)
                    {
                        cp = channelsParent.list.Find(a => a.id == c.parentID);
                    }
                    if (cp != null && c != null)
                    {
                        result.AddRange(c.contentList);
                        Channels.Channel.Inherite(ref result, cp.contentList);
                    }
                    else if (c != null)
                    {
                        result.AddRange(c.contentList);
                    }

                    // condition parent only
                    cp = null;
                    if (c == null && channelsParent != null)
                    {
                        cp = channelsParent.list.Find(a => a.id == channelId);
                    }
                    if (cp != null)
                    {
                        result.AddRange(cp.contentList);
                    }
                    break;
                case WorkModes.Calculation:
                    if (channelsFinal != null)
                    {
                        c = channelsFinal.list.Find(a => a.id == channelId);
                        if (c != null)
                        {
                            result.AddRange(c.contentList);
                        }
                    }
                    break;
                default:
                case WorkModes.None:
                    break;
            }
            return result;
        }

        public bool DeleteSelectedChannelCheck(out string? errMsg, out int errLevel)
        {
            errLevel = -1;
            errMsg = null;
            if (channelsCurrent == null || channelsCurrent.list.Count == 0)
            {
                errMsg = "No channel to delete.";
                errLevel = 0;
                return false;
            }
            if (selectedMaintainChannel == null)
            {
                errMsg = "Select a channel first.";
                errLevel = 1;
                return false;
            }
            if (selectedMaintainChannel.data == null)
            {
                errMsg = "Can not delete channels from parent.";
                errLevel = 1;
                return false;
            }
            return true;
        }
        public void DeleteSelectedChannel()
        {
            if (channelsCurrent == null || selectedMaintainChannel == null || selectedMaintainChannel.data == null)
            {
                throw new NullReferenceException("channelsCurrent is null, or no selected channel data.");
            }
            // delete from cur thing list, save
            channelsCurrent.list.Remove(selectedMaintainChannel.data);
            channelsCurrent.Save();

            // delete from VM, refresh
            if (selectedMaintainChannel.dataParent == null)
            {
                maintainChannels.Remove(selectedMaintainChannel);
            }
            else
            {
                selectedMaintainChannel.data = null;
                selectedMaintainChannel.Update();
            }
        }
        public void CreateNewOrUpdateChannel(Channels.Channel newData, ImageSource? newImg, out DataGridItemModelChannel? newVM)
        {
            if (channelsCurrent == null)
            {
                throw new Exception("(Impossible) channelsCurrent is null.");
            }

            newVM = null;
            newData.id = Guid.NewGuid();
            string? newImgNameAs = null;
            Channels.Channel? channelToSaveImg = newData;

            if (selectedMaintainChannel == null)
            {
                // create
                channelsCurrent.list.Add(newData);

                newVM = new DataGridItemModelChannel(null, newData);
                if (newImg != null)
                {
                    newVM.Image = newImg.ToBitmapImage();
                }
                maintainChannels.Add(newVM);
            }
            else if (selectedMaintainChannel.dataParent != null && selectedMaintainChannel.data == null)
            {
                // inherite
                newData.parentID = selectedMaintainChannel.dataParent.id;
                Channels.Simplify(ref newData, selectedMaintainChannel.dataParent);

                if (newImg != null)
                {
                    selectedMaintainChannel.Image = newImg.ToBitmapImage();
                    if (newData != null && newData.name != null)
                    {
                        newImgNameAs = newData.name;
                    }
                    else if (selectedMaintainChannel.dataParent != null && selectedMaintainChannel.dataParent.name != null)
                    {
                        newImgNameAs = selectedMaintainChannel.dataParent.name;
                    }
                }
                selectedMaintainChannel.data = newData;
                channelsCurrent.list.Add(newData);
            }
            else if (selectedMaintainChannel.data != null)
            {
                // update
                Channels.Channel curChannel = selectedMaintainChannel.data;
                curChannel.DataFrom(newData);
                channelToSaveImg = curChannel;
                if (selectedMaintainChannel.dataParent != null)
                {
                    Channels.Simplify(ref curChannel, selectedMaintainChannel.dataParent);
                    if (curChannel.CheckIsEmpty())
                    {
                        channelsCurrent.list.Remove(curChannel);
                        selectedMaintainChannel.data = null;
                    }
                }
                //else // dataParent == null
                //{
                //    // no change
                //}

            }
            channelsCurrent.Save();

            if (newImg != null)
            {
                ImageIO.Remove(channelToSaveImg);
                ImageIO.PutIn(channelToSaveImg, newImgNameAs, newImg);
            }
            if (newVM == null)
            {
                selectedMaintainChannel?.Update();
            }
        }

        #endregion



        #region recipe, get PIO list, delete, create, update

        /// <summary>
        /// 获取配方的内容列表，插件、原料或产品
        /// </summary>
        /// <param name="recipeId">配方id</param>
        /// <param name="contentType">输出列表类型，0-插件，1-原料，2-产品</param>
        /// <returns></returns>
        internal List<Recipes.Recipe.PIOItem> GetNewRecipePIOList(Guid recipeId, int contentType)
        {
            List<Recipes.Recipe.PIOItem> result = new List<Recipes.Recipe.PIOItem>();
            Recipes.Recipe? r = null, rp = null;
            switch (WorkMode)
            {
                case WorkModes.Maintaince:
                    // condition inherited, or origin
                    if (recipesCurrent != null)
                    {
                        r = recipesCurrent.list.Find(a => a.id == recipeId);
                    }
                    if (recipesParent != null && r != null && r.parentID != null)
                    {
                        rp = recipesParent.list.Find(a => a.id == r.parentID);
                    }
                    if (rp != null && r != null)
                    {
                        switch (contentType)
                        {
                            case 0:
                                result.AddRange(Recipes.Recipe.CopyList(r.accessories));
                                Recipes.Recipe.Inherite(ref result, rp.accessories);
                                break;
                            case 1:
                                result.AddRange(Recipes.Recipe.CopyList(r.inputs));
                                Recipes.Recipe.Inherite(ref result, rp.inputs);
                                break;
                            case 2:
                                result.AddRange(Recipes.Recipe.CopyList(r.outputs));
                                Recipes.Recipe.Inherite(ref result, rp.outputs);
                                break;
                        }
                    }
                    else if (r != null)
                    {
                        switch (contentType)
                        {
                            case 0:
                                result.AddRange(Recipes.Recipe.CopyList(r.accessories));
                                break;
                            case 1:
                                result.AddRange(Recipes.Recipe.CopyList(r.inputs));
                                break;
                            case 2:
                                result.AddRange(Recipes.Recipe.CopyList(r.outputs));
                                break;
                        }
                    }

                    // condition parent only
                    rp = null;
                    if (r == null && recipesParent != null)
                    {
                        rp = recipesParent.list.Find(a => a.id == recipeId);
                    }
                    if (rp != null)
                    {
                        switch (contentType)
                        {
                            case 0:
                                result.AddRange(Recipes.Recipe.CopyList(rp.accessories));
                                break;
                            case 1:
                                result.AddRange(Recipes.Recipe.CopyList(rp.inputs));
                                break;
                            case 2:
                                result.AddRange(Recipes.Recipe.CopyList(rp.outputs));
                                break;
                        }
                    }
                    break;
                case WorkModes.Calculation:
                    if (recipesFinal != null)
                    {
                        r = recipesFinal.list.Find(r => r.id == recipeId);
                        if (r != null)
                        {
                            switch (contentType)
                            {
                                case 0:
                                    result.AddRange(Recipes.Recipe.CopyList(r.accessories));
                                    break;
                                case 1:
                                    result.AddRange(Recipes.Recipe.CopyList(r.inputs));
                                    break;
                                case 2:
                                    result.AddRange(Recipes.Recipe.CopyList(r.outputs));
                                    break;
                            }
                        }
                    }
                    break;
                default:
                case WorkModes.None:
                    break;
            }
            return result;
        }

        public bool DeleteSelectedRecipeCheck(out string? errMsg, out int errLevel)
        {
            errMsg = null;
            errLevel = -1;
            if (recipesCurrent == null || recipesCurrent.list.Count == 0)
            {
                errMsg = "No recipe to delete.";
                errLevel = 0;
                return false;
            }
            if (selectedMaintainRecipe == null)
            {
                errMsg = "Select a recipe first.";
                errLevel = 1;
                return false;
            }
            if (selectedMaintainRecipe.data == null)
            {
                errMsg = "Can not delete recipes from parent.";
                errLevel = 1;
                return false;
            }
            return true;
        }
        public void DeleteSelectedRecipe()
        {
            if (recipesCurrent == null || selectedMaintainRecipe == null || selectedMaintainRecipe.data == null)
            {
                throw new NullReferenceException("recipesCurrent is null, or no selected recipe data.");
            }
            // delete from cur thing list, save
            recipesCurrent.list.Remove(selectedMaintainRecipe.data);
            recipesCurrent.Save();

            // delete from VM, refresh
            if (selectedMaintainRecipe.dataParent == null)
            {
                maintainRecipes.Remove(selectedMaintainRecipe);
            }
            else
            {
                selectedMaintainRecipe.data = null;
                selectedMaintainRecipe.Update();
            }
        }
        public void CreateNewOrUpdateRecipe(Recipes.Recipe newData, ImageSource? newImg, out DataGridItemModelRecipe? newVM)
        {
            if (recipesCurrent == null)
            {
                throw new Exception("(Impossible) No current recipe.");
            }
            if (newData == null)
            {
                throw new Exception("newData is null.");
            }

            newVM = null;
            newData.id = Guid.NewGuid();
            string? newImgNameAs = null;
            Recipes.Recipe? recipeToSaveImg = newData;
            if (selectedMaintainRecipe == null)
            {
                // create
                recipesCurrent.list.Add(newData);

                newVM = new DataGridItemModelRecipe(null, newData);
                if (newImg != null)
                {
                    newVM.Image = newImg.ToBitmapImage();
                }
                maintainRecipes.Add(newVM);
            }
            else if (selectedMaintainRecipe.dataParent != null && selectedMaintainRecipe.data == null)
            {
                // inherite
                newData.parentID = selectedMaintainRecipe.dataParent.id;
                Recipes.Simplify(ref newData, selectedMaintainRecipe.dataParent);
                if (newImg != null)
                {
                    if (newData != null && newData.name != null)
                    {
                        newImgNameAs = newData.name;
                    }
                    else if (selectedMaintainRecipe.dataParent != null && selectedMaintainRecipe.dataParent.name != null)
                    {
                        newImgNameAs = selectedMaintainRecipe.dataParent.name;
                    }
                }
                recipesCurrent.list.Add(newData);
                selectedMaintainRecipe.data = newData;

            }
            else if (selectedMaintainRecipe.data != null)
            {
                // update
                //if (selectedMaintainRecipe == null || selectedMaintainRecipe.data == null)
                //{
                //    throw new Exception("(Impossible) Current recipe is null.");
                //}
                Recipes.Recipe curRecipe = selectedMaintainRecipe.data;
                curRecipe.DataFrom(newData);
                recipeToSaveImg = curRecipe;
                if (selectedMaintainRecipe.dataParent != null)
                {
                    Recipes.Simplify(ref curRecipe, selectedMaintainRecipe.dataParent);
                    if (curRecipe.CheckIsEmpty())
                    {
                        recipesCurrent.list.Remove(curRecipe);
                        selectedMaintainRecipe.data = null;
                    }
                }
            }
            recipesCurrent.Save();

            if (newImg != null)
            {
                ImageIO.Remove(recipeToSaveImg);
                ImageIO.PutIn(recipeToSaveImg, newImgNameAs, newImg);
            }
            if (newVM == null)
            {
                selectedMaintainRecipe?.Update();
            }
        }
        #endregion

        #endregion




        #region calculation related

        public Things? thingsFinal;
        public Recipes? recipesFinal;
        public Channels? channelsFinal;

        public ObservableCollection<DataGridItemModelThing> calculationThings = new ObservableCollection<DataGridItemModelThing>();
        public ObservableCollection<DataGridItemModelRecipe> calculationRecipes = new ObservableCollection<DataGridItemModelRecipe>();
        public ObservableCollection<DataGridItemModelChannel> calculationChannels = new ObservableCollection<DataGridItemModelChannel>();


        public void ReInitCalculation(TreeViewNodeModelScene scene)
        {
            SceneName = scene.Text;
            WorkMode = WorkModes.Calculation;
            thingsFinal = null;
            recipesFinal = null;
            channelsFinal = null;
            calculationThings.Clear();
            calculationRecipes.Clear();
            calculationChannels.Clear();
            Things curThings;
            Recipes curRecipes;
            Channels curChannels;
            List<TreeViewNodeModelScene> nodeChain = sceneMgr.GetSceneChain(scene);
            TreeViewNodeModelScene curNode;
            StringBuilder strBdr = new StringBuilder();
            // things
            for (int i = 0, iv = nodeChain.Count; i < iv; ++i)
            {
                curNode = nodeChain[i];
                strBdr.Append(curNode.Text);
                strBdr.Append(" => ");
                curThings = new Things(curNode);
                if (thingsFinal == null)
                {
                    thingsFinal = curThings;
                }
                else
                {
                    curThings.Inherit(thingsFinal);
                    thingsFinal = curThings;
                }
            }
            if (strBdr.Length >= 4)
            {
                strBdr.Remove(strBdr.Length - 4, 4);
            }
            SceneFullPath = strBdr.ToString();

            // remove excluded things
            if (thingsFinal != null)
            {
                RemoveExcludeds(ref thingsFinal.list);
            }

            calculationThings.Clear();
            if (thingsFinal != null)
            {
                foreach (Things.Thing t in thingsFinal.list)
                {
                    calculationThings.Add(new DataGridItemModelThing(null, t));
                }
            }

            // channels
            for (int i = 0, iv = nodeChain.Count; i < iv; ++i)
            {
                curNode = nodeChain[i];
                curChannels = new Channels(curNode);
                if (channelsFinal == null)
                {
                    channelsFinal = curChannels;
                }
                else
                {
                    curChannels.Inherit(channelsFinal);
                    channelsFinal = curChannels;
                }
            }
            // remove excluded channels
            if (channelsFinal != null)
            {
                RemoveExcludeds(ref channelsFinal.list);
            }


            calculationChannels.Clear();
            if (channelsFinal != null)
            {
                foreach (Channels.Channel c in channelsFinal.list)
                {
                    calculationChannels.Add(new DataGridItemModelChannel(null, c));
                }
            }

            // recipes
            for (int i = 0, iv = nodeChain.Count; i < iv; ++i)
            {
                curNode = nodeChain[i];
                curRecipes = new Recipes(curNode);
                if (recipesFinal == null)
                {
                    recipesFinal = curRecipes;
                }
                else
                {
                    curRecipes.Inherit(recipesFinal);
                    recipesFinal = curRecipes;
                }
            }
            // remove excluded recipes
            if (recipesFinal != null)
            {
                RemoveExcludeds(ref recipesFinal.list);
            }

            calculationRecipes.Clear();
            if (recipesFinal != null)
            {
                foreach (Recipes.Recipe r in recipesFinal.list)
                {
                    calculationRecipes.Add(new DataGridItemModelRecipe(null, r));
                }
            }

            void RemoveExcludeds<T>(ref List<T> list)
            {
                object? obj;
                for (int i = list.Count - 1; i >= 0; --i)
                {
                    obj = list[i];
                    if (obj != null
                        && obj is Things.ThingBase
                        && ((Things.ThingBase)obj).isExcluded == true)
                    {
                        list.RemoveAt(i);
                    }
                }
            }

            // use thingsFinal, channelsFinal n' recipesFinal for calculation
        }

        public Calculator NewCalculate(Things.Thing product)
        {
            Calculator cal = new Calculator(product);
            return cal;
        }





        #endregion
    }
}
