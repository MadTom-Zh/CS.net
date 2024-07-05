using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MadTomDev.App.Classes.Recipes;
using static System.Windows.Forms.LinkLabel;

namespace MadTomDev.App.Classes
{
    public class ProcessingChains
    {
        private Core core = Core.Instance;
        Things.Thing targetProduct;
        private readonly List<SearchResult> _SearchResults = new List<SearchResult>();
        public List<SearchResult> SearchResults { get => _SearchResults; }

        /// <summary>
        /// 获取所有此物品的生产制程；
        /// 默认生产速度为1 /s
        /// 排序，原料少>速度快>工序少；
        /// </summary>
        /// <param name="targetProduct"></param>
        public ProcessingChains(Things.Thing targetProduct)
        {
            this.targetProduct = targetProduct;

            if (core.recipesFinal == null)
            {
                return;
            }
            List<SearchHelper> shList = new List<SearchHelper>();
            SearchHelper curSH;
            foreach (Recipes.Recipe r in GetRecipes(targetProduct))
            {
                // chain start
                curSH = new SearchHelper(r, targetProduct);
                shList.Add(curSH);
                // back loop
                SearchLoop(0, ref shList, curSH, curSH.finalProcess);
            }

            int i, iv;
            foreach (SearchHelper sh in shList)
            {
                // get all possible thing that go through channels
                sh.RegisterAllPosibleThings();

                // get available channels
                sh.RegisterAvailableChannels();

                // 检查现有通道是否支持全部连接
                sh.IsLinksPossible = sh.CheckChannels();
            }

            // remove sh that is not possible to link;
            for (i = shList.Count - 1; i >= 0; --i)
            {
                if (!shList[i].IsLinksPossible)
                {
                    shList.RemoveAt(i);
                }
            }

            // register other statics 
            foreach (SearchHelper sh in shList)
            {
                // get sufficient products
                sh.RegisterSufficientProducts();
            }


            // sort shList, calculate statics







            _SearchResults.AddRange(shList);

            // get AIOCP lists
            _Accessories.Clear();
            _Inputs.Clear();
            _Outputs.Clear();
            _Processings.Clear();
            SearchResult sr;
            for (i = 0, iv = _SearchResults.Count; i < iv; ++i)
            {
                sr = _SearchResults[i];
                _Accessories.AddRange(sr.AllAccessories);
                _Inputs.AddRange(sr.AllInputs);
                _Outputs.AddRange(sr.AllFinalOutputs);
                _Processings.AddRange(sr.AllRecipes);
                _EssentialChannels.AddRange(sr.allAvailableChannels);
                sr._ReSetLinksChannel();
            }
            SearchHelper.EliminateDuplication<Things.Thing>(ref _Accessories);
            SearchHelper.EliminateDuplication<Things.Thing>(ref _Inputs);
            SearchHelper.EliminateDuplication<Things.Thing>(ref _Outputs);
            SearchHelper.EliminateDuplication<Recipes.Recipe>(ref _Processings);
            SearchHelper.EliminateDuplication<Channels.Channel>(ref _EssentialChannels);
        }
        private List<Recipes.Recipe> GetRecipes(Things.Thing withOutput)
        {
            List<Recipes.Recipe> result = new List<Recipes.Recipe>();
            if (core.recipesFinal == null)
            {
                return result;
            }
            Recipes.Recipe.PIOItem? foundOutput;
            foreach (Recipes.Recipe r in core.recipesFinal.list)
            {
                foundOutput = r.outputs.Find(a => a.item != null && a.item.id == withOutput.id);
                if (foundOutput != null)
                {
                    result.Add(r);
                }
            }
            return result;
        }

        private void SearchLoop(int curLoopLv, ref List<SearchHelper> shList, SearchHelper sh, ProcessingNode pNode, int inputIdx = 0)
        {
            if (curLoopLv > 30)
            {
                throw new Exception($"Nest over [{30}] times!!"); ;
            }

            // close right, generate products
            sh.RegisterByproducts(pNode);

            // loop left
            Recipes.Recipe.PIOItem input;
            SearchHelper cloneSH;
            double outRate, inRate, inRateNeeded;
            ProcessingNode leftPNode;
            for (int i = inputIdx, iv = pNode.recipe.inputs.Count; i < iv; ++i)
            {
                input = pNode.recipe.inputs[i];
                if (input.item == null)
                {
                    throw new NullReferenceException($"Input[{i}] in recipe[{pNode.recipe.name}] is null.");
                }
                if (input.quantity == null)
                {
                    throw new NullReferenceException($"Quantity of input[{i}] in recipe[{pNode.recipe.name}] is null.");
                }

                // 当存在副产作为原料时，首先使用副产品；
                inRateNeeded = 0;
                inRate = SearchHelper.CalBaseSpeed(input.quantity.ValueCurrentInGeneral, pNode);

                if (sh.HaveSufficientProduct(input.item, out outRate))
                {
                    sh.LinkProductToInput(pNode, input.item);
                    if (outRate < inRate)
                    {
                        inRateNeeded = inRate - outRate;
                    }
                }
                else
                {
                    inRateNeeded = inRate;
                }

                // 如果副产品不够，使用新机组，最后使用原料
                if (inRateNeeded == 0)
                {
                    continue;
                }
                foreach (Recipes.Recipe r in GetRecipes(input.item))
                {
                    //if (sh.HaveRecipe(r))
                    //{
                    //    continue;
                    //}
                    cloneSH = sh.Clone();
                    shList.Add(cloneSH);
                    leftPNode = cloneSH.LinkLeftProcessing(sh.allProcesses.IndexOf(pNode), i, inRateNeeded, r);
                    SearchLoop(curLoopLv, ref shList, cloneSH, pNode, i + 1);

                    // 左级循环
                    SearchLoop(curLoopLv + 1, ref shList, cloneSH, leftPNode);
                }
                sh.LinkLeftSource(pNode, i);
            }
        }


        public ProcessingHead? selectedResult { private set; get; }
        public double SpeedOrigin { private set; get; } = 1;
        public double SpeedNew { private set; get; } = 1;


        private readonly List<Recipes.Recipe> _Processings = new List<Recipes.Recipe>();
        public List<Recipes.Recipe> Processings { get => _Processings; }
        private readonly List<Things.Thing> _Accessories = new List<Things.Thing>();
        public List<Things.Thing> Accessories { get => _Accessories; }
        private readonly List<Things.Thing> _Inputs = new List<Things.Thing>();
        public List<Things.Thing> Inputs { get => _Inputs; }
        private readonly List<Things.Thing> _Outputs = new List<Things.Thing>();
        public List<Things.Thing> Outputs { get => _Outputs; }

        /// <summary>
        /// 所有制程中可能出现的所有通道
        /// </summary>
        public List<Channels.Channel> EssentialChannels { get => _EssentialChannels; }
        private readonly List<Channels.Channel> _EssentialChannels = new List<Channels.Channel>();






        public class ProcessingNodeBase
        {
            public virtual Guid DataId { get; protected set; }
            public List<ProcessingLink> linksPrev = new List<ProcessingLink>();
            public List<ProcessingLink> linksNext = new List<ProcessingLink>();
            public int IndexOfPrevLink(ProcessingLink link)
            {
                return linksPrev.IndexOf(link);
            }
            public int IndexOfNextLink(ProcessingLink link)
            {
                return linksNext.IndexOf(link);
            }
        }
        public class ProcessingHead : ProcessingNodeBase
        {
            public Things.Thing thing;
            public double speed = 0;
            public double calSpeed = 0;
            public ProcessingHead(Things.Thing thing, double speed)
            {
                DataId = thing.id;
                this.thing = thing;
                this.speed = speed;
            }

            public ProcessingHead(ProcessingHead copyFrom)
            {
                DataId = copyFrom.thing.id;
                this.thing = copyFrom.thing;
                this.speed = copyFrom.speed;
            }

        }
        public class ProcessingNode : ProcessingNodeBase
        {
            public Recipes.Recipe recipe;
            public int ProcessLevel { get; internal set; }
            public double baseMultiple = 0;
            public double calMultiple = 0;

            public ProcessingNode(Recipes.Recipe recipe)
            {
                DataId = recipe.id;
                this.recipe = recipe;
            }
            public void CalNSetBaseMultiple(ProcessingHead oneProduct, double outputRate)
            {
                // 从输出找到对应产品
                Recipes.Recipe.PIOItem? output = recipe.outputs.Find(a => a.item != null && a.item.id == oneProduct.DataId);
                if (output == null)
                {
                    throw SearchHelper.Error_Recipe_Output_notFound(oneProduct.thing.name, recipe.name);
                }
                CalNSetBaseMultiple(recipe.outputs.IndexOf(output), outputRate);
            }
            public void CalNSetBaseMultiple(int outputIndex, double outputRate)
            {
                if (0 < outputIndex || recipe.outputs.Count <= outputIndex)
                {
                    throw new IndexOutOfRangeException($"Index[{outputIndex}] is out of recipe-outputs range.");
                }
                Recipes.Recipe.PIOItem pioItem = recipe.outputs[outputIndex];
                if (pioItem.quantity == null)
                {
                    throw SearchHelper.Error_Recipe_Output_Quantity_isNull(pioItem.item?.name, recipe.name);
                }
                // 按产品输出速度，计算处理速度
                baseMultiple = outputRate / SearchHelper.CalBaseSpeed(pioItem.quantity.ValueCurrentInGeneral, recipe.period, 1, recipe.name); ;
            }

            public int IndexOfInput(Things.Thing inputThing)
            {
                Things.Thing? curInputThing;
                for (int i = 0, iv = recipe.inputs.Count; i < iv; ++i)
                {
                    curInputThing = recipe.inputs[i].item;
                    if (curInputThing != null
                        && curInputThing.id == inputThing.id)
                    {
                        return i;
                    }
                }
                return -1;
            }
            public int IndexOfOutput(Things.Thing outputThing)
            {
                Things.Thing? curOutputThing;
                for (int i = 0, iv = recipe.outputs.Count; i < iv; ++i)
                {
                    curOutputThing = recipe.outputs[i].item;
                    if (curOutputThing != null
                        && curOutputThing.id == outputThing.id)
                    {
                        return i;
                    }
                }
                return -1;
            }

        }
        public class ProcessingLink
        {
            public Channels.Channel? channel;
            public Things.Thing thing;
            public double speed = 0;
            public double calSpeed = 0;
            public ProcessingNodeBase nodePrev;
            public ProcessingNodeBase nodeNext;
            public ProcessingLink(ProcessingNodeBase nodePrev, ProcessingNodeBase nodeNext, Things.Thing thing, double speed)
            {
                this.nodePrev = nodePrev;
                this.nodeNext = nodeNext;
                this.thing = thing;
                this.speed = speed;
            }
        }
        public class SearchResult
        {

            public ProcessingNode finalProcess;
            public ProcessingHead finalProduct;
            public List<ProcessingHead> allSources = new List<ProcessingHead>();
            public List<ProcessingHead> allFinalProducts = new List<ProcessingHead>();
            public List<ProcessingHead> sufficientProducts = new List<ProcessingHead>();
            public List<ProcessingNode> allProcesses = new List<ProcessingNode>();
            public List<ProcessingLink> allLinks = new List<ProcessingLink>();

            public List<Channels.Channel> allAvailableChannels = new List<Channels.Channel>();
            public List<Channels.Channel> optimizedChannels = new List<Channels.Channel>();
            public List<Things.Thing> allPossibleThings = new List<Things.Thing>();

            public double NewSpeed { private set; get; } = 1;
            public void ReCalSpeed(double newSpeed)
            {
                NewSpeed = newSpeed;
                foreach (ProcessingHead p in allSources)
                {
                    p.calSpeed = newSpeed * p.speed;
                }
                foreach (ProcessingHead p in allFinalProducts)
                {
                    p.calSpeed = newSpeed * p.speed;
                }
                foreach (ProcessingNode p in allProcesses)
                {
                    p.calMultiple = newSpeed * p.baseMultiple;
                }
                foreach (ProcessingLink l in allLinks)
                {
                    l.calSpeed = newSpeed * l.speed;
                }
            }

            public event Action<SearchResult> IntegrityChanged;

            #region 修改可用通道，检查连接是否完整

            /// <summary>
            /// 初始状态下（全部通道可用），工序是否可以正常连接
            /// </summary>
            public bool IsLinksPossible { internal set; get; }
            /// <summary>
            /// 排除过滤掉的通道，仅用剩余通道，工序是否可以正常连接
            /// </summary>
            public bool IsLinksIntact { private set; get; }

            private List<Things.ThingBase> _FilteredOutChannels = new List<Things.ThingBase>();
            public List<Things.ThingBase> FilteredOutChannels
            {
                get => _FilteredOutChannels;
            }
            public void AddFilterOutChannel(Things.ThingBase channel)
            {
                if (_FilteredOutChannels.Contains(channel))
                {
                    return;
                }
                _FilteredOutChannels.Add(channel);
                _CheckChannels();
                _ReSetLinksChannel();
            }
            public void RemoveFilterOutChannel(Things.ThingBase channel)
            {
                if (!_FilteredOutChannels.Contains(channel))
                {
                    return;
                }
                _FilteredOutChannels.Remove(channel);
                _CheckChannels();
                _ReSetLinksChannel();
            }
            private void _CheckChannels()
            {
                bool oriLI = IsLinksIntact;
                if (CheckChannels() != oriLI)
                {
                    IntegrityChanged?.Invoke(this);
                }
            }
            internal void _ReSetLinksChannel()
            {
                Channels.Channel? optChannel;
                foreach (ProcessingLink pLink in allLinks)
                {
                    pLink.channel = null;
                    optChannel = optimizedChannels.Find(a => a.contentList.Find(b => b.contentId == pLink.thing.id).contentId != Guid.Empty);
                    if (optChannel != null)
                    {
                        pLink.channel = optChannel;
                    }
                }
            }


            /// <summary>
            /// 检查当前通道（排除过滤掉的通道），是否可以正常连接工序
            /// </summary>
            /// <returns></returns>
            public bool CheckChannels()
            {
                optimizedChannels.Clear();

                List<Guid> tmpThingIdList = new List<Guid>();
                tmpThingIdList.AddRange(allPossibleThings.Select(a => a.id));

                List<Channels.Channel> availableChannelList = new List<Channels.Channel>();
                availableChannelList.AddRange(allAvailableChannels);
                foreach (Channels.Channel c in _FilteredOutChannels)
                {
                    availableChannelList.Remove(c);
                }
                //availableChannelList.Sort((a, b) =>
                //{
                //    if (a.speed == null && b.speed == null)
                //    {
                //        return 0;
                //    }
                //    if (a.speed == null)
                //    {
                //        return 1;
                //    }
                //    else if (b.speed == null)
                //    {
                //        return -1;
                //    }
                //    else if (a.speed == b.speed)
                //    {
                //        return 0;
                //    }
                //    else
                //    {
                //        return (a.speed > b.speed) ? -1 : 1;
                //    }
                //});



                Channels.Channel.ContentItem testCI;
                Channels.Channel curChannel;
                for (int i = 0, iv = availableChannelList.Count, j; i < iv; ++i)
                {
                    curChannel = availableChannelList[i];
                    for (j = tmpThingIdList.Count - 1; j >= 0; --j)
                    {
                        testCI = curChannel.contentList.Find(a => a.contentId == tmpThingIdList[j]);
                        if (testCI.contentId != Guid.Empty)
                        {
                            optimizedChannels.Add(curChannel);
                            tmpThingIdList.RemoveAt(j);
                        }
                    }
                    if (tmpThingIdList.Count == 0)
                    {
                        break;
                    }
                }
                SearchHelper.EliminateDuplication(ref optimizedChannels);
                IsLinksIntact = tmpThingIdList.Count == 0;
                return IsLinksIntact;
            }
            #endregion

            #region 修改可用配方，修改可用 插件、输入、输出、配方，检查删减后是否完整
            public bool IsIngredientsIntact { get; internal set; } = true;
            private List<Things.ThingBase> _FilteredOutAccessories = new List<Things.ThingBase>();
            public List<Things.ThingBase> FilteredOutAccessories
            {
                get => _FilteredOutAccessories;
            }
            private List<Things.ThingBase> _FilteredOutInputs = new List<Things.ThingBase>();
            public List<Things.ThingBase> FilteredOutInputs
            {
                get => _FilteredOutInputs;
            }
            private List<Things.ThingBase> _FilteredOutOutputs = new List<Things.ThingBase>();
            public List<Things.ThingBase> FilteredOutOutputs
            {
                get => _FilteredOutOutputs;
            }
            private List<Things.ThingBase> _FilteredOutProcedures = new List<Things.ThingBase>();
            public List<Things.ThingBase> FilteredOutProcedures
            {
                get => _FilteredOutProcedures;
            }

            internal void AddFilterOutAccessory(Things.ThingBase accessory, bool checkIngredients = true)
            {
                if (_FilteredOutAccessories.Contains(accessory))
                {
                    return;
                }
                _FilteredOutAccessories.Add(accessory);
                if (checkIngredients)
                {
                    _CheckIngredients();
                }
            }
            internal void RemoveFilterOutAccessory(Things.ThingBase accessory, bool checkIngredients = true)
            {
                if (!_FilteredOutAccessories.Contains(accessory))
                {
                    return;
                }
                _FilteredOutAccessories.Remove(accessory);
                if (checkIngredients)
                {
                    _CheckIngredients();
                }
            }
            internal void AddFilterOutInput(Things.ThingBase input, bool checkIngredients = true)
            {
                if (_FilteredOutInputs.Contains(input))
                {
                    return;
                }
                _FilteredOutInputs.Add(input);
                if (checkIngredients)
                {
                    _CheckIngredients();
                }
            }
            internal void RemoveFilterOutInput(Things.ThingBase input, bool checkIngredients = true)
            {
                if (!_FilteredOutInputs.Contains(input))
                {
                    return;
                }
                _FilteredOutInputs.Remove(input);
                if (checkIngredients)
                {
                    _CheckIngredients();
                }
            }
            internal void AddFilterOutOutput(Things.ThingBase output, bool checkIngredients = true)
            {
                if (_FilteredOutOutputs.Contains(output))
                {
                    return;
                }
                _FilteredOutOutputs.Add(output);
                if (checkIngredients)
                {
                    _CheckIngredients();
                }
            }
            internal void RemoveFilterOutOutput(Things.ThingBase output, bool checkIngredients = true)
            {
                if (!_FilteredOutOutputs.Contains(output))
                {
                    return;
                }
                _FilteredOutOutputs.Remove(output);
                if (checkIngredients)
                {
                    _CheckIngredients();
                }
            }
            internal void AddFilterOutProcedure(Things.ThingBase procedure, bool checkIngredients = true)
            {
                if (_FilteredOutProcedures.Contains(procedure))
                {
                    return;
                }
                _FilteredOutProcedures.Add(procedure);
                if (checkIngredients)
                {
                    _CheckIngredients();
                }
            }
            internal void RemoveFilterOutProcedure(Things.ThingBase procedure, bool checkIngredients = true)
            {
                if (!_FilteredOutProcedures.Contains(procedure))
                {
                    return;
                }
                _FilteredOutProcedures.Remove(procedure);
                if (checkIngredients)
                {
                    _CheckIngredients();
                }
            }
            private void _CheckIngredients()
            {
                bool oriII = IsIngredientsIntact;
                if (CheckIngredients() != oriII)
                {
                    IntegrityChanged?.Invoke(this);
                }
            }

            public bool CheckIngredients()
            {
                if (!Check(_AllAccessories, _FilteredOutAccessories))
                {
                    IsIngredientsIntact = false;
                    return false;
                }
                if (!Check(_AllInputs, _FilteredOutInputs))
                {
                    IsIngredientsIntact = false;
                    return false;
                }
                if (!Check(_AllFinalOutputs, _FilteredOutOutputs))
                {
                    IsIngredientsIntact = false;
                    return false;
                }
                if (!CheckR(_AllRecipes, _FilteredOutProcedures))
                {
                    IsIngredientsIntact = false;
                    return false;
                }
                IsIngredientsIntact = true;
                return true;


                bool Check(List<Things.Thing> sourceList, List<Things.ThingBase> filterList)
                {
                    foreach (Things.ThingBase f in filterList)
                    {
                        if (sourceList.Find(a => a.id == f.id) != null)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                bool CheckR(List<Recipes.Recipe> sourceList, List<Things.ThingBase> filterList)
                {
                    foreach (Things.ThingBase f in filterList)
                    {
                        if (sourceList.Find(a => a.id == f.id) != null)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            #endregion

            #region statistics
            private List<Things.Thing> _AllAccessories = new List<Things.Thing>();
            public List<Things.Thing> AllAccessories
            {
                get
                {
                    if (_AllAccessories.Count == 0)
                    {
                        IEnumerable<Things.Thing?>? tmpList;
                        for (int i = 0, iv = allProcesses.Count; i < iv; ++i)
                        {
                            tmpList = allProcesses[i].recipe?.accessories.Select(a => a.item);
                            if (tmpList != null)
                            {
                                foreach (Things.Thing? t in tmpList)
                                {
                                    if (t != null)
                                    {
                                        _AllAccessories.Add(t);
                                    }
                                }
                            }
                        }
                        SearchHelper.EliminateDuplication<Things.Thing>(ref _AllAccessories);
                    }
                    return _AllAccessories;
                }
            }

            private List<Things.Thing> _AllInputs = new List<Things.Thing>();
            public List<Things.Thing> AllInputs
            {
                get
                {
                    if (_AllInputs.Count == 0)
                    {
                        _AllInputs.AddRange(allSources.Select(a => a.thing));
                        SearchHelper.EliminateDuplication<Things.Thing>(ref _AllInputs);
                    }
                    return _AllInputs;
                }
            }

            private List<Things.Thing> _AllFinalOutputs = new List<Things.Thing>();
            public List<Things.Thing> AllFinalOutputs
            {
                get
                {
                    if (_AllFinalOutputs.Count == 0)
                    {
                        _AllFinalOutputs.AddRange(allFinalProducts.Select(a => a.thing));
                        SearchHelper.EliminateDuplication<Things.Thing>(ref _AllFinalOutputs);
                    }
                    return _AllFinalOutputs;
                }
            }

            private List<Recipes.Recipe> _AllRecipes = new List<Recipes.Recipe>();
            public List<Recipes.Recipe> AllRecipes
            {
                get
                {
                    if (_AllRecipes.Count == 0)
                    {
                        _AllRecipes.AddRange(allProcesses.Select(a => a.recipe));
                    }
                    return _AllRecipes;
                }
            }

            #endregion
        }
        public class SearchHelper : SearchResult
        {


            public SearchHelper(Recipes.Recipe recipe, Things.Thing productThing)
            {
                finalProduct = new ProcessingHead(productThing, 1.0);
                allFinalProducts.Add(finalProduct);

                finalProcess = new ProcessingNode(recipe) { ProcessLevel = 0, };
                finalProcess.CalNSetBaseMultiple(finalProduct, 1.0);
                allProcesses.Add(finalProcess);

                _Link(finalProcess, finalProduct, finalProduct.speed);

                RegisterByproducts(finalProcess);
            }

            #region quick link

            /// <summary>
            /// 新建匹配输入速度的源，并将其连接
            /// </summary>
            /// <param name="pNode">处理节点</param>
            /// <param name="inputIdx">输入端序号</param>
            internal void LinkLeftSource(ProcessingNode pNode, int inputIdx)
            {
                Recipes.Recipe.PIOItem pioItem = pNode.recipe.inputs[inputIdx];
                Things.Thing? leftThing = pioItem.item;
                if (leftThing == null)
                {
                    throw Error_Recipe_Input_Item_isNull(inputIdx, pNode.recipe.name);
                }
                if (pioItem.quantity == null)
                {
                    throw Error_Recipe_Input_Quantity_isNull(inputIdx, pNode.recipe.name);
                }

                double inputRateNeeded = GetNeededInFlowRate(pNode, inputIdx);
                if (inputRateNeeded == 0)
                {
                    return;
                }

                ProcessingHead leftHead = new ProcessingHead(leftThing, inputRateNeeded);
                allSources.Add(leftHead);

                _Link(leftHead, pNode, inputRateNeeded);
            }


            /// <summary>
            /// 给当前节点连接的某项输入连接处理节点，并返回这个节点
            /// </summary>
            /// <param name="rightPNode">要添加左处理的节点</param>
            /// <param name="rightInputIdx">左输入的索引号</param>
            /// <param name="leftRecipe">要连接的处理配方</param>
            /// <returns>新增的左处理节点</returns>
            internal ProcessingNode LinkLeftProcessing(ProcessingNode rightPNode, int inputIdx, double inputRate, Recipes.Recipe leftRecipe, bool autoRegesterByproducts = true)
            {
                Recipes.Recipe.PIOItem input = rightPNode.recipe.inputs[inputIdx];
                if (input.item == null)
                {
                    throw Error_Recipe_Input_Item_isNull(inputIdx, rightPNode.recipe.name);
                }

                ProcessingNode leftPNode = new ProcessingNode(leftRecipe)
                { ProcessLevel = rightPNode.ProcessLevel + 1, };
                leftPNode.CalNSetBaseMultiple(leftPNode.IndexOfOutput(input.item), inputRate);
                allProcesses.Add(leftPNode);

                _Link(leftPNode, rightPNode, input.item, inputRate);
                if (autoRegesterByproducts)
                {
                    RegisterByproducts(leftPNode);
                }

                return leftPNode;
            }
            internal ProcessingNode LinkLeftProcessing(int pNodeIndex, int inputIdx, double inputRate, Recipes.Recipe leftRecipe, bool autoRegesterByproducts = true)
            {
                return LinkLeftProcessing(allProcesses[pNodeIndex], inputIdx, inputRate, leftRecipe, autoRegesterByproducts);
            }
            private ProcessingLink _Link(ProcessingHead leftHead, ProcessingNode rightNode, double speed)
            {
                if (rightNode.recipe.inputs.Find(a => a.item != null && a.item.id == leftHead.thing.id) == null)
                {
                    throw Error_Recipe_Input_Item_isNull(leftHead.thing.name, rightNode.recipe.name);
                }
                double outRate = GetAvailableOutFlowRate(leftHead);
                if (outRate < speed)
                {
                    throw Error_Not_Enough_FlowRate(outRate, speed);
                }
                ProcessingLink link = new ProcessingLink(leftHead, rightNode, leftHead.thing, speed);
                leftHead.linksNext.Add(link);
                rightNode.linksPrev.Add(link);
                allLinks.Add(link);
                return link;
            }
            private ProcessingLink _Link(ProcessingNode leftNode, ProcessingHead rightHead, double speed)
            {
                if (leftNode.recipe.outputs.Find(a => a.item != null && a.item.id == rightHead.thing.id) == null)
                {
                    throw Error_Recipe_Output_Item_isNull(rightHead.thing.name, leftNode.recipe.name);
                }

                // 检查speed是否超出允许的最大值
                double outRate = GetAvailableOutFlowRate(leftNode, rightHead.thing);
                if (outRate < speed)
                {
                    throw Error_Not_Enough_FlowRate(outRate, speed);
                }

                ProcessingLink link = new ProcessingLink(leftNode, rightHead, rightHead.thing, speed);
                leftNode.linksNext.Add(link);
                rightHead.linksPrev.Add(link);
                allLinks.Add(link);
                return link;
            }
            private ProcessingLink _Link(ProcessingNode leftNode, ProcessingNode rightNode, Things.Thing thing, double speed)
            {
                if (leftNode.recipe.outputs.Find(a => a.item != null && a.item.id == thing.id) == null)
                {
                    throw Error_Recipe_Output_Item_isNull(thing.name, leftNode.recipe.name);
                }
                if (rightNode.recipe.inputs.Find(a => a.item != null && a.item.id == thing.id) == null)
                {
                    throw Error_Recipe_Input_Item_isNull(thing.name, rightNode.recipe.name);
                }
                double outRate = GetAvailableOutFlowRate(leftNode, thing);
                if (outRate < speed)
                {
                    throw Error_Not_Enough_FlowRate(outRate, speed);
                }
                ProcessingLink link = new ProcessingLink(leftNode, rightNode, thing, speed);
                leftNode.linksNext.Add(link);
                rightNode.linksPrev.Add(link);
                allLinks.Add(link);
                return link;
            }
            public int RegisterByproducts(ProcessingNode pNode)
            {
                int count = 0;
                ProcessingLink? linkN;
                Recipes.Recipe.PIOItem output;
                ProcessingHead outputHead;
                double outputSpeed;
                for (int i = 0, iv = pNode.recipe.outputs.Count; i < iv; ++i)
                {
                    output = pNode.recipe.outputs[i];
                    if (output.item == null)
                    {
                        throw Error_Recipe_Output_Item_isNull(i, pNode.recipe.name);
                    }
                    linkN = pNode.linksNext.Find(a => a.thing.id == output.item.id);
                    if (linkN != null)
                    {
                        continue;
                    }
                    if (output.quantity == null)
                    {
                        throw Error_Recipe_Output_Quantity_isNull(i, pNode.recipe.name);
                    }

                    // empty right found, register
                    outputSpeed = SearchHelper.CalBaseSpeed(output.quantity.ValueCurrentInGeneral, pNode);
                    outputHead = new ProcessingHead(output.item, outputSpeed);
                    allFinalProducts.Add(outputHead);

                    _Link(pNode, outputHead, outputSpeed);

                    ++count;
                }
                return count;
            }

            internal void LinkProductToInput(ProcessingNode rightPNode, Things.Thing product)
            {
                // 首先计算此节点对此物品，需要多大输入流速                
                int inputIndex = rightPNode.IndexOfInput(product);
                Recipes.Recipe.PIOItem input = rightPNode.recipe.inputs[inputIndex];
                if (input.quantity == null)
                {
                    throw Error_Recipe_Input_Quantity_isNull(inputIndex, rightPNode.recipe.name);
                }
                double neededInputSpeed = GetNeededInFlowRate(rightPNode, inputIndex);
                if (neededInputSpeed <= 0)
                {
                    throw new Exception("No band for more input.");
                }

                // 按需提供流速，如果供大于求，则直接满足，如果求大于供，则全部供应；
                double curProdOutSpeedLeft;
                foreach (ProcessingHead p in allFinalProducts)
                {
                    if (p.DataId != product.id)
                    {
                        continue;
                    }
                    curProdOutSpeedLeft = GetAvailableOutFlowRate(p);
                    if (curProdOutSpeedLeft <= 0)
                    {
                        continue;
                    }
                    if (neededInputSpeed <= curProdOutSpeedLeft)
                    {
                        // 完全满足需求
                        _Link(p, rightPNode, neededInputSpeed);
                        break;
                    }
                    else
                    {
                        // 部分满足，需要下一循环（最后也可能会无法完全满足）
                        _Link(p, rightPNode, curProdOutSpeedLeft);
                        neededInputSpeed -= curProdOutSpeedLeft;
                    }
                }
            }



            /// <summary>
            /// 最后执行，用于记录所有可输出物品
            /// </summary>
            internal void RegisterSufficientProducts()
            {
                ProcessingHead prod;
                for (int i = 0, iv = allFinalProducts.Count; i < iv; ++i)
                {
                    prod = allFinalProducts[i];
                    if (GetAvailableOutFlowRate(prod) > 0)
                    {
                        sufficientProducts.Add(prod);
                    }
                }
            }

            /// <summary>
            /// 记录所有可能出现的物品，包括源、产品，和工序的原料和副产品
            /// </summary>
            internal void RegisterAllPosibleThings()
            {
                allPossibleThings.AddRange(allSources.Select(a => a.thing));
                allPossibleThings.AddRange(allFinalProducts.Select(a => a.thing));
                foreach (ProcessingNode pNode in allProcesses)
                {
                    foreach (Things.Thing? t in pNode.recipe.inputs.Select(a => a.item))
                    {
                        if (t != null)
                        {
                            allPossibleThings.Add(t);
                        }
                    }
                    foreach (Things.Thing? t in pNode.recipe.outputs.Select(a => a.item))
                    {
                        if (t != null)
                        {
                            allPossibleThings.Add(t);
                        }
                    }
                }
                EliminateDuplication<Things.Thing>(ref allPossibleThings);
            }

            /// <summary>
            /// 记录将所有可使用的通道
            /// </summary>
            internal void RegisterAvailableChannels()
            {
                if (allPossibleThings.Count == 0)
                {
                    RegisterAllPosibleThings();
                }
                Channels? finalChannels = Core.Instance.channelsFinal;
                bool channelItemFound;
                Channels.Channel.ContentItem testChannelItem;
                if (finalChannels != null)
                {
                    foreach (Channels.Channel c in finalChannels.list)
                    {
                        channelItemFound = false;
                        foreach (Things.Thing t in allPossibleThings)
                        {
                            testChannelItem = c.contentList.Find(a => a.contentId == t.id);
                            if (testChannelItem.contentId != Guid.Empty)
                            {
                                channelItemFound = true;
                                break;
                            }
                        }
                        if (channelItemFound)
                        {
                            allAvailableChannels.Add(c);
                        }
                    }
                    allAvailableChannels.Sort((a, b) =>
                    {
                        bool aSpeedGreater = false;
                        if (a.speed == null && b.speed == null)
                        {
                            return 0;
                        }
                        else if (a.speed == null)
                        {
                            aSpeedGreater = b.speed < 0;
                        }
                        else if (b.speed == null)
                        {
                            aSpeedGreater = a.speed > 0;
                        }
                        else if (a.speed == b.speed)
                        {
                            return 0;
                        }
                        else
                        {
                            aSpeedGreater = a.speed > b.speed;
                        }
                        return aSpeedGreater ? -1 : 1;
                    });
                }
            }

            #endregion


            #region checks, calculates

            public bool HaveRecipe(Recipes.Recipe r)
            {
                ProcessingNode? testNode = allProcesses.Find(a => a.DataId == r.id);
                return testNode != null;
            }
            internal bool HaveSufficientProduct(Things.Thing item, out double outRate)
            {
                outRate = 0;
                List<ProcessingLink> outLinkList;
                foreach (ProcessingHead p in allFinalProducts)
                {
                    if (p.thing.id != item.id)
                    {
                        continue;
                    }
                    outRate += p.speed;

                    outLinkList = allLinks.FindAll(a => a.nodePrev == p);
                    foreach (ProcessingLink outLink in outLinkList)
                    {
                        outRate -= outLink.speed;
                    }
                }
                return outRate > 0;
            }


            public static double CalBaseSpeed(double quantityGeneral, double? period, double baseMultiple, string? recipeName)
            {
                if (period == null)
                {
                    throw SearchHelper.Error_Recipe_Peroid_isNullOrZero(recipeName);
                }
                return quantityGeneral / (double)period * baseMultiple;
            }
            public static double CalBaseSpeed(double quantityGeneral, ProcessingNode pNode)
            {
                return CalBaseSpeed(quantityGeneral, pNode.recipe.period, pNode.baseMultiple, pNode.recipe.name);
            }

            #endregion


            #region get flow rates

            internal void GetInFlowRate(ProcessingNode pNode, int inputIndex, out double inRate, out double inputRateCurrent)
            {
                inputRateCurrent = 0;
                Recipes.Recipe.PIOItem pioItem = pNode.recipe.inputs[inputIndex];
                Recipes.Recipe.Quantity? q = pioItem.quantity;
                if (pioItem.item == null)
                {
                    throw Error_Recipe_Input_Item_isNull(inputIndex, pNode.recipe.name);
                }
                if (q == null)
                {
                    throw Error_Recipe_Input_Quantity_isNull(inputIndex, pNode.recipe.name);
                }
                inRate = SearchHelper.CalBaseSpeed(q.ValueCurrentInGeneral, pNode);

                List<ProcessingLink> inlinkList = allLinks.FindAll(a => a.nodeNext == pNode && a.thing.id == pioItem.item.id);
                foreach (ProcessingLink inLink in inlinkList)
                {
                    inputRateCurrent += inLink.speed;
                }
            }
            internal double GetNeededInFlowRate(ProcessingNode pNode, Things.Thing inputThing)
            {
                int inputIndex = pNode.IndexOfInput(inputThing);
                return GetNeededInFlowRate(pNode, inputIndex);
            }
            internal double GetNeededInFlowRate(ProcessingNode pNode, int inputIndex)
            {
                GetInFlowRate(pNode, inputIndex, out double inputSpeedMax, out double inputSpeedCurrent);
                return inputSpeedMax - inputSpeedCurrent;
            }


            internal void GetOutFlowRate(ProcessingHead leftHead, out double outRate, out double outputRateCurrent)
            {
                outRate = leftHead.speed;
                outputRateCurrent = 0;
                foreach (ProcessingLink outLink in allLinks.FindAll(a => a.nodePrev == leftHead))
                {
                    outputRateCurrent += outLink.speed;
                }
            }
            internal void GetOutFlowRate(ProcessingNode leftPNode, int outputIndex, out double outRate, out double outputRateCurrent)
            {
                Recipes.Recipe.PIOItem output = leftPNode.recipe.outputs[outputIndex];
                if (output.quantity == null)
                {
                    throw Error_Recipe_Output_Quantity_isNull(outputIndex, leftPNode.recipe.name);
                }
                outRate = SearchHelper.CalBaseSpeed(output.quantity.ValueCurrentInGeneral, leftPNode);
                outputRateCurrent = 0;
                foreach (ProcessingLink outLink in allLinks.FindAll(a => a.nodePrev == leftPNode && a.thing == output.item))
                {
                    outputRateCurrent += outLink.speed;
                }
            }
            internal double GetAvailableOutFlowRate(ProcessingHead leftHead)
            {
                GetOutFlowRate(leftHead, out double outputRate, out double outputRateCurrent);
                return outputRate - outputRateCurrent;
            }
            internal double GetAvailableOutFlowRate(ProcessingNode leftNode, Things.Thing outputThing)
            {
                int outputIndex = leftNode.IndexOfOutput(outputThing);
                return GetAvailableOutFlowRate(leftNode, outputIndex);
            }
            internal double GetAvailableOutFlowRate(ProcessingNode leftNode, int outputIndex)
            {
                GetOutFlowRate(leftNode, outputIndex, out double outputRate, out double outputRateCurrent);
                return outputRate - outputRateCurrent;
            }

            #endregion


            #region exceptions
            public static Exception Error_Recipe_Input_Item_isNull(int inputIndex, string? recipeName)
            {
                return new NullReferenceException($"Item of input[{inputIndex}] in recipe[{recipeName}] is null.");
            }
            public static Exception Error_Recipe_Input_Item_isNull(string? inputThingName, string? recipeName)
            {
                return new NullReferenceException($"Item[{inputThingName}] of input in recipe[{recipeName}] is null.");
            }
            public static Exception Error_Recipe_Output_Item_isNull(int outputIndex, string? recipeName)
            {
                return new NullReferenceException($"Item of output[{outputIndex}] in recipe[{recipeName}] is null.");
            }
            public static Exception Error_Recipe_Output_Item_isNull(string? outputThingName, string? recipeName)
            {
                return new NullReferenceException($"Item[{outputThingName}] of output in recipe[{recipeName}] is null.");
            }
            public static Exception Error_Recipe_Input_Quantity_isNull(int inputIndex, string? recipeName)
            {
                return new NullReferenceException($"Quantity of input[{inputIndex}] in recipe[{recipeName}] is null.");
            }
            public static Exception Error_Recipe_Input_notFound(string? inputThingName, string? recipeName)
            {
                return new NullReferenceException($"Can't find input[{inputThingName}] in recipe[{recipeName}].");
            }
            public static Exception Error_Recipe_Output_notFound(string? outputThingName, string? recipeName)
            {
                return new NullReferenceException($"Can't find output[{outputThingName}] in recipe[{recipeName}].");
            }
            public static Exception Error_Recipe_Output_Quantity_isNull(int outputIndex, string? recipeName)
            {
                return new NullReferenceException($"Quantity of output[{outputIndex}] in recipe[{recipeName}] is null.");
            }
            public static Exception Error_Recipe_Output_Quantity_isNull(string? outputThingName, string? recipeName)
            {
                return new NullReferenceException($"Quantity of output[{outputThingName}] in recipe[{recipeName}] is null.");
            }

            public static Exception Error_Not_Enough_FlowRate(double outRate, double needRate)
            {
                throw new Exception($"Not enough flow rate left[{outRate}] for needs[{needRate}].");
            }

            public static Exception Error_Recipe_Peroid_isNullOrZero(string? recipeName)
            {
                throw new Exception($"The peroid of recipe[{recipeName}] is null or zero.");
            }
            public static Exception Error_Channel_isNull()
            {
                return new NullReferenceException($"Channel is null.");
            }
            public static Exception Error_Channel_Speed_isNull(Channels.Channel channel)
            {
                return new NullReferenceException($"Speed of channel[{channel.name}] is null.");
            }
            
            #endregion


            #region other helper

            public static void EliminateDuplication<T>(ref List<T> list)
            {
                int i, iv, j, jv;
                T a, b;
                for (i = 0, jv = list.Count, iv = jv - 1; i < iv; ++i)
                {
                    a = list[i];
                    for (j = i + 1; j < jv; ++j)
                    {
                        b = list[j];
                        if (a != null && a.Equals(b))
                        {
                            list.RemoveAt(j);
                            --j; --iv; --jv;
                        }
                    }
                }
            }

            #endregion



            public SearchHelper Clone()
            {
                SearchHelper clone = new SearchHelper(finalProcess.recipe, finalProduct.thing);
                _Clone(ref clone, clone.finalProcess, this, this.finalProcess);

                return clone;
            }
            private void _Clone(ref SearchHelper cloneHelper, ProcessingNode cloneNode, SearchHelper sourceHelper, ProcessingNode sourceNode)
            {
                ProcessingLink referLink;
                int i, iv;

                // right to product
                RegisterByproducts(cloneNode);

                int inputIndex;
                ProcessingNode referLeftPNode, newLeftPNode;
                List<ProcessingNode> newLeftPNodes = new List<ProcessingNode>();
                List<ProcessingNode> referLeftPNodes = new List<ProcessingNode>();
                for (i = 0, iv = sourceNode.linksPrev.Count; i < iv; ++i)
                {
                    referLink = sourceNode.linksPrev[i];
                    inputIndex = sourceNode.IndexOfInput(referLink.thing);
                    if (referLink.nodePrev is ProcessingNode)
                    {
                        // left node is processing
                        // already add input from product, use processing last
                        referLeftPNode = (ProcessingNode)referLink.nodePrev;
                        referLeftPNodes.Add(referLeftPNode);

                        newLeftPNode = cloneHelper.LinkLeftProcessing(cloneNode, inputIndex, referLink.speed, sourceNode.recipe);
                        newLeftPNodes.Add(newLeftPNode);

                        // loop, level first, depth second
                    }
                    else // referLink.nodePrev is ProcessingHead, source or product
                    {
                        if (sourceHelper.allSources.Contains(referLink.nodePrev))
                        {
                            // left head is source
                            cloneHelper.LinkLeftSource(cloneNode, inputIndex);
                        }
                        else // node in allProducts
                        {
                            // left head is product
                            cloneHelper.LinkProductToInput(cloneNode, referLink.thing);
                        }
                    }
                }
                // loop left
                for (i = 0, iv = newLeftPNodes.Count; i < iv; ++i)
                {
                    _Clone(ref cloneHelper, newLeftPNodes[i], sourceHelper, referLeftPNodes[i]);
                }
            }

        }
    }
}
