using MadTomDev.App.VMs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static MadTomDev.App.Classes.Things;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace MadTomDev.App.Classes
{
    public class Recipes : ThingsBase
    {
        //private static string FLAG_PARENTID = "ParentID"; // 0 null
        //private static string FLAG_ID = "ID";
        //private static string FLAG_NAME = "Name"; // 2 null
        //private static string FLAG_DESCRIPTION = "Description"; // 3 null
        //private static string FLAG_ISEXCLUDED = "IsExcluded"; // 4 null
        private static string FLAG_PROCESSOR = "Processor";
        private static string FLAG_ACCESSORIES = "Accessories"; // 6 null
        private static string FLAG_PERIOD = "Period";
        private static string FLAG_INPUTS = "Inputs"; // 8 null
        private static string FLAG_OUTPUTS = "Outputs"; // 9 null

        public List<Recipe> list = new List<Recipe>();
        public Recipes(TreeViewNodeModelScene scene) : base(scene)
        {
            Scene = scene;
            _csvFile = Path.Combine(scene.GetDirFullName(), SceneMgr.FILENAME_RECIPES);
            Reload();
        }
        public void Reload()
        {
            list.Clear();
            IsInherited = false;
            string[] row;
            if (File.Exists(_csvFile))
            {
                using (Common.CSVHelper.Reader reader = new Common.CSVHelper.Reader(_csvFile))
                {
                    bool titleFound = false;
                    Recipe? testRecipe = null;
                    while (!reader.IsEoF)
                    {
                        row = reader.ReadRow();
                        if (titleFound)
                        {
                            if (CheckIsMainDataRow(row))
                            {
                                // main data
                                testRecipe = GetMain(row);
                                if (testRecipe != null)
                                {
                                    list.Add(testRecipe);
                                }
                            }
                            else
                            {
                                // sub data
                                if (testRecipe != null)
                                {
                                    AddSubList(testRecipe, row);
                                }
                            }
                        }
                        else
                        {
                            if (CheckIsTitleRow(row))
                            {
                                titleFound = true;
                            }
                        }
                    }
                }
            }

            bool CheckIsTitleRow(string[] row)
            {
                // ParentID  ID  Name  Type  IsExcluded  Unit  Description
                if (row.Length >= 7)
                {
                    if (row[0] == FLAG_PARENTID
                        && row[1] == FLAG_ID
                        && row[2] == FLAG_NAME
                        && row[3] == FLAG_DESCRIPTION
                        && row[4] == FLAG_ISEXCLUDED
                        && row[5] == FLAG_PROCESSOR
                        && row[6] == FLAG_ACCESSORIES
                        && row[7] == FLAG_PERIOD
                        && row[8] == FLAG_INPUTS
                        && row[9] == FLAG_OUTPUTS)
                    {
                        return true;
                    }
                }
                return false;
            }
            bool CheckIsMainDataRow(string[] row)
            {
                if (row.Length >= 8)
                {
                    if (row[0].Length == 0
                        || row[1].Length == 0
                        || row[2].Length == 0)
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
            Recipe? GetMain(string[] row)
            {
                Recipe result = new Recipe(this);
                Guid testID;
                bool testB;
                decimal testD;
                if (row[0] != SceneMgr.VALUE_NULL)
                {
                    if (Guid.TryParse(row[0], out testID))
                    {
                        result.parentID = testID;
                    }
                }
                if (!Guid.TryParse(row[1], out testID))
                {
                    return null;
                }
                result.id = testID;
                if (string.IsNullOrWhiteSpace(row[2]))
                {
                    return null;
                }
                if (row[2] != SceneMgr.VALUE_NULL)
                {
                    result.name = row[2];
                }
                if (row[3] != SceneMgr.VALUE_NULL)
                {
                    result.description = row[3];
                }
                if (row[4] != SceneMgr.VALUE_NULL)
                {
                    if (bool.TryParse(row[4], out testB))
                    {
                        result.isExcluded = testB;
                    }
                }
                if (row[5] != SceneMgr.VALUE_NULL)
                {
                    if (Guid.TryParse(row[5], out testID))
                    {
                        result.processor = testID;
                    }
                }
                if (row[7] != SceneMgr.VALUE_NULL)
                {
                    if (decimal.TryParse(row[7], out testD))
                    {
                        result.period = testD;
                    }
                }
                return result;
            }
            void AddSubList(Recipe r, string[] row)
            {
                if (row.Length >= 10)
                {
                    if (!string.IsNullOrWhiteSpace(row[6]))
                    {
                        r.accessories.Add(new Recipe.PIOItem(row[6]));
                    }
                    if (!string.IsNullOrWhiteSpace(row[8]))
                    {
                        r.inputs.Add(new Recipe.PIOItem(row[8]));
                    }
                    if (!string.IsNullOrWhiteSpace(row[9]))
                    {
                        r.outputs.Add(new Recipe.PIOItem(row[9]));
                    }
                }
            }
        }
        public void Save()
        {
            if (IsInherited)
            {
                throw new InvalidOperationException("Inherited Recipes could not be saved.");
            }
            using (Common.CSVHelper.Writer writer = new Common.CSVHelper.Writer(_csvFile))
            {
                // write field names
                string[] row = new string[]
                    {
                        FLAG_PARENTID,
                        FLAG_ID,
                        FLAG_NAME,
                        FLAG_DESCRIPTION,
                        FLAG_ISEXCLUDED,
                        FLAG_PROCESSOR,
                        FLAG_ACCESSORIES,
                        FLAG_PERIOD,
                        FLAG_INPUTS,
                        FLAG_OUTPUTS,
                    };
                writer.WriteRow(row);

                // write contents
                int i, j, k, iv, jv, kv;
                foreach (Recipe r in list)
                {
                    // basic info
                    row[0] = r.parentID == null ? SceneMgr.VALUE_NULL : ((Guid)r.parentID).ToString();
                    row[1] = r.id.ToString();
                    row[2] = r.name == null ? SceneMgr.VALUE_NULL : r.name;
                    row[3] = r.description == null ? SceneMgr.VALUE_NULL : r.description;
                    row[4] = r.isExcluded == null ? SceneMgr.VALUE_NULL : ((bool)r.isExcluded).ToString();
                    row[5] = r.processor == null ? SceneMgr.VALUE_NULL : ((Guid)r.processor).ToString();
                    row[6] = "";
                    row[7] = r.period == null ? SceneMgr.VALUE_NULL : ((double)r.period).ToString();
                    row[8] = row[9] = "";
                    writer.WriteRow(row);

                    // sub items
                    row[0] = row[1] = row[2] = row[3] = row[4] = row[5] = row[7] = "";
                    for (i = 0, j = 0, k = 0, iv = r.accessories.Count, jv = r.inputs.Count, kv = r.outputs.Count;
                        i < iv || j < jv || k < kv; ++i, ++j, ++k)
                    {
                        // accessories
                        if (i < iv)
                        {
                            row[6] = r.accessories[i].IOContent;
                        }
                        else
                        {
                            row[6] = "";
                        }
                        // inputs
                        if (j < jv)
                        {
                            row[8] = r.inputs[j].IOContent;
                        }
                        else
                        {
                            row[8] = "";
                        }
                        // outputs
                        if (k < kv)
                        {
                            row[9] = r.outputs[k].IOContent;
                        }
                        else
                        {
                            row[9] = "";
                        }
                        writer.WriteRow(row);
                    }
                }
                writer.Flush();
            }
        }

        public void Inherit(Recipes parent)
        {
            Recipe? pr;
            List<Recipe> tmpList = new List<Recipe>();
            tmpList.AddRange(parent.list);
            foreach (Recipe r in list)
            {
                if (r.parentID != null)
                {
                    pr = tmpList.Find(a => a.id == r.parentID);
                    if (pr != null)
                    {
                        r.Inherit(pr);
                        tmpList.Remove(pr);
                    }
                }
            }
            list.AddRange(tmpList);

            IsInherited = true;
        }

        internal static void Simplify(ref Recipe child, Recipe parent)
        {
            if (child.parentID != parent.id)
            {
                throw new ArgumentException($"Recipe[{child.name}] is not the child of [{parent.name}].");
            }
            if (child.name == parent.name)
            {
                child.name = null;
            }
            if (child.description == parent.description)
            {
                child.description = null;
            }
            if (child.isExcluded == parent.isExcluded)
            {
                child.isExcluded = null;
            }
            if (child.processor == parent.processor)
            {
                child.processor = null;
            }
            if (child.period == parent.period)
            {
                child.period = null;
            }
            Simplify_List(ref child.accessories, parent.accessories);
            Simplify_List(ref child.inputs, parent.inputs);
            Simplify_List(ref child.outputs, parent.outputs);
        }
        private static void Simplify_List(ref List<Recipe.PIOItem> childList, List<Recipe.PIOItem> parentList)
        {
            Recipe.PIOItem c;
            List<Recipe.PIOItem> copiedPList = Recipe.CopyList(parentList);
            Recipe.PIOItem? p;
            for (int i = childList.Count - 1; i >= 0; --i)
            {
                c = childList[i];
                if (c.thing == null)
                {
                    continue;
                }
                p = copiedPList.Find(a => a.thing != null && a.thing.id == c.thing.id);
                if (p == null)
                {
                    continue;
                }

                // found same item
                if (p.CheckValueEqual(c))
                {
                    copiedPList.Remove(p);
                    childList.RemoveAt(i);
                }
            }
            // exclude not exists items
            foreach (Recipe.PIOItem ip in copiedPList)
            {
                c = ip.Clone();
                c.quantity?.Clear();
                c.isExcluded = true;
                childList.Add(c);
            }
        }

        public class Recipe : ThingBase
        {
            public Recipe(Recipes parent, bool genNewId = false) : base(parent)
            {
                if (genNewId)
                {
                    id = Guid.NewGuid();
                }
            }
            internal void DataFrom(Recipe newData)
            {
                this.name = newData.name;
                this.description = newData.description;
                this.isExcluded = newData.isExcluded;
                this.processor = newData.processor;
                this.accessories.Clear();
                this.accessories.AddRange(newData.accessories);
                this.period = newData.period;
                this.inputs.Clear();
                this.inputs.AddRange(newData.inputs);
                this.outputs.Clear();
                this.outputs.AddRange(newData.outputs);
            }

            public Guid? processor = null;
            public List<PIOItem> accessories = new List<PIOItem>();
            public decimal? period = 0;
            public List<PIOItem> inputs = new List<PIOItem>();
            public List<PIOItem> outputs = new List<PIOItem>();

            public void Inherit(Recipe parent)
            {
                ((ThingBase)this).Inherit(parent);
                if (parent == null)
                {
                    return;
                }
                if (parentID != parent.id)
                {
                    throw new InvalidOperationException($"Recipt:Parent[{parent.id}] is a false for child[{parentID}]");
                }
                if (name == null && parent.name != null)
                {
                    name = parent.name;
                }
                if (description == null && parent.description != null)
                {
                    description = parent.description;
                }
                if (isExcluded == null && parent.isExcluded != null)
                {
                    isExcluded = parent.isExcluded;
                }
                if (processor == null && parent.processor != null)
                {
                    processor = parent.processor;
                }
                if (period == null && parent.period != null)
                {
                    period = parent.period;
                }

                Inherite(ref accessories, parent.accessories);
                Inherite(ref inputs, parent.inputs);
                Inherite(ref outputs, parent.outputs);

            }
            public static void Inherite(ref List<PIOItem> childList, List<PIOItem> parentList)
            {
                List<PIOItem> copyList = CopyList(parentList);
                PIOItem c;
                PIOItem? p;
                for (int i = 0, iv = childList.Count; i < iv; ++i)
                {
                    c = childList[i];
                    p = copyList.Find(a => a.thing?.id == c.thing?.id);
                    if (p == null)
                    {
                        continue;
                    }
                    copyList.Remove(p);
                    if (c.isExcluded)
                    {
                        childList.RemoveAt(i);
                        --i; --iv;
                    }
                }
                childList.AddRange(copyList);
            }
            public static List<PIOItem> CopyList(List<PIOItem> oriList)
            {
                List<PIOItem> result = new List<PIOItem>();
                foreach (PIOItem p in oriList)
                {
                    result.Add(p.Clone());
                }
                return result;
            }

            internal bool CheckIsEmpty()
            {
                if (name != null)
                {
                    return false;
                }
                if (description != null)
                {
                    return false;
                }
                if (processor != null)
                {
                    return false;
                }
                if (period != null)
                {
                    return false;
                }
                if (isExcluded != null)
                {
                    return false;
                }
                if (accessories != null && accessories.Count > 0)
                {
                    return false;
                }
                if (inputs != null && inputs.Count > 0)
                {
                    return false;
                }
                if (outputs != null && outputs.Count > 0)
                {
                    return false;
                }
                return true;
            }

            internal decimal GetBaseSpeed_perSec(bool inputOrOutput, int portIndex)
            {
                List<Recipes.Recipe.PIOItem> ports;
                if (inputOrOutput)
                {
                    ports = inputs;
                }
                else
                {
                    ports = outputs;
                }
                if (portIndex < 0 || ports.Count <= portIndex)
                {
                    throw new IndexOutOfRangeException($"Index[{portIndex}] out of range[length:{ports}].");
                }
                Recipes.Recipe.PIOItem p = ports[portIndex];
                if (p.quantity is null)
                {
                    if (inputOrOutput)
                        throw ProcessingChains.ResultHelper.Error_Recipe_Input_Quantity_isNull(portIndex, name);
                    else
                        throw ProcessingChains.ResultHelper.Error_Recipe_Output_Quantity_isNull(portIndex, name);
                }
                if (period is null)
                {
                    throw ProcessingChains.ResultHelper.Error_Recipe_Peroid_isNullOrZero(name);
                }
                return p.quantity.ValueCurrentInGeneral / period.Value;
            }

            public class PIOItem
            {
                public Things.Thing? thing;
                public bool usingType = false;
                public bool isExcluded = false;
                public Quantity? quantity;

                public PIOItem(Things.Thing? thing, Quantity? quantity, bool usingType = false, bool isExcluded = false)
                {
                    this.thing = thing;
                    this.quantity = quantity;
                    this.usingType = usingType;
                    this.isExcluded = isExcluded;
                }
                public PIOItem(string IOContent)
                {
                    this.IOContent = IOContent;
                }
                public string IOContent
                {
                    get
                    {
                        StringBuilder strBdr = new StringBuilder();
                        strBdr.Append(isExcluded ? "-" : "+");
                        strBdr.Append(usingType ? "T " : "  ");
                        if (thing != null)
                        {
                            strBdr.Append(thing.id.ToString());
                        }
                        strBdr.Append("|");
                        strBdr.Append(quantity?.IOContent);
                        return strBdr.ToString();
                    }
                    set
                    {
                        string[] parts = value.Split('|');
                        string p1 = parts[0];
                        isExcluded = p1[0] == '-';
                        usingType = p1[1] == 'T';
                        Thing? found = Core.Instance.FindThing(Guid.Parse(p1.Substring(3)));
                        if (found != null)
                        {
                            thing = found;
                        }
                        if (parts.Length > 1)
                        {
                            if (quantity != null)
                            {
                                quantity.IOContent = parts[1];
                            }
                            else
                            {
                                quantity = new Quantity(parts[1]);
                            }
                        }
                    }
                }

                internal PIOItem Clone()
                {
                    return new PIOItem(
                        thing?.Clone(),
                        quantity?.Clone(),
                        usingType, isExcluded);
                }

                public bool CheckValueEqual(object b)
                {
                    if (b is not PIOItem)
                    {
                        return false;
                    }
                    PIOItem vb = (PIOItem)b;
                    if ((this.thing != null && vb.thing == null)
                        || (this.thing == null && vb.thing != null))
                    {
                        return false;
                    }
                    if ((this.thing != null && vb.thing != null)
                        && (this.thing.id != vb.thing.id))
                    {
                        return false;
                    }
                    if (this.usingType != vb.usingType)
                    {
                        return false;
                    }
                    if (this.isExcluded != vb.isExcluded)
                    {
                        return false;
                    }
                    if (this.quantity != vb.quantity)
                    {
                        return false;
                    }
                    return true;
                }

                internal void Overwrite(ref PIOItem child)
                {
                    child.thing = thing;
                    child.usingType = usingType;
                    child.isExcluded = isExcluded;
                    child.quantity = quantity;
                }
            }
            public class Quantity
            {

                private string _StringValueFix = "";
                public string StringValueFix
                {
                    get
                    {
                        return _StringValueFix;
                    }
                    set
                    {
                        if (Common.SimpleValueHelper.TryCal(value, out valueFix))
                        {
                            _StringValueFix = value;
                        }
                    }
                }
                private string _StringValueMinus = "";
                public string StringValueMinus
                {
                    get
                    {
                        return _StringValueMinus;
                    }
                    set
                    {
                        if (Common.SimpleValueHelper.TryCal(value, out valueMinus))
                        {
                            _StringValueMinus = value;
                        }
                    }
                }
                private string _StringValuePlus = "";
                public string StringValuePlus
                {
                    get
                    {
                        return _StringValuePlus;
                    }
                    set
                    {
                        if (Common.SimpleValueHelper.TryCal(value, out valuePlus))
                        {
                            _StringValuePlus = value;
                        }
                    }
                }

                public Quantity()
                {
                }
                public Quantity(string IOContent)
                {
                    this.IOContent = IOContent;
                }
                public string IOContent
                {
                    get
                    {
                        StringBuilder strBdr = new StringBuilder();
                        strBdr.Append(_StringValueFix);
                        strBdr.Append(";");
                        strBdr.Append(_StringValueMinus);
                        strBdr.Append(";");
                        strBdr.Append(_StringValuePlus);

                        return strBdr.ToString();
                    }
                    set
                    {
                        string[] parts = value.Split(';');
                        if (parts.Length > 2)
                        {
                            StringValueFix = parts[0];
                            StringValueMinus = parts[1];
                            StringValuePlus = parts[2];
                        }
                    }
                }


                Random random = new Random((int)DateTime.Now.Ticks);
                public decimal ValueCurrent
                {
                    get
                    {
                        return valueFix + valueMinus + (decimal)random.NextDouble() * (valuePlus - valueMinus);
                    }
                }
                public decimal ValueCurrentInGeneral
                {
                    get
                    {
                        return valueFix - valueMinus + valuePlus;
                    }
                }

                public decimal valueFix;
                public decimal valueMinus;
                public decimal valuePlus;

                internal Quantity Clone()
                {
                    Quantity clone = new Quantity()
                    {
                        _StringValueFix = _StringValueFix,
                        _StringValueMinus = _StringValueMinus,
                        _StringValuePlus = _StringValuePlus,
                        valueFix = valueFix,
                        valueMinus = valueMinus,
                        valuePlus = valuePlus,
                    };
                    return clone;
                }

                internal void Clear()
                {
                    _StringValueFix = "";
                    _StringValueMinus = "";
                    _StringValuePlus = "";
                    valueFix = 0;
                    valueMinus = 0;
                    valuePlus = 0;
                }

                public static bool operator ==(Quantity? a, object? b)
                {
                    if (a is null && b is null)
                    {
                        return true;
                    }
                    else if (a is null && b is not null)
                    {
                        return false;
                    }
                    else if (a is not null && b is null)
                    {
                        return false;
                    }
                    if (b is not Quantity)
                    {
                        return false;
                    }
                    Quantity vb = (Quantity)b;
                    if (a.valueFix == vb.valueFix
                        && a.valueMinus == vb.valueMinus
                        && a.valuePlus == vb.valuePlus)
                    {
                        return true;
                    }
                    return false;
                }
                public static bool operator !=(Quantity? a, object? b)
                {
                    return !(a == b);
                }
                public override bool Equals(object? obj)
                {
                    if (obj == null || obj is not Quantity)
                    {
                        return false;
                    }
                    return this == (Quantity)obj;
                }
                public override int GetHashCode()
                {
                    return valueFix.GetHashCode() * valueMinus.GetHashCode() * valuePlus.GetHashCode();
                }
            }
        }
    }
}
