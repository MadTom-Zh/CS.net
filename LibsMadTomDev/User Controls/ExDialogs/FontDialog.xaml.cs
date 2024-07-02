using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for FontDialog.xaml
    /// </summary>
    public partial class FontDialog : Window
    {
        public FontDialog()
        {
            InitializeComponent();
            InitData();
        }

        private void InitData()
        {
            foreach (FontFamily ff in Fonts.SystemFontFamilies)
            {
                fontFamilyVMList.Add(new FontFamilyVM()
                { text = ff.FamilyNames.Values.FirstOrDefault(), fontFamily = ff, });
            }
            listBox_fontFamily.ItemsSource = fontFamilyVMList;
            Type fontWeightsType = typeof(FontWeights);
            foreach (PropertyInfo fwPi in fontWeightsType.GetProperties())
            {
                fontWeightVMList.Add(new FontWeightVM()
                { text = fwPi.Name, fontWeight = (FontWeight)fwPi.GetValue(null), });
            }
            listBox_fontWeight.ItemsSource = fontWeightVMList;
            listBox_fontFamily.SelectedItem = fontFamilyVMList[0];

            listBox_fontWeight.SelectedItem = fontWeightVMList[0];

            tb_sample.FontFamily = SettingFontFamily;
            tb_sample.FontWeight = SettingFontWeight;
            tb_sample.FontSize = (double)numericUpDown_fontSize.Value;
        }


        public  FontFamily SettingFontFamily
        {
            get => ((FontFamilyVM)listBox_fontFamily.SelectedItem).fontFamily;
            set
            {
                listBox_fontFamily.SelectedItem = fontFamilyVMList.Find(
                    a => a.text.ToLower() == value.Source.ToLower());
                listBox_fontFamily.ScrollIntoView(listBox_fontFamily.SelectedItem);
            }
        }

        public FontWeight SettingFontWeight
        {
            get => ((FontWeightVM)listBox_fontWeight.SelectedItem).fontWeight;
            set
            {
                listBox_fontWeight.SelectedItem = fontWeightVMList.Find(
                    a => a.fontWeight.Equals(value));
                listBox_fontWeight.ScrollIntoView(listBox_fontWeight.SelectedItem);
            }
        }
        public double SettingFontSize
        {
            get => (double)numericUpDown_fontSize.Value;
            set
            {
                numericUpDown_fontSize.Value = (decimal)value;
            }
        }

        public TextDecorationCollection SettingTextDecoration
        {
            get
            {
                if (toggleButton_baseline.IsChecked == true)
                    return TextDecorations.Baseline;
                else if (toggleButton_overline.IsChecked == true)
                    return TextDecorations.OverLine;
                else if (toggleButton_underline.IsChecked == true)
                    return TextDecorations.Underline;
                else if (toggleButton_strikethrough.IsChecked == true)
                    return TextDecorations.Strikethrough;
                else
                    return null;
            }
            set
            {
                toggleButton_baseline.IsChecked = false;
                toggleButton_overline.IsChecked = false;
                toggleButton_strikethrough.IsChecked = false;
                toggleButton_underline.IsChecked = false;

                if (value == TextDecorations.Baseline)
                {
                    toggleButton_baseline.IsChecked = true;
                    tb_sample.TextDecorations = TextDecorations.Baseline;
                }
                else if (value == TextDecorations.OverLine)
                {
                    toggleButton_overline.IsChecked = true;
                    tb_sample.TextDecorations = TextDecorations.OverLine;
                }
                else if (value == TextDecorations.Strikethrough)
                {
                    toggleButton_strikethrough.IsChecked = true;
                    tb_sample.TextDecorations = TextDecorations.Strikethrough;
                }
                else if (value == TextDecorations.Underline)
                {
                    toggleButton_underline.IsChecked = true;
                    tb_sample.TextDecorations = TextDecorations.Underline;
                }
                else
                {
                    tb_sample.TextDecorations = null;
                }
            }
        }

        public FontStyle SettingFontStyle
        {
            get
            {
                if (toggleButton_italic.IsChecked == true)
                    return FontStyles.Italic;
                else if (toggleButton_oblique.IsChecked == true)
                    return FontStyles.Oblique;
                else
                    return FontStyles.Normal;
            }
            set
            {
                toggleButton_italic.IsChecked = false;
                toggleButton_oblique.IsChecked = false;

                if (value == FontStyles.Italic)
                {
                    toggleButton_italic.IsChecked = true;
                    tb_sample.FontStyle = FontStyles.Italic;
                }
                else if (value == FontStyles.Oblique)
                {
                    toggleButton_oblique.IsChecked = true;
                    tb_sample.FontStyle = FontStyles.Oblique;
                }
                else
                {
                    tb_sample.FontStyle = FontStyles.Normal;
                }

            }
        }


        List<FontFamilyVM> fontFamilyVMList = new List<FontFamilyVM>();
        List<FontWeightVM> fontWeightVMList = new List<FontWeightVM>();
        public class FontFamilyVM
        {
            public string text { set; get; }
            public FontFamily fontFamily { set; get; }
        }
        public class FontWeightVM
        {
            public string text { set; get; }
            public FontWeight fontWeight { set; get; }
        }

        private void listBox_fontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox_fontFamily.SelectedItem == null)
                return;
            tb_sample.FontFamily = ((FontFamilyVM)listBox_fontFamily.SelectedItem).fontFamily;
        }

        private void listBox_fontWeight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tb_sample.FontWeight = ((FontWeightVM)listBox_fontWeight.SelectedItem).fontWeight;
        }

        private void numericUpDown_fontSize_ValueChanged(NumericUpDown sender)
        {
            if (tb_sample == null)
                return;
            tb_sample.FontSize = (double)numericUpDown_fontSize.Value;
        }

        private void toggleButton_textDecoration_Checked(object sender, RoutedEventArgs e)
        {
            if (toggleButton_baseline != sender)
                toggleButton_baseline.IsChecked = false;
            if (toggleButton_underline != sender)
                toggleButton_underline.IsChecked = false;
            if (toggleButton_strikethrough != sender)
                toggleButton_strikethrough.IsChecked = false;
            if (toggleButton_overline != sender)
                toggleButton_overline.IsChecked = false;
            tb_sample.TextDecorations = SettingTextDecoration;
        }

        private void toggleButton_textDecoration_Unchecked(object sender, RoutedEventArgs e)
        {
            tb_sample.TextDecorations = SettingTextDecoration;
        }

        private void toggleButton_fontStyle_Checked(object sender, RoutedEventArgs e)
        {
            if (toggleButton_italic != sender)
                toggleButton_italic.IsChecked = false;
            if (toggleButton_oblique != sender)
                toggleButton_oblique.IsChecked = false;
            tb_sample.FontStyle = SettingFontStyle;
        }

        private void toggleButton_fontStyle_Unchecked(object sender, RoutedEventArgs e)
        {
            tb_sample.FontStyle = SettingFontStyle;
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
