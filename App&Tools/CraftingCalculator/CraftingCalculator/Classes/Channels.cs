using MadTomDev.App.VMs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static MadTomDev.App.Classes.Recipes;
using static MadTomDev.App.Classes.Recipes.Recipe;
using static MadTomDev.App.Classes.Things;

namespace MadTomDev.App.Classes
{
    public class Channels :ThingsBase
    {
        //private static string FLAG_PARENTID = "ParentID"; // null
        //private static string FLAG_ID = "ID";
        //private static string FLAG_NAME = "Name";
        //private static string FLAG_ISEXCLUDED = "IsExcluded"; // null
        private static string FLAG_SPEED = "Speed"; // null
        private static string FLAG_CONTENTLIST = "ContentList"; // null
        //private static string FLAG_DESCRIPTION = "Description"; // null

        public List<Channel> list = new List<Channel>();
        public Channels(TreeViewNodeModelScene scene) : base(scene) 
        {
            _csvFile = Path.Combine(scene.GetDirFullName(), SceneMgr.FILENAME_CHANNELS);
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
                    Channel? testChannel = null;

                    while (!reader.IsEoF)
                    {
                        row = reader.ReadRow();
                        if (titleFound)
                        {
                            // data to ram
                            if (CheckIsMainDataRow(row))
                            {
                                // main data
                                testChannel = GetMain(row);
                                if (testChannel != null)
                                {
                                    list.Add(testChannel);
                                }
                            }
                            else
                            {
                                // sub data
                                if (testChannel != null)
                                {
                                    AddSubList(ref testChannel, row);
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
                // ParentID  ID  Name  IsExcluded  Speed  ContentList   []   []   []   []  Description
                if (row.Length >= 9)
                {
                    if (row[0] == FLAG_PARENTID
                        && row[1] == FLAG_ID
                        && row[2] == FLAG_NAME
                        && row[3] == FLAG_ISEXCLUDED
                        && row[4] == FLAG_SPEED
                        && row[5] == FLAG_CONTENTLIST
                        && row[10] == FLAG_DESCRIPTION)
                    {
                        return true;
                    }
                }
                return false;
            }
            bool CheckIsMainDataRow(string[] row)
            {
                if (row.Length >= 4)
                {
                    if (row[1].Length == 0)
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
            Channel? GetMain(string[] row)
            {
                Channel result = new Channel(this);
                Guid testID;
                bool testB;
                double testD;
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
                if (row[3] != SceneMgr.VALUE_NULL)
                {
                    if (bool.TryParse(row[3], out testB))
                    {
                        result.isExcluded = testB;
                    }
                }
                if (row[4] != SceneMgr.VALUE_NULL)
                {
                    if (double.TryParse(row[4], out testD))
                    {
                        result.speed = testD;
                    }
                }
                if (row[10] != SceneMgr.VALUE_NULL)
                {
                    result.description = row[10];
                }
                return result;
            }
            Channel.ContentItem? testCI;
            void AddSubList(ref Channel c, string[] row)
            {
                if (row.Length >= 9)
                {
                    testCI = GetCI(row[5]);
                    if (testCI != null)
                    {
                        c.contentList.Add((Channel.ContentItem)testCI);
                    }
                    testCI = GetCI(row[6]);
                    if (testCI != null)
                    {
                        c.contentList.Add((Channel.ContentItem)testCI);
                    }
                    testCI = GetCI(row[7]);
                    if (testCI != null)
                    {
                        c.contentList.Add((Channel.ContentItem)testCI);
                    }
                    testCI = GetCI(row[8]);
                    if (testCI != null)
                    {
                        c.contentList.Add((Channel.ContentItem)testCI);
                    }
                    testCI = GetCI(row[9]);
                    if (testCI != null)
                    {
                        c.contentList.Add((Channel.ContentItem)testCI);
                    }
                }
            }
            Channel.ContentItem? GetCI(string ioContent)
            {
                if (ioContent.Length < 2)
                {
                    return null;
                }
                if (Guid.TryParse(ioContent.Substring(1), out Guid g))
                {
                    return new Channel.ContentItem()
                    {
                        addOrRemove = ioContent[0] == '+',
                        contentId = g,
                    };
                }
                return null;
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
                    FLAG_ISEXCLUDED,
                    FLAG_SPEED,
                    FLAG_CONTENTLIST,
                    "",
                    "",
                    "",
                    "",
                    FLAG_DESCRIPTION,
                };
                writer.WriteRow(row);

                // write content
                Channel.ContentItem cc;
                foreach (Channel c in list)
                {
                    row[0] = c.parentID == null ? SceneMgr.VALUE_NULL : ((Guid)c.parentID).ToString();
                    row[1] = c.id.ToString();
                    row[2] = c.name == null ? SceneMgr.VALUE_NULL : c.name;
                    row[3] = c.isExcluded == null ? SceneMgr.VALUE_NULL : ((bool)c.isExcluded).ToString();
                    row[4] = c.speed == null ? SceneMgr.VALUE_NULL : ((double)c.speed).ToString();
                    row[5] = row[6] = row[7] = row[8] = row[9] = "";
                    row[10] = c.description == null ? SceneMgr.VALUE_NULL : c.description;
                    writer.WriteRow(row);

                    // write content list
                    for (int i = 0, iv = c.contentList.Count; i < iv; ++i)
                    {
                        row[0] = row[1] = row[2] = row[3] = row[4] = row[6] = row[7] = row[8] = row[9] = row[10] = "";
                        cc = c.contentList[i];
                        row[5] = ContentItem2IOContent(ref cc);
                        if (++i < iv)
                        {
                            cc = c.contentList[i];
                            row[6] = ContentItem2IOContent(ref cc);
                        }
                        if (++i < iv)
                        {
                            cc = c.contentList[i];
                            row[7] = ContentItem2IOContent(ref cc);
                        }
                        if (++i < iv)
                        {
                            cc = c.contentList[i];
                            row[8] = ContentItem2IOContent(ref cc);
                        }
                        if (++i < iv)
                        {
                            cc = c.contentList[i];
                            row[9] = ContentItem2IOContent(ref cc);
                        }
                        writer.WriteRow(row);
                    }

                }
                writer.Flush();
            }
            string ContentItem2IOContent(ref Channel.ContentItem c)
            {
                if (c.addOrRemove)
                {
                    return "+" + c.contentId.ToString();
                }
                else
                {
                    return "-" + c.contentId.ToString();
                }
            }
        }

        public void Inherit(Channels parent)
        {
            Channel? pt;
            List<Channel> pList = new List<Channel>();
            pList.AddRange(parent.list);
            foreach (Channel t in list)
            {
                if (t.parentID != null)
                {
                    pt = pList.Find(a => a.id == t.parentID);
                    if (pt != null && pt.isExcluded != true)
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
                if (pt.isExcluded != true)
                {
                    list.Insert(0, pt);
                }
            }

            IsInherited = true;
        }
        internal static void Simplify(ref Channel child, Channel parent)
        {
            if (child.parentID != parent.id)
            {
                throw new ArgumentException($"Recipe[{child.name}] is not the child of [{parent.name}].");
            }
            if (child.name == parent.name)
            {
                child.name = null;
            }
            if (child.isExcluded == parent.isExcluded)
            {
                child.isExcluded = null;
            }
            else if (child.isExcluded != true && parent.isExcluded == null)
            {
                child.isExcluded = null;
            }
            if (child.speed == parent.speed)
            {
                child.speed = null;
            }

            Channel.ContentItem c;
            List<Channel.ContentItem> copiedPList = new List<Channel.ContentItem>();
            copiedPList.AddRange(parent.contentList); // only add, no remove
            Channel.ContentItem p;
            for (int i = child.contentList.Count - 1; i >= 0; --i)
            {
                c = child.contentList[i];
                p = copiedPList.Find(a => a.contentId == c.contentId);
                if (p.contentId != Guid.Empty)
                // && c.addOrRemove == true
                {
                    copiedPList.Remove(p);
                    child.contentList.RemoveAt(i);
                }
                else if (c.addOrRemove == false)
                {
                    child.contentList.RemoveAt(i);
                }
            }
            // exclude not exists items
            foreach (Channel.ContentItem ip in copiedPList)
            {
                c = new Channel.ContentItem();
                c.contentId = ip.contentId;
                c.addOrRemove = false;
                child.contentList.Add(c);
            }

            if (child.description == parent.description)
            {
                child.description = null;
            }
        }

        public class Channel : ThingBase
        {
            public Channel(Channels parent, bool genNewId = false) : base(parent)
            {
                if (genNewId)
                {
                    id = Guid.NewGuid();
                }
            }
            internal void DataFrom(Channel newData)
            {
                this.name = newData.name;
                this.isExcluded = newData.isExcluded;
                this.speed = newData.speed;
                this.contentList.Clear();
                this.contentList.AddRange(newData.contentList);
                this.description = newData.description;
            }

            public double? speed;
            public List<ContentItem> contentList = new List<ContentItem>();
            public string ContentListTx
            {
                get
                {
                    StringBuilder strBdr = new StringBuilder();
                    Things.Thing? foundThing;
                    for (int i = 0, iv = contentList.Count; i < iv; ++i)
                    {
                        foundThing = Core.Instance.FindThing(contentList[i].contentId);
                        if (foundThing != null)
                        {
                            strBdr.Append(foundThing.name);
                            strBdr.Append(", ");
                        }
                    }
                    if (strBdr.Length > 1)
                    {
                        strBdr.Remove(strBdr.Length - 2, 2);
                    }
                    return strBdr.ToString();
                }
            }

            public struct ContentItem
            {
                public bool addOrRemove;
                public Guid contentId;
            }

            public void Inherit(Channel parent)
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
                if (speed == null && parent.speed != null)
                {
                    speed = parent.speed;
                }

                List<ContentItem> copyList = new List<ContentItem>();
                copyList.AddRange(parent.contentList); // all add, no remove
                ContentItem c, p;
                for (int i = contentList.Count - 1; i >= 0; --i)
                {
                    c = contentList[i];
                    p = copyList.Find(a => a.contentId == c.contentId);
                    copyList.Remove(p);
                    if (c.addOrRemove == false)
                    {
                        contentList.RemoveAt(i);
                    }
                }
                contentList.AddRange(copyList);

                if (description == null && parent.description != null)
                {
                    description = parent.description;
                }
            }
            public static void Inherite(ref List<Channel.ContentItem> childContentList, List<Channel.ContentItem> parentContentList)
            {
                List<Channel.ContentItem> copyList = new List<Channel.ContentItem>();
                copyList.AddRange(parentContentList);
                Channel.ContentItem c;
                Channel.ContentItem p;
                for (int i = 0, iv = childContentList.Count; i < iv; ++i)
                {
                    c = childContentList[i];
                    p = copyList.Find(a => a.contentId == c.contentId);
                    if (p.contentId == Guid.Empty)
                    {
                        continue;
                    }
                    copyList.Remove(p);
                    if (c.addOrRemove == false)
                    {
                        childContentList.RemoveAt(i);
                        --i; --iv;
                    }
                }
                childContentList.AddRange(copyList);
            }

            internal Channel Clone()
            {
                Channel clone = new Channel((Channels)Container)
                {
                    //Container = Container,
                    parentID = parentID,
                    id = id,
                    name = name,
                    isExcluded = isExcluded,
                    speed = speed,
                    //contentList
                    description = description,
                };
                clone.contentList.AddRange(contentList);
                return clone;
            }

            internal bool CheckIsEmpty()
            {
                if (name != null)
                {
                    return false;
                }
                if (isExcluded != null)
                {
                    return false;
                }
                if (speed != null)
                {
                    return false;
                }
                if (contentList != null && contentList.Count > 0)
                {
                    return false;
                }
                if (description != null)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
