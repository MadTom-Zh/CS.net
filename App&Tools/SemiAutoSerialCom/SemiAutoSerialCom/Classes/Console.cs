using MadTomDev.App.Ctrls;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace MadTomDev.App.Classes
{
    public class Console : IDisposable
    {
        private Core core = Core.Instance;
        public DGI_ComCfg cfg;
        public Com com;
        public TabItemHeader tabHeader;
        public PanelConsole tabPanel;
        public Console(string comName, DGI_ComCfg cfg)
        {
            this.cfg = cfg;
            // 新建控件
            tabHeader = new TabItemHeader() { Console = this, };
            tabHeader.SetTextUp(comName);
            tabHeader.SetTextDown(cfg.Name);
            tabPanel = new PanelConsole() { Console = this, };
        }

        /// <summary>
        /// 事件，string，消息内容；
        /// </summary>
        public event Action<Console, string> RaiseEvent;
        public event Action<Console,  SerialError> RaiseSerialError;
        /// <summary>
        /// 数据交互，第一bool，true-接收，false-发送；string，内容；
        /// </summary>
        public event Action<Console, bool, string> RaiseTraffic;

        public Task<bool> TryOpenComAsync(string comName)
        {
            return Task.Run(bool () =>
            {
                bool result = false;
                if (com == null)
                {
                    // 从空闲port中尝试打开串口
                    RaiseEvent?.Invoke(this, $"Try opening com port[{comName}] using config [{cfg.Name}]...");
                    Com testCom = null;

                    testCom = new Com(cfg, comName);
                    if (!testCom.IsOpen)
                    {
                        testCom.Dispose();
                        testCom = null;
                    }

                    if (testCom != null)
                    {
                        com = testCom;
                        com.comPort.ErrorReceived += ComPort_ErrorReceived;
                        com.comPort.PinChanged += ComPort_PinChanged;
                        StartListening();

                        RaiseEvent?.Invoke(this, $"Com port[{this.comName}] open.");
                        result = true;
                    }
                    else
                    {
                        RaiseEvent?.Invoke(this, $"Com port[{comName}] not available.");
                    }
                }
                else
                {
                    if (!com.IsOpen)
                    {
                        com.comPort.Open();
                    }

                    if (com.IsOpen)
                    {
                        RaiseEvent?.Invoke(this, $"Com port [{com.comPort.PortName}] re-open");
                    }
                    else
                    {
                        RaiseEvent?.Invoke(this, "Open com port failed");
                    }
                }
                return result;
            });
        }

        private void ComPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            RaiseTraffic?.Invoke(this, true, $"PinChanged: {e.EventType}");
        }

        private void ComPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            RaiseTraffic?.Invoke(this, true, $"ErrorReceived: {e.EventType}");
        }

        private bool listeningFlag = false;
        private void StartListening()
        {
            Task.Run(() =>
            {
                listeningFlag = true;
                string inStr;
                do
                {
                    if (com.comPort != null)
                    {
                        try
                        {
                            inStr = com.comPort.ReadLine();
                            RaiseTraffic?.Invoke(this, true, inStr);
                        }
                        catch (OperationCanceledException e1)
                        {
                            listeningFlag = false;
                            core.TryLog(e1);
                            RaiseEvent?.Invoke(this, e1.Message);
                        }
                        catch (Exception e2)
                        {
                            listeningFlag = false;
                            core.TryLog(e2);
                            RaiseEvent?.Invoke(this, e2.Message);
                        }
                    }
                }
                while (listeningFlag);
            });
        }
        // 据说DataReceived是低优先级方法，有可能会漏字符……
        //private StringBuilder ComPort_DataReceived_StrBdr = new StringBuilder();
        //private void ComPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    if (e.EventType == SerialData.Chars)
        //    {
        //        ComPort_DataReceived_StrBdr.Append("a");
        //    }
        //    else // SerialData.Eof
        //    {
        //        string result = ComPort_DataReceived_StrBdr.ToString();
        //        ComPort_DataReceived_StrBdr.Clear();
        //        RegisterReceived(ref result, out bool isReply);
        //    }
        //}

        public int SendCounter { private set; get; } = 0;

        internal bool Send(string cmd, bool raiseTraffic)
        {
            //tabPanel.TextOutAppendLine(" >> " + cmd, true);
            if (com.comPort.IsOpen)
            {
                com.comPort.WriteLine(cmd);
                ++SendCounter;
                if (raiseTraffic)
                {
                    RaiseTraffic?.Invoke(this, false, cmd);
                }
                return true;
            }
            else
            {
                RaiseEvent?.Invoke(this, "Com has closed!!");
            }
            return false;
        }




        public string comName
        {
            get
            {
                if (com != null)
                {
                    return com.comPort.PortName;
                }
                return null;
            }
        }

        public void Dispose()
        {
            if (com != null)
            {
                listeningFlag = false;
                //com.comPort.DataReceived -= ComPort_DataReceived;
                com.comPort.ErrorReceived -= ComPort_ErrorReceived;
                com.comPort.PinChanged -= ComPort_PinChanged;
                com.Dispose();
            }
            tabHeader = null;
            tabPanel = null;
        }
    }
}
