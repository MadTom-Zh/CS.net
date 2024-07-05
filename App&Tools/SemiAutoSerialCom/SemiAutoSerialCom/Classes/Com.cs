using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.App.Classes
{

    public class Com : IDisposable
    {
        public DGI_ComCfg cfg;
        public SerialPort comPort;

        public Com(DGI_ComCfg cfg, string portName)
        {
            this.cfg = cfg;
            comPort = new SerialPort()
            {
                PortName = portName,
                BaudRate = cfg.Speed,
                DataBits = cfg.DataBits,
                StopBits = cfg.StopBits,
                Parity = cfg.Parity,

                RtsEnable = true,
                DtrEnable = true,

                //ReadTimeout = 100  //  -1
            };
            comPort.Disposed += ComPort_Disposed;
            comPort.Open();
        }


        public bool IsOpen
        {
            get
            {
                if (comPort != null && comPort.IsOpen)
                    return true;
                return false;
            }
        }

        private void ComPort_Disposed(object? sender, EventArgs e)
        {
            comPort = null;
        }
        public void Dispose()
        {
            comPort?.Dispose();
        }
    }
}
