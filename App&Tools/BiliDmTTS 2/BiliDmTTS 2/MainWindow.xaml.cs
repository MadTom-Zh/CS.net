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

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Core core;
        private void Window_Initialized(object sender, EventArgs e)
        {
        }
        private void Window_Loaded(object sender, EventArgs e)
        {
            core = new Core(this);

            dg_replacement.ItemsSource = core.db.replacements;
            dg_blackUser.ItemsSource = core.db.blackUsers;
            dg_blackWords.ItemsSource = core.db.blackWords;
            dg_blackPins.ItemsSource = core.db.blackPins;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            core.MainWindowSavePosition();
            core.SaveSettingsFromUI();
            core.Dispose();
        }



        #region log back colors

        public enum LogTypes
        {
            Enter, Msg,Gift, Sys,Other, Warning,Voice,
        }

        private Brush _bsh_logEnter = null;
        public Brush bsh_logEnter
        {
            get
            {
                if (_bsh_logEnter == null)
                    _bsh_logEnter = core.TryGetBrush(Core.setting_flag_logEnter, Brushes.LightGreen);
                return _bsh_logEnter;
            }
        }
        private Brush _bsh_logMsg = null;
        public Brush bsh_logMsg
        {
            get
            {
                if (_bsh_logMsg == null)
                    _bsh_logMsg = core.TryGetBrush(Core.setting_flag_logMsg, Brushes.White);
                return _bsh_logMsg;
            }
        }
        private Brush _bsh_logGift = null;
        public Brush bsh_logGift
        {
            get
            {
                if (_bsh_logGift == null)
                    _bsh_logGift = core.TryGetBrush(Core.setting_flag_logGift, Brushes.Yellow);
                return _bsh_logGift;
            }
        }
        private Brush _bsh_logSys = null;
        public Brush bsh_logSys
        {
            get
            {
                if (_bsh_logSys == null)
                    _bsh_logSys = core.TryGetBrush(Core.setting_flag_logSys, Brushes.LightBlue);
                return _bsh_logSys;
            }
        }
        private Brush _bsh_logOther = null;
        public Brush bsh_logOther
        {
            get
            {
                if (_bsh_logOther == null)
                    _bsh_logOther = core.TryGetBrush(Core.setting_flag_logOther, Brushes.LightGray);
                return _bsh_logOther;
            }
        }
        private Brush _bsh_logWarning = null;
        public Brush bsh_logWarning
        {
            get
            {
                if (_bsh_logWarning == null)
                    _bsh_logWarning = core.TryGetBrush(Core.setting_flag_logWarning, Brushes.Orange);
                return _bsh_logWarning;
            }
        }
        private Brush _bsh_logVoice = null;
        public Brush bsh_logVoice
        {
            get
            {
                if (_bsh_logVoice == null)
                    _bsh_logVoice = core.TryGetBrush(Core.setting_flag_logVoice, Brushes.LightPink);
                return _bsh_logVoice;
            }
        }
        #endregion
        public void BulletLog(string textLine, LogTypes logType)
        {
            if (string.IsNullOrWhiteSpace(textLine))
                return;
            textLine = DateTime.Now.ToString("HH:mm:ss") + " " + textLine;
            Dispatcher.Invoke(() =>
            {
                TextRange tr = new TextRange(
                    rtb_log.Document.ContentStart,
                    rtb_log.Document.ContentEnd);
                if (textLine.Length + tr.Text.Length > 5000)
                {
                    dm_newLogPage();
                }
                Paragraph pr = new Paragraph();
                pr.Inlines.Add(textLine);
                switch (logType)
                {
                    case LogTypes.Enter: pr.Background = bsh_logEnter; break;
                    case LogTypes.Gift: pr.Background = bsh_logGift; break;
                    case LogTypes.Msg: pr.Background = bsh_logMsg; break;
                    case LogTypes.Other: pr.Background = bsh_logOther; break;
                    case LogTypes.Sys: pr.Background = bsh_logSys; break;
                    case LogTypes.Voice: pr.Background = bsh_logVoice; break;
                    case LogTypes.Warning: pr.Background = bsh_logWarning; break;
                }                
                rtb_log.Document.Blocks.Add(pr);
                rtb_log.ScrollToEnd();
            });
        }


        private int dm_newLogPage_pageNo = 1;
        public void dm_newLogPage()
        {
            Button newPageBtn = new Button()
            {
                Content = "Pg" + (dm_newLogPage_pageNo++).ToString(),
                Width = 36,
                Tag = rtb_log.Document,
            };
            newPageBtn.Click += NewPageBtn_Click;
            sp_logPages.Children.Add(newPageBtn);

            rtb_log.Document = new FlowDocument();
        }

        private void NewPageBtn_Click(object sender, RoutedEventArgs e)
        {
            Button ctrl = (Button)sender;
            WindowStaticViewLogPage logWin = new WindowStaticViewLogPage();
            logWin.Title += " - " + ctrl.Content.ToString();
            logWin.rtb.Document = (FlowDocument)ctrl.Tag;
            logWin.Show();
            sp_logPages.Children.Remove(ctrl);
        }

        private void btn_dm_logPagesClear_Click(object sender, RoutedEventArgs e)
        {
            sp_logPages.Children.Clear();
        }


        private void btn_dm_log_Click(object sender, RoutedEventArgs e)
        {
            core.tabLog_btnLog_Clicked();
        }
        private void btn_dm_staticView_Click(object sender, RoutedEventArgs e)
        {
            WindowStaticViewLogPage logWin = new WindowStaticViewLogPage();

            //TextRange textRange = new TextRange(rtb_log.Document.ContentStart, rtb_log.Document.ContentEnd);
            //MemoryStream ms = new MemoryStream();
            //textRange.Save(ms, DataFormats.Rtf);

            object doc = rtb_log.Document;
            rtb_log.Document = new FlowDocument();

            //string rtgTx = Encoding.Default.GetString(ms.ToArray());

            logWin.Show();

            //FlowDocument fd = new FlowDocument();
            //MemoryStream ms1 = new MemoryStream(Encoding.ASCII.GetBytes(rtgTx));
            //TextRange textRange1 = new TextRange(fd.ContentStart, fd.ContentEnd);
            //ms.Position = 0;
            //textRange1.Load(ms1, DataFormats.Rtf);
            logWin.rtb.Document = (FlowDocument)doc;

        }
        private void btn_dm_clearVoiceQueue_Click(object sender, RoutedEventArgs e)
        {
            core.tabLog_btnClearVoiceQueue_Clicked();
        }
        private void btn_dm_clear_Click(object sender, RoutedEventArgs e)
        {
            rtb_log.Document.Blocks.Clear();
        }


        private void tb_dgCellEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            tb_dgCellEditor.Visibility = Visibility.Collapsed;
        }

        private void tb_dgCellEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //switch (e.Key)
            //{
            //    case Key.Escape:
            //        tb_dgCellEditor.Visibility = Visibility.Collapsed;
            //        break;
            //        case Key.Enter
            //}
        }


        private DataGrid dataGrid_editing = null;

        #region replacement, black-word, black-pin

        private void btn_replace_down_Click(object sender, RoutedEventArgs e)
        {
            if (dg_replacement.SelectedItem != null)
            {
                DataBase.ReplacementItem selected = (DataBase.ReplacementItem)dg_replacement.SelectedItem;
                core.db.MoveDownReplacement(selected);
            }
        }

        private void btn_replace_up_Click(object sender, RoutedEventArgs e)
        {
            if (dg_replacement.SelectedItem != null)
            {
                DataBase.ReplacementItem selected = (DataBase.ReplacementItem)dg_replacement.SelectedItem;
                core.db.MoveUpReplacement(selected);
            }
        }
        private void btn_blackWrod_up_Click(object sender, RoutedEventArgs e)
        {
            if (dg_blackWords.SelectedItem != null)
            {
                DataBase.ValueRemarkItem selected = (DataBase.ValueRemarkItem)dg_blackWords.SelectedItem;
                core.db.MoveUpBlackWord(selected);
            }
        }

        private void btn_blackWrod_down_Click(object sender, RoutedEventArgs e)
        {
            if (dg_blackWords.SelectedItem != null)
            {
                DataBase.ValueRemarkItem selected = (DataBase.ValueRemarkItem)dg_blackWords.SelectedItem;
                core.db.MoveDownBlackWord(selected);
            }
        }

        private void btn_blackPin_up_Click(object sender, RoutedEventArgs e)
        {
            if (dg_blackPins.SelectedItem != null)
            {
                DataBase.ValueRemarkItem selected = (DataBase.ValueRemarkItem)dg_blackPins.SelectedItem;
                core.db.MoveUpBlackPin(selected);
            }
        }

        private void btn_blackPin_down_Click(object sender, RoutedEventArgs e)
        {
            if (dg_blackPins.SelectedItem != null)
            {
                DataBase.ValueRemarkItem selected = (DataBase.ValueRemarkItem)dg_blackPins.SelectedItem;
                core.db.MoveDownBlackPin(selected);
            }
        }

            #endregion

            private void btn_test_Click(object sender, RoutedEventArgs e)
        {
            core.TestMsg(tb_testUser.Text, tb_testMsg.Text);
        }

        private void cb_enableReplace_CheckChanged(object sender, RoutedEventArgs e)
        {

        }

        private void cb_enableFilter_blackUser_CheckChanged(object sender, RoutedEventArgs e)
        {

        }

        private void cb_enableFilter_blackWords_CheckChanged(object sender, RoutedEventArgs e)
        {

        }

        private void cb_enableFilter_blackPins_CheckChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
