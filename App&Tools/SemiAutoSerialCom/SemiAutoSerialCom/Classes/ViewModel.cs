using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.App.Classes
{
    public class DGI_ComCfg : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private string _Name;
        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                RaisePropertyChanged("Name");
            }
        }

        private int _Speed;
        public int Speed
        {
            get => _Speed;
            set
            {
                _Speed = value;
                SpeedTx = value.ToString();
            }
        }
        private string _SpeedTx;
        public string SpeedTx
        {
            get => _SpeedTx;
            set
            {
                _SpeedTx = value;
                RaisePropertyChanged("SpeedTx");
            }
        }

        private int _DataBits;
        public int DataBits
        {
            get => _DataBits;
            set
            {
                _DataBits = value;
                DataBitsTx = value.ToString();
            }
        }
        private string _DataBitsTx;
        public string DataBitsTx
        {
            get => _DataBitsTx;
            set
            {
                _DataBitsTx = value;
                RaisePropertyChanged("DataBitsTx");
            }
        }

        private StopBits _StopBits = StopBits.One;
        public StopBits StopBits
        {
            get => _StopBits;
            set
            {
                _StopBits = value;
                StopBitsTx = value.ToString();
            }
        }
        private string _StopBitsTx;
        public string StopBitsTx
        {
            get => _StopBitsTx;
            set
            {
                _StopBitsTx = value;
                RaisePropertyChanged("StopBitsTx");
            }
        }

        private Parity _Parity = Parity.None;
        public Parity Parity
        {
            get => _Parity;
            set
            {
                _Parity = value;
                ParityTx = value.ToString();
            }
        }
        private string _ParityTx;
        public string ParityTx
        {
            get => _ParityTx;
            set
            {
                _ParityTx = value;
                RaisePropertyChanged("ParityTx");
            }
        }

        private FlowControl _FlowControl = FlowControl.Next;
        public FlowControl FlowControl
        {
            get => _FlowControl;
            set
            {
                _FlowControl = value;
                FlowControlTx = value.ToString();
            }
        }
        private string _FlowControlTx;
        public string FlowControlTx
        {
            get => _FlowControlTx;
            set
            {
                _FlowControlTx = value;
                RaisePropertyChanged("FlowControlTx");
            }
        }

        private int _QTimeOut;
        public int QTimeOut
        {
            get => _QTimeOut;
            set
            {
                _QTimeOut = value;
                QTimeOutTx = value.ToString();
            }
        }
        private string _QTimeOutTx;
        public string QTimeOutTx
        {
            get => _QTimeOutTx;
            set
            {
                _QTimeOutTx = value;
                RaisePropertyChanged("QTimeOutTx");
            }
        }

        public List<string> translatorList = new List<string>();
    }
}
