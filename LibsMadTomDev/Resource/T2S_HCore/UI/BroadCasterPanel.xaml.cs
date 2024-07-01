using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
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
    /// Interaction logic for BroadCasterPanel.xaml
    /// </summary>
    public partial class BroadCasterPanel : UserControl
    {
        public BroadCasterPanel()
        {
            InitializeComponent();
        }



        private BroadCaster broadCaster;
        private RichTextBoxScroller richTBoxScroller;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            broadCaster = new BroadCaster();
            cb_voices.Items.Clear();
            for (int i = 0; i < broadCaster.Voices.Count; i++)
            {
                cb_voices.Items.Add(broadCaster.Voices[i].description);
            }
            if (cb_voices.Items.Count > 0)
            {
                cb_voices.SelectedIndex = 0;
            }

            broadCaster.StreamStart += BroadCaster_StreamStart;
            broadCaster.StreamEnd += BroadCaster_StreamEnd;
            broadCaster.StreamEndAndJITListGend += BroadCaster_StreamEndAndJITListGend;
            broadCaster.Speeking_SentenceBoundary += BroadCaster_Speeking_SentenceBoundary;
            broadCaster.Speeking_WordBoundary += BroadCaster_Speeking_WordBoundary;
            broadCaster.Speeking_Viseme += BroadCaster_Speeking_Viseme;

            LoadImages();

            tb_path.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\T2S Packages";
            if (!Directory.Exists(tb_path.Text)) Directory.CreateDirectory(tb_path.Text);
            selectPathDlg.SelectedPath = tb_path.Text;

            richTBoxScroller = new RichTextBoxScroller(rtb);

            selectPathDlg.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            tb_path.Text = selectPathDlg.SelectedPath;
            tb_packageName.Text = "VoicePkg";
        }


        private void BroadCaster_StreamStart(BroadCaster sender)
        {
            //throw new NotImplementedException();
            btn_savePackage.IsEnabled = false;
        }
        private List<JITEvent> _completeJITEventList;
        private void BroadCaster_StreamEnd(object? sender, BroadCaster.StreamEnd_Args e)
        {
            //throw new NotImplementedException();
            btn_stop_Click(btn_stop, new RoutedEventArgs());
            //timer.Enabled = false;
        }

        private void BroadCaster_StreamEndAndJITListGend(object? sender, BroadCaster.StreamEndAndJITListGendEventArgs e)
        {
            //throw new NotImplementedException();
            _completeJITEventList = e.completeJITEventList;
            btn_savePackage.IsEnabled = true;

            if (PackageSave_saveIndx)
            {
                using (StreamWriter sw = File.AppendText(PackageSave_idxFullName))
                {
                    for (int i = 0; i < _completeJITEventList.Count; i++)
                    {
                        sw.WriteLine(_completeJITEventList[i].IOContent);
                    }
                    sw.Flush();
                }
            }
            PackageSave_saveIndx = false;
        }

        #region mic face images

        List<BitmapSource> imageList_faceAndMouth = new List<BitmapSource>();
        List<BitmapSource> imageList_eyes = new List<BitmapSource>();
        private bool imgsAvailable = true;
        private void LoadImages()
        {
            bool allFiles = true;
            try
            {
                string imgPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                string file = System.IO.Path.Combine(imgPath, "mic.png");
                imageList_faceAndMouth.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;
                file = System.IO.Path.Combine(imgPath, "mic_mouth_2.png");
                imageList_faceAndMouth.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;
                file = System.IO.Path.Combine(imgPath, "mic_mouth_3.png");
                imageList_faceAndMouth.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;
                file = System.IO.Path.Combine(imgPath, "mic_mouth_4.png");
                imageList_faceAndMouth.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;
                file = System.IO.Path.Combine(imgPath, "mic_mouth_5.png");
                imageList_faceAndMouth.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;
                file = System.IO.Path.Combine(imgPath, "mic_mouth_6.png");
                imageList_faceAndMouth.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;
                file = System.IO.Path.Combine(imgPath, "mic_mouth_7.png");
                imageList_faceAndMouth.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;
                file = System.IO.Path.Combine(imgPath, "mic_mouth_8.png");
                imageList_faceAndMouth.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;
                file = System.IO.Path.Combine(imgPath, "mic_mouth_9.png");
                imageList_faceAndMouth.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;
                file = System.IO.Path.Combine(imgPath, "mic_mouth_10.png");
                imageList_faceAndMouth.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;
                file = System.IO.Path.Combine(imgPath, "mic_mouth_11.png");
                imageList_faceAndMouth.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;
                file = System.IO.Path.Combine(imgPath, "mic_mouth_12.png");
                imageList_faceAndMouth.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;
                file = System.IO.Path.Combine(imgPath, "mic_mouth_13.png");
                imageList_faceAndMouth.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;

                file = System.IO.Path.Combine(imgPath, "mic_eyes_narrow.png");
                imageList_eyes.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;
                file = System.IO.Path.Combine(imgPath, "mic_eyes_closed.png");
                imageList_eyes.Add(new BitmapImage(new Uri(file)));
                if (!File.Exists(file)) allFiles = false;
            }
            catch (Exception err)
            {
                imgsAvailable = false;
                MessageBox.Show("MIC头像动画缺失，或载入错误");
            }
            if (!allFiles)
                MessageBox.Show("MIC头像动画缺失");
            ImageReset();
        }
        private void ImageReset()
        {
            if (imgsAvailable)
            {
                img_lips.Source = null;
                img_eyes.Source = null;
                img_base.Source = imageList_faceAndMouth[0];
            }
        }
        int imageCover_mouthIndex = -1;
        int imageCover_eyeIndex = -1;

        private void BroadCaster_Speeking_Viseme(object? sender, BroadCaster.Speeking_VisemeEventArgs e)
        {
            //return;


            //0,  // SP_VISEME_0 = 0,    // Silence
            switch (e.id)
            {
                case 1:
                case 2:
                case 3:
                case 5:
                case 11:
                    imageCover_mouthIndex = 10; break;
                case 4:
                    imageCover_mouthIndex = 9; break;
                case 6:
                    imageCover_mouthIndex = 8; break;
                case 7:
                    imageCover_mouthIndex = 1; break;
                case 8:
                    imageCover_mouthIndex = 12; break;
                case 9:
                    imageCover_mouthIndex = 8; break;
                case 10:
                    imageCover_mouthIndex = 11; break;
                case 12:
                    imageCover_mouthIndex = 8; break;
                case 13:
                    imageCover_mouthIndex = 2; break;
                case 14:
                    imageCover_mouthIndex = 5; break;
                case 15:
                    imageCover_mouthIndex = 6; break;
                case 16:
                    imageCover_mouthIndex = 7; break;
                case 17:
                    imageCover_mouthIndex = 4; break;
                case 18:
                    imageCover_mouthIndex = 3; break;
                case 19:
                    imageCover_mouthIndex = 6; break;
                case 20:
                    imageCover_mouthIndex = 8; break;
                case 21:
                default:
                    imageCover_mouthIndex = 0; break;
            }

            if (e.id % 6 == 2)
            {
                imageCover_eyeIndex = 0;
            }
            else if (e.id % 6 == 5)
            {
                imageCover_eyeIndex = 1;
            }
            else imageCover_eyeIndex = -1;

            if (imgsAvailable)
            {
                if (imageCover_mouthIndex > 0)
                {
                    img_lips.Source = imageList_faceAndMouth[imageCover_mouthIndex];
                }
                else
                {
                    img_lips.Source = null;
                }
                if (imageCover_eyeIndex >= 0)
                {
                    img_eyes.Source = imageList_eyes[imageCover_eyeIndex];
                }
                else
                {
                    img_eyes.Source = null;
                }
            }
            else
            {
                img_lips.Source = null;
                img_eyes.Source = null;
                img_base.Source = null;
            }
        }


        #endregion


        private void BroadCaster_Speeking_SentenceBoundary(object? sender, BroadCaster.Speeking_SentenceBoundaryEventArgs e)
        {
            //throw new NotImplementedException();
            //textBox_events.Text += "\r\n" + "Sentence, streamPosi [" + broadCaster.Speeking_SentenceBoundary_streamPosition + "], index [" + T2S_HCore.Speeking_SentenceBoundary_readingSentenceIndex + "], start [" + T2S_HCore.Speeking_SentenceBoundary_sentenceStartIndex + "], length [" + T2S_HCore.Speeking_SentenceBoundary_sentenceLength + "].";

        }

        private BroadCaster.Speeking_WordBoundaryEventArgs? BroadCaster_Speeking_WordBoundary_LastE;
        private void BroadCaster_Speeking_WordBoundary(object? sender, BroadCaster.Speeking_WordBoundaryEventArgs e)
        {
            //throw new NotImplementedException();
            //_richTextBoxScrooler.Set_Selection_AndScroll(e.wordStartIndex, e.wordLength);
            //textBox_events.Text += "\r\n" + "Word, streamPosi [" + broadCaster.Speeking_WorkBoundary_streamPosition + "], start [" + T2S_HCore.Speeking_WorkBoundary_wordStartIndex + "], length [" + T2S_HCore.Speeking_WorkBoundary_wordLength + "].";

            //richTBoxScroller.FindNext(e.word, out int start, out int length);
            //richTBoxScroller.Set_Selection_AndScroll(start, length);

            if (BroadCaster_Speeking_WordBoundary_LastE != null)
            {
                if (BroadCaster_Speeking_WordBoundary_LastE.wordStartIndex >= e.wordStartIndex)
                    return;
            }

            richTBoxScroller.FindNext(e.word, out TextPointer start, out TextPointer end);
            richTBoxScroller.Set_Selection_AndScroll(start, end);
            BroadCaster_Speeking_WordBoundary_LastE = e;
        }

        private void comboBox_voices_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = broadCaster.Voices.Count - 1; i >= 0; i--)
            {
                if (broadCaster.Voices[i].description == cb_voices.Text)
                {
                    broadCaster.voice = broadCaster.Voices[i];
                    break;
                }
            }
        }
        private void sld_rate_ValueChanged(NumericSlider sender)
        {
            if (broadCaster != null)
                broadCaster.Rate = (int)sld_rate.Value;
        }

        private void sld_volume_ValueChanged(NumericSlider sender)
        {
            if (broadCaster != null)
                broadCaster.Volume = (int)sld_volume.Value;
        }

        //private static string button_Play_Cap_Play = "_Play";
        //private static string button_Play_Cap_Pause = "&Pause";
        private void btn_read_Click(object sender, RoutedEventArgs e)
        {
            //if (broadCaster.isPlaying)
            //{
            //    broadCaster.Pause();
            //    button_Play.Text = button_Play_Cap_Play;
            //}
            //else if (broadCaster.isPaused)
            //{
            //    broadCaster.Resume();
            //    button_Play.Text = button_Play_Cap_Pause;
            //}
            //else
            //{
            //    richTextBox.Focus();
            //    broadCaster.Speek(richTextBox.Text);
            //    button_Play.Text = button_Play_Cap_Pause;
            //}

            btn_savePackage.IsEnabled = false;
            BroadCaster_Speeking_WordBoundary_LastE = null;

            rtb.Focus();
            try
            {
                broadCaster.Speek(RtbText);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            //button_Play.Text = button_Play_Cap_Pause;
            btn_read.IsEnabled = false;
        }
        private string RtbText
        {
            get
            {
                TextRange allText = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                return allText.Text;
            }
            set
            {
                TextRange allText = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                allText.Text = value;
            }
        }
        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            btn_savePackage.IsEnabled = false;
            broadCaster.Stop();
            rtb.SelectAll();
            rtb.Focus();
            //button_Play.Text = button_Play_Cap_Play;
            btn_read.IsEnabled = true;
            richTBoxScroller.FindReset();
        }

        private Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
        private void btn_saveWav_Click(object sender, RoutedEventArgs e)
        {
            saveFileDialog.Filter = "Wave file|*.wav|All Files|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveWav(saveFileDialog.FileName);
            }
        }
        private async void SaveWav(string fullPath)
        {
            //broadCaster.SaveWave(richTextBox.Text, fullPath);

            this.Cursor = Cursors.Wait;
            await Task.Run(() => { broadCaster.SaveWave_FStream(RtbText, fullPath); });
            this.Cursor = Cursors.Arrow;
        }


        private Ookii.Dialogs.Wpf.VistaFolderBrowserDialog selectPathDlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

        private void btn_selectPath_Click(object sender, RoutedEventArgs e)
        {
            if (selectPathDlg.ShowDialog() == true)
            {
                tb_path.Text = selectPathDlg.SelectedPath;
            }
        }
        private void btn_savePackage_Click(object sender, RoutedEventArgs e)
        {
            if (PackageExists(tb_path.Text, tb_packageName.Text))
            {
                if (MessageBox.Show("Package already Exists, Cancel Processing!?",
                    "Warnning!",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning)
                    == MessageBoxResult.Yes)
                {
                    return;
                }
                else PackageDel(tb_path.Text, tb_packageName.Text);
            }
            PackageSave(tb_path.Text, tb_packageName.Text);

            SystemSounds.Asterisk.Play();
        }
        public bool PackageExists(string storPath, string packName)
        {
            return Directory.Exists(storPath + "\\" + packName);
        }
        internal void SaveRtf(string fileFullName)
        {
            using (Stream fileStream = File.OpenWrite(fileFullName))
            {
                new TextRange(
                    rtb.Document.ContentStart,
                    rtb.Document.ContentEnd)
                        .Save(fileStream, DataFormats.Rtf);

                fileStream.Flush();
            }
        }
        internal void LoadRtf(string fileFullName)
        {
            FileInfo fi = new FileInfo(fileFullName);

            using (Stream fileStream = File.OpenRead(fileFullName))
            {
                new TextRange(
                    rtb.Document.ContentStart,
                    rtb.Document.ContentEnd)
                        .Load(fileStream, DataFormats.Rtf);
            }
        }
        private bool PackageSave_saveIndx = false;
        private string PackageSave_idxFullName;
        public void PackageSave(string storPath, string packName)
        {
            string packPath = storPath + "\\" + packName;
            PackageSave_saveIndx = true;
            PackageSave_idxFullName = packPath + "\\indx.txt";
            Directory.CreateDirectory(packPath);
            SaveRtf(packPath + "\\doc.rtf");
            SaveWav(packPath + "\\vic.wav");
        }
        public void PackageDel(string storPath, string packName)
        {
            Directory.Delete(storPath + "\\" + packName, true);
        }

        private Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
        private void btn_loadRTF_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() == true)
            {
                broadCaster.Stop();
                LoadRtf(openFileDialog.FileName);
            }
        }

        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            if (broadCaster.isPlaying)
            {
                btn_stop_Click(btn_stop, new RoutedEventArgs());
            }
        }

        private System.Threading.Timer timer_autoReadCB;
        private void cb_autoReadCB_Checked(object sender, RoutedEventArgs e)
        {
            if (timer_autoReadCB == null)
            {
                timer_autoReadCB = new System.Threading.Timer(timer_autoReadCB_Tick);
            }

            btn_stop_Click(btn_stop, new RoutedEventArgs());
            timer_autoReadCB_Tick_text = Clipboard.GetText();
            timer_autoReadCB.Change(0, 50);
        }
        private void cb_autoReadCB_Unchecked(object sender, RoutedEventArgs e)
        {
            timer_autoReadCB.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            btn_stop_Click(btn_stop, new RoutedEventArgs());
        }
        private string timer_autoReadCB_Tick_text;
        private void timer_autoReadCB_Tick(object? s)
        {
            Dispatcher.Invoke(() =>
            {
                string gotText = Clipboard.GetText().Trim();
                if (gotText.Length == 0) return;
                if (timer_autoReadCB_Tick_text != gotText)
                {
                    timer_autoReadCB_Tick_text = gotText;
                    btn_stop_Click(btn_stop, new RoutedEventArgs());
                    RtbText = gotText;
                    btn_read_Click(btn_read, new RoutedEventArgs());
                }
            });
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            broadCaster.Stop();
        }

    }
}
