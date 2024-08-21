using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MadTomDev.Demo.Ctrls
{
    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : UserControl, INotifyPropertyChanged
    {
        public Tile()
        {
            InitializeComponent();
        }

        public object tagPlayer;

        private Class.MapData.Seat _Seat;
        public Class.MapData.Seat Seat
        {
            get => _Seat;
            set
            {
                _Seat = value;
                Canvas.SetLeft(this, value.x);
                Canvas.SetTop(this, value.y);
                this.Width = value.width;
                this.Height = value.height;
            }
        }
        public Action<Tile> actionTileMouseDown;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private BitmapSource _ImgBG;
        public BitmapSource ImgBG
        {
            get => _ImgBG;
            set
            {
                _ImgBG = value;
                img_bg.Source = value;
                RaisePropertyChanged("ImgBG");
            }
        }
        private string _PatternName;
        public string PatternName
        {
            get => _PatternName;
            set
            {
                ImgPattern = Core.Instance.imgMgr.imgDict_tilePaterns[value];
                _PatternName = value;
            }
        }
        private BitmapSource _ImgPattern;
        public BitmapSource ImgPattern
        {
            get => _ImgPattern;
            set
            {
                _ImgPattern = value;
                img_pattern.Source = value;
                RaisePropertyChanged("ImgPattern");
            }
        }

        private bool _IsEnabled = true;
        public new bool IsEnabled
        {
            get => _IsEnabled;
            set
            {
                _IsEnabled = value;
                RaisePropertyChanged("ImgPattern");
                if (value)
                    MaskGrayVisibility = Visibility.Collapsed;
                else
                    MaskGrayVisibility = Visibility.Visible;
            }
        }
        private Visibility _MaskGrayVisibility = Visibility.Collapsed;
        public Visibility MaskGrayVisibility
        {
            get => _MaskGrayVisibility;
            set
            {
                _MaskGrayVisibility = value;
                rect_gray.Visibility = value;
                RaisePropertyChanged("MaskGrayVisibility");
            }
        }



        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled)
                actionTileMouseDown?.Invoke(this);
        }
    }
}
