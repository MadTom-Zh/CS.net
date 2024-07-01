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

using MadTomDev.UI;

namespace MadTomDev.Resources.T2S_HCore.UI
{
    /// <summary>
    /// Interaction logic for MultiVoicePanel.xaml
    /// </summary>
    public partial class MultiVoicePanel : UserControl
    {
        public MultiVoicePanel()
        {
            InitializeComponent();
        }


        private BroadCaster bc = BroadCaster.GetInstance();
        private MultiVoice mv = MultiVoice.GetInstance();
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // load combox voices\
            cbb_voices.Items.Clear();
            foreach (BroadCaster.Voice v in bc.Voices)
            {
                cbb_voices.Items.Add(v.description);
            }
            cbb_voices.SelectionChanged += Cbb_voices_SelectionChanged;
            cbb_voices.DropDownClosed += Cbb_voices_DropDownClosed;

            // load combox languages
            cbb_langs.Items.Clear();
            foreach (MultiVoice.LanguageType lang in Enum.GetValues(typeof(MultiVoice.LanguageType)))
            {
                cbb_langs.Items.Add(lang);
            }
            cbb_langs.SelectionChanged += Cbb_langs_SelectionChanged;
            cbb_langs.DropDownClosed += Cbb_langs_DropDownClosed;

            // rate N' volume
            sld.ValueChanged += Sld_ValueChanged;
            sld.LostFocus += Sld_LostFocus;

            dg_voiceLib.ItemsSource = dataGridSource;
            ReloadVoiceLib();

            mv.MVSpeekEnd += Mv_MVSpeekEnd;
            bc.SaveWave_FStream_Complete += Bc_SaveWave_FStream_Complete;
        }


        private void Mv_MVSpeekEnd(object? sender, BroadCaster.StreamEnd_Args e)
        {
            btn_read.Content = "播放";
            this.IsEnabled = true;
        }
        private void Bc_SaveWave_FStream_Complete(object? sender, EventArgs e)
        {
            this.IsEnabled = true;
        }
        private void ReloadVoiceLib()
        {
            dataGridSource.Clear();
            int pri = 1;
            foreach (MultiVoice.VoiceLanguageLib.VLLItem vllItem in mv.voiceLangLib.lib)
            {
                dataGridSource.Add(new DGVoiceItem(vllItem)
                {
                    priority = pri++,
                });
            }
        }

        public class DGVoiceItem : INotifyPropertyChanged
        {
            public MultiVoice.VoiceLanguageLib.VLLItem voiceItem;
            public DGVoiceItem(MultiVoice.VoiceLanguageLib.VLLItem voiceItem)
            {
                this.voiceItem = voiceItem;
            }
            private int _priority = -1;
            public int priority
            {
                get => _priority;
                set
                {
                    _priority = value;
                    OnPropertyChanged("priority");
                }
            }
            public string voiceName
            {
                get => voiceItem.VoiceName;
                set
                {
                    voiceItem.VoiceName = value;
                    OnPropertyChanged("voiceName");
                }
            }
            public MultiVoice.LanguageType language
            {
                get => voiceItem.Language;
                set
                {
                    voiceItem.Language = value;
                    OnPropertyChanged("language");
                }
            }
            public int rate
            {
                get => voiceItem.CustomSpeed;
                set
                {
                    voiceItem.CustomSpeed = value;
                    OnPropertyChanged("rate");
                }
            }
            public int volume
            {
                get => voiceItem.CustomVolume;
                set
                {
                    voiceItem.CustomVolume = value;
                    OnPropertyChanged("volume");
                }
            }
            public bool available
            {
                get => voiceItem.Avaliable;
            }
            public event PropertyChangedEventHandler PropertyChanged;
            protected internal virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<DGVoiceItem> dataGridSource = new ObservableCollection<DGVoiceItem>();

        private DGVoiceItem? dg_voiceLib_editingItem;
        private int dg_voiceLib_editingColIdx;
        private void dg_voiceLib_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
            DataGridCell cell = (DataGridCell)e.Column.GetCellContent(e.Row).Parent;
            Point cellPt = cell.PointToScreen(this.PointFromScreen(new Point()));
            dg_voiceLib_editingItem = (DGVoiceItem)e.Row.Item;

            dg_voiceLib_editingColIdx = e.Column.DisplayIndex;
            if (dg_voiceLib_editingColIdx == 1) // voice
            {
                bdr_cbb_voices.Margin = new Thickness(
                    cellPt.X - 4, // left
                    cellPt.Y - 3, // top
                    0, 0);
                bdr_cbb_voices.Height = cell.ActualHeight + 8;
                bdr_cbb_voices.Width = cell.ActualWidth + 6;
                bdr_cbb_voices.Visibility = Visibility.Visible;
                cbb_voices.SelectedItem = dg_voiceLib_editingItem.voiceName;
                cbb_voices.IsDropDownOpen = true;
            }
            else if (dg_voiceLib_editingColIdx == 2) // lang
            {
                bdr_cbb_langs.Margin = new Thickness(
                    cellPt.X - 4, // left
                    cellPt.Y - 3, // top
                    0, 0);
                bdr_cbb_langs.Height = cell.ActualHeight + 8;
                bdr_cbb_langs.Width = cell.ActualWidth + 6;
                bdr_cbb_langs.Visibility = Visibility.Visible;
                cbb_langs.SelectedItem = dg_voiceLib_editingItem.language;
                cbb_langs.IsDropDownOpen = true;
            }
            else if (dg_voiceLib_editingColIdx == 3) // rate
            {
                bdr_sld.Margin = new Thickness(
                    cellPt.X, // left
                    cellPt.Y, // top
                    0, 0);
                bdr_sld.Height = cell.ActualHeight;
                bdr_sld.Width = cell.ActualWidth;
                double curValue = dg_voiceLib_editingItem.rate;
                sld.Minimum = -10;
                sld.Maximum = 10;
                sld.Value = curValue;
                bdr_sld.Visibility = Visibility.Visible;
                sld.Focus();
            }
            else if (dg_voiceLib_editingColIdx == 4) // volume
            {
                bdr_sld.Margin = new Thickness(
                    cellPt.X, // left
                    cellPt.Y, // top
                    0, 0);
                bdr_sld.Height = cell.ActualHeight;
                bdr_sld.Width = cell.ActualWidth;
                double curValue = dg_voiceLib_editingItem.volume;
                sld.Minimum = 0;
                sld.Maximum = 100;
                sld.Value = curValue;
                bdr_sld.Visibility = Visibility.Visible;
                sld.Focus();
            }
        }
        private void Cbb_voices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_voiceLib_editingItem != null)
            {
                dg_voiceLib_editingItem.voiceName = cbb_voices.SelectedItem.ToString();
                mv.voiceLangLib.AddUpdateLibItem(dg_voiceLib_editingItem.voiceItem);
            }
        }
        private void Cbb_voices_DropDownClosed(object? sender, EventArgs e)
        {
            dg_voiceLib_editingItem = null;
            bdr_cbb_voices.Visibility = Visibility.Collapsed;
        }
        private void Cbb_langs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_voiceLib_editingItem != null)
            {
                dg_voiceLib_editingItem.language
                    = (MultiVoice.LanguageType)Enum.Parse(typeof(MultiVoice.LanguageType), cbb_langs.SelectedItem.ToString());
                mv.voiceLangLib.AddUpdateLibItem(dg_voiceLib_editingItem.voiceItem);
            }
        }
        private void Cbb_langs_DropDownClosed(object? sender, EventArgs e)
        {
            dg_voiceLib_editingItem = null;
            bdr_cbb_langs.Visibility = Visibility.Collapsed;
        }
        private void dg_voiceLib_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (bdr_sld.Visibility == Visibility.Visible)
            {
                Sld_SetToDataGrid();
                dg_voiceLib_editingItem = null;
                bdr_sld.Visibility = Visibility.Collapsed;
            }
        }
        private void Sld_ValueChanged(NumericSlider sender)
        {
            Sld_SetToDataGrid();
        }
        private void Sld_LostFocus(object sender, RoutedEventArgs e)
        {
            dg_voiceLib_editingItem = null;
            bdr_sld.Visibility = Visibility.Collapsed;
        }

        private void Sld_SetToDataGrid()
        {
            if (dg_voiceLib_editingItem != null)
            {
                if (dg_voiceLib_editingColIdx == 3) // rate
                {
                    dg_voiceLib_editingItem.rate
                        = (int)(sld.Value + 0.5);
                    mv.voiceLangLib.AddUpdateLibItem(dg_voiceLib_editingItem.voiceItem);
                }
                else if (dg_voiceLib_editingColIdx == 4) // volume
                {
                    dg_voiceLib_editingItem.volume
                        = (int)(sld.Value + 0.5);
                    mv.voiceLangLib.AddUpdateLibItem(dg_voiceLib_editingItem.voiceItem);
                }
            }

            //dg_voiceLib_editingItem = null;
            //bdr_sld.Visibility = Visibility.Collapsed;
        }
        private void dg_voiceLib_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && dg_voiceLib.SelectedItem != null)
            {
                DGVoiceItem itemToDel = (DGVoiceItem)dg_voiceLib.SelectedItem;
                mv.voiceLangLib.RemoveLibItem(itemToDel.voiceItem);
                dataGridSource.Remove(itemToDel);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (mv.isPlaying)
                mv.Stop();
            mv.voiceLangLib.SaveLib();
        }



        #region btn new, moveUp, moveDown

        private void btn_new_Click(object sender, RoutedEventArgs e)
        {
            if (mv.voiceLangLib.AddIdle())
            {
                ReloadVoiceLib();
            }
            else
            {
                MessageBox.Show("已经没有未分配的语音了。", "信息", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private void btn_moveUp_Click(object sender, RoutedEventArgs e)
        {
            if (dg_voiceLib.SelectedItem != null)
            {
                DGVoiceItem selected = (DGVoiceItem)dg_voiceLib.SelectedItem;
                mv.voiceLangLib.MoveItemUp(selected.voiceName);
                int sIdx = dataGridSource.IndexOf(selected);
                if (sIdx > 0)
                {
                    ReloadVoiceLib();
                    dg_voiceLib.SelectedIndex = sIdx - 1;
                    dg_voiceLib.Focus();

                    //dataGridSource.Remove(selected);
                    //dataGridSource.Insert(sIdx - 1, selected);
                    //dg_voiceLib.SelectedItem = selected;
                    //dg_voiceLib.Focus();
                    //dataGridSource[sIdx - 1].priority = sIdx;
                    //dataGridSource[sIdx].priority = sIdx + 1;  // 不起作用！！？？？
                }
            }
        }
        private void btn_moveDown_Click(object sender, RoutedEventArgs e)
        {
            if (dg_voiceLib.SelectedItem != null)
            {
                DGVoiceItem selected = (DGVoiceItem)dg_voiceLib.SelectedItem;
                mv.voiceLangLib.MoveItemDown(selected.voiceName);
                int sIdx = dataGridSource.IndexOf(selected);
                if (sIdx < dataGridSource.Count - 1)
                {
                    ReloadVoiceLib();
                    dg_voiceLib.SelectedIndex = sIdx + 1;
                    dg_voiceLib.Focus();

                    //dataGridSource.Remove(selected);
                    //dataGridSource.Insert(sIdx + 1, selected);
                    //dg_voiceLib.SelectedItem = selected;
                    //dg_voiceLib.Focus();
                    //dataGridSource[sIdx].priority = sIdx + 1;  // 不起作用！！？？？
                    //dataGridSource[sIdx + 1].priority = sIdx + 2;
                }
            }
        }

        #endregion


        #region auto read clipBoard

        private System.Threading.Timer timer_autoReadClipboard;
        private string? timer_autoReadCB_Tick_text = null;
        private void cb_autoReadCB_Checked(object sender, RoutedEventArgs e)
        {
            if (timer_autoReadClipboard == null)
            {
                timer_autoReadClipboard = new System.Threading.Timer(timer_autoReadClipboard_Tick);
            }
            timer_autoReadCB_Tick_text = Clipboard.GetText().Trim();
            timer_autoReadClipboard.Change(0, 50);
        }
        private void cb_autoReadCB_Unchecked(object sender, RoutedEventArgs e)
        {
            timer_autoReadClipboard.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            mv.Stop();
        }
        private void timer_autoReadClipboard_Tick(object? s)
        {
            Dispatcher.Invoke(() =>
            {
                string gotText = Clipboard.GetText().Trim();
                if (gotText.Length == 0) return;
                if (timer_autoReadCB_Tick_text != gotText)
                {
                    timer_autoReadCB_Tick_text = gotText;
                    mv.Stop();
                    tb_test.Text = gotText;
                    btn_read_Click(btn_read, new RoutedEventArgs());
                }
            });
        }

        #endregion


        #region btn saveSegs, saveOne, read

        private Microsoft.Win32.SaveFileDialog saveFileDialog;
        private void btn_saveSegs_Click(object sender, RoutedEventArgs e)
        {
            TryInitSaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                string filename = saveFileDialog.FileName;
                if (filename.ToLower().EndsWith(".wav"))
                {
                    filename = filename.Substring(0, filename.Length - 4) + "_";
                }
                else filename += "_";
                this.IsEnabled = false;
                mv.Speek2WaveMultiFiles(tb_test.Text, filename);
                System.Media.SystemSounds.Asterisk.Play();
            }
        }

        private void btn_saveOne_Click(object sender, RoutedEventArgs e)
        {
            TryInitSaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                string filename = saveFileDialog.FileName;
                this.IsEnabled = false;
                bc.SaveWave_FStream_MultiLangInOne(MultiVoice.TextMultiFragments.Detach(tb_test.Text), mv.voiceLangLib, filename);
                System.Media.SystemSounds.Asterisk.Play();
            }
        }
        private void TryInitSaveFileDialog()
        {
            if (saveFileDialog == null)
            {
                saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "Wave File|*.wav";
            }
        }

        private void btn_read_Click(object sender, RoutedEventArgs e)
        {
            if (mv.isPlaying)
            {
                mv.Stop();
                btn_read.Content = "朗读";
            }
            else
            {
                mv.voiceLangLib.SaveLib();
                mv.Speek(tb_test.Text);
                btn_read.Content = "停止";
            }
        }

        #endregion
    }
}
