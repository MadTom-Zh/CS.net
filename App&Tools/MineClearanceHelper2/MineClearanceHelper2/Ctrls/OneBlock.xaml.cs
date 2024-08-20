using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MadTomDev.App.Classes;

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for OneBlock.xaml
    /// </summary>
    public partial class OneBlock : UserControl
    {
        public OneBlock()
        {
            InitializeComponent();
        }
        public BlockMatrix.OneBlockData blockData;
        public static double LEN_SIDE = 10;

        private BlockMatrix.OneBlockData.Status _DetectStatus;
        public BlockMatrix.OneBlockData.Status DetectStatus
        {
            get
            {
                return _DetectStatus;
            }               
            
        }
        public void SetDetectStatus(BlockMatrix.OneBlockData.Status newStatus,bool backSetToData = false)
        {
            _DetectStatus = newStatus;
            switch (_DetectStatus)
            {
                case BlockMatrix.OneBlockData.Status.Ground:
                    Set_Skin(Colors.Gray, true, Colors.Black, "", false);
                    break;

                case BlockMatrix.OneBlockData.Status.MineSignal_Lv1:
                    Set_Skin(Colors.Silver, false, Colors.Blue, "1", false);
                    break;
                case BlockMatrix.OneBlockData.Status.MineSignal_Lv2:
                    Set_Skin(Colors.Silver, false, Colors.Green, "2", false);
                    break;
                case BlockMatrix.OneBlockData.Status.MineSignal_Lv3:
                    Set_Skin(Colors.Silver, false, Colors.Red, "3", false);
                    break;
                case BlockMatrix.OneBlockData.Status.MineSignal_Lv4:
                    Set_Skin(Colors.GreenYellow, false, Colors.Black, "4", false);
                    break;
                case BlockMatrix.OneBlockData.Status.MineSignal_Lv5:
                    Set_Skin(Colors.Yellow, false, Colors.Black, "5", false);
                    break;
                case BlockMatrix.OneBlockData.Status.MineSignal_Lv6:
                    Set_Skin(Colors.Orange, false, Colors.Black, "6", false);
                    break;
                case BlockMatrix.OneBlockData.Status.MineSignal_Lv7:
                    Set_Skin(Colors.OrangeRed, false, Colors.Black, "7", true);
                    break;
                case BlockMatrix.OneBlockData.Status.MineSignal_Lv8:
                    Set_Skin(Colors.Black, false, Colors.OrangeRed, "8", true);
                    break;

                case BlockMatrix.OneBlockData.Status.Deteced_Save:
                    Set_Skin(Colors.Green, true, Colors.White, ":)", true);
                    break;
                case BlockMatrix.OneBlockData.Status.Deteced_Mine:
                    Set_Skin(Colors.Firebrick, true, Colors.LightYellow, "*", true);
                    break;
                case BlockMatrix.OneBlockData.Status.Deteced_Unknow:
                    Set_Skin(Colors.DodgerBlue, true, Colors.White, "?", true);
                    break;
            }
            if (backSetToData && blockData != null)
            {
                blockData.SetStatus(newStatus, false);
            }
        }
        public void Set_Skin(Color skinClr, bool isLightFromUpLeft, Color textClr, string text, bool isTextBold)
        {
            Brush bs_skin = new SolidColorBrush(skinClr);
            polygon_leftUp.Fill
                = polygon_middle.Fill
                = polygon_rightDown.Fill
                = bs_skin;
            if (isLightFromUpLeft == true)
            {
                polygon_leftUp.Opacity = 0.4;
                polygon_middle.Opacity = 0.7;
                polygon_rightDown.Opacity = 1;
            }
            else
            {
                polygon_leftUp.Opacity = 1;
                polygon_middle.Opacity = 0.7;
                polygon_rightDown.Opacity = 0.4;
            }

            Brush bs_text = new SolidColorBrush(textClr);
            textBlock.Text = text;
            textBlock.Foreground = bs_text;
            if (isTextBold == true)
            {
                textBlock.FontWeight = FontWeights.Bold;
            }
            else
            {
                textBlock.FontWeight = FontWeights.Normal;
            }
        }

        public event EventHandler Call_Detect;
        public event EventHandler Call_Dispose;

        public void Call_iDetect()
        {
            if (Call_Detect != null)
            {
                Call_Detect(this, new EventArgs());
            }
        }
        public void Call_iDispose()
        {
            if (Call_Dispose != null)
            {
                Call_Dispose(this, new EventArgs());
            }
        }

        public void Set_SUp()
        {
            int statusCode = (int)DetectStatus;
            if (statusCode < 8)
            {
                SetDetectStatus((BlockMatrix.OneBlockData.Status)(statusCode + 1),true);
            }
            else
            {
                SetDetectStatus(BlockMatrix.OneBlockData.Status.MineSignal_Lv1,true);
            }
        }
        public void Set_SDown()
        {
            int statusCode = (int)DetectStatus;
            if (statusCode > 0 && statusCode < 9)
            {
                SetDetectStatus((BlockMatrix.OneBlockData.Status)(statusCode - 1),true);
            }
            else
            {
                if (Call_Dispose != null)
                {
                    Call_Dispose(this, new EventArgs());
                }
            }
        }

        public OneBlock Clone()
        {
            OneBlock clone = new OneBlock()
            {
                _DetectStatus = this._DetectStatus,
            };
            return clone;
        }
    }
}
