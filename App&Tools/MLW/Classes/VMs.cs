using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Text;
using System.Windows.Media.Imaging;

namespace MLW_Succubus_Storys.Classes
{
    public class VMChatItem
    {
        public string TxCharName { set; get; }
        public string TxMsg { set; get; }

        /// <summary>
        /// do NOT set
        /// </summary>
        public HorizontalTextAlignment TxAlign { set; get; }


        /// <summary>
        /// do NOT set
        /// </summary>
        public double GridLeftWidth { set; get; }

        /// <summary>
        /// do NOT set
        /// </summary>
        public double GridRightWidth { set; get; }

        private BitmapSource _ImgLeft { set; get; }
        private BitmapSource _ImgRight { set; get; }
        public BitmapSource ImgLeft
        {
            set
            {
                _ImgRight = null;
                GridRightWidth = 0;
                _ImgLeft = value;
                if (value != null)
                {
                    GridLeftWidth = 42;
                    TxAlign = HorizontalTextAlignment.Left;
                }
            }
            get => _ImgLeft;
        }
        public BitmapSource ImgRight
        {
            set
            {
                _ImgLeft = null;
                GridLeftWidth = 0;
                _ImgRight = value;
                if (value != null)
                {
                    GridRightWidth = 42;
                    TxAlign = HorizontalTextAlignment.Right;
                }
            }
            get => _ImgRight;
        }
    }
}
