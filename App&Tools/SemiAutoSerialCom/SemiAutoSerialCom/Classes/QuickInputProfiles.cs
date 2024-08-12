using MadTomDev.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace MadTomDev.App.Classes
{
    public class QuickInputProfiles
    {
        private SettingXML xmlHelper;
        private string settingFile = "quickInputs.xml";
        public QuickInputProfiles()
        {
            xmlHelper = new SettingXML("SASC")
            { xmlFile = settingFile };
            ReLoad();
        }

        private static string Flag_Profile = "P";
        private static string Flag_ProfileName = "Name";
        public void ReLoad()
        {
            if (File.Exists(settingFile))
            {
                profiles.Clear();
                string profileName;
                List<string> profile;
                xmlHelper.Reload(settingFile);
                foreach (SettingXML.Node p in xmlHelper.rootNode.Children)
                {
                    if (p.nodeName == Flag_Profile)
                    {
                        profileName = p.attributes[Flag_ProfileName];
                        if (string.IsNullOrWhiteSpace(profileName))
                        {
                            continue;
                        }
                        if (!profiles.ContainsKey(profileName))
                        {
                            profile = new List<string>();
                            for (int i = 0, iv = p.Children.Count; i < iv; ++i)
                            {
                                profile.Add(p.Children[i].Text);
                            }
                            profiles.Add(profileName, profile);
                        }
                    }
                }
            }
        }
        public void Save()
        {
            SettingXML.Node xmlRoot = xmlHelper.rootNode;
            xmlRoot.Children.Clear();
            SettingXML.Node pNode;
            List<string> profile;
            foreach (string pName in profiles.Keys)
            {
                pNode = new SettingXML.Node()
                { nodeName = Flag_Profile, };
                pNode.attributes.AddUpdate(Flag_ProfileName, pName);

                profile = profiles[pName];
                for (int i = 0, iv = profile.Count; i < iv; ++i)
                {
                    pNode.Add(new SettingXML.Node()
                    { nodeName = "i", Text = profile[i], });
                }
                xmlRoot.Add(pNode);
            }
            xmlHelper.Save();
        }

        private Dictionary<string, List<string>> profiles = new Dictionary<string, List<string>>();

        public IEnumerable<string> ProfileNameList
        {
            get => profiles.Keys;
        }

        public bool AddProfile(string profileName)
        {
            if (HasProfile(profileName))
            {
                return false;
            }
            profiles.Add(profileName, new List<string>());
            return true;
        }

        public bool RemoveProfile(string profileName)
        {
            if (HasProfile(profileName))
            {
                profiles.Remove(profileName);
                return true;
            }
            return false;
        }

        public bool HasProfile(string profileName)
        {
            return profiles.ContainsKey(profileName);
        }
        public List<string>? GetProfile(string profileName)
        {
            if (HasProfile(profileName))
            {
                return profiles[profileName];
            }
            return null;
        }
    }
}
