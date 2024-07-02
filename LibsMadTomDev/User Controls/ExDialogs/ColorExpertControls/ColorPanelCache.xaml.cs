using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MadTomDev.UI.ColorExpertControls
{
    /// <summary>
    /// Interaction logic for ColorPanelCache.xaml
    /// </summary>
    public partial class ColorPanelCache : UserControl
    {
        public ColorPanelCache()
        {
            InitializeComponent();
        }

        private Class.ColorExpertCore core;
        private BitmapSource chessboardImg;
        private ImageBrush chessboardBrush;
        public void InitFromStaticCore()
        {
            Init(Class.ColorExpertCore.GetInstance());
        }
        public void Init(Class.ColorExpertCore core)
        {
            this.core = core;
            chessboardImg = QuickGraphics.Image.GetChessboardImage(30, 30);
            chessboardBrush = new ImageBrush(chessboardImg)
            { TileMode = TileMode.Tile, Viewport = new Rect(0, 0, 0.5, 1) };

            btnVMList.Clear();
            Color curClr;
            for (int i = 0, iv = core.workingColorList.Count; i < iv; i++)
            {
                curClr = core.workingColorList[i];
                btnVMList.Add(new RadioButtonVM()
                {
                    parentList = btnVMList,
                    index = i,
                    Text = (i + 1).ToString("00"),
                    isChecked = i == core.WorkingColorIndex,
                    chessboardBrush = chessboardBrush,
                    background = curClr,
                    foreground = core.GetBetterForeColor(curClr),
                    SelectChanged = new Action<int>(ClrBtnCheckChanged),
                });
            }
            itemsControl.ItemsSource = btnVMList;

            core.WorkingColorChanged -= ColorMgr_WorkingColorChanged;
            core.WorkingColorChanged += ColorMgr_WorkingColorChanged;
        }
            
        private void ClrBtnCheckChanged(int newIndex)
        {
            core.WorkingColorIndex = newIndex;
        }
        private void ColorMgr_WorkingColorChanged(object sender, int workingColorIndex, object changer)
        {
            RadioButtonVM rb = btnVMList[workingColorIndex];
            Color curClr = core.WorkingColor;
            rb.background = curClr;
            rb.foreground = core.GetBetterForeColor(curClr);
        }

        ObservableCollection<RadioButtonVM> btnVMList = new ObservableCollection<RadioButtonVM>();
        public class RadioButtonVM : INotifyPropertyChanged
        {
            public ObservableCollection<RadioButtonVM> parentList;
            public int index;
            private string _Text;
            public string Text
            {
                get => _Text;
                set
                {
                    _Text = value;
                    RaisePropertyChanged(nameof(Text));
                }
            }

            public ImageBrush chessboardBrush { set; get; }

            private Color _background;
            public Color background
            {
                get => _background;
                set
                {
                    _background = value;
                    RaisePropertyChanged(nameof(background));
                    backgroundBrush = new SolidColorBrush(value);
                }
            }
            private SolidColorBrush _backgroundBrush;
            public SolidColorBrush backgroundBrush
            {
                get => _backgroundBrush;
                set
                {
                    _backgroundBrush = value;
                    RaisePropertyChanged(nameof(backgroundBrush));
                }
            }
            private Color _foreground;
            public Color foreground
            {
                get => _foreground;
                set
                {
                    _foreground = value;
                    RaisePropertyChanged(nameof(foreground));
                    foregroundBrush = new SolidColorBrush(value);
                }
            }
            private SolidColorBrush _foregroundBrush;
            public SolidColorBrush foregroundBrush
            {
                get => _foregroundBrush;
                set
                {
                    _foregroundBrush = value;
                    RaisePropertyChanged(nameof(foregroundBrush));
                }
            }

            private bool _isChecked;
            public bool isChecked
            {
                get => _isChecked;
                set
                {
                    _isChecked = value;
                    RaisePropertyChanged(nameof(isChecked));
                    if (value)
                    {
                        foreach (RadioButtonVM r in parentList)
                        {
                            if (r != this)
                            {
                                r.isChecked = false;
                            }
                        }
                        SelectChanged?.Invoke(index);
                    }
                }
            }
            public Action<int> SelectChanged;

            public event PropertyChangedEventHandler PropertyChanged;
            private void RaisePropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
