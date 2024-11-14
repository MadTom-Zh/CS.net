using MadTomDev.App.Ctrls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using static MadTomDev.App.Classes.Recipes;
using static MadTomDev.App.Classes.Recipes.Recipe;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MadTomDev.App.Classes
{
    public class ProcessingChains
    {
        private Core core = Core.Instance;
        Things.Thing targetProduct;
        private readonly List<ResultHelper> _SearchResults = new List<ResultHelper>();
        public static decimal aboutZero = (decimal)0.000000000001;
        public List<ResultHelper> SearchResults { get => _SearchResults; }

        public HashSet<Things.Thing> allowedSourceThings = new HashSet<Things.Thing>();
        public HashSet<Things.Thing> allowedProductThings = new HashSet<Things.Thing>();
        //public HashSet<Channels.Channel> allowedChannels = new HashSet<Channels.Channel>();
        /// <summary>
        /// channels[0] has greatest speed
        /// </summary>
        public List<Channels.Channel> optimizedChannels = new List<Channels.Channel>();
        public HashSet<Recipes.Recipe> allowedRecipes = new HashSet<Recipe>();
        //public HashSet<Things.Thing> allowedAccessories = new HashSet<Things.Thing>();

        public Channels.Channel GetOptimizedChannel(Guid thingId)
        {
            for (int i = 0, iv = optimizedChannels.Count; i < iv; ++i)
            {
                if (optimizedChannels[i].contentList.Find(a => a.contentId == thingId).contentId != Guid.Empty)
                {
                    return optimizedChannels[i];
                }
            }
            throw new Exception("Cant find a usable channel");
        }
        public static Channels.Channel? GetOptimizedChannel(IEnumerable<Channels.Channel> channels, Guid thingId)
        {
            List<Channels.Channel> usableChannels = new List<Channels.Channel>();
            foreach (Channels.Channel c in channels)
            {
                if (c.contentList.Find(a => a.contentId == thingId).contentId != Guid.Empty)
                {
                    usableChannels.Add(c);
                }
            }
            if (usableChannels.Count > 0)
            {
                SortChannelsSpeedDesc(ref usableChannels);
                return usableChannels[0];
            }
            return null;
        }

        /// <summary>
        /// 获取所有此物品的生产制程；
        /// 默认生产速度为1 /s
        /// 排序，原料少>速度快>工序少；
        /// </summary>
        /// <param name="targetProduct"></param>
        public ProcessingChains(Things.Thing targetProduct, List<Guid> filterRecipeIDList,
            List<Guid> filterAccessoryIDList, List<Guid> filterChannelIDList,
            List<Guid> filterInputIDList, List<Guid> filterOutputIDList)
        {
            this.targetProduct = targetProduct;

            if (core.recipesFinal == null)
            {
                return;
            }

            #region get allowed items
            int i, iv;
            ResultHelper.GetRelatedIdList(
                targetProduct.id,
                out List<Guid> rIdList,
                out List<Guid> aIdList,
                out List<Guid> iIdList,
                out List<Guid> oIdList);



            if (core.thingsFinal is not null)
            {
                Things.Thing? foundThing;
                foreach (Guid id in iIdList)
                {
                    if (filterInputIDList.Contains(id) == false)
                    {
                        foundThing = core.FindThing(id);
                        if (foundThing is not null)
                        {
                            allowedSourceThings.Add(foundThing);
                        }
                    }
                }
                foreach (Guid id in oIdList)
                {
                    if (filterOutputIDList.Contains(id) == false)
                    {
                        foundThing = core.FindThing(id);
                        if (foundThing is not null)
                        {
                            allowedProductThings.Add(foundThing);
                        }
                    }
                }
            }
            if (allowedProductThings.Contains(targetProduct) == false)
            {
                return;
            }
            List<Channels.Channel> curChannelList;
            if (core.channelsFinal is not null)
            {
                curChannelList = core.channelsFinal.list;
                foreach (Channels.Channel c in curChannelList)
                {
                    if (filterChannelIDList.Contains(c.id) == false)
                    {
                        optimizedChannels.Add(c);
                    }
                }
            }
            if (optimizedChannels.Count == 0)
            {
                MessageBox.Show("No channel.");
                return;
            }
            if (core.recipesFinal is not null)
            {
                Recipes.Recipe? foundRecipe;
                foreach (Guid id in rIdList)
                {
                    if (filterRecipeIDList.Contains(id) == false)
                    {
                        foundRecipe = core.FindRecipe(id);
                        if (foundRecipe is not null)
                        {
                            if (foundRecipe.accessories.Count > 0)
                            {
                                bool filtered = false;
                                foreach (PIOItem pio in foundRecipe.accessories)
                                {
                                    if (pio.thing is not null
                                        && filterAccessoryIDList.Contains(pio.thing.id))
                                    {
                                        filtered = true;
                                        break;
                                    }
                                }
                                if (filtered)
                                {
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                    // if has non-transable things, skip
                    if (CheckCanTrans(foundRecipe, optimizedChannels) == false)
                    {
                        continue;
                    }
                    allowedRecipes.Add(foundRecipe);
                }
            }
            bool CheckCanTrans(Recipe recipe, List<Channels.Channel> channelList)
            {
                int canTransInputCount = 0, canTransOutputCount = 0;
                int inputTotal = recipe.inputs.Count, outputTotal = recipe.outputs.Count;
                foreach (Channels.Channel c in channelList)
                {
                    foreach (PIOItem pio in recipe.inputs)
                    {
                        if (pio.thing is null)
                        {
                            continue;
                        }
                        if (c.contentList.Find(a => a.contentId == pio.thing.id).contentId != Guid.Empty)
                        {
                            canTransInputCount++;
                            continue;
                        }
                    }
                    foreach (PIOItem pio in recipe.outputs)
                    {
                        if (pio.thing is null)
                        {
                            continue;
                        }
                        if (c.contentList.Find(a => a.contentId == pio.thing.id).contentId != Guid.Empty)
                        {
                            canTransOutputCount++;
                            continue;
                        }
                    }
                    if (canTransInputCount == inputTotal && canTransOutputCount == outputTotal)
                    {
                        return true;
                    }
                }
                return false;
            }
            if (allowedRecipes.Count == 0)
            {
                MessageBox.Show("No recipe.");
                return;
            }
            SortChannelsSpeedDesc(ref optimizedChannels);
            #endregion



            List<ResultHelper> shList = new List<ResultHelper>();
            ResultHelper curSH;
            foreach (Recipes.Recipe r in GetRecipes(allowedRecipes, targetProduct))
            {
                // chain start
                curSH = new ResultHelper(this, r, targetProduct);
                shList.Add(curSH);
                // back loop
                if (!SearchLoop(0, ref shList, curSH))
                {
                    return;
                }
            }

            int j, jv, k, kv;


            ProcessingHead head1, head2;
            ProcessingLink link;
            Channels.Channel? foundChannel;
            foreach (ResultHelper sh in shList)
            {
                sh.Closure();
            }
            for (i = shList.Count - 1; i >= 0; --i)
            {
                if (shList[i].CheckInputsNOutputs() == false)
                {
                    shList.RemoveAt(i);
                }
            }
            if (shList.Count > 1000)
            {
                MessageBox.Show("Over 1000 Results.");
                return;
            }
            i = shList.Count - 1;
            bool haveSh = i > 0;
            if (haveSh && shList.Count == 0)
            {
                MessageBox.Show("No possible channel, or no result.");
            }




            _SearchResults.AddRange(shList);

            // get AIOCP lists
            _Accessories.Clear();
            _Inputs.Clear();
            _Outputs.Clear();
            _Processings.Clear();
            ResultHelper sr;
            for (i = 0, iv = _SearchResults.Count; i < iv; ++i)
            {
                sr = _SearchResults[i];
                _Accessories.AddRange(sr.AllAccessories);
                _Inputs.AddRange(sr.AllInputs);
                _Outputs.AddRange(sr.AllFinalOutputs);
                _Processings.AddRange(sr.AllRecipes);
                _EssentialChannels.AddRange(sr.allUsedChannels);
            }
            ResultHelper.EliminateDuplication<Things.Thing>(ref _Accessories);
            ResultHelper.EliminateDuplication<Things.Thing>(ref _Inputs);
            ResultHelper.EliminateDuplication<Things.Thing>(ref _Outputs);
            ResultHelper.EliminateDuplication<Recipes.Recipe>(ref _Processings);
            ResultHelper.EliminateDuplication<Channels.Channel>(ref _EssentialChannels);

        }

        public static void SortChannelsSpeedDesc(ref List<Channels.Channel> channelList)
        {
            channelList.Sort((a, b) =>
            {
                if (a.speed is null && b.speed is null) return 0;
                bool aSpeedGreater = false;
                if (b.speed is null) aSpeedGreater = a.speed > 0;
                else if (a.speed is null) aSpeedGreater = b.speed < 0;
                else aSpeedGreater = b.speed < a.speed;
                return aSpeedGreater ? -1 : 1;
            });
        }

        public static List<Recipes.Recipe> GetRecipes(IEnumerable<Recipe> source, Things.Thing withOutput)
        {
            return source.Where(r => r.outputs.Any(
                b => b.thing is not null && b.thing.id == withOutput.id))
                .ToList();
        }
        public static List<Recipes.Recipe> GetRecipes(Things.Thing withOutput)
        {
            List<Recipes.Recipe> result = new List<Recipes.Recipe>();
            if (Core.Instance.recipesFinal == null)
            {
                return result;
            }
            Recipes.Recipe.PIOItem? foundOutput;
            foreach (Recipes.Recipe r in Core.Instance.recipesFinal.list)
            {
                foundOutput = r.outputs.Find(a => a.thing != null && a.thing.id == withOutput.id);
                if (foundOutput != null)
                {
                    result.Add(r);
                }
            }
            return result;
        }

        private bool SearchLoop(int curLoopLv, ref List<ResultHelper> shList, ResultHelper sh)
        {
            if (curLoopLv > 30)
            {
                return false;
                throw new Exception($"Nest over [{curLoopLv}] times!!"); ;
            }

            // close right, generate products
            foreach (ProcessingNode pNode in sh.allProcesses)
            {
                sh.RegisterAllByproducts(pNode);
            }

            // loop left
            Recipes.Recipe.PIOItem input;
            decimal outSpeed, inSpeed, inSpeedNeeded;
            ProcessingNode cloneLeftPNode;

            foreach (ProcessingNode pNode in sh.allProcesses)
            {
                for (int i = 0, iv = pNode.recipe.inputs.Count; i < iv; ++i)
                {
                    if (pNode.GetNeighbourNodeBaseList(true, i).Count > 0)
                    {
                        continue;
                    }

                    input = pNode.recipe.inputs[i];
                    if (input.thing == null)
                    {
                        throw new NullReferenceException($"Input[{i}] in recipe[{pNode.recipe.name}] is null.");
                    }
                    if (input.quantity == null)
                    {
                        throw new NullReferenceException($"Quantity of input[{i}] in recipe[{pNode.recipe.name}] is null.");
                    }


                    // 当存在副产作为原料时，首先使用副产品；
                    inSpeedNeeded = 0;
                    inSpeed = pNode.GetPortSpeed_perSec(true, i);

                    if (sh.HaveSufficientProduct(input.thing, out outSpeed))
                    {
                        ResultHelper.LinkProductToInput(sh, pNode, input.thing);
                        if (outSpeed < inSpeed)
                        {
                            inSpeedNeeded = inSpeed - outSpeed;
                        }
                    }
                    else
                    {
                        inSpeedNeeded = inSpeed;
                    }
                    if (inSpeedNeeded < aboutZero)
                    {
                        continue;
                    }

                    // 检查当前是否存在已有原料，有则调用原料
                    if (sh.allSources.Find(a => a.DataId == input.thing.id) != null)
                    {
                        sh.LinkLeftSource(pNode, i);
                        continue;
                    }


                    // 如果副产品不够，使用新机组，最后使用新原料
                    bool needStepBack = false;
                    foreach (Recipes.Recipe r in GetRecipes(allowedRecipes, input.thing))
                    {

                        ProcessingNode? existPNode = sh.FindProcess(r);
                        if (existPNode != null)
                        {
                            // 找到需要的产出
                            PIOItem? output = existPNode.FindOutput(input.thing.id, out int outputIndex);
                            if (output == null || output.quantity == null)
                            {
                                throw new Exception("Imposible, it can't be null.");
                            }
                            sh.GetOutFlowSpeed(existPNode, outputIndex, out decimal eOutRate, out decimal eCurOutRate);


                            decimal availableOutRate = eOutRate - eCurOutRate;
                            if ((availableOutRate - inSpeedNeeded) < aboutZero)
                            {
                                // equal
                                if (IncreaseBaseMultiple_Loop(sh, existPNode, existPNode.baseQuantity))
                                {
                                    needStepBack = true;
                                    break;
                                }
                            }
                            else
                            {
                                decimal eNewOutRate = eCurOutRate + inSpeedNeeded;


                                // 加大已存在 工序的 生产速度
                                if (IncreaseBaseMultiple_Loop(sh, existPNode, existPNode.baseQuantity * (eNewOutRate / eOutRate)))
                                {
                                    needStepBack = true;
                                    break;
                                }
                            }

                            continue;
                        }


                        ResultHelper cloneSH = sh.Clone();
                        List<Guid> cloneSHEmptyPNodeIdList = new List<Guid>();
                        shList.Add(cloneSH);

                        //leftPNode = cloneSH.LinkLeftProcessing(SearchHelper.IndexOfThingList(ref sh.allProcesses, pNode), i, inRateNeeded, r);
                        //int idxOfProcess = SearchHelper.IndexOfThingList(ref cloneSH.allProcesses, pNode);
                        ProcessingNode cloneSHCurPNode = cloneSH.allProcesses[ResultHelper.IndexOfThingList(ref cloneSH.allProcesses, pNode)];
                        cloneLeftPNode = cloneSH.LinkLeftProcessing(cloneSHCurPNode, i, inSpeedNeeded, r);


                        //SearchLoop(curLoopLv + 1, ref shList, cloneSH, cloneLeftPNode);

                        //cloneSHCurPNode = cloneSH.allProcesses[SearchHelper.IndexOfThingList(ref cloneSH.allProcesses, pNode)];
                        //for (int j = i + 1, jv = cloneSHCurPNode.recipe.inputs.Count; j < jv; ++j)
                        //{
                        //    SearchLoop(curLoopLv, ref shList, cloneSH, cloneSHCurPNode, j);
                        //}

                        if (!SearchLoop(curLoopLv + 1, ref shList, cloneSH))
                        {
                            return false;
                        }
                    }
                    if (needStepBack)
                    {
                        --i;
                        continue;
                    }


                    //if (pNode.recipe.name.ToLower().Contains("mine copper"))
                    //{
                    //    ;
                    //}

                    sh.LinkLeftSource(pNode, i);
                    //if (i + 1 < iv)
                    //{
                    //    cloneSH = sh.Clone();
                    //    shList.Add(cloneSH);
                    //    ProcessingNode cloneSHCurPNode = cloneSH.allProcesses[SearchHelper.IndexOfThingList(ref cloneSH.allProcesses, pNode)];
                    //    SearchLoop(curLoopLv, ref shList, cloneSH, cloneSHCurPNode, i + 1);
                    //}
                }
            }
            return true;
        }
        /// <summary>
        /// 增加当前处理节点的速率； 新增产品全部加入副产品；前置节点关联加速；如果输入没有关联，则忽略；
        /// </summary>
        /// <param name="sh">处理连图</param>
        /// <param name="pNode">加速节点</param>
        /// <param name="newBaseMultiPle">新倍速</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool IncreaseBaseMultiple_Loop(ResultHelper sh, ProcessingNode pNode, decimal newBaseMultiPle)
        {
            if (pNode.baseQuantity >= newBaseMultiPle)
            {
                return false;
            }

            if (newBaseMultiPle < aboutZero)
            {
                ;
            }

            // 多余产品，列入副产品
            //decimal increasedBaseMultiple = newBaseMultiPle - pNode.baseMultiple;
            //decimal increasedOutputRate;
            PIOItem pioItem;
            List<ProcessingLink> prevLinks;
            decimal oriBaseMultiple = pNode.baseQuantity;
            pNode.baseQuantity = newBaseMultiPle;
            sh.RegisterAllByproducts(pNode);


            // 左侧工序以此全部加大速度；
            for (int i = 0, iv = pNode.recipe.inputs.Count; i < iv; ++i)
            {
                pioItem = pNode.recipe.inputs[i];
                if (pioItem.thing == null)
                {
                    throw new Exception("Thing is Null");
                }
                prevLinks = pNode.FindPrevLinks(pioItem.thing.id);
                if (prevLinks.Count == 0)
                {
                    // 还没有左侧输入，不管
                    continue;
                }

                // 左侧有连接，可能是 工序， 副产品，也可能是原料
                // 如果是原料，则直接增加对应连接和原料速率；
                // 如果是工序，则左循环；
                // 如果是副产品（继续连续循环，可能会溢出），跳过，不管；
                foreach (ProcessingLink l in prevLinks)
                {
                    if (l.nodePrev is ProcessingHead)
                    {
                        if (sh.allFinalProducts.Contains(l.nodePrev))
                        {
                            // 引入了副产品，暂时不知道改怎么处理呀，忽略；
                            ;
                        }
                        else if (sh.allSources.Contains(l.nodePrev))
                        {
                            // 引入了源，增加源；
                            sh.LinkLeftSource(pNode, i);
                            break;
                        }
                    }
                    else // is ProcessingNode
                    {
                        if (pioItem.quantity == null)
                        {
                            throw new Exception("Quantity is null.");
                        }
                        if (pNode.recipe.period == null)
                        {
                            throw new Exception("Period is null.");
                        }
                        sh.GetInFlowSpeed(pNode, i, out decimal inputRateMax, out decimal inputRateCurrent);
                        decimal incSpeed = inputRateMax - inputRateCurrent;

                        if (l.channel is null || l.channel.speed is null)
                        {
                            throw new Exception("Channel is null or has no speed.");
                        }
                        decimal oriSpeed = l.GetBaseSpeed();
                        l.baseQuantity += incSpeed / l.channel.speed.Value;
                        ProcessingNode leftPNode = (ProcessingNode)l.nodePrev;
                        IncreaseBaseMultiple_Loop(sh, leftPNode, leftPNode.baseQuantity * ((oriSpeed + incSpeed) / oriSpeed));

                        break;
                    }
                }
            }
            return true;
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
            public decimal baseQuantity = 1;
            public decimal calQuantity = 1;
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

            public List<ProcessingLink> FindNextLinks(Guid thingId)
            {
                return linksNext.FindAll(a => a.thing.id == thingId).ToList();
            }
            public List<ProcessingLink> FindPrevLinks(Guid thingId)
            {
                return linksPrev.FindAll(a => a.thing.id == thingId).ToList();
            }
        }
        public class ProcessingHead : ProcessingNodeBase
        {
            public UIElement? ui;
            public bool isSourceOrProduct;
            public Things.Thing thing;
            public ProcessingHead(bool isSourceOrProduct, Things.Thing thing, decimal baseQuantity)
            {
                DataId = thing.id;
                this.isSourceOrProduct = isSourceOrProduct;
                this.thing = thing;
                this.baseQuantity = baseQuantity;
                this.calQuantity = baseQuantity;
            }

            public ProcessingHead(ProcessingHead copyFrom)
            {
                DataId = copyFrom.thing.id;
                this.isSourceOrProduct = copyFrom.isSourceOrProduct;
                this.thing = copyFrom.thing;
                this.baseQuantity = copyFrom.baseQuantity;
                this.calQuantity = copyFrom.calQuantity;
            }

        }
        public class ProcessingNode : ProcessingNodeBase
        {
            public UIElement? ui;
            public Recipes.Recipe recipe;

            /// <summary>
            /// 最后产品生产工序层级为0，向前则递增；
            /// </summary>
            public int ProcessLevel { get; internal set; }

            public ProcessingNode(Recipes.Recipe recipe)
            {
                DataId = recipe.id;
                this.recipe = recipe;
            }
            public void CalNSetBaseMultiple(ProcessingHead oneProduct, decimal outputQuantity)
            {
                // 从输出找到对应产品
                Recipes.Recipe.PIOItem? output = recipe.outputs.Find(a => a.thing != null && a.thing.id == oneProduct.DataId);
                if (output == null)
                {
                    throw ResultHelper.Error_Recipe_Output_notFound(oneProduct.thing.name, recipe.name);
                }
                Recipes.Recipe.PIOItem? test = recipe.outputs.Find(
                    a => a.thing is not null && output.thing is not null && a.thing.id == output.thing.id);
                if (test == null)
                {
                    throw ResultHelper.Error_Recipe_Output_notFound(oneProduct.thing.name, recipe.name);
                }
                CalNSetBaseMultiple(ResultHelper.IndexOfThingList(ref recipe.outputs, test), outputQuantity);
            }
            public void CalNSetBaseMultiple(int outputIndex, decimal outputQuantity)
            {
                // 按产品输出速度，计算处理速度
                decimal newQuantity = outputQuantity / GetPortSpeed_perSec(false, outputIndex);
                baseQuantity = newQuantity;
            }
            public decimal GetPortSpeed_perSec(bool onInputOrOutput, int portIndex, decimal? newBaseQuantity = null)
            {
                Quantity? q;
                if (onInputOrOutput)
                {
                    q = recipe.inputs[portIndex].quantity;
                }
                else
                {
                    q = recipe.outputs[portIndex].quantity;
                }
                if (q is null || recipe.period is null)
                {
                    if (onInputOrOutput)
                    {
                        throw ResultHelper.Error_Recipe_Input_Quantity_isNull(portIndex, recipe.name);
                    }
                    else
                    {
                        throw ResultHelper.Error_Recipe_Output_Quantity_isNull(portIndex, recipe.name);
                    }
                }
                if (newBaseQuantity is null)
                {
                    return q.ValueCurrentInGeneral / recipe.period.Value * baseQuantity;
                }
                else
                {
                    return q.ValueCurrentInGeneral / recipe.period.Value * newBaseQuantity.Value;
                }
            }
            public decimal GetPortSpeed_perSec(bool onInputOrOutput, Recipes.Recipe.PIOItem? portPIO, decimal? newBaseQuantity = null)
            {
                if (portPIO is null || portPIO.thing is null)
                {
                    return 0;
                }
                int portIndex;
                if (onInputOrOutput)
                {
                    portIndex = IndexOfInput(portPIO.thing);
                }
                else
                {
                    portIndex = IndexOfOutput(portPIO.thing);
                }
                if (portIndex < 0)
                {
                    return 0;
                }
                return GetPortSpeed_perSec(onInputOrOutput, portIndex, newBaseQuantity);
            }
            public decimal GetPortSpeed_perSec(bool onInputOrOutput, ProcessingLink? link, decimal? newBaseQuantity = null)
            {
                if (link is null)
                {
                    return 0;
                }
                int portIndex = -1;
                if (onInputOrOutput)
                {
                    if (linksPrev.Contains(link))
                    {
                        portIndex = IndexOfInput(link.thing);
                    }
                }
                else
                {
                    if (linksNext.Contains(link))
                    {
                        portIndex = IndexOfOutput(link.thing);
                    }
                }
                if (portIndex >= 0)
                {
                    return GetPortSpeed_perSec(onInputOrOutput, portIndex, newBaseQuantity);
                }
                return 0;
            }
            public decimal? GetPortLinksSpeed_perSec(bool onInputOrOutput, int portIndex)
            {
                List<ProcessingLink> links;
                if (onInputOrOutput)
                {
                    links = FindPrevLinks(portIndex);
                }
                else
                {
                    links = FindNextLinks(portIndex);
                }
                if (links.Count == 0)
                {
                    return null;
                }
                decimal result = 0;
                foreach (ProcessingLink l in links)
                {
                    result += l.GetBaseSpeed();
                }
                return result;
            }
            public int IndexOfInput(Things.Thing inputThing)
            {
                Things.Thing? curInputThing;
                for (int i = 0, iv = recipe.inputs.Count; i < iv; ++i)
                {
                    curInputThing = recipe.inputs[i].thing;
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
                    curOutputThing = recipe.outputs[i].thing;
                    if (curOutputThing != null
                        && curOutputThing.id == outputThing.id)
                    {
                        return i;
                    }
                }
                return -1;
            }

            internal List<ProcessingNodeBase> GetNeighbourNodeBaseList(
                bool onInputOrOutput, int portIndex)
            {
                List<ProcessingNodeBase> result = new List<ProcessingNodeBase>();
                if (portIndex < 0)
                {
                    return result;
                }
                List<Recipes.Recipe.PIOItem> pioList;
                if (onInputOrOutput)
                {
                    pioList = recipe.inputs;
                }
                else
                {
                    pioList = recipe.outputs;
                }
                if (portIndex >= pioList.Count)
                {
                    return result;
                }
                Recipes.Recipe.PIOItem pio = pioList[portIndex];
                ProcessingNodeBase curNode;
                foreach (ProcessingLink l in linksPrev)
                {
                    if (l.thing == pio.thing && l.nodePrev is ProcessingNodeBase)
                    {
                        if (onInputOrOutput)
                        {
                            curNode = l.nodePrev;
                        }
                        else
                        {
                            curNode = l.nodeNext;
                        }
                        if (result.Contains(curNode) == false)
                        {
                            result.Add(curNode);
                        }
                    }
                }
                return result;
            }

            public PIOItem? FindOutput(Guid tId, out int outputIndex)
            {
                outputIndex = -1;
                PIOItem? item;
                for (int i = 0, iv = recipe.outputs.Count; i < iv; ++i)
                {
                    item = recipe.outputs[i];
                    if (item.thing != null
                        && item.thing.id == tId)
                    {
                        outputIndex = i;
                        return item;
                    }
                }
                return null;
            }
            public PIOItem? FindInput(Guid tId, out int inputIndex)
            {
                inputIndex = -1;
                PIOItem? item;
                for (int i = 0, iv = recipe.outputs.Count; i < iv; ++i)
                {
                    item = recipe.outputs[i];
                    if (item.thing != null
                        && item.thing.id == tId)
                    {
                        inputIndex = i;
                        return item;
                    }
                }
                return null;
            }


            public List<ProcessingLink> FindNextLinks(int portIndex)
            {
                List<ProcessingLink> result = new List<ProcessingLink>();
                if (portIndex < 0
                    || recipe.outputs.Count <= portIndex)
                {
                    return result;
                }
                Recipes.Recipe.PIOItem portItem = recipe.outputs[portIndex];
                if (portItem.thing is null)
                {
                    return result;
                }
                return FindNextLinks(portItem.thing.id);
            }
            public List<ProcessingLink> FindPrevLinks(int portIndex)
            {
                List<ProcessingLink> result = new List<ProcessingLink>();
                if (portIndex < 0
                    || recipe.inputs.Count <= portIndex)
                {
                    return result;
                }
                Recipes.Recipe.PIOItem portItem = recipe.inputs[portIndex];
                if (portItem.thing is null)
                {
                    return result;
                }
                return FindPrevLinks(portItem.thing.id);
            }
        }
        public class ProcessingLink
        {
            public UIElement? ui;
            public Channels.Channel channel;
            public Things.Thing thing;
            public decimal baseQuantity = 0;
            public decimal calQuantity = 0;
            public ProcessingNodeBase nodePrev;
            public ProcessingNodeBase nodeNext;
            public ProcessingLink(
                ProcessingNodeBase nodePrev, ProcessingNodeBase nodeNext,
                Things.Thing thing, Channels.Channel channel, decimal speed)
            {
                this.nodePrev = nodePrev;
                this.nodeNext = nodeNext;
                this.thing = thing;
                this.channel = channel;
                if (channel.speed is null)
                {
                    throw ProcessingChains.ResultHelper.Error_Channel_Speed_isNull(channel);
                }
                this.baseQuantity = speed / channel.speed.Value;
                this.calQuantity = this.baseQuantity;
            }


            internal decimal GetChannelSpeed()
            {
                if (channel is null)
                {
                    throw ResultHelper.Error_Channel_isNull();
                }
                if (channel.speed is null)
                {
                    throw ResultHelper.Error_Channel_Speed_isNull(channel);
                }
                return channel.speed.Value;
            }
            public decimal GetBaseSpeed()
            {
                return baseQuantity * GetChannelSpeed();
            }
            public decimal GetCalSpeed()
            {
                return calQuantity * GetChannelSpeed();
            }
        }
        public class ResultHelper
        {

            public ProcessingNode finalProcess;
            public ProcessingHead finalProduct;
            public List<ProcessingHead> allSources = new List<ProcessingHead>();
            public List<ProcessingHead> allFinalProducts = new List<ProcessingHead>();
            public List<ProcessingHead> sufficientProducts = new List<ProcessingHead>();
            public List<ProcessingNode> allProcesses = new List<ProcessingNode>();
            public List<ProcessingLink> allLinks = new List<ProcessingLink>();

            public HashSet<Channels.Channel> allUsedChannels = new HashSet<Channels.Channel>();
            public List<Channels.Channel> optimizedChannels = new List<Channels.Channel>();
            public List<Things.Thing> allPossibleThings = new List<Things.Thing>();

            public decimal newMultiple { private set; get; } = 1;
            public void ReCalSpeed(decimal newMultiple)
            {
                this.newMultiple = newMultiple;
                foreach (ProcessingHead p in allSources)
                {
                    p.calQuantity = newMultiple * p.baseQuantity;
                }
                foreach (ProcessingHead p in allFinalProducts)
                {
                    p.calQuantity = newMultiple * p.baseQuantity;
                }
                foreach (ProcessingNode p in allProcesses)
                {
                    p.calQuantity = newMultiple * p.baseQuantity;
                }
                foreach (ProcessingLink l in allLinks)
                {
                    l.calQuantity = newMultiple * l.baseQuantity;
                }
            }


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
                            tmpList = allProcesses[i].recipe?.accessories.Select(a => a.thing);
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
                    }
                    return _AllAccessories;
                }
            }
            private List<Things.Thing> _AllAccessories_noDuplicates = new List<Things.Thing>();
            public List<Things.Thing> AllAccessories_noDuplicates
            {
                get
                {
                    if (_AllAccessories_noDuplicates.Count == 0)
                    {
                        _AllAccessories_noDuplicates.AddRange(AllAccessories);
                        ResultHelper.EliminateDuplication<Things.Thing>(ref _AllAccessories_noDuplicates);
                    }
                    return _AllAccessories_noDuplicates;
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
                        ResultHelper.EliminateDuplication<Things.Thing>(ref _AllInputs);
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
                        ResultHelper.EliminateDuplication<Things.Thing>(ref _AllFinalOutputs);
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





            public ResultHelper() { }
            public ProcessingChains parent;
            public ResultHelper(ProcessingChains parent, Recipes.Recipe recipe, Things.Thing productThing)
            {
                this.parent = parent;
                finalProduct = new ProcessingHead(false, productThing, 1);
                allFinalProducts.Add(finalProduct);

                finalProcess = new ProcessingNode(recipe) { ProcessLevel = 0, };
                finalProcess.CalNSetBaseMultiple(finalProduct, 1);
                allProcesses.Add(finalProcess);

                Link(this, finalProcess, finalProduct,
                    parent.GetOptimizedChannel(finalProduct.DataId),
                    finalProduct.baseQuantity);

                RegisterAllByproducts(finalProcess);
            }

            internal void Closure()
            {
                // get all possible thing that go through channels
                AddToThings(allSources.Select(a => a.thing));
                AddToThings(allFinalProducts.Select(a => a.thing));
                foreach (ProcessingNode pNode in allProcesses)
                {
                    AddToThings(pNode.recipe.inputs.Select(a => a.thing));
                    AddToThings(pNode.recipe.outputs.Select(a => a.thing));
                }
                void AddToThings(IEnumerable<Things.Thing?> thingList)
                {
                    foreach (Things.Thing? t in thingList)
                    {
                        if (t is null)
                        {
                            continue;
                        }
                        allPossibleThings.Add(t);
                    }
                }


                int i, iv, j, jv;
                ProcessingNode pNode1, pNode2;
                // 将重复的 源 ，合并到一起
                MergeSameHeads(ref allSources);

                // 将重复的 产出，合并到一起
                MergeSameHeads(ref allFinalProducts);

                // 精简 情况 1
                // 输入 电力、煤炭，，，， 中间环节有发电；
                // 此时，应该加大中间环节的 发电， 然后自发电当作原料，省略输入的 电力；
                if (TrySimplify01())
                {
                    MergeSameHeads(ref allSources);
                    MergeSameHeads(ref allFinalProducts);
                }
                // register other statics 
                // get sufficient products
                RegisterSufficientProducts();
                // 有时，会出现产量为0的产品，如果有，则去掉；
                RemoveEmptyProductOutputs();


                // 2024 0802
                // processors，按id分组
                for (i = 0, iv = allProcesses.Count - 1, jv = iv + 1; i < iv; ++i)
                {
                    pNode1 = allProcesses[i];
                    for (j = i + 1; j < jv; ++j)
                    {
                        pNode2 = allProcesses[j];
                        if (pNode1.recipe.processor == pNode2.recipe.processor)
                        {
                            if (j == i + 1)
                            {
                                break;
                            }
                            allProcesses.Remove(pNode2);
                            allProcesses.Insert(i + 1, pNode2);
                            break;
                        }
                    }
                }

            }
            public static void MergeSameHeads(ref List<ProcessingHead> headList)
            {
                int i, iv, j, k, kv;
                ProcessingHead head1, head2;
                ProcessingLink link;
                for (i = 0, iv = headList.Count - 1; i < iv; ++i)
                {
                    head1 = headList[i];
                    for (j = i + 1; j <= iv; ++j)
                    {
                        head2 = headList[j];
                        if (head1.DataId == head2.DataId)
                        {
                            // 将head2 的 数量，叠加到 head1
                            head1.baseQuantity += head2.baseQuantity;
                            head1.calQuantity += head2.calQuantity;

                            // 将head2 的连接，转移到head1
                            for (k = 0, kv = head2.linksPrev.Count; k < kv; ++k)
                            {
                                link = head2.linksPrev[k];
                                head1.linksPrev.Add(link);
                                link.nodeNext = head1;
                            }
                            head2.linksPrev.Clear();
                            for (k = 0, kv = head2.linksNext.Count; k < kv; ++k)
                            {
                                link = head2.linksNext[k];
                                head1.linksNext.Add(link);
                                link.nodePrev = head1;
                            }
                            head2.linksNext.Clear();
                            headList.Remove(head2);
                            --i; --j; --iv;
                            break;
                        }
                    }
                }
            }
            public bool CheckInputsNOutputs()
            {
                foreach (ProcessingHead head in allFinalProducts)
                {
                    if (parent.allowedProductThings.Contains(head.thing) == false)
                    {
                        return false;
                    }
                }
                foreach (ProcessingHead head in allSources)
                {
                    if (parent.allowedSourceThings.Contains(head.thing) == false)
                    {
                        return false;
                    }
                }
                return true;
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
                Things.Thing? leftThing = pioItem.thing;
                if (leftThing == null)
                {
                    throw Error_Recipe_Input_Item_isNull(inputIdx, pNode.recipe.name);
                }
                if (pioItem.quantity == null)
                {
                    throw Error_Recipe_Input_Quantity_isNull(inputIdx, pNode.recipe.name);
                }

                decimal inputRateNeeded = GetNeededInFlowSpeed(pNode, inputIdx);
                if (Math.Abs(inputRateNeeded) < aboutZero)
                {
                    return;
                    //throw new Exception("No more band for this input");
                }

                ProcessingHead leftHead = new ProcessingHead(true, leftThing, inputRateNeeded);
                allSources.Add(leftHead);

                Channels.Channel channel = parent.GetOptimizedChannel(leftHead.thing.id);
                Link(this, leftHead, pNode, channel, inputRateNeeded);
            }

            public static void LinkLeftAllSources(ResultHelper sr)
            {
                int i, iv;
                decimal portSpeed, totalLinkSpeed, inputRateNeeded;
                PIOItem portItem;
                foreach (ProcessingNode pNode in sr.allProcesses)
                {
                    for (i = 0, iv = pNode.recipe.inputs.Count; i < iv; ++i)
                    {
                        portItem = pNode.recipe.inputs[i];
                        if (pNode.FindPrevLinks(i).Count != 0
                            || portItem.thing is null)
                        {
                            continue;
                        }

                        GetFlowSpeeds(pNode, true, i, out portSpeed, out totalLinkSpeed);
                        inputRateNeeded = portSpeed - totalLinkSpeed;
                        if (inputRateNeeded < aboutZero)
                        {
                            continue;
                        }
                        Channels.Channel? channel = GetOptimizedChannel(sr.allUsedChannels, portItem.thing.id);
                        if (channel is null)
                        {
                            continue;
                        }

                        ProcessingHead leftHead = new ProcessingHead(true, portItem.thing, inputRateNeeded);
                        sr.allSources.Add(leftHead);

                        Link(sr, leftHead, pNode, channel, inputRateNeeded);
                    }
                }
            }
            public static void LinkRightAllProducts(ResultHelper sr)
            {
                int i, iv;
                decimal portSpeed, totalLinkSpeed, outputRateNeeded;
                PIOItem portItem;
                foreach (ProcessingNode pNode in sr.allProcesses)
                {
                    for (i = 0, iv = pNode.recipe.outputs.Count; i < iv; ++i)
                    {
                        portItem = pNode.recipe.outputs[i];
                        if (pNode.FindNextLinks(i).Count != 0
                            || portItem.thing is null)
                        {
                            continue;
                        }

                        GetFlowSpeeds(pNode, false, i, out portSpeed, out totalLinkSpeed);
                        outputRateNeeded = portSpeed - totalLinkSpeed;
                        if (outputRateNeeded < aboutZero)
                        {
                            continue;
                        }
                        Channels.Channel? channel = GetOptimizedChannel(sr.allUsedChannels, portItem.thing.id);
                        if (channel is null)
                        {
                            continue;
                        }

                        ProcessingHead rightHead = new ProcessingHead(false, portItem.thing, outputRateNeeded);
                        sr.allFinalProducts.Add(rightHead);

                        Link(sr, pNode, rightHead, channel, outputRateNeeded);
                    }
                }
            }

            /// <summary>
            /// 给当前节点连接的某项输入连接处理节点，并返回这个节点
            /// </summary>
            /// <param name="rightPNode">要添加左处理的节点</param>
            /// <param name="rightInputIdx">左输入的索引号</param>
            /// <param name="leftRecipe">要连接的处理配方</param>
            /// <returns>新增的左处理节点</returns>
            internal ProcessingNode LinkLeftProcessing(
                ProcessingNode rightPNode, int inputIdx, decimal inputSpeed,
                Recipes.Recipe leftRecipe, bool autoRegesterByproducts = true)
            {
                Recipes.Recipe.PIOItem input = rightPNode.recipe.inputs[inputIdx];
                if (input.thing == null)
                {
                    throw Error_Recipe_Input_Item_isNull(inputIdx, rightPNode.recipe.name);
                }

                ProcessingNode leftPNode = new ProcessingNode(leftRecipe)
                { ProcessLevel = rightPNode.ProcessLevel + 1, };
                leftPNode.CalNSetBaseMultiple(leftPNode.IndexOfOutput(input.thing), inputSpeed);
                allProcesses.Add(leftPNode);

                Channels.Channel channel = parent.GetOptimizedChannel(input.thing.id);
                Link(this, leftPNode, rightPNode, input.thing, channel, inputSpeed);
                if (autoRegesterByproducts)
                {
                    RegisterAllByproducts(leftPNode);
                }

                return leftPNode;
            }
            internal ProcessingNode LinkLeftProcessing(
                int pNodeIndex, int inputIdx, decimal inputRate,
                Recipes.Recipe leftRecipe, bool autoRegesterByproducts = true)
            {
                return LinkLeftProcessing(
                    allProcesses[pNodeIndex], inputIdx, inputRate,
                    leftRecipe, autoRegesterByproducts);
            }

            public static ProcessingLink Link(ResultHelper sr,
                ProcessingHead leftHead, ProcessingNode rightNode,
                Channels.Channel channel, decimal speed)
            {
                // check free speed
                GetFlowSpeeds(leftHead, false, out decimal headSpeed, out decimal totalHeadLinkSpeed);
                decimal freeHeadSpeed = headSpeed - totalHeadLinkSpeed;
                if (freeHeadSpeed < -aboutZero)
                {
                    throw Error_PortSpeed_isNegative(freeHeadSpeed);
                }
                if (freeHeadSpeed + aboutZero < speed)
                {
                    throw Error_Not_Enough_FlowRate(freeHeadSpeed, speed);
                }
                int inputPortIndex = rightNode.IndexOfInput(leftHead.thing);
                GetFlowSpeeds(rightNode, true, inputPortIndex, out decimal portSpeed, out decimal totalInputLinkSpeed);
                decimal freePortSpeed = portSpeed - totalInputLinkSpeed;
                if (freePortSpeed < -aboutZero)
                {
                    throw Error_PortSpeed_isNegative(freePortSpeed);
                }
                if (freePortSpeed + aboutZero < speed)
                {
                    throw Error_Not_Enough_FlowRate(freePortSpeed, speed);
                }

                // link
                ProcessingLink link = new ProcessingLink(leftHead, rightNode, leftHead.thing, channel, speed);
                leftHead.linksNext.Add(link);
                rightNode.linksPrev.Add(link);
                sr.allLinks.Add(link);
                sr.allUsedChannels.Add(channel);
                return link;
            }
            public static ProcessingLink Link(ResultHelper sr,
                ProcessingNode leftNode, ProcessingHead rightHead,
                Channels.Channel channel, decimal speed)
            {
                // check free speed
                int outputPortIndex = leftNode.IndexOfOutput(rightHead.thing);
                GetFlowSpeeds(leftNode, false, outputPortIndex,
                    out decimal portSpeed, out decimal totalPortLinkSpeed);
                decimal freePortSpeed = portSpeed - totalPortLinkSpeed;
                if (freePortSpeed < -aboutZero)
                {
                    throw Error_PortSpeed_isNegative(freePortSpeed);
                }
                if (freePortSpeed + aboutZero < speed)
                {
                    throw Error_Not_Enough_FlowRate(freePortSpeed, speed);
                }
                GetFlowSpeeds(rightHead, true, out decimal headSpeed, out decimal totalHeadLinkSpeed);
                decimal freeHeadSpeed = headSpeed - totalHeadLinkSpeed;
                if (freeHeadSpeed < -aboutZero)
                {
                    throw Error_PortSpeed_isNegative(freeHeadSpeed);
                }
                if (freeHeadSpeed + aboutZero < speed)
                {
                    throw Error_Not_Enough_FlowRate(freeHeadSpeed, speed);
                }

                // link
                ProcessingLink link = new ProcessingLink(leftNode, rightHead, rightHead.thing, channel, speed);
                leftNode.linksNext.Add(link);
                rightHead.linksPrev.Add(link);
                sr.allLinks.Add(link);
                sr.allUsedChannels.Add(channel);
                return link;
            }
            public static ProcessingLink Link(ResultHelper sr, ProcessingNode leftNode, ProcessingNode rightNode,
                Things.Thing thing, Channels.Channel channel, decimal speed)
            {
                // check free speed
                int outputPortIndex = leftNode.IndexOfOutput(thing);
                GetFlowSpeeds(leftNode, false, outputPortIndex,
                    out decimal outputPortSpeed, out decimal totalOutputPortLinkSpeed);
                decimal freeOutputPortSpeed = outputPortSpeed - totalOutputPortLinkSpeed;
                if (freeOutputPortSpeed < -aboutZero)
                {
                    throw Error_PortSpeed_isNegative(freeOutputPortSpeed);
                }
                if (freeOutputPortSpeed + aboutZero < speed)
                {
                    throw Error_Not_Enough_FlowRate(freeOutputPortSpeed, speed);
                }
                int inputPortIndex = rightNode.IndexOfInput(thing);
                GetFlowSpeeds(rightNode, true, inputPortIndex,
                    out decimal inputPortSpeed, out decimal totalInputPortLinkSpeed);
                decimal freeInputPortSpeed = inputPortSpeed - totalInputPortLinkSpeed;
                if (freeInputPortSpeed < -aboutZero)
                {
                    throw Error_PortSpeed_isNegative(freeInputPortSpeed);
                }
                if (freeInputPortSpeed + aboutZero < speed)
                {
                    throw Error_Not_Enough_FlowRate(freeInputPortSpeed, speed);
                }
                // link
                ProcessingLink link = new ProcessingLink(leftNode, rightNode, thing, channel, speed);
                leftNode.linksNext.Add(link);
                rightNode.linksPrev.Add(link);
                sr.allLinks.Add(link);
                sr.allUsedChannels.Add(channel);
                return link;
            }

            private static int _IndexOfThingList(ref List<ProcessingNode> sourceList, Guid id)
            {
                ProcessingNode? curPN;
                for (int i = 0, iv = sourceList.Count; i < iv; ++i)
                {
                    curPN = sourceList[i];
                    if (curPN.DataId == id)
                    {
                        return i;
                    }
                }
                return -1;
            }
            private static int _IndexOfThingList(ref List<PIOItem> sourceList, Guid id)
            {
                Things.Thing? curT;
                for (int i = 0, iv = sourceList.Count; i < iv; ++i)
                {
                    curT = sourceList[i].thing;
                    if (curT != null && curT.id == id)
                    {
                        return i;
                    }
                }
                return -1;
            }
            public static int IndexOfThingList(ref List<ProcessingNode> sourceList, object? o)
            {
                if (o == null)
                {
                    return -1;
                }
                ProcessingNode? pn = null;
                if (o is ProcessingNode)
                {
                    pn = (ProcessingNode)o;
                }
                else
                {
                    throw new Exception("other situation.");
                }
                return _IndexOfThingList(ref sourceList, pn.DataId);
            }
            public static int IndexOfThingList(ref List<PIOItem> sourceList, object? o)
            {
                if (o == null)
                {
                    return -1;
                }
                Things.Thing? t = null;
                if (o is Recipes.Recipe.PIOItem)
                {
                    t = ((Recipes.Recipe.PIOItem)o).thing;
                }
                else
                {
                    throw new Exception("other situation.");
                }
                if (t == null)
                {
                    return -1;
                }
                return _IndexOfThingList(ref sourceList, t.id);
            }

            public int RegisterAllByproducts(ProcessingNode pNode)
            {
                int count = 0;
                for (int i = 0, iv = pNode.recipe.outputs.Count; i < iv; ++i)
                {
                    if (RegisterByproducts(pNode, i))
                    {
                        ++count;
                    }
                }
                return count;
            }
            public bool RegisterByproducts(ProcessingNode pNode, int outputIndex)
            {
                PIOItem outputPort = pNode.recipe.outputs[outputIndex];
                if (outputPort.thing is null)
                {
                    return false;
                }

                GetFlowSpeeds(pNode, false, outputIndex,
                    out decimal portSpeed, out decimal totalLinkSpeed);
                decimal freeSpeed = portSpeed - totalLinkSpeed;
                if (freeSpeed < 0 || freeSpeed < aboutZero)
                {
                    return false;
                }

                ProcessingHead outputHead = new ProcessingHead(false, outputPort.thing, freeSpeed);
                allFinalProducts.Add(outputHead);
                Link(this, pNode, outputHead,
                    parent.GetOptimizedChannel(outputPort.thing.id),
                    freeSpeed);

                return true;
            }

            public static bool LinkProductToInput(ResultHelper sr, ProcessingNode rightPNode, Things.Thing product)
            {
                // 首先计算此节点对此物品，需要多大输入流速                
                int inputIndex = rightPNode.IndexOfInput(product);
                Recipes.Recipe.PIOItem input = rightPNode.recipe.inputs[inputIndex];
                if (input.quantity == null)
                {
                    throw Error_Recipe_Input_Quantity_isNull(inputIndex, rightPNode.recipe.name);
                }
                decimal neededInputSpeed = sr.GetNeededInFlowSpeed(rightPNode, inputIndex);
                if (neededInputSpeed < aboutZero)
                {
                    return false;
                    //throw new Exception("No band for more input.");
                }

                // 按需提供流速，如果供大于求，则直接满足，如果求大于供，则全部供应；
                decimal curProdOutSpeedLeft;
                foreach (ProcessingHead p in sr.allFinalProducts)
                {
                    if (p.DataId != product.id)
                    {
                        continue;
                    }
                    curProdOutSpeedLeft = sr.GetAvailableOutFlowSpeed(p);
                    if (curProdOutSpeedLeft < aboutZero)
                    {
                        continue;
                    }
                    if (neededInputSpeed <= curProdOutSpeedLeft)
                    {
                        // 完全满足需求
                        Link(sr, p, rightPNode,
                            sr.parent.GetOptimizedChannel(product.id),
                            neededInputSpeed);
                        break;
                    }
                    else
                    {
                        // 部分满足，需要下一循环（最后也可能会无法完全满足）
                        Link(sr, p, rightPNode,
                            sr.parent.GetOptimizedChannel(product.id),
                            curProdOutSpeedLeft);
                        neededInputSpeed -= curProdOutSpeedLeft;
                    }
                }
                return true;
            }
            public static bool ReduceInputFromProduct(ResultHelper rh, Things.Thing product)
            {
                ProcessingHead? sourceHead = rh.allSources.Find(a => a.thing == product);
                if (sourceHead is null)
                {
                    return false;
                }
                ProcessingHead? productHead = rh.allFinalProducts.Find(a => a.thing == product);
                if (productHead is null)
                {
                    return false;
                }
                GetFlowSpeeds(productHead, false, out decimal productSpeed, out decimal totalProductSpeed);
                decimal availableProductSpeed = productSpeed - totalProductSpeed;
                if (availableProductSpeed < aboutZero)
                {
                    return false;
                }

                ProcessingLink link;
                decimal linkSpeed;
                bool result = false;
                for (int i = sourceHead.linksNext.Count - 1; i >= 0; --i)
                {
                    link = sourceHead.linksNext[i];
                    linkSpeed = link.GetBaseSpeed();
                    if (linkSpeed <= availableProductSpeed + aboutZero)
                    {
                        sourceHead.linksNext.Remove(link);
                        sourceHead.baseQuantity -= linkSpeed;
                        sourceHead.calQuantity = sourceHead.baseQuantity;

                        link.nodePrev = productHead;
                        productHead.linksNext.Add(link);
                        availableProductSpeed -= linkSpeed;

                        if (sourceHead.linksNext.Count == 0)
                        {
                            rh.allSources.Remove(sourceHead);
                        }
                        result = true;
                    }
                }
                return result;
            }
            public static int ReduceAllInputsFromProducts(ResultHelper rh)
            {
                int result = 0;
                for (int i = 0, iv = rh.allSources.Count; i < iv; ++i)
                {
                    if (ReduceInputFromProduct(rh, rh.allSources[i].thing))
                    {
                        result++;
                        i--;
                        iv = rh.allSources.Count;
                    }
                }
                return result;
            }


            /// <summary>
            /// 最后执行，用于记录所有可输出物品
            /// </summary>
            internal void RegisterSufficientProducts()
            {
                ProcessingHead prod;
                decimal availableOutRate;
                for (int i = 0, iv = allFinalProducts.Count; i < iv; ++i)
                {
                    prod = allFinalProducts[i];
                    availableOutRate = GetAvailableOutFlowSpeed(prod);
                    if (availableOutRate > aboutZero)
                    {
                        sufficientProducts.Add(prod);
                    }
                }
            }
            internal void RemoveEmptyProductOutputs()
            {
                ProcessingHead prod;
                ProcessingNode pNode;
                for (int i = allFinalProducts.Count - 1; i >= 0; --i)
                {
                    prod = allFinalProducts[i];
                    if (prod.linksNext.Count > 0)
                    {
                        continue;
                    }

                    if (Math.Abs(prod.baseQuantity) < aboutZero)
                    {
                        foreach (ProcessingLink leftLink in prod.linksPrev)
                        {
                            if (leftLink.nodePrev is not ProcessingNode)
                            {
                                continue;
                            }
                            pNode = (ProcessingNode)leftLink.nodePrev;
                            pNode.linksNext.Remove(leftLink);
                            allLinks.Remove(leftLink);
                        }
                        prod.linksPrev.Clear();
                        allFinalProducts.Remove(prod);
                    }
                }
            }


            #endregion


            #region checks, calculates

            public ProcessingNode? FindProcess(Recipes.Recipe r)
            {
                ProcessingNode? testNode = allProcesses.Find(a => a.DataId == r.id);
                return testNode;
            }
            public List<ProcessingNode> FindAllProcesses_withOutput(Guid outputThingId)
            {
                List<ProcessingNode> result = new List<ProcessingNode>();
                foreach (ProcessingNode pn in allProcesses)
                {
                    if (pn.recipe.outputs.Find(a => a.thing != null && a.thing.id == outputThingId) != null)
                    {
                        result.Add(pn);
                    }
                }
                return result;
            }
            internal bool HaveSufficientProduct(Things.Thing item, out decimal outSpeed)
            {
                outSpeed = 0;
                List<ProcessingLink> outLinkList;
                foreach (ProcessingHead h in allFinalProducts)
                {
                    if (h.thing.id != item.id)
                    {
                        continue;
                    }
                    outSpeed += h.baseQuantity;

                    outLinkList = allLinks.FindAll(a => a.nodePrev == h);
                    foreach (ProcessingLink outLink in outLinkList)
                    {
                        if (outLink.channel is null
                            || outLink.channel.speed is null)
                        {
                            continue;
                        }
                        outSpeed -= outLink.baseQuantity * outLink.channel.speed.Value;
                    }
                }
                return outSpeed > aboutZero;
            }

            #endregion


            #region get flow speeds

            internal void GetInFlowSpeed(ProcessingNode pNode, int inputIndex,
                out decimal inputSpeedMax, out decimal inputSpeedCurrent)
            {
                inputSpeedCurrent = 0;
                Recipes.Recipe.PIOItem pioItem = pNode.recipe.inputs[inputIndex];
                inputSpeedMax = pNode.GetPortSpeed_perSec(true, inputIndex);

                List<ProcessingLink> inlinkList = allLinks.FindAll(
                    a => a.nodeNext == pNode && pioItem.thing is not null
                        && a.thing.id == pioItem.thing.id);
                foreach (ProcessingLink inLink in inlinkList)
                {
                    inputSpeedCurrent += inLink.GetBaseSpeed();
                }
            }
            public static void GetFlowSpeeds(ProcessingNode pNode,
                bool inOrOut, int portIndex,
                out decimal portSpeed, out decimal totalLinkSpeed)
            {
                portSpeed = pNode.GetPortSpeed_perSec(inOrOut, portIndex);
                List<ProcessingLink> links;
                if (inOrOut)
                {
                    links = pNode.FindPrevLinks(portIndex);
                }
                else
                {
                    links = pNode.FindNextLinks(portIndex);
                }
                totalLinkSpeed = 0;
                foreach (ProcessingLink l in links)
                {
                    totalLinkSpeed += l.GetBaseSpeed();
                }
            }
            public static void GetFlowSpeeds(ProcessingHead head,
                bool inOrOut,
                out decimal headSpeed, out decimal totalLinkSpeed)
            {
                headSpeed = head.baseQuantity;
                List<ProcessingLink> links;
                if (inOrOut)
                {
                    links = head.linksPrev;
                }
                else
                {
                    links = head.linksNext;
                }
                totalLinkSpeed = 0;
                foreach (ProcessingLink l in links)
                {
                    totalLinkSpeed += l.GetBaseSpeed();
                }
            }

            internal decimal GetNeededInFlowSpeed(ProcessingNode pNode, Things.Thing inputThing)
            {
                int inputIndex = pNode.IndexOfInput(inputThing);
                return GetNeededInFlowSpeed(pNode, inputIndex);
            }
            internal decimal GetNeededInFlowSpeed(ProcessingNode pNode, int inputIndex)
            {
                GetInFlowSpeed(pNode, inputIndex, out decimal inputSpeedMax, out decimal inputSpeedCurrent);
                return inputSpeedMax - inputSpeedCurrent;
            }


            internal void GetOutFlowRate(ProcessingHead leftHead, out decimal outputRateMax, out decimal outputRateCurrent)
            {
                outputRateMax = leftHead.baseQuantity;
                outputRateCurrent = 0;
                foreach (ProcessingLink outLink in allLinks.FindAll(a => a.nodePrev == leftHead))
                {
                    outputRateCurrent += outLink.GetBaseSpeed();
                }
            }
            /// <summary>
            /// 获取输出速率
            /// </summary>
            /// <param name="leftPNode">做输出的节点</param>
            /// <param name="outputIndex">输出项索引号</param>
            /// <param name="outputSpeedMax">最大输出速度</param>
            /// <param name="outputSpeedCurrent">当前输出速度，基于外连接速度合计</param>
            internal void GetOutFlowSpeed(ProcessingNode leftPNode, int outputIndex, out decimal outputSpeedMax, out decimal outputSpeedCurrent)
            {
                Recipes.Recipe.PIOItem output = leftPNode.recipe.outputs[outputIndex];
                outputSpeedMax = leftPNode.GetPortSpeed_perSec(false, outputIndex);
                outputSpeedCurrent = 0;
                foreach (ProcessingLink outLink in allLinks.FindAll(a => a.nodePrev == leftPNode && a.thing == output.thing))
                {
                    outputSpeedCurrent += outLink.GetBaseSpeed();
                }
            }
            internal decimal GetAvailableOutFlowSpeed(ProcessingHead leftHead)
            {
                GetOutFlowRate(leftHead, out decimal outputRate, out decimal outputRateCurrent);
                return outputRate - outputRateCurrent;
            }
            internal decimal GetAvailableOutFlowSpeed(ProcessingNode leftNode, Things.Thing outputThing)
            {
                int outputIndex = leftNode.IndexOfOutput(outputThing);
                return GetAvailableOutFlowSpeed(leftNode, outputIndex);
            }
            internal decimal GetAvailableOutFlowSpeed(ProcessingNode leftNode, int outputIndex)
            {
                GetOutFlowSpeed(leftNode, outputIndex, out decimal outputRate, out decimal outputRateCurrent);
                return outputRate - outputRateCurrent;
            }

            #endregion


            #region exceptions
            public static Exception Error_Recipe_Processer_isNull(string? recipeName)
            {
                return new NullReferenceException($"Processor of recipe[{recipeName}] is null.");
            }
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

            public static Exception Error_Not_Enough_FlowRate(decimal outRate, decimal needRate)
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
            public static Exception Error_PortSpeed_isNegative(decimal nSpeed)
            {
                return new NullReferenceException($"Speed[{nSpeed}] is negative.");
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

            public bool TrySimplify01(int? deepth = null)
            {
                if (deepth is null)
                {
                    deepth = 0;
                }
                bool simplified = false;
                ProcessingHead s;
                for (int i = 0, iv = allSources.Count; i < iv; ++i)
                {
                    s = allSources[i];
                    foreach (ProcessingNode curPN in FindAllProcesses_withOutput(s.thing.id))
                    {
                        // 如果中间环节产出的物品速率，比原料提供速率低，那么放弃，因为不可能满足需求；
                        // 只考虑当前处理节点之前的原料，其他分支的不计算；
                        GetOutFlowSpeed(curPN, curPN.IndexOfOutput(s.thing), out decimal outputRateMax, out decimal outputRateCurrent);
                        decimal sourceOutRate_inBranch = _GetSourceOutRate_inBranch(curPN, s.thing.id);
                        if (sourceOutRate_inBranch >= outputRateMax)
                        {
                            continue;
                        }

                        // 合计所有原料提供速率
                        decimal sourceOutRate_all = 0;
                        foreach (ProcessingHead s1 in allSources)
                        {
                            if (s1.thing.id == s.thing.id)
                            {
                                sourceOutRate_all += s.baseQuantity;
                            }
                        }

                        // 计算要简化原料所需提供的新速率
                        // 新速度
                        //decimal newOutputRate = (sourceOutRate_all - sourceOutRate_inBranch + outputRateMax) * outputRateMax
                        //                        / (outputRateMax - sourceOutRate_inBranch);
                        decimal newOutputRate_multiple = (sourceOutRate_all - sourceOutRate_inBranch + outputRateMax)
                                                / (outputRateMax - sourceOutRate_inBranch);
                        if (Math.Abs(newOutputRate_multiple - 1) < (decimal)0.000001)
                        {
                            continue;
                        }


                        // 记住需要更换原料，为副产品 的 节点；
                        ProcessingNode sPNode;
                        List<ProcessingNode> simpledPNodeList = new List<ProcessingNode>();
                        foreach (ProcessingLink l in s.linksNext)
                        {
                            sPNode = (ProcessingNode)l.nodeNext;
                            simpledPNodeList.Add(sPNode);
                            // 取消连接；
                            sPNode.linksPrev.Remove(l);
                            allLinks.Remove(l);
                        }
                        s.linksNext.Clear();
                        // 消除原料节点
                        allSources.Remove(s);
                        --i;
                        --iv;


                        // 增加当前节点的产能， 以及前置

                        IncreaseBaseMultiple_Loop(this, curPN, curPN.baseQuantity * newOutputRate_multiple);

                        // 将之前需要原料的节点，连接副产品为输入
                        foreach (ProcessingNode pn in simpledPNodeList)
                        {
                            if (LinkProductToInput(this, pn, s.thing))
                            {
                                simplified = true;
                            }
                        }
                        break;
                    }
                }
                if (deepth >= 10)
                {
                    throw new Exception("Simplify in dead loop.");
                }
                if (simplified)
                {
                    TrySimplify01(++deepth);
                }
                return simplified;
            }
            private decimal _GetSourceOutRate_inBranch(ProcessingNode curPN, Guid thingId)
            {
                decimal result = 0;
                foreach (ProcessingLink l in curPN.linksPrev)
                {
                    if (l.thing.id != thingId)
                    {
                        continue;
                    }

                    if (l.nodePrev is ProcessingHead)
                    {
                        if (allSources.Contains(l.nodePrev))
                        {
                            result += l.GetBaseSpeed();
                        }
                    }
                    else if (l.nodePrev is ProcessingNode)
                    {
                        result += _GetSourceOutRate_inBranch((ProcessingNode)l.nodePrev, thingId);
                    }
                }
                return result;
            }

            public static void GetRelatedIdList(Guid productId,
                out List<Guid> recipeIdList, out List<Guid> accessoryIdList,
                out List<Guid> inputIdList, out List<Guid> outputIdList)
            {
                recipeIdList = new List<Guid>();
                accessoryIdList = new List<Guid>();
                inputIdList = new List<Guid>();
                outputIdList = new List<Guid>();

                outputIdList.Add(productId);

                foreach (Recipes.Recipe r in ProcessingChains.GetRecipes(Core.Instance.FindThing(productId)))
                {
                    _GetRelatedIdList_loop(r, ref recipeIdList, ref accessoryIdList, ref inputIdList, ref outputIdList);
                }

                // 2024 0801
                // 处理配方中，按处理器id分组；
                Guid id1, id2;
                Recipes.Recipe r1, r2;
                for (int i = 0, iv = recipeIdList.Count - 1, j, jv = iv + 1; i < iv; ++i)
                {
                    id1 = recipeIdList[i];
                    r1 = Core.Instance.FindRecipe(id1);
                    for (j = i + 1; j < jv; ++j)
                    {
                        id2 = recipeIdList[j];
                        r2 = Core.Instance.FindRecipe(id2);
                        if (r1.processor == r2.processor)
                        {
                            if (j == i + 1)
                            {
                                break;
                            }
                            recipeIdList.RemoveAt(j);
                            recipeIdList.Insert(i + 1, id2);
                            break;
                        }
                    }
                }
            }
            private static void _GetRelatedIdList_loop(Recipes.Recipe r,
                ref List<Guid> recipeIdList, ref List<Guid> accessoryIdList,
                ref List<Guid> inputIdList, ref List<Guid> outputIdList)
            {
                if (r.processor == null)
                {
                    return;
                }
                Guid testId = (Guid)r.id;
                if (!recipeIdList.Contains(testId))
                {
                    recipeIdList.Add(testId);
                }
                foreach (PIOItem i in r.accessories)
                {
                    if (i.thing == null)
                    {
                        continue;
                    }
                    testId = i.thing.id;
                    if (!accessoryIdList.Contains(testId))
                    {
                        accessoryIdList.Add(testId);
                    }
                }
                foreach (PIOItem i in r.outputs)
                {
                    if (i.thing == null)
                    {
                        continue;
                    }
                    testId = i.thing.id;
                    if (!outputIdList.Contains(testId))
                    {
                        outputIdList.Add(testId);
                    }
                }
                foreach (PIOItem i in r.inputs)
                {
                    if (i.thing == null)
                    {
                        continue;
                    }
                    testId = i.thing.id;
                    if (!inputIdList.Contains(testId))
                    {
                        inputIdList.Add(testId);
                    }

                    foreach (Recipes.Recipe ir in GetRecipes(i.thing))
                    {
                        if (ir.processor == null)
                        {
                            continue;
                        }
                        if (recipeIdList.Contains((Guid)ir.id))
                        {
                            continue;
                        }
                        _GetRelatedIdList_loop(ir,
                            ref recipeIdList, ref accessoryIdList,
                            ref inputIdList, ref outputIdList);
                    }
                }
            }

            #endregion



            public ResultHelper Clone()
            {
                ResultHelper clone = new ResultHelper(parent, finalProcess.recipe, finalProduct.thing);
                _Clone(ref clone, clone.finalProcess, this, this.finalProcess);

                return clone;
            }
            private void _Clone(ref ResultHelper cloneHelper, ProcessingNode cloneNode, ResultHelper sourceHelper, ProcessingNode sourceNode)
            {
                ProcessingLink referLink;
                int i, iv;

                // right to product
                RegisterAllByproducts(cloneNode);

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

                        newLeftPNode = cloneHelper.LinkLeftProcessing(
                            cloneNode,
                            inputIndex,
                            referLink.GetBaseSpeed(),
                            referLeftPNode.recipe);
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
                            LinkProductToInput(cloneHelper, cloneNode, referLink.thing);
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
