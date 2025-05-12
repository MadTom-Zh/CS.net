using System;
using System.Collections.Generic;
using System.Drawing;
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
using Brushes = System.Windows.Media.Brushes;
using SystemColors = System.Windows.SystemColors;

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for Cell.xaml
    /// </summary>
    public partial class Cell : UserControl
    {
        public Cell()
        {
            InitializeComponent();
        }


        private bool _IsErrorFlaged = false;
        public bool IsErrorFlaged
        {
            get => _IsErrorFlaged;
            set
            {
                if (_IsErrorFlaged == value)
                {
                    return;
                }
                _IsErrorFlaged = value;
                UpdateBG();
            }
        }
        private void UpdateBG()
        {
            if (_IsErrorFlaged)
            {
                rectBG.Fill = Brushes.Orange;
            }
            else if (_IsHighlighted)
            {
                rectBG.Fill = SystemColors.GradientActiveCaptionBrush;
            }
            else
            {
                rectBG.Fill = SystemColors.ControlBrush;
            }
        }


        private bool _IsHighlighted = false;
        public bool IsHighlighted
        {
            get => _IsHighlighted;
            set
            {
                if (_IsHighlighted == value)
                {
                    return;
                }
                _IsHighlighted = value;
                UpdateBG();
            }
        }

        private bool _IsLocked = false;
        public bool IsLocked
        {
            get => _IsLocked;
            set
            {
                if (_IsLocked == value)
                {
                    return;
                }
                _IsLocked = value;
                if (_IsLocked)
                {
                    tbvChar.FontWeight = FontWeights.Bold;
                }
                else
                {
                    tbvChar.FontWeight = FontWeights.Normal;
                }
            }
        }


        private bool _IsChecked = false;
        public bool IsChecked
        {
            get => _IsChecked;
            set
            {
                if (_IsChecked == value)
                {
                    return;
                }
                _IsChecked = value;
                bdrChecked.Visibility = _IsChecked ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        private bool _IsEnabled = true;
        public new bool IsEnabled
        {
            get => _IsEnabled;
            set
            {
                if (_IsEnabled == value)
                {
                    return;
                }
                this.IsEnabled = value;
                _IsEnabled = value;
                if (_IsEnabled)
                {
                    tbvChar.Foreground = SystemColors.ControlTextBrush;
                }
                else
                {
                    tbvChar.Foreground = SystemColors.GrayTextBrush;
                }
            }
        }

        public string Text
        {
            get => tbvChar.Text;
            set
            {
                if (_IsLocked)
                {
                    throw new InvalidOperationException("Locked!");
                }
                tbvChar.Text = value;
            }
        }
    }
}
