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
using System.Windows.Navigation;
using System.Windows.Shapes;

using MadTomDev.Common;

namespace MadTomDev.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            exColorPanel.InitFromStaticCore();
            isIniting = false;
        }
        private bool isIniting = true;
        private async void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            if (e.OriginalSource == e.Source && e.Source is TabControl)
            {
                TabControl tc = (TabControl)e.Source;
                if (tc == tabControl_root)
                {
                    switch (((TabItem)tc.SelectedItem).Header.ToString())
                    {
                        case "Color":
                            if (TryInitColorsInited)
                                break;
                            this.Cursor = Cursors.Wait;
                            await TryInitColors();
                            this.Cursor = Cursors.Arrow;
                            break;
                        case "Icon":
                            if (TryInitIconsInited)
                                break;
                            this.Cursor = Cursors.Wait;
                            await TryInitIcons();
                            this.Cursor = Cursors.Arrow;
                            break;
                        case "SystemSound":
                            if (TryInitSystemSoundsInited)
                                break;
                            this.Cursor = Cursors.Wait;
                            await TryInitSystemSounds();
                            this.Cursor = Cursors.Arrow;
                            break;
                        case "CharCode":
                            if (TryInitCharCodesInited)
                                break;
                            this.Cursor = Cursors.Wait;
                            await TryInitCharCodes();
                            this.Cursor = Cursors.Arrow;
                            break;
                        case "Languages":
                            if (TryInitLanguagesInited)
                                break;
                            this.Cursor = Cursors.Wait;
                            await TryInitLanguages();
                            this.Cursor = Cursors.Arrow;
                            break;
                        case "CRC":
                            if (TryInitCRCInited)
                                break;
                            this.Cursor = Cursors.Wait;
                            await TryInitCRC();
                            this.Cursor = Cursors.Arrow;
                            break;
                    }
                }
            }
        }

        #region colors

        private bool TryInitColorsInited = false;
        private Task TryInitColors()
        {
            TryInitColorsInited = true;

            return Task.Run(() =>
            {

                Ctrls.ColorNameARGB clrItem;

                Type SystemColorsType = typeof(SystemColors);
                Type colorType = typeof(Color);
                Type resourceKeyType = typeof(ResourceKey);

                Brush b = null;
                SolidColorBrush scb;
                StringBuilder strBdr = new StringBuilder();
                Type ColorsType = typeof(Colors);

                Dispatcher.Invoke(() =>
                {
                    wPanel_systemColors.Children.Clear();
                    wPanel_colors.Children.Clear();
                    foreach (PropertyInfo clrInfo in SystemColorsType.GetProperties())
                    {
                        AddColorToPanel(wPanel_systemColors, clrInfo, true);
                    }
                    foreach (PropertyInfo clrInfo in ColorsType.GetProperties())
                    {
                        AddColorToPanel(wPanel_colors, clrInfo, false);
                    }

                    void AddColorToPanel(WrapPanel container, PropertyInfo colorPI, bool isSystemColor)
                    {
                        if (isSystemColor)
                        {
                            if (colorPI.PropertyType != colorType && colorPI.PropertyType != resourceKeyType)
                            {
                                b = (Brush)colorPI.GetValue(null);
                            }
                            else return;
                        }
                        else
                        {
                            if (colorPI.PropertyType == colorType)
                            {
                                b = new SolidColorBrush((Color)colorPI.GetValue(null));
                            }
                            else return;
                        }

                        clrItem = new Ctrls.ColorNameARGB()
                        {
                            Brush = b,
                        };
                        strBdr.Clear();
                        strBdr.Append(colorPI.Name);
                        strBdr.AppendLine();
                        if (b is SolidColorBrush)
                        {
                            scb = (SolidColorBrush)b;
                            strBdr.Append(scb.Color.ToString());
                            clrItem.Click += (s1, e1) =>
                            {
                                exColorPanel.WorkingColor = ((SolidColorBrush)((Ctrls.ColorNameARGB)s1).bdrColor.Background).Color;
                            };
                        }
                        else
                        {
                            strBdr.Append("*Not solid color");
                        }
                        clrItem.Text = strBdr.ToString();
                        container.Children.Add(clrItem);
                    }
                });
            });
        }

        private void btn_colorFilterClear_Click(object sender, RoutedEventArgs e)
        {
            tb_colorFilter.Text = "";
        }

        private void tb_colorFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            string key = tb_colorFilter.Text.Trim().ToLower();
            foreach (Ctrls.ColorNameARGB cp in wPanel_systemColors.Children)
            {
                if (cp.Text.ToLower().Contains(key))
                    cp.Visibility = Visibility.Visible;
                else
                    cp.Visibility = Visibility.Collapsed;
            }
            foreach (Ctrls.ColorNameARGB cp in wPanel_colors.Children)
            {
                if (cp.Text.ToLower().Contains(key))
                    cp.Visibility = Visibility.Visible;
                else
                    cp.Visibility = Visibility.Collapsed;
            }
        }
        #endregion


        #region icons
        private bool TryInitIconsInited = false;
        private Task TryInitIcons()
        {
            TryInitIconsInited = true;

            return Task.Run(() =>
            {
                Common.IconHelper.SystemIcons iconHelperS = Common.IconHelper.SystemIcons.Instance;
                Common.IconHelper.Shell32Icons iconHelper32 = Common.IconHelper.Shell32Icons.Instance;
                Ctrls.IconInfo iconCtrl;
                BitmapSource test;
                Dispatcher.Invoke(() =>
                {
                    wPanel_iconSystem.Children.Clear();
                    foreach (string key in iconHelperS.dictIcon.Keys)
                    {
                        iconCtrl = new Ctrls.IconInfo()
                        {
                            IconBig = iconHelperS[key, true],
                            IconSmall = iconHelperS[key, false],
                            Text = key,
                        };
                        wPanel_iconSystem.Children.Add(iconCtrl);
                    }

                    wPanel_iconShell32.Children.Clear();
                    for (int i = 0; i < 1000; i++)
                    {
                        test = iconHelper32.GetIcon(i, true);
                        if (test == null)
                            break;
                        iconCtrl = new Ctrls.IconInfo()
                        {
                            IconBig = test,
                            IconSmall = iconHelper32.GetIcon(i, false),
                            Text = i.ToString(),
                        };
                        wPanel_iconShell32.Children.Add(iconCtrl);
                    }
                });
            });
        }

        #endregion


        #region sounds
        private bool TryInitSystemSoundsInited = false;
        private Task TryInitSystemSounds()
        {
            TryInitSystemSoundsInited = true;

            return Task.Run(() =>
            {

                Type ssType = typeof(System.Media.SystemSounds);
                Type sType = typeof(System.Media.SystemSound);
                Button btn;
                Thickness marg = new Thickness(4);
                Dispatcher.Invoke(() =>
                {
                    wPanel_systemSounds.Children.Clear();
                    foreach (PropertyInfo pi in ssType.GetProperties())
                    {
                        if (pi.PropertyType == sType)
                        {
                            btn = new Button()
                            {
                                Margin = marg,
                                Width = 120,
                                Height = 50,
                                Content = pi.Name,
                                Tag = pi.GetValue(null),
                            };
                            btn.Click += (s1, e1) =>
                            {
                                ((System.Media.SystemSound)((Button)s1).Tag).Play();
                            };
                            wPanel_systemSounds.Children.Add(btn);
                        }
                    }
                });
            });
        }
        #endregion


        #region charcodes
        private bool TryInitCharCodesInited = false;
        private Task TryInitCharCodes()
        {
            TryInitCharCodesInited = true;


            wPanel_charCodes.Children.Clear();
            timerCharCodes = new System.Threading.Timer(timerCharCodesTick);
            timerCharCodes.Change(500, 500);

            return Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    wPanel_keyCodes.Children.Clear();
                    Ctrls.CharValue cv;
                    foreach (Key k in Enum.GetValues(typeof(Key)))
                    {
                        if (dictKeyUI.ContainsKey(k))
                            continue;

                        cv = new Ctrls.CharValue()
                        { TextChar = k.ToString(), TextValue = ((int)k).ToString(), };
                        wPanel_keyCodes.Children.Add(cv);
                        dictKeyUI.Add(k, cv);
                    }
                });
            });
        }
        private Dictionary<Key, Ctrls.CharValue> dictKeyUI = new Dictionary<Key, Ctrls.CharValue>();
        private Dictionary<Key, Ctrls.CharValue> dictKeyUI_pressed = new Dictionary<Key, Ctrls.CharValue>();
        private void tb_input_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key k;
            if (e.Key == Key.System)
            {
                k = e.SystemKey;
                e.Handled = true;
            }
            else
            {
                k = e.Key;
            }
            if (dictKeyUI_pressed.ContainsKey(k))
                return;
            if (dictKeyUI.ContainsKey(k))
            {
                Ctrls.CharValue cv = dictKeyUI[k];
                cv.SetSelected(true);
                dictKeyUI_pressed.Add(k, cv);
            }
        }
        private void tb_input_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            Key k;
            if (e.Key == Key.System)
                k = e.SystemKey;
            else
                k = e.Key;
            if (dictKeyUI_pressed.ContainsKey(k))
            {
                Ctrls.CharValue cv = dictKeyUI_pressed[k];
                cv.SetSelected(false);
                dictKeyUI_pressed.Remove(k);
            }
        }

        private void tb_input_TextChanged(object sender, TextChangedEventArgs e)
        {
            timerCharCodesSkip = false;
            timerCharCodesTimePre = DateTime.Now;

        }
        private System.Threading.Timer timerCharCodes;
        private bool timerCharCodesSkip = false;
        private DateTime timerCharCodesTimePre = DateTime.MaxValue;
        private void timerCharCodesTick(object s)
        {
            if (timerCharCodesSkip)
                return;

            if ((DateTime.Now - timerCharCodesTimePre).TotalMilliseconds > 400)
            {
                timerCharCodesSkip = true;
                ReShowCharCodes();
            }
        }
        private Task ReShowCharCodes()
        {
            return Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    string tx = tb_input.Text;
                    wPanel_charCodes.Children.Clear();
                    Ctrls.CharValue cv;
                    foreach (char c in tx)
                    {
                        cv = new Ctrls.CharValue()
                        { TextChar = c.ToString(), TextValue = ((int)c).ToString(), IsUIChar = true, };
                        wPanel_charCodes.Children.Add(cv);
                    }
                });
            });
        }
        #endregion


        #region pinyin
        private void tbPinyinSource_TextChanged(object sender, TextChangedEventArgs e)
        {
            pinyinSource = tbPinyinSource.Text;
            GetNewPinYin();
        }
        private string pinyinSource;
        private bool GetNewPinYin_working = false;
        private void GetNewPinYin()
        {
            if (isIniting)
                return;

            if (GetNewPinYin_working)
                return;
            GetNewPinYin_working = true;

            int caseStyle; // 0-all lower, 1-first upper, 2-all upper;
            int lengthStyle; // 0-only first, 1-full;
            if (rbAllLower.IsChecked == true) caseStyle = 0;
            else if (rbFirstUpper.IsChecked == true) caseStyle = 1;
            else caseStyle = 2;
            if (rbFirst.IsChecked == true) lengthStyle = 0;
            else lengthStyle = 1;

            string separ = tbPinYinSeparator.Text;

            Task.Run(() =>
            {
                string curPinyin, result;
                while (true)
                {
                    curPinyin = pinyinSource;
                    //Pinyin

                    result = TinyPinyin.PinyinHelper.GetPinyin(curPinyin);

                    if (curPinyin != pinyinSource)
                        continue;
                    else
                        break;
                }
                if (result != null)
                {
                    result = result.Replace("\r \n", "\r\n");
                    result = result.Replace(" \r", "\r");
                    result = result.Replace("\n ", "\n");
                    StringBuilder strBdr = new StringBuilder();
                    if (lengthStyle == 0)
                    {
                        char c;
                        bool justC = false;
                        for (int i = 0, iv = result.Length; i < iv; ++i)
                        {
                            c = result[i];

                            if ('A' <= c && c <= 'Z')
                            {
                                if (justC)
                                    continue;

                                justC = true;
                                strBdr.Append(c);
                            }
                            else
                            {
                                justC = false;
                                strBdr.Append(c);
                            }
                        }
                    }
                    else
                    {
                        strBdr.Append(result);
                    }
                    result = strBdr.ToString();
                    strBdr.Clear();
                    switch (caseStyle)
                    {
                        case 0: // all lower
                            result = result.ToLower();
                            break;
                        case 1: // first upper
                            char c;
                            bool justC = false;
                            for (int i = 0, iv = result.Length; i < iv; ++i)
                            {
                                c = result[i];

                                if ('A' <= c && c <= 'Z')
                                {
                                    if (justC)
                                    {
                                        strBdr.Append(c.ToString().ToLower());
                                    }
                                    else
                                    {
                                        justC = true;
                                        strBdr.Append(c);
                                    }
                                }
                                else
                                {
                                    justC = false;
                                    strBdr.Append(c);
                                }
                            }
                            result = strBdr.ToString();
                            break;
                        default:
                        case 2: // all upper
                            break;
                    }
                    if (separ != null)
                        result = result.Replace(" ", separ);
                    else
                        result = result.Replace(" ", "");
                }
                Dispatcher.Invoke(() =>
                {
                    tbPinYinTarget.Text = result;
                });
                GetNewPinYin_working = false;
            });
        }

        private void rbPyChecked(object sender, RoutedEventArgs e)
        {
            GetNewPinYin();
        }

        private void tbPinYinSeparator_textChanged(object sender, TextChangedEventArgs e)
        {
            GetNewPinYin();
        }
        #endregion


        #region languages

        private bool TryInitLanguagesInited = false;
        private List<DGI_language> dataGrid_languages_source = new List<DGI_language>();
        private Task TryInitLanguages()
        {
            TryInitLanguagesInited = true;

            //this.Cursor = Cursors.AppStarting;
            dataGrid_languages.ItemsSource = null;
            dataGrid_languages.Items.Clear();
            dataGrid_languages.Items.Add(new DGI_language() { Name = "Loading...", });
            return Task.Run(() =>
            {
                dataGrid_languages_source.Clear();
                System.Globalization.CultureInfo[] ciArray
                    = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);

                Dispatcher.Invoke(() =>
                {
                    foreach (System.Globalization.CultureInfo ci in ciArray)
                    {
                        dataGrid_languages_source.Add(new DGI_language()
                        {
                            LCID = ci.LCID.ToString(),
                            Name = ci.Name,
                            DisplayName = ci.DisplayName,
                            NativeNameName = ci.NativeName,
                            EnglishName = ci.EnglishName,
                            IsNeutral = ci.IsNeutralCulture,
                            IsReadOnly = ci.IsReadOnly,
                            Parent = ci.Parent.Name,
                            Types = ci.CultureTypes.ToString().Replace(Environment.NewLine, ", "),
                            //DateTimeFormat = ci.DateTimeFormat.FullDateTimePattern,
                            //NumberFormat = $"{ci.NumberFormat.CurrencyPositivePattern} | {ci.NumberFormat.PercentPositivePattern}",
                            KeyboardLayoutId = ci.KeyboardLayoutId.ToString(),
                            leftLanguageTag = ci.IetfLanguageTag,
                        });
                    }
                    dataGrid_languages.Items.Clear();
                    dataGrid_languages.ItemsSource = dataGrid_languages_source;
                    //this.Cursor = Cursors.Arrow;
                });
            });
        }
        #endregion

        #region CRC

        private bool TryInitCRCInited = false;
        private Task TryInitCRC()
        {
            TryInitCRCInited = true;
            Data.CommonCrcParameters.Reload();

            return Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    for (int i = wp_crcModels.Children.Count - 1; i >= 2; --i)
                    {
                        UIElement ui = wp_crcModels.Children[i];
                        if (ui is RadioButton)
                        {
                            ((RadioButton)ui).Checked -= rb_crcModels_Checked;
                        }
                        wp_crcModels.Children.RemoveAt(i);
                    }

                    foreach (string mName in Data.CommonCrcParameters.GetParametersModelNames())
                    {
                        RadioButton newRb = new RadioButton()
                        {
                            Content = mName,
                            Tag = Data.CommonCrcParameters.GetParameters(mName),
                            Margin = new Thickness(4, 1, 4, 0),
                        };
                        newRb.Checked += rb_crcModels_Checked;

                        wp_crcModels.Children.Add(newRb);
                    }
                });
            });
        }
        private void rb_crcModels_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is not RadioButton
                || !TryInitCRCInited
                || grid_crcCustomChars == null)
            {
                return;
            }
            RadioButton rb = (RadioButton)sender;
            if (rb.Tag is Data.CrcParameters)
            {
                // common model
                selectedCrcPars = (Data.CrcParameters)rb.Tag;
                grid_crcCustomChars.IsEnabled = false;
                tb_crcParWidth.Text = selectedCrcPars.Width.ToString();
                tb_crcParPoly.Text = selectedCrcPars.Polynomial.ToHexString(false);
                tb_crcParInit.Text = selectedCrcPars.InitialValue.ToHexString(false);
                tb_crcParXorOut.Text = selectedCrcPars.XorOutValue.ToHexString(false);
                cb_crcParRefIn.IsChecked = selectedCrcPars.ReflectIn;
                cb_crcParRefOut.IsChecked = selectedCrcPars.ReflectOut;
            }
            else
            {
                // custom
                selectedCrcPars = null;
                grid_crcCustomChars.IsEnabled = true;
            }
            crc = null;
        }

        private void tb_crcInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (crc != null)
            {
                return;
            }

            if (selectedCrcPars == null)
            {
                try
                {
                    selectedCrcPars = new Data.CrcParameters(
                        int.Parse(tb_crcParWidth.Text),
                        Convert.ToUInt64(tb_crcParPoly.Text, 16),
                        Convert.ToUInt64(tb_crcParInit.Text, 16),
                        Convert.ToUInt64(tb_crcParXorOut.Text, 16),
                        cb_crcParRefIn.IsChecked == true,
                        cb_crcParRefOut.IsChecked == true
                    );
                    crc = new Data.Crc(selectedCrcPars);
                }
                catch (Exception err)
                {
                    MessageBox.Show(this, err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                crc = new Data.Crc(selectedCrcPars);
            }
        }

        private void tb_crcInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            CRCRestart();
        }

        private Data.CrcParameters? selectedCrcPars = null;
        private Data.Crc? crc = null;
        private string CRCInput = null;
        private bool CRCBusy = false;
        private void CRCRestart()
        {
            if (crc == null)
            {
                return;
            }

            CRCInput = tb_crcInput.Text;
            tbv_crc.IsEnabled = false;
            tbv_crcBin.IsEnabled = false;
            if (CRCBusy)
            {
                return;
            }

            CRCBusy = true;
            Task.Run(() =>
            {
                Task.Delay(300);
                string curCrcInput = CRCInput;
                List<byte[]> dataRows = GetDataRows(curCrcInput);


                StringBuilder strBdrCrc = new StringBuilder();
                StringBuilder strBdrCrcBin = new StringBuilder();
                for (int i = 0, iv = dataRows.Count; i < iv; ++i)
                {
                    if (dataRows[i].Length > 0)
                    {
                        byte[] crcValue = crc.CalculateCheckValue(dataRows[i]);
                        strBdrCrc.Append(crcValue.ToHexString().ToUpper());
                        strBdrCrcBin.Append(crcValue.ToBinString());
                    }
                    strBdrCrc.AppendLine();
                    strBdrCrcBin.AppendLine();
                }
                Dispatcher.Invoke(() =>
                {
                    tbv_crc.Text = strBdrCrc.ToString();
                    tbv_crcBin.Text = strBdrCrcBin.ToString();
                    tbv_crc.IsEnabled = true;
                    tbv_crcBin.IsEnabled = true;
                });

                CRCBusy = false;
                if (curCrcInput != CRCInput)
                {
                    CRCRestart();
                }
            });

            List<byte[]> GetDataRows(string str)
            {
                str = str.Replace("\r\n", "\n").Replace("\r", "\n");
                string[] lines = str.Split('\n');
                List<byte[]> result = new List<byte[]>();
                foreach (string l in lines)
                {
                    result.Add(GetDataRow(l));
                }
                return result;
            }
            byte[] GetDataRow(string rowStr)
            {
                int strType = 0;
                Dispatcher.Invoke(() =>
                {
                    if (rbCRCDec.IsChecked == true) strType = 10;
                    else if (rbCRCBin.IsChecked == true) strType = 2;
                    else if (rbCRCOct.IsChecked == true) strType = 8;
                    else if (rbCRCHex.IsChecked == true) strType = 16;
                    else if (rbCRCStr.IsChecked == true) strType = 0;
                });
                switch (strType)
                {
                    case 2:
                        return rowStr.TryGetBytes(SimpleValueHelper.ByteStringFormates.Bin);
                    case 8:
                        return rowStr.TryGetBytes(SimpleValueHelper.ByteStringFormates.Oct);
                    case 10:
                        return rowStr.TryGetBytes(SimpleValueHelper.ByteStringFormates.Dec);
                    case 16:
                        return rowStr.TryGetBytes(SimpleValueHelper.ByteStringFormates.Hex);
                    default:
                    case 0:
                        return Encoding.UTF8.GetBytes(rowStr);
                }
            }
        }



        #region auto scroll
        private void tb_crcInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // not working
            //tb_crcScrollSync();
            tb_crcInput_isMouseDown = false;
        }
        private void tb_crcScrollSync()
        {
            tbv_crc.ScrollToVerticalOffset(tb_crcInput.VerticalOffset);
            tbv_crcBin.ScrollToVerticalOffset(tb_crcInput.VerticalOffset);
        }

        private bool tb_crcInput_isMouseDown = false;
        private void tb_crcInput_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            tb_crcInput_isMouseDown = true;
        }

        private void tb_crcInput_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            tb_crcInput_isMouseDown = false;
        }

        private void tb_crcInput_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (tb_crcInput_isMouseDown)
            {
                tb_crcScrollSync();
            }
        }
        private void tb_crcInput_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            tb_crcScrollSync();
        }

        private async void tb_crcInput_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            await Task.Delay(50);
            tb_crcScrollSync();
        }
        #endregion

        #endregion

    }
}
