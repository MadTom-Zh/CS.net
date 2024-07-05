using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using System.IO;
using MadTomDev.Data;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using System.Windows.Media.Converters;
using System.IO.Ports;
using System.Reflection.Emit;
using System.Xml.Linq;
using System.Windows;

using FontStyle = System.Windows.FontStyle;
using FontFamily = System.Windows.Media.FontFamily;
using System.Reflection;

namespace MadTomDev.App.Classes
{
    public class Settings
    {
        private SettingXML xmlHelper;
        private string settingFile = "config.xml";
        public Settings()
        {
            xmlHelper = new SettingXML("SASC")
            { xmlFile = settingFile };
            ReLoad();
        }

        // root
        //     global
        //         output bg fg
        //         input  bg fg
        //         behaviour useTimePrefix timeFormat closeOnlineAct closeActOffline
        //     comList
        //         com  name speed databits stopbits parity flowControl qTimeout isIdCheck
        //              [cd 0]idCheckSend
        //              [cd 1]idCheckReceive

        private static string Flag_Global = "Global";
        private static string Flag_Global_FontFamily = "FontFamily";
        private static string Flag_Global_FontSize = "FontSize";
        private static string Flag_Global_FontStyle = "FontStyle";
        private static string Flag_Global_FontWeight_OpenTypeWeight = "FontOpenTypeWeight";
        private static string Flag_Global_Output = "Output";
        private static string Flag_Global_Input = "Input";
        private static string Flag_Global_Behaviour = "Behaviour";
        private static string Flag_Global_Behaviour_useTimePrefix = "useTimePrefix";
        private static string Flag_Global_Behaviour_useLog = "useTimePrefix";
        private static string Flag_Global_Behaviour_timeFormat = "timeFormat";

        private static string Flag_Att_BGColor = "BGColor";
        private static string Flag_Att_FGColor = "FGColor";
        private static string Flag_ComList = "ComList";
        private static string Flag_Com = "Com";
        private static string Flag_Com_Name = "Name";
        private static string Flag_Com_Speed = "Speed";
        private static string Flag_Com_DataBits = "DataBits";
        private static string Flag_Com_StopBits = "StopBits";
        private static string Flag_Com_Parity = "Parity";
        private static string Flag_Com_FlowControl = "FlowControl";

        private static string Flag_Com_Translators = "Translators";
        private static string Flag_Com_Translator = "T";

        private static SolidColorBrush defaultBG = new SolidColorBrush(Colors.Black);
        private static SolidColorBrush defaultFG = new SolidColorBrush(Colors.Lime);

        public void ReLoad()
        {
            cfgList.Clear();
            if (File.Exists(settingFile))
            {
                xmlHelper.Reload();
                // xml to var
                SettingXML.Node testNode = xmlHelper.rootNode.FindFirst(Flag_Global);
                SettingXML.Node subNode;
                SetGlobalDefault();
                if (testNode != null)
                {
                    string testStr = testNode.attributes[Flag_Global_FontFamily];
                    if (!string.IsNullOrWhiteSpace(testStr))
                        fontFamily = new FontFamily(testStr);
                    else
                        fontFamily = new FontFamily("Consolas");

                    if (float.TryParse(testNode.attributes[Flag_Global_FontSize], out float vF)
                        && vF > 1)
                        fontSize = vF;
                    else
                        fontSize = 12;

                    testStr = testNode.attributes[Flag_Global_FontStyle];
                    if (!string.IsNullOrWhiteSpace(testStr))
                    {
                        Type tFontStyles = typeof(FontStyles);
                        bool notFound = true;
                        foreach (PropertyInfo pi in tFontStyles.GetProperties())
                        {
                            if (pi.Name == testStr)
                            {
                                notFound = false;
                                fontStyle = (FontStyle)pi.GetValue(null);
                                break;
                            }
                        }
                        if (notFound)
                            fontStyle = FontStyles.Normal;
                    }
                    else
                        fontStyle = FontStyles.Normal;


                    if (int.TryParse(testNode.attributes[Flag_Global_FontWeight_OpenTypeWeight], out int vInt))
                        fontWeight = FontWeight.FromOpenTypeWeight(vInt);
                    else
                        fontWeight = FontWeights.Regular;

                    subNode = testNode.FindFirst(Flag_Global_Output);
                    if (subNode != null)
                    {
                        if (TryGetColorBrush(subNode.attributes[Flag_Att_BGColor], out SolidColorBrush brush))
                            clrOutputBG = brush;
                        if (TryGetColorBrush(subNode.attributes[Flag_Att_FGColor], out brush))
                            clrOutputFG = brush;
                    }
                    subNode = testNode.FindFirst(Flag_Global_Input);
                    if (subNode != null)
                    {
                        if (TryGetColorBrush(subNode.attributes[Flag_Att_BGColor], out SolidColorBrush brush))
                            clrInputBG = brush;
                        if (TryGetColorBrush(subNode.attributes[Flag_Att_FGColor], out brush))
                            clrInputFG = brush;
                    }
                    subNode = testNode.FindFirst(Flag_Global_Behaviour);
                    if (subNode != null)
                    {
                        if (bool.TryParse(subNode.attributes[Flag_Global_Behaviour_useTimePrefix], out bool v))
                            isOutputTime = v;
                        if (bool.TryParse(subNode.attributes[Flag_Global_Behaviour_useLog], out v))
                            isLog = v;
                        outputTimeFormat = subNode.attributes[Flag_Global_Behaviour_timeFormat];
                    }
                }
                testNode = xmlHelper.rootNode.FindFirst(Flag_ComList);
                if (testNode != null)
                {
                    DGI_ComCfg newCC;
                    SettingXML.Node? translatorsNode;
                    string testStr;
                    foreach (SettingXML.Node c in testNode.Children)
                    {
                        if (c.nodeName != Flag_Com)
                            continue;

                        newCC = new DGI_ComCfg()
                        {
                            Name = c.attributes[Flag_Com_Name],
                        };

                        if (int.TryParse(c.attributes[Flag_Com_Speed], out int vInt))
                            newCC.Speed = vInt;
                        if (int.TryParse(c.attributes[Flag_Com_DataBits], out vInt))
                            newCC.DataBits = vInt;
                        if (TryParseStopBits(c.attributes[Flag_Com_StopBits], out StopBits vS))
                            newCC.StopBits = vS;
                        if (TryParseParity(c.attributes[Flag_Com_Parity], out Parity vP))
                            newCC.Parity = vP;
                        if (TryParseFlowControl(c.attributes[Flag_Com_FlowControl], out FlowControl vFC))
                            newCC.FlowControl = vFC;

                        newCC.translatorList.Clear();
                        translatorsNode = c.Children.Find(t => t.nodeName == Flag_Com_Translators);
                        if (translatorsNode != null)
                        {
                            foreach (SettingXML.Node l in translatorsNode.Children)
                            {
                                newCC.translatorList.Add(l.Text);
                            }
                        }
                        cfgList.Add(newCC);
                    }
                }
                else
                {
                    // set default
                    xmlHelper.rootNode.Clear();
                    SetGlobalDefault();
                }
            }
            else
            {
                // set default
                xmlHelper.rootNode.Clear();
                SetGlobalDefault();
            }

            void SetGlobalDefault()
            {
                fontFamily = new FontFamily("Consolas");
                fontSize = 12f;
                fontStyle = FontStyles.Normal;
                fontWeight = FontWeights.Regular;

                clrInputBG = clrOutputBG = defaultBG;
                clrInputFG = clrOutputFG = defaultFG;
                isOutputTime = true;
                isLog = false;
                outputTimeFormat = "HH:mm:ss.fff";
            }
            bool TryGetColorBrush(string txValue, out SolidColorBrush value)
            {
                value = null;
                try
                {
                    Color clr = (Color)ColorConverter.ConvertFromString(txValue);
                    value = new SolidColorBrush(clr);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        public void Save()
        {
            // var to xml
            xmlHelper.rootNode.Children.Clear();

            #region global
            SettingXML.Node node = new SettingXML.Node() { nodeName = Flag_Global };

            node.attributes.AddUpdate(Flag_Global_FontFamily, fontFamily.ToString());
            node.attributes.AddUpdate(Flag_Global_FontSize, fontSize.ToString());
            node.attributes.AddUpdate(Flag_Global_FontStyle, fontStyle.ToString());
            node.attributes.AddUpdate(Flag_Global_FontWeight_OpenTypeWeight, fontWeight.ToOpenTypeWeight().ToString());

            SettingXML.Node subNode = new SettingXML.Node() { nodeName = Flag_Global_Output };
            subNode.attributes.AddUpdate(Flag_Att_BGColor, clrOutputBG.Color.ToString());
            subNode.attributes.AddUpdate(Flag_Att_FGColor, clrOutputFG.Color.ToString());
            node.Children.Add(subNode);

            subNode = new SettingXML.Node() { nodeName = Flag_Global_Input };
            subNode.attributes.AddUpdate(Flag_Att_BGColor, clrInputBG.Color.ToString());
            subNode.attributes.AddUpdate(Flag_Att_FGColor, clrInputFG.Color.ToString());
            node.Children.Add(subNode);

            subNode = new SettingXML.Node() { nodeName = Flag_Global_Behaviour };
            subNode.attributes.AddUpdate(Flag_Global_Behaviour_useTimePrefix, isOutputTime.ToString());
            subNode.attributes.AddUpdate(Flag_Global_Behaviour_useLog, isLog.ToString());
            subNode.attributes.AddUpdate(Flag_Global_Behaviour_timeFormat, outputTimeFormat);
            node.Children.Add(subNode);

            xmlHelper.rootNode.Children.Add(node);
            #endregion

            #region com list
            node = new SettingXML.Node() { nodeName = Flag_ComList };
            SettingXML.Node translatorsNode;
            foreach (DGI_ComCfg c in cfgList)
            {
                subNode = new SettingXML.Node() { nodeName = Flag_Com };
                subNode.attributes.AddUpdate(Flag_Com_Name, c.Name);
                subNode.attributes.AddUpdate(Flag_Com_Speed, c.Speed.ToString());
                subNode.attributes.AddUpdate(Flag_Com_DataBits, c.DataBits.ToString());
                subNode.attributes.AddUpdate(Flag_Com_StopBits, c.StopBits.ToString());
                subNode.attributes.AddUpdate(Flag_Com_Parity, c.Parity.ToString());
                subNode.attributes.AddUpdate(Flag_Com_FlowControl, c.FlowControl.ToString());

                if (c.translatorList.Count > 0)
                {
                    translatorsNode = new SettingXML.Node()
                    { nodeName = Flag_Com_Translators, };
                    foreach (string l in c.translatorList)
                    {
                        translatorsNode.Add(new SettingXML.Node
                        {
                            nodeName = Flag_Com_Translator,
                            Text = l,
                        }); ;
                    }
                    subNode.Add(translatorsNode);
                }

                node.Children.Add(subNode);
            }

            xmlHelper.rootNode.Children.Add(node);
            #endregion

            xmlHelper.Save();
        }

        public static bool TryParseStopBits(string t, out StopBits v)
        {
            v = StopBits.One;
            if (Enum.TryParse(typeof(StopBits), t, out object? o)
                && o != null)
            {
                v = (StopBits)o;
                return true;
            }
            return false;
        }
        public static bool TryParseParity(string t, out Parity v)
        {
            v = Parity.None;
            if (Enum.TryParse(typeof(Parity), t, out object? o)
                && o != null)
            {
                v = (Parity)o;
                return true;
            }
            return false;
        }
        public static bool TryParseFlowControl(string t, out FlowControl v)
        {
            v = FlowControl.Next;
            if (Enum.TryParse(typeof(FlowControl), t, out object? o)
                && o != null)
            {
                v = (FlowControl)o;
                return true;
            }
            return false;
        }

        public List<DGI_ComCfg> cfgList = new List<DGI_ComCfg>();

        public SolidColorBrush clrOutputBG, clrOutputFG;
        public SolidColorBrush clrInputBG, clrInputFG;
        public FontFamily fontFamily;
        public double fontSize;
        public FontStyle fontStyle;
        public FontWeight fontWeight;
        public bool isOutputTime;
        public bool isLog;
        public string outputTimeFormat;
    }
}
