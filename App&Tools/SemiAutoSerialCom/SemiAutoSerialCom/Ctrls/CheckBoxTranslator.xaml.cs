using System;
using System.Collections.Generic;
using System.IO;
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
using static MadTomDev.App.Core;

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for CheckBoxTranslator.xaml
    /// </summary>
    public partial class CheckBoxTranslator : UserControl
    {
        public CheckBoxTranslator()
        {
            InitializeComponent();
        }

        public event Action<CheckBoxTranslator> CheckChanged;
        private void cb_CheckChanged(object sender, RoutedEventArgs e)
        {
            CheckChanged?.Invoke(this);
        }
        public bool IsChecked
        {
            get => cb.IsChecked == true;
            set => cb.IsChecked = value;
        }
        public TranslatorInfo TranslatorInfo;
        public string TextTitle
        {
            get => tbTitle.Text;
            set => tbTitle.Text = value;
        }
        public string TextContent
        {
            get => tbContent.Text;
            set => tbContent.Text = value;
        }

    }
}
