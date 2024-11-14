using MadTomDev.App.VMs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MadTomDev.App.Classes.Recipes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace MadTomDev.App.Classes
{
    public class ThingsBase
    {
        protected string? _csvFile;
        public ThingsBase(TreeViewNodeModelScene scene)
        {
            this.Scene = scene;
        }
        public TreeViewNodeModelScene Scene { get; protected set; }

        public bool IsInherited { get; protected set; } = false;

        protected static string FLAG_PARENTID = "ParentID"; // null
        protected static string FLAG_ID = "ID";
        protected static string FLAG_ISEXCLUDED = "IsExcluded"; // null
        protected static string FLAG_NAME = "Name";
        protected static string FLAG_DESCRIPTION = "Description"; // null
    }
    public class Things : ThingsBase
    {
        private static string FLAG_TYPE = "Type"; // null
        private static string FLAG_UNIT = "Unit"; // null

        public List<Thing> list = new List<Thing>();
        public Things(TreeViewNodeModelScene scene) : base(scene)
        {
            _csvFile = Path.Combine(scene.GetDirFullName(), SceneMgr.FILENAME_THINGS);
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
                    Thing? testThing = null;

                    while (!reader.IsEoF)
                    {
                        row = reader.ReadRow();
                        if (titleFound && CheckIsDataRow(row))
                        {
                            // data to ram
                            testThing = GetThing(row);
                            if (testThing != null)
                            {
                                list.Add(testThing);
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
                        && row[3] == FLAG_TYPE
                        && row[4] == FLAG_ISEXCLUDED
                        && row[5] == FLAG_UNIT
                        && row[6] == FLAG_DESCRIPTION)
                    {
                        return true;
                    }
                }
                return false;
            }
            bool CheckIsDataRow(string[] row)
            {
                if (row.Length >= 7)
                {
                    if (row[1].Length == 0)
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
            Thing? GetThing(string[] row)
            {
                Thing result = new Thing(this);
                Guid testID;
                bool testB;
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
                if (row[2] != SceneMgr.VALUE_NULL)
                {
                    result.name = row[2];
                }
                //if (row[3] != SceneMgr.VALUE_NULL)
                //{
                //    result.type = row[3];
                //}
                if (row[4] != SceneMgr.VALUE_NULL)
                {
                    if (bool.TryParse(row[4], out testB))
                    {
                        result.isExcluded = testB;
                    }
                }
                //if (row[5] != SceneMgr.VALUE_NULL)
                //{
                //    result.unit = row[5];
                //}
                if (row[6] != SceneMgr.VALUE_NULL)
                {
                    result.description = row[6];
                }
                return result;
            }
        }
        public void Save()
        {
            if (IsInherited)
            {
                throw new InvalidOperationException("Inherited Things could not be saved.");
            }
            using (Common.CSVHelper.Writer writer = new Common.CSVHelper.Writer(_csvFile))
            {
                // write title
                string[] row = new string[]
                {
                    FLAG_PARENTID,
                    FLAG_ID,
                    FLAG_NAME,
                    FLAG_TYPE,
                    FLAG_ISEXCLUDED,
                    FLAG_UNIT,
                    FLAG_DESCRIPTION,
                };
                writer.WriteRow(row);

                // write content
                foreach (Thing t in list)
                {
                    row[0] = t.parentID == null ? SceneMgr.VALUE_NULL : ((Guid)t.parentID).ToString();
                    row[1] = t.id.ToString();
                    row[2] = t.name == null ? SceneMgr.VALUE_NULL : t.name;
                    //row[3] = t.type == null ? SceneMgr.VALUE_NULL : t.type;
                    row[4] = t.isExcluded == null ? SceneMgr.VALUE_NULL : t.isExcluded.ToString();
                    //row[5] = t.unit == null ? SceneMgr.VALUE_NULL : t.unit;
                    row[6] = t.description == null ? SceneMgr.VALUE_NULL : t.description;

                    writer.WriteRow(row);
                }
                writer.Flush();
            }
        }

        public void Inherit(Things parent)
        {
            Thing? pt;
            List<Thing> pList = new List<Thing>();
            pList.AddRange(parent.list);
            foreach (Thing t in list)
            {
                if (t.parentID != null)
                {
                    pt = pList.Find(a => a.id == t.parentID);
                    if (pt != null && pt.isExcluded == false)
                    {
                        t.Inherit(pt);
                        pList.Remove(pt);
                    }
                }
            }
            // add rest parent things
            for (int i = pList.Count - 1; i >= 0; --i)
            {
                pt = pList[i];
                if (pt.isExcluded == false)
                {
                    list.Insert(0, pt);
                }
            }

            IsInherited = true;
        }
        internal static void Simplify(ref Thing child, Thing parent)
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
        }

        public class ThingBase
        {
            public ThingBase(ThingsBase Container)
            {
                this.Container = Container;
            }
            public ThingsBase Container { protected set; get; }
            public Guid? parentID = null;
            public Guid id;
            public bool? isExcluded;
            public string? name;
            public string? description = null;

            protected List<ThingBase> _InheritedList = new List<ThingBase>();
            public List<ThingBase> InheritedList
            {
                get => _InheritedList;
            }
            public void Inherit(ThingBase parent)
            {
                _InheritedList.Add(parent);
            }

            public string? GetSceneDir()
            {
                return Container.Scene.GetDirFullName();
            }
        }
        public class Thing : ThingBase
        {
            public Thing(Things parent, bool genNewId = false) : base(parent)
            {
                if (genNewId)
                {
                    id = Guid.NewGuid();
                }
            }
            internal void DataFrom(Thing newData)
            {
                this.name = newData.name;
                this.isExcluded = newData.isExcluded;
                this.description = newData.description;
            }

            public void Inherit(Thing parent)
            {
                ((ThingBase)this).Inherit(parent);
                if (parent == null)
                {
                    return;
                }
                if (parentID != parent.id)
                {
                    throw new InvalidOperationException($"Thing:Parent[{parent.id}] is a false for child[{parentID}]");
                }
                if (name == null && parent.name != null)
                {
                    name = parent.name;
                }
                if (isExcluded == null && parent.isExcluded != null)
                {
                    isExcluded = parent.isExcluded;
                }
                if (description == null && parent.description != null)
                {
                    description = parent.description;
                }
            }

            internal Thing Clone()
            {
                Thing clone = new Thing((Things)Container)
                {
                    //Parent = Parent,
                    parentID = parentID,
                    id = id,
                    name = name,
                    isExcluded = isExcluded,
                    description = description,
                };
                return clone;
            }
        }
    }
}
