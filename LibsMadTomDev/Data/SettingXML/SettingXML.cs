using System.Xml;

namespace MadTomDev.Data
{
    /// <summary>
    /// 新建示例时只在内存中创建对象，若要保存到文件，请执行save；
    /// 使用时，直接对rootNode进性存取即可，最后别忘了save；
    /// 如果要撤销变动，请执行reload重新从文件加载；
    /// </summary>
    public class SettingXML
    {
        public string xmlFile;
        public SettingXML(string rootNodeName = "Root")
        {
            rootNode = new Node() { nodeName = rootNodeName };
        }
        public static SettingXML FromFile(string xmlFile)
        {
            SettingXML result = new SettingXML();
            result.Reload(xmlFile);
            return result;
        }

        #region load or save file

        public bool Reload(string xmlFile)
        {
            this.xmlFile = xmlFile;
            rootNode = null;
            if (File.Exists(xmlFile))
            {
                rootNode = new Node();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);
                LoadXMLLoop(rootNode, (XmlElement)xmlDoc.ChildNodes[1]);
                return true;
            }
            return false;
        }
        public void Reload()
        {
            Reload(xmlFile);
        }
        private void LoadXMLLoop(Node node, XmlElement xmlNode)
        {
            node.nodeName = xmlNode.Name;
            foreach (XmlAttribute xmlAtt in xmlNode.Attributes)
            {
                node.attributes.attList.Add(xmlAtt.Name, xmlAtt.Value);
            }
            if (xmlNode.HasChildNodes)
            {
                if (xmlNode.ChildNodes.Count == 1
                    && (xmlNode.ChildNodes[0].NodeType == XmlNodeType.CDATA
                        || xmlNode.ChildNodes[0].NodeType == XmlNodeType.Text))
                {
                    node.Text = xmlNode.ChildNodes[0].Value;
                }
                else
                {
                    Node subNode;
                    foreach (XmlNode subXmlNode in xmlNode.ChildNodes)
                    {
                        if (subXmlNode is XmlElement)
                        {
                            subNode = new Node();
                            LoadXMLLoop(subNode, (XmlElement)subXmlNode);
                            node.Add(subNode);
                        }
                        else if (subXmlNode is XmlCDataSection)
                        {
                            subNode = new Node()
                            { Text = ((XmlCDataSection)subXmlNode).Value, };
                            node.Add(subNode);
                        }
                    }
                }
            }
        }
        public void Save()
        {
            Save(xmlFile);
        }
        public void Save(string xmlFile)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
            XmlElement rootXml = xmlDoc.CreateElement(rootNode.nodeName);
            SaveLoop(xmlDoc, rootXml, rootNode);
            xmlDoc.AppendChild(rootXml);
            xmlDoc.Save(xmlFile);
            this.xmlFile = xmlFile;
        }
        private void SaveLoop(XmlDocument xmlDoc, XmlElement xmlNode, Node node)
        {
            XmlAttribute xmlAtt;
            foreach (string key in node.attributes.attList.Keys)
            {
                xmlAtt = xmlDoc.CreateAttribute(key);
                xmlAtt.Value = node.attributes.attList[key];
                xmlNode.Attributes.Append(xmlAtt);
            }
            if (string.IsNullOrEmpty(node.Text))
            {
                XmlElement subXmlNode;
                foreach (Node subNode in node.Children)
                {
                    if (string.IsNullOrWhiteSpace(subNode.nodeName))
                    {
                        xmlNode.AppendChild(xmlDoc.CreateCDataSection(subNode.Text));
                    }
                    else
                    {
                        subXmlNode = xmlDoc.CreateElement(subNode.nodeName);
                        SaveLoop(xmlDoc, subXmlNode, subNode);
                        xmlNode.AppendChild(subXmlNode);
                    }
                }
            }
            else
            {
                XmlCDataSection xmlCData = xmlDoc.CreateCDataSection(node.Text);
                xmlNode.AppendChild(xmlCData);
            }
        }
        #endregion


        public Node rootNode;
        public class Node
        {
            public Node()
            {
            }
            public Node(string nodeName)
            {
                this.nodeName = nodeName;
            }
            public string nodeName = "node";

            private string _Text = null;
            /// <summary>
            /// the text of the node, if it's not null, no children nodes will be saved;
            /// </summary>
            public string Text
            {
                get => _Text;
                set
                {
                    if (_Text == value)
                        return;
                    _Text = value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        children.Clear();
                    }
                }
            }
            private List<Node> children = new List<Node>();
            public List<Node> Children
            {
                get => children;
            }
            public List<Node> this[string name]
            {
                get
                {
                    List<Node> result = new List<Node>();
                    foreach (Node n in children)
                    {
                        if (n.nodeName == name)
                            result.Add(n);
                    }
                    return result;
                }
            }
            public Node FindFirst(string name)
            {
                foreach (Node n in children)
                {
                    if (n.nodeName == name)
                        return n;
                }
                return null;
            }

            /// <summary>
            /// text will be set to null;
            /// </summary>
            /// <param name="newNode"></param>
            public void Add(Node newNode)
            {
                _Text = null;
                children.Add(newNode);
            }
            public void Move(Node node, int toIndex)
            {
                if (children.Remove(node))
                {
                    children.Insert(toIndex, node);
                }
            }
            public void Remove(Node node)
            {
                children.Remove(node);
            }

            public void Clear()
            {
                children.Clear();
                attributes.Clear();
            }

            public AttributesClass attributes = new AttributesClass();
            public class AttributesClass
            {
                public Dictionary<string, string> attList = new Dictionary<string, string>();
                public string this[string name]
                {
                    get
                    {
                        if (attList.ContainsKey(name))
                            return attList[name];
                        else
                            return null;
                    }
                }
                public void AddUpdate(string name, string value)
                {
                    if (attList.ContainsKey(name))
                        attList[name] = value;
                    else
                        attList.Add(name, value);
                }
                public void Clear()
                {
                    attList.Clear();
                }
            }
        }
    }
}