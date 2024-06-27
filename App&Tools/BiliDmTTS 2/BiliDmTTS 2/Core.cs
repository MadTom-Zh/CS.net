using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using MadTomDev.Common;
using MadTomDev.Data;

using MadTomDev.Resources.T2S_HCore;
using MadTomDev.UI;

namespace MadTomDev.App
{
    class Core : IDisposable
    {
        public MainWindow mainWindow = null;
        private Logger logger;
        private MillisecondTimer timeTimer;
        private SettingsTxt settings;
        public DataBase db;
        private MultiVoice voicer;
        public BiliDmEngin.Engin dmEngin;

        public Core(MainWindow mainWindow)
        {

            logger = new Logger()
            {
                BaseDir = Common.Variables.IOPath.LogDir,
                BaseFileNamePre = "BiliDM2",
            };
            settings = new SettingsTxt()
            {
                SettingsFileFullName = Path.Combine(Common.Variables.IOPath.SettingDir, "BiliDM2.txt"),
            };
            db = new DataBase();

            this.mainWindow = mainWindow;

            MainWindowLoadPosition();
            LoadSettingsToUI();

            timeTimer = MillisecondTimer.GetInstance();
            timeTimer.LoopType = MillisecondTimer.LoopTypes.Wait8ms;
            timeTimer.Interval = MillisecondTimer.Intervals.ms500;
            timeTimer.Tick += TimeTimer_Tick;

            voicer = MultiVoice.GetInstance();

            dmEngin = new BiliDmEngin.Engin(logger);
            dmEngin.Connected += DmEngin_Connected;
            dmEngin.Disconnected += DmEngin_Disconnected;
            dmEngin.NewBullet += DmEngin_NewBullet;

            if (int.TryParse(settings[setting_flag_roomId], out int rId))
                dmEngin.RoomID = rId;
            ReConnect();
            VoiceLoopStart();
        }


        #region settings <==> UI

        private static string setting_flag_roomId = "roomId";

        private static string setting_flag_dm_showRead_msg = "dm_showRead_msg";
        private static string setting_flag_dm_showRead_gift = "dm_showRead_gift";
        private static string setting_flag_dm_showRead_userEnter = "dm_showRead_userEnter";
        private static string setting_flag_dm_showRead_system = "dm_showRead_system";
        private static string setting_flag_dm_showRead_other = "dm_showRead_other";
        private static string setting_flag_dm_show_voice = "dm_show_voice";

        private static string setting_flag_pre_changeToHalfSmbs = "pre_changeToHalfSmbs";
        private static string setting_flag_pre_removeSpaces = "pre_removeSpaces";

        private static string setting_flag_basic_ignoreNumSmb = "basic_ignoreNumSmb";
        private static string setting_flag_basic_ignoreNumEnd = "basic_ignoreNumEnd";
        private static string setting_flag_basic_ignoreShort = "basic_ignoreShort";
        private static string setting_flag_basic_short = "basic_short";
        private static string setting_flag_basic_ignore5chars = "basic_ignore5chars";
        private static string setting_flag_basic_ignore3words = "basic_ignore3words";

        private static string setting_flag_duplicated_enabled = "duplicated_enabled";
        private static string setting_flag_duplicated_queueLength = "duplicated_queueLength";
        private static string setting_flag_duplicated_minSimpleLength = "duplicated_minSimpleLength";

        private static string setting_flag_voice_readUserName = "voice_readUserName";
        private static string setting_flag_voice_queueMax = "voice_queueMax";
        private static string setting_flag_voice_interval = "voice_interval";


        private bool? GetCheckBoxIschecked(CheckBox ctrl)
        {
            bool? result = null;
            try
            {
                ctrl.Dispatcher.Invoke(() =>
                { result = ctrl.IsChecked; });
            }
            catch (Exception)
            { }
            return result;
        }
        private int GetNSValue(NumericSlider ctrl)
        {
            int result = -1;
            ctrl.Dispatcher.Invoke(() => { result = (int)ctrl.Value; });
            return result;
        }
        public bool? dm_showRead_msg { get => GetCheckBoxIschecked(mainWindow.cb_msg); }
        public bool? dm_showRead_gift { get => GetCheckBoxIschecked(mainWindow.cb_gift); }
        public bool? dm_showRead_userEnter { get => GetCheckBoxIschecked(mainWindow.cb_enter); }
        public bool? dm_showRead_system { get => GetCheckBoxIschecked(mainWindow.cb_system); }
        public bool? dm_showRead_other { get => GetCheckBoxIschecked(mainWindow.cb_other); }
        public bool? dm_show_voice { get => GetCheckBoxIschecked(mainWindow.cb_dm_voice); }

        public bool pre_changeToHalfSmbs { get => GetCheckBoxIschecked(mainWindow.cb_changeToHalfSmbs) == true; }
        public bool pre_removeSpaces { get => GetCheckBoxIschecked(mainWindow.cb_removeSpaces) == true; }

        public bool basic_ignoreNumSmb { get => GetCheckBoxIschecked(mainWindow.cb_basic_ignoreNumSmb) == true; }
        public bool basic_ignoreNumEnd { get => GetCheckBoxIschecked(mainWindow.cb_basic_ignoreNumEnd) == true; }
        public bool basic_ignoreShort { get => GetCheckBoxIschecked(mainWindow.cb_basic_ignoreShort) == true; }
        public int basic_short { get => GetNSValue(mainWindow.ns_basic_short); }
        public bool basic_ignore5chars { get => GetCheckBoxIschecked(mainWindow.cb_basic_ignore5chars) == true; }
        public bool basic_ignore3words { get => GetCheckBoxIschecked(mainWindow.cb_basic_ignore3words) == true; }

        public bool duplicated_enabled { get => GetCheckBoxIschecked(mainWindow.cb_duplicated) == true; }
        public int duplicated_maxQueueLength { get => GetNSValue(mainWindow.ns_duplicated_SimpleQueueLength); }
        public int duplicated_minSimpleLength { get => GetNSValue(mainWindow.ns_duplicated_minSimpleLength); }

        public bool voice_readUserName { get => GetCheckBoxIschecked(mainWindow.cb_readUserName) == true; }
        public int voice_queueMax { get => GetNSValue(mainWindow.ns_voiceQueueMax); }
        public int voice_interval { get => GetNSValue(mainWindow.ns_voiceInterval); }

        internal void SetRoomId(int rId)
        {
            settings[setting_flag_roomId] = rId.ToString();
            dmEngin.RoomID = rId;
        }
        private void LoadSettingsToUI()
        {
            if (settings[setting_flag_roomId] == null)
                settings[setting_flag_roomId] = "668931";
            mainWindow.tb_roomId.Text = settings[setting_flag_roomId];

            LoadSettingsToUI_loadCheckBox(setting_flag_dm_showRead_msg, mainWindow.cb_msg, true);
            LoadSettingsToUI_loadCheckBox(setting_flag_dm_showRead_gift, mainWindow.cb_gift, true);
            LoadSettingsToUI_loadCheckBox(setting_flag_dm_showRead_userEnter, mainWindow.cb_enter, true);
            LoadSettingsToUI_loadCheckBox(setting_flag_dm_showRead_system, mainWindow.cb_system, null);
            LoadSettingsToUI_loadCheckBox(setting_flag_dm_showRead_other, mainWindow.cb_other, null);
            LoadSettingsToUI_loadCheckBox(setting_flag_dm_show_voice, mainWindow.cb_dm_voice, false);

            LoadSettingsToUI_loadCheckBox(setting_flag_pre_changeToHalfSmbs, mainWindow.cb_changeToHalfSmbs, true);
            LoadSettingsToUI_loadCheckBox(setting_flag_pre_removeSpaces, mainWindow.cb_removeSpaces, true);

            LoadSettingsToUI_loadCheckBox(setting_flag_basic_ignoreNumSmb, mainWindow.cb_basic_ignoreNumSmb, false);
            LoadSettingsToUI_loadCheckBox(setting_flag_basic_ignoreNumEnd, mainWindow.cb_basic_ignoreNumEnd, false);
            LoadSettingsToUI_loadCheckBox(setting_flag_basic_ignoreShort, mainWindow.cb_basic_ignoreShort, false);
            LoadSettingsToUI_loadNumericSlider(setting_flag_basic_short, mainWindow.ns_basic_short, 1);
            LoadSettingsToUI_loadCheckBox(setting_flag_basic_ignore5chars, mainWindow.cb_basic_ignore5chars, false);
            LoadSettingsToUI_loadCheckBox(setting_flag_basic_ignore3words, mainWindow.cb_basic_ignore3words, true);

            LoadSettingsToUI_loadCheckBox(setting_flag_duplicated_enabled, mainWindow.cb_duplicated, true);
            LoadSettingsToUI_loadNumericSlider(setting_flag_duplicated_queueLength, mainWindow.ns_duplicated_SimpleQueueLength, 3);
            LoadSettingsToUI_loadNumericSlider(setting_flag_duplicated_minSimpleLength, mainWindow.ns_duplicated_minSimpleLength, 3);

            LoadSettingsToUI_loadCheckBox(setting_flag_voice_readUserName, mainWindow.cb_readUserName, true);
            LoadSettingsToUI_loadNumericSlider(setting_flag_voice_queueMax, mainWindow.ns_voiceQueueMax, 3);
            LoadSettingsToUI_loadNumericSlider(setting_flag_voice_interval, mainWindow.ns_voiceInterval, 5);
        }


        private void LoadSettingsToUI_loadCheckBox(string key, CheckBox cb, bool? defaultValue)
        {
            if (!settings.HasKey(key))
            {
                if (defaultValue == null)
                {
                    settings[key] = "null";
                    cb.IsChecked = null;
                }
                else
                {
                    settings[key] = defaultValue.ToString();
                    cb.IsChecked = defaultValue == true;
                }
            }
            else
            {
                if (bool.TryParse(settings[key], out bool srMsg))
                    cb.IsChecked = srMsg;
                else
                    cb.IsChecked = null;
            }
        }
        private void LoadSettingsToUI_loadNumericSlider(string key, NumericSlider ns, int defaultValue)
        {
            if (!settings.HasKey(key))
            {
                settings[key] = defaultValue.ToString();
                ns.Value = defaultValue;
            }
            else
            {
                if (int.TryParse(settings[key], out int i))
                {
                    ns.Value = i;
                }
                else
                {
                    settings[key] = defaultValue.ToString();
                    ns.Value = defaultValue;
                }
            }
        }


        public void SaveSettingsFromUI()
        {
            settings[setting_flag_dm_showRead_msg]
                = dm_showRead_msg == null ? "null" : (dm_showRead_msg == true).ToString();
            settings[setting_flag_dm_showRead_gift]
                = dm_showRead_gift == null ? "null" : (dm_showRead_gift == true).ToString();
            settings[setting_flag_dm_showRead_userEnter]
                = dm_showRead_userEnter == null ? "null" : (dm_showRead_userEnter == true).ToString();
            settings[setting_flag_dm_showRead_system]
                = dm_showRead_system == null ? "null" : (dm_showRead_system == true).ToString();
            settings[setting_flag_dm_showRead_other]
                = dm_showRead_other == null ? "null" : (dm_showRead_other == true).ToString();

            settings[setting_flag_pre_changeToHalfSmbs]
                = pre_changeToHalfSmbs.ToString();
            settings[setting_flag_pre_removeSpaces]
                = pre_removeSpaces.ToString();

            settings[setting_flag_basic_ignoreNumSmb]
                = basic_ignoreNumSmb.ToString();
            settings[setting_flag_basic_ignoreNumEnd]
                = basic_ignoreNumEnd.ToString();
            settings[setting_flag_basic_ignoreShort]
                = basic_ignoreShort.ToString();
            settings[setting_flag_basic_short]
                = basic_short.ToString();
            settings[setting_flag_basic_ignore5chars]
                = basic_ignore5chars.ToString();
            settings[setting_flag_basic_ignore3words]
                = basic_ignore3words.ToString();

            settings[setting_flag_duplicated_enabled]
                = duplicated_enabled.ToString();
            settings[setting_flag_duplicated_queueLength]
                = duplicated_maxQueueLength.ToString();
            settings[setting_flag_duplicated_minSimpleLength]
                = duplicated_minSimpleLength.ToString();

            settings[setting_flag_voice_readUserName]
                = voice_readUserName.ToString();
            settings[setting_flag_voice_queueMax]
                = voice_queueMax.ToString();
            settings[setting_flag_voice_interval]
                = voice_interval.ToString();



            settings.Save();
        }

        #endregion


        #region mainWindow position restore/save

        private static string setting_flag_WndState = "windowState";
        private static string setting_flag_WndPosi_X = "windowPosition_X";
        private static string setting_flag_WndPosi_Y = "windowPosition_Y";
        private static string setting_flag_WndSize_W = "windowSize_W";
        private static string setting_flag_WndSize_H = "windowSize_H";
        public void MainWindowLoadPosition()
        {
            if (double.TryParse(settings[setting_flag_WndPosi_X], out double x))
                mainWindow.Left = x;
            if (double.TryParse(settings[setting_flag_WndPosi_Y], out double y))
                mainWindow.Top = y;
            if (double.TryParse(settings[setting_flag_WndSize_W], out double w))
                mainWindow.Width = w;
            if (double.TryParse(settings[setting_flag_WndSize_H], out double h))
                mainWindow.Height = h;
            if (Enum.TryParse(typeof(System.Windows.WindowState), settings[setting_flag_WndState], out object v))
                mainWindow.WindowState = (System.Windows.WindowState)v;
        }
        public void MainWindowSavePosition()
        {
            settings[setting_flag_WndState] = mainWindow.WindowState.ToString();
            settings[setting_flag_WndPosi_X] = mainWindow.Left.ToString();
            settings[setting_flag_WndPosi_Y] = mainWindow.Top.ToString();
            settings[setting_flag_WndSize_W] = mainWindow.ActualWidth.ToString();
            settings[setting_flag_WndSize_H] = mainWindow.ActualHeight.ToString();
        }

        #endregion


        #region date time

        private bool TimeTimer_Tick_uiNotInited = true;
        private int TimeTimer_Tick_day = 0;
        private void TimeTimer_Tick(MillisecondTimer sender, DateTime tickTime, bool is500ms, bool is1000ms)
        {
            mainWindow.Dispatcher.Invoke(() =>
            {
                if (TimeTimer_Tick_uiNotInited)
                {
                    TimeTimer_Tick_day = tickTime.Day;
                    mainWindow.tb_date.Text = tickTime.ToString("yyyy-MM-dd");
                    mainWindow.tb_day.Text = tickTime.DayOfWeek.ToString();
                    if (is500ms)
                        mainWindow.tb_time.Text = tickTime.ToString("HH mm ss");
                    else
                        mainWindow.tb_time.Text = tickTime.ToString("HH:mm:ss");
                }
                else
                {
                    if (TimeTimer_Tick_day != tickTime.Day)
                    {
                        TimeTimer_Tick_day = tickTime.Day;
                        mainWindow.tb_date.Text = tickTime.ToString("yyyy-MM-dd");
                        mainWindow.tb_day.Text = tickTime.DayOfWeek.ToString();
                    }
                    if (is500ms)
                        mainWindow.tb_time.Text = tickTime.ToString("HH mm ss");
                    else
                        mainWindow.tb_time.Text = tickTime.ToString("HH:mm:ss");
                }

                if (is1000ms)
                {
                    DmEngin_ReconnectTimerTick();
                }
            });
        }


        #endregion


        #region log, btns, clearVoiceQueue, log,     --staticView, --clear
        public void tabLog_btnLog_Clicked()
        {
            System.Diagnostics.Process.Start("explorer.exe", logger.BaseDir);
        }
        public void tabLog_btnClearVoiceQueue_Clicked()
        {
            bulletLock.EnterWriteLock();
            try
            {
                voiceTextQueue.Clear();
            }
            finally
            {
                bulletLock.ExitWriteLock();
            }
        }


        #region log background colors

        public static string setting_flag_logEnter = "clr_logEnter"; // green
        public static string setting_flag_logMsg = "clr_logMsg"; // white
        public static string setting_flag_logGift = "clr_logGift"; // yellow
        public static string setting_flag_logSys = "clr_logSys"; // blue
        public static string setting_flag_logOther = "clr_logOther"; // gray
        public static string setting_flag_logWarning = "clr_logWarning"; // orange
        public static string setting_flag_logVoice = "clr_logVoice"; // pink

        public Brush TryGetBrush(string key, Brush defaultBsh)
        {
            if (!settings.HasKey(key))
            {
                settings[key] = defaultBsh.ToString();
                return defaultBsh;
            }
            else
            {
                try
                {
                    return (System.Windows.Media.Brush)new BrushConverter().ConvertFromString(settings[key]);
                }
                catch (Exception)
                {
                    settings[key] = defaultBsh.ToString();
                    return defaultBsh;
                }
            }
        }

        #endregion

        #endregion


        #region dm engin re-connect

        public void ReConnect(bool resetCounter = true)
        {
            if (resetCounter)
                DmEngin_reconnectCounter = 0;
            mainWindow.btn_reconnect.Visibility = System.Windows.Visibility.Collapsed;
            mainWindow.tb_reconnect.Text = "正在连接...";
            mainWindow.bdr_reconnect.Visibility = System.Windows.Visibility.Collapsed;
            dmEngin.Connect();
        }

        private static string setting_flag_reconnectTimes = "reconnectTimes";
        private static string setting_flag_reconnectIntervalSec = "reconnectIntervalSec";
        private int DmEngin_reconnectTimes = -1;
        private int DmEngin_reconnectIntervalSec = -1;
        private int DmEngin_reconnectCounter = -1;
        private DateTime DmEngin_disconnectTime = DateTime.MaxValue;
        private void DmEngin_Connected(BiliDmEngin.Engin sender)
        {
            DmEngin_ReconnectTimerTickDisabled = true;
            mainWindow.btn_reconnect.Visibility = System.Windows.Visibility.Collapsed;
            mainWindow.tb_reconnect.Text = "已连接到：" + dmEngin.RoomID.ToString();
            mainWindow.bdr_reconnect.Visibility = System.Windows.Visibility.Collapsed;
        }
        private void DmEngin_Disconnected(BiliDmEngin.Engin sender, Exception err)
        {
            #region init reconnect times, interval
            if (DmEngin_reconnectTimes < 0)
            {
                if (int.TryParse(settings[setting_flag_reconnectTimes], out int t))
                {
                    DmEngin_reconnectTimes = t;
                }
                else
                {
                    DmEngin_reconnectTimes = 4;
                    settings[setting_flag_reconnectTimes] = DmEngin_reconnectTimes.ToString();
                }
            }
            if (DmEngin_reconnectIntervalSec < 0)
            {
                if (int.TryParse(settings[setting_flag_reconnectIntervalSec], out int itv))
                {
                    DmEngin_reconnectIntervalSec = itv;
                }
                else
                {
                    DmEngin_reconnectIntervalSec = 4;
                    settings[setting_flag_reconnectIntervalSec] = DmEngin_reconnectIntervalSec.ToString();
                }
            }
            #endregion

            if (DmEngin_reconnectCounter >= DmEngin_reconnectTimes)
            {
                mainWindow.btn_reconnect.Visibility = System.Windows.Visibility.Visible;
                mainWindow.tb_reconnect.Text = "已停止重试";
                mainWindow.bdr_reconnect.Visibility = System.Windows.Visibility.Visible;
                mainWindow.tbk_reconnect_tooltip.Text = "在第" + DmEngin_reconnectCounter + "次重连失败后，停止重试";
                return;
            }


            mainWindow.btn_reconnect.Visibility = System.Windows.Visibility.Visible;
            mainWindow.tb_reconnect.Text = "即将重连";
            mainWindow.bdr_reconnect.Visibility = System.Windows.Visibility.Visible;
            mainWindow.tbk_reconnect_tooltip.Text = "将进行第" + (DmEngin_reconnectCounter + 1) + "次重连";

            DmEngin_disconnectTime = DateTime.Now;
            DmEngin_ReconnectTimerTickDisabled = false;
        }
        private bool DmEngin_ReconnectTimerTickDisabled = true;
        private void DmEngin_ReconnectTimerTick()
        {
            // tick every second

            if (DmEngin_ReconnectTimerTickDisabled)
                return;

            int secCur = (int)(DateTime.Now - DmEngin_disconnectTime).TotalSeconds;

            if (secCur >= DmEngin_reconnectIntervalSec)
            {
                DmEngin_disconnectTime = DateTime.MaxValue;
                ReConnect(false);
                DmEngin_ReconnectTimerTickDisabled = true;
            }
            else
            {
                mainWindow.tb_reconnect.Text = (DmEngin_reconnectIntervalSec - secCur) + "秒后重连";
            }
        }

        #endregion


        #region show progress bullets, show progress voice queue

        private int counter_bulletsAll = 0;
        private int counter_bulletsProcessed = 0;
        private int counter_bulletsRead = 0;
        private void MainWindowShowBulletsProcess()
        {
            mainWindow.Dispatcher.Invoke(() =>
            {
                mainWindow.pgb_pRate_knownData.Maximum = counter_bulletsAll;
                mainWindow.pgb_pRate_knownData.Value = counter_bulletsProcessed;
                mainWindow.pgb_pRate_read.Maximum = counter_bulletsAll;
                mainWindow.pgb_pRate_read.Value = counter_bulletsRead;

                mainWindow.tb_pRate.Text = counter_bulletsRead + " / " + counter_bulletsProcessed + " / " + counter_bulletsAll;
            });
        }

        private void MainWindowShowVoiceQueueProcess()
        {
            mainWindow.Dispatcher.Invoke(() =>
            {
                int voiceTxCount = voiceTextQueue.Count + voiceTextQueue_gift.Count;
                int max = Math.Max(voice_queueMax, voiceTxCount);
                int cur = voiceTxCount;
                mainWindow.pgb_vRate.Maximum = max;
                mainWindow.pgb_vRate.Value = cur;
                mainWindow.tb_vRate.Text = cur.ToString() + " / " + max.ToString();
            });
        }

        #endregion


        #region bullet queue, gift queue

        private Queue<BiliDmEngin.Bullet> beltBullets = new Queue<BiliDmEngin.Bullet>();
        private Queue<BiliDmEngin.Bullet> beltGifts = new Queue<BiliDmEngin.Bullet>();

        ReaderWriterLockSlim bulletLock = new ReaderWriterLockSlim();
        private void BulletEnqueue(BiliDmEngin.Bullet inBullet)
        {
            bulletLock.EnterWriteLock();
            try
            {
                beltBullets.Enqueue(inBullet);
            }
            finally
            {
                bulletLock.ExitWriteLock();
            }
        }
        private BiliDmEngin.Bullet BulletDequeue()
        {
            BiliDmEngin.Bullet result = null;
            bulletLock.EnterWriteLock();
            try
            {
                result = beltBullets.Dequeue();
            }
            finally
            {
                bulletLock.ExitWriteLock();
            }
            return result;
        }
        private void GiftEnqueue(BiliDmEngin.Bullet inBullet)
        {
            bulletLock.EnterWriteLock();
            try
            {
                beltGifts.Enqueue(inBullet);
            }
            finally
            {
                bulletLock.ExitWriteLock();
            }
        }
        private BiliDmEngin.Bullet GiftDequeue()
        {
            BiliDmEngin.Bullet result = null;
            bulletLock.EnterWriteLock();
            try
            {
                result = beltGifts.Dequeue();
            }
            finally
            {
                bulletLock.ExitWriteLock();
            }
            return result;
        }

        #endregion


        #region bullet from engin, process ==> show log, voice text queue
        private void DmEngin_NewBullet(BiliDmEngin.Engin sender, BiliDmEngin.Bullet newBullet)
        {
            counter_bulletsAll++;
            MainWindowShowBulletsProcess();

            BulletEnqueue(newBullet);
            ProcessLoop();
        }

        internal void TestMsg(string tester, string msg)
        {
            DmEngin_NewBullet(null, new BiliDmEngin.Bullet(BiliDmEngin.Bullet.BulletType.Msg)
            { userID = "0", userName = tester, userMsg = msg, });
        }

        //private bool ProcessLoop_busy = false;
        private void ProcessLoop()
        {
            Task.Run(() =>
            {
                //if (ProcessLoop_busy)
                //    return;
                //Console.WriteLine("0");
                //ProcessLoop_busy = true;

                BiliDmEngin.Bullet bullet;
                string finalVoiceText, failReason, msgCompleteText, bvEnqueueMsg, tx1;
                bool? showLogNVoice;
                MainWindow.LogTypes logType;
                while (beltBullets.Count > 0)
                {
                    bullet = BulletDequeue();
                    msgCompleteText = bullet.CompleteText;

                    if (bullet.type == BiliDmEngin.Bullet.BulletType.Gft)
                    {
                        switch (dm_showRead_gift)
                        {
                            case true:
                                GiftEnqueue(bullet);
                                mainWindow.BulletLog(msgCompleteText, MainWindow.LogTypes.Gift);
                                break;
                            case null:
                                mainWindow.BulletLog(msgCompleteText, MainWindow.LogTypes.Gift);
                                break;
                            case false:
                                break;
                        }
                    }
                    else
                    {

                        if (string.IsNullOrEmpty(bullet.userID) == false
                            && db.HaveBlackUser(bullet.userID, out DataBase.ValueRemarkItem u))
                        {
                            if (u.showLog)
                            {
                                mainWindow.BulletLog($"屏蔽用户({bullet.userID}): {msgCompleteText}", MainWindow.LogTypes.Warning);
                            }
                            continue;
                        }


                        switch (bullet.type)
                        {
                            case BiliDmEngin.Bullet.BulletType.Other:
                                showLogNVoice = dm_showRead_other;
                                logType = MainWindow.LogTypes.Other;
                                tx1 = bullet.sysMsg;
                                break;
                            case BiliDmEngin.Bullet.BulletType.UserEnter:
                                showLogNVoice = dm_showRead_userEnter;
                                logType = MainWindow.LogTypes.Enter;
                                tx1 = msgCompleteText;
                                break;
                            case BiliDmEngin.Bullet.BulletType.Msg:
                                showLogNVoice = dm_showRead_msg;
                                logType = MainWindow.LogTypes.Msg;
                                if (voice_readUserName)
                                    tx1 = bullet.userName + " : " + bullet.userMsg;
                                else
                                    tx1 = bullet.userMsg;
                                break;
                            default:
                            case BiliDmEngin.Bullet.BulletType.Sys:
                                showLogNVoice = dm_showRead_system;
                                logType = MainWindow.LogTypes.Sys;
                                tx1 = msgCompleteText;
                                break;
                        }

                        if (showLogNVoice != false)
                        {
                            if (ProcessMsg(tx1, out finalVoiceText, out failReason))
                            {
                                if (logType == MainWindow.LogTypes.Msg)
                                    mainWindow.BulletLog(msgCompleteText, logType);
                                else
                                    mainWindow.BulletLog(tx1, logType);
                                if (showLogNVoice == true)
                                {
                                    if (TryBulletVoiceEnqueue(finalVoiceText, out bvEnqueueMsg))
                                    {
                                        mainWindow.BulletLog(bvEnqueueMsg, MainWindow.LogTypes.Sys);
                                    }
                                    else
                                    {
                                        mainWindow.BulletLog(bvEnqueueMsg, MainWindow.LogTypes.Warning);
                                    }
                                }
                            }
                            else
                            {
                                if (failReason != null)
                                {
                                    if (logType == MainWindow.LogTypes.Msg)
                                        mainWindow.BulletLog(msgCompleteText, logType);
                                    else
                                        mainWindow.BulletLog(tx1, logType);
                                }
                                mainWindow.BulletLog(failReason, MainWindow.LogTypes.Warning);
                            }
                        }
                    }
                }
                //ProcessLoop_busy = false;
                //Console.WriteLine("F");
            });
        }

        private bool ProcessMsg(string oriText, out string? finalVoiceText, out string? failReason)
        {
            finalVoiceText = null;
            failReason = null;

            // check the 1st time
            if (db.blackWords.Count > 0)
            {
                if (db.BatchCheckBlackWords(oriText, out DataBase.ValueRemarkItem bw) == false)
                {
                    if (bw.showLog)
                        failReason = $"消息中发现屏蔽词\"{bw}\"";
                    else
                        failReason = null;
                    return false;
                }
            }

            if (pre_changeToHalfSmbs)
            {
                oriText = oriText.Replace("　", " ").Replace("？", "?").Replace("！", "!").Replace("，", ",");
            }
            oriText = oriText.Replace("　", " ");
            while (oriText.Contains("  "))
                oriText = oriText.Replace("  ", " ");
            oriText = oriText.Trim();
            List<MultiVoice.TextMultiFragments.Fragment> fragments;
            if (pre_removeSpaces)
            {
                StringBuilder strBdr = new StringBuilder();
                fragments = MultiVoice.TextMultiFragments.Detach(oriText);
                foreach (MultiVoice.TextMultiFragments.Fragment fg in fragments)
                {
                    if (fg.fgLang == MultiVoice.LanguageType.ZH
                        || fg.fgLang == MultiVoice.LanguageType.JA)
                    {
                        strBdr.Append(fg.fgValue.Replace(" ", ""));
                    }
                    else
                    {
                        strBdr.Append(fg.fgValue);
                    }
                }
                oriText = strBdr.ToString();
            }
            if (string.IsNullOrWhiteSpace(oriText))
            {
                failReason = "空，或空白文本";
                return false;
            }

            if (db.replacements.Count > 1)
            {
                oriText = db.BatchReplacement(oriText);
            }

            string msgTestText;
            if (oriText.Contains(":"))
                msgTestText = oriText.Substring(oriText.IndexOf(":") + 1);
            else
                msgTestText = oriText;
            string oriTextNoSmb = MultiVoice.TextHelper.Text_RemoveAllSymbols(msgTestText);
            if (basic_ignoreNumSmb)
            {
                bool notHasMajorLang = true;
                fragments = MultiVoice.TextMultiFragments.Detach(oriTextNoSmb);
                foreach (MultiVoice.TextMultiFragments.Fragment fg in fragments)
                {
                    if (fg.fgLang == MultiVoice.LanguageType.ZH
                        || fg.fgLang == MultiVoice.LanguageType.JA
                        || fg.fgLang == MultiVoice.LanguageType.EN)
                    {
                        notHasMajorLang = false;
                        break;
                    }
                }
                if (notHasMajorLang)
                {
                    failReason = "消息中没有可读语言";
                    return false;
                }
            }
            if (basic_ignoreNumEnd)
            {
                if (int.TryParse(oriText.Substring(oriText.Length - 1), out int i))
                {
                    failReason = "消息以数字结尾";
                    return false;
                }
            }
            if (basic_ignoreShort)
            {
                if (msgTestText.Length <= basic_short)
                {
                    failReason = "消息文本长度太短";
                    return false;
                }
            }
            if (basic_ignore5chars)
            {
                if (Check_5SameChar(oriTextNoSmb, out string dup5chars) == false)
                {
                    failReason = $"消息中出现重复5连字\"{dup5chars}\"";
                    return false;
                }
            }
            if (basic_ignore3words)
            {
                if (Check_3SameWord(oriTextNoSmb, out string dup3words) == false)
                {
                    failReason = $"消息中出现重复3连词\"{dup3words}\"";
                    return false;
                }
            }

            if (oriText.Contains(":"))
                oriTextNoSmb
                    = MultiVoice.TextHelper.Text_RemoveAllSymbols(oriText.Substring(0, oriText.IndexOf(":")))
                    + ":" + oriTextNoSmb;


            // check the 2nd time
            if (db.blackWords.Count > 0)
            {
                if (db.BatchCheckBlackWords(oriTextNoSmb, out DataBase.ValueRemarkItem bw) == false)
                {
                    if (bw.showLog)
                        failReason = $"消息中发现屏蔽词\"{bw}\"";
                    else
                        failReason = null;
                    return false;
                }
            }
            if (db.blackPins.Count > 0)
            {
                fragments = MultiVoice.TextMultiFragments.Detach(oriTextNoSmb);
                foreach (MultiVoice.TextMultiFragments.Fragment f in fragments)
                {
                    if (f.fgLang == MultiVoice.LanguageType.ZH
                        && db.BatchCheckBlackPins(f.fgValue, out DataBase.ValueRemarkItem bw, out int _1, out int _2) == false)
                    {
                        if (bw.showLog)
                            failReason = $"消息中发现屏蔽音\"{bw}\"";
                        else
                            failReason = null;
                        return false;
                    }
                }
            }

            if (duplicated_enabled)
            {
                // 在最短消息的基础上，如果后续消息包含这个串，则算作是重复
                string msgNoSmb;
                if (oriTextNoSmb.Contains(":"))
                    msgNoSmb = oriTextNoSmb.Substring(oriTextNoSmb.IndexOf(":") + 1);
                else
                    msgNoSmb = oriTextNoSmb;



                string msgNoSmbPY = SimpleStringHelper.Chinese2PINYIN.GetFullPINYIN(msgNoSmb);



                foreach (string simple in duplicatedPYList)
                {
                    if (msgNoSmbPY.Contains(simple))
                    {
                        failReason = $"出现重复样本\"{simple}\"";
                        return false;
                    }
                }
                if (msgNoSmb.Length >= duplicated_minSimpleLength)
                {
                    while (duplicatedPYList.Count >= duplicated_maxQueueLength)
                        duplicatedPYList.RemoveAt(0);
                    duplicatedPYList.Add(msgNoSmbPY);
                }
            }


            counter_bulletsProcessed++;
            MainWindowShowBulletsProcess();

            finalVoiceText = oriText;
            return true;
        }


        public bool Check_5SameChar(string text, out string dupedChar)
        {
            bool pass = true;
            dupedChar = "";
            text = text.Replace(" ", "");
            if (text.Length < 5)
            { }
            else
            {
                string c;
                int startIdx;
                int textLength = text.Length;
                for (startIdx = 0; startIdx <= textLength - 5; startIdx++)
                {
                    pass = false;
                    c = text.Substring(startIdx, 1);
                    dupedChar = c;
                    for (int i = startIdx + 1; i <= startIdx + 4; i++)
                    {
                        if (c != text.Substring(i, 1))
                        {
                            pass = true;
                            dupedChar = "";
                            break;
                        }
                    }
                    if (pass == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool Check_3SameWord(string text, out string dupedWord)
        {
            string word0, word1, word2;
            text = text.Replace(" ", "");
            for (int wordLength = 2, wordLengthV = 6; wordLength <= wordLengthV; wordLength++)
            {
                for (int startIdx = 0, startIdxV = text.Length - (3 * wordLength); startIdx <= startIdxV; startIdx++)
                {
                    word0 = text.Substring(startIdx, wordLength);
                    word1 = text.Substring(startIdx + wordLength, wordLength);
                    word2 = text.Substring(startIdx + wordLength + wordLength, wordLength);
                    if (word0 == word1 && word1 == word2)
                    {
                        dupedWord = word0;
                        return false;
                    }
                }
            }

            dupedWord = "";
            return true;
        }

        public List<string> duplicatedPYList = new List<string>();

        #endregion






        private Queue<string> voiceTextQueue = new Queue<string>();
        private Queue<string> voiceTextQueue_gift = new Queue<string>();
        private bool TryBulletVoiceEnqueue(string finalTx2Speech, out string outMsg, bool isGiftVoice = false)
        {
            outMsg = null;
            if (voiceTextQueue.Count >= voice_queueMax
                && isGiftVoice == false)
            {
                outMsg = "放弃语音，队列已满";
                return false;
            }

            bulletLock.EnterWriteLock();
            try
            {
                voiceTextQueue.Enqueue(finalTx2Speech);
            }
            finally
            {
                bulletLock.ExitWriteLock();
            }

            return true;
        }
        private void GftBulletVoiceEnqueue(string giftVoiceText)
        {

            bulletLock.EnterWriteLock();
            try
            {
                voiceTextQueue_gift.Enqueue(giftVoiceText);
                MainWindowShowVoiceQueueProcess();
            }
            finally
            {
                bulletLock.ExitWriteLock();
            }
        }

        private bool VoiceLoopStop_flag = false;
        private void VoiceLoopStop()
        {
            VoiceLoopStop_flag = true;
        }
        private void VoiceLoopStart()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (VoiceLoopStop_flag)
                        break;
                    GiftCheckOut();
                    string curVoiceText;
                    while (voiceTextQueue_gift.Count > 0)
                    {
                        if (VoiceLoopStop_flag)
                            break;
                        bulletLock.EnterWriteLock();
                        try
                        {
                            curVoiceText = voiceTextQueue_gift.Dequeue();
                        }
                        finally
                        {
                            bulletLock.ExitWriteLock();
                        }
                        if (dm_show_voice == true)
                            mainWindow.BulletLog(curVoiceText, MainWindow.LogTypes.Voice);
                        voicer.Speek(curVoiceText);// async ?? sync ??
                        VoiceLoop_waitUntillDone();

                        MainWindowShowVoiceQueueProcess();

                        counter_bulletsRead++;
                        MainWindowShowBulletsProcess();
                        if (VoiceLoopStop_flag)
                            break;

                        Thread.Sleep(voice_interval * 1000);
                    }
                    if (voiceTextQueue.Count > 0)
                    {
                        if (VoiceLoopStop_flag)
                            break;
                        bulletLock.EnterWriteLock();
                        try
                        {
                            curVoiceText = voiceTextQueue.Dequeue();
                        }
                        finally
                        {
                            bulletLock.ExitWriteLock();
                        }
                        if (dm_show_voice == true)
                            mainWindow.BulletLog(curVoiceText, MainWindow.LogTypes.Voice);
                        voicer.Speek(curVoiceText);// async ?? sync ??
                        VoiceLoop_waitUntillDone();

                        MainWindowShowVoiceQueueProcess();

                        counter_bulletsRead++;
                        MainWindowShowBulletsProcess();
                        if (VoiceLoopStop_flag)
                            break;
                        Thread.Sleep(voice_interval * 1000);
                    }
                    if (VoiceLoopStop_flag)
                        break;
                    Thread.Sleep(50);
                }
            });
        }
        private void GiftCheckOut()
        {
            while (beltGifts.Count > 0)
            {
                VoiceLoop_waitUntillDone();
                Thread.Sleep(voice_interval * 1000);

                StringBuilder giftVoiceTextBdr = new StringBuilder();

                List<BiliDmEngin.Bullet> giftBullets = new List<BiliDmEngin.Bullet>();
                bulletLock.EnterWriteLock();
                try
                {
                    while (beltGifts.Count > 0)
                    {
                        giftBullets.Add(beltGifts.Dequeue());
                    }
                }
                finally
                {
                    bulletLock.ExitWriteLock();
                }



                List<string> nameList = new List<string>();
                Dictionary<string, int> giftCounter = new Dictionary<string, int>();
                BiliDmEngin.Bullet curGift;
                while (giftBullets.Count > 0)
                {
                    curGift = giftBullets[0];
                    giftBullets.RemoveAt(0);
                    if (nameList.Find(a => a == curGift.userName) == null)
                    {
                        nameList.Add(curGift.userName);
                    }

                    if (giftCounter.ContainsKey(curGift.giftName))
                    {
                        giftCounter[curGift.giftName] += curGift.giftCount;
                    }
                    else
                    {
                        giftCounter.Add(curGift.giftName, curGift.giftCount);
                    }
                }

                #region 整理用户播报
                giftVoiceTextBdr.Append("收到");

                // 1用户，播报全名和礼物
                // 2用户，播报用户全名和礼物合计
                // 3+用户，播报前三用户省略名(等)和礼物合计
                StringBuilder curUserNamesBdr = new StringBuilder();
                if (nameList.Count == 1)
                {
                    curUserNamesBdr.Append(nameList[0]);
                }
                else if (nameList.Count == 2)
                {
                    curUserNamesBdr.Append(nameList[0]);
                    curUserNamesBdr.Append(" 和 ");
                    curUserNamesBdr.Append(nameList[1]);
                }
                else if (nameList.Count == 3)
                {
                    curUserNamesBdr.Append(ShortenUserName(nameList[0]));
                    curUserNamesBdr.Append(", ");
                    curUserNamesBdr.Append(ShortenUserName(nameList[1]));
                    curUserNamesBdr.Append(" 和 ");
                    curUserNamesBdr.Append(ShortenUserName(nameList[2]));
                }
                else if (nameList.Count > 3)
                {
                    curUserNamesBdr.Append(ShortenUserName(nameList[0]));
                    curUserNamesBdr.Append(", ");
                    curUserNamesBdr.Append(ShortenUserName(nameList[1]));
                    curUserNamesBdr.Append(" 和 ");
                    curUserNamesBdr.Append(ShortenUserName(nameList[2]));
                }
                string curUserNames = db.BatchReplacement(curUserNamesBdr.ToString());
                DataBase.ValueRemarkItem blackWord;
                while (!db.BatchCheckBlackWords(curUserNames, out blackWord))
                {
                    curUserNames.Replace(blackWord.value, "嘤嘤");
                }
                int bpStart, bpLength;
                while (!db.BatchCheckBlackPins(curUserNames, out blackWord, out bpStart, out bpLength))
                {
                    curUserNames
                        = curUserNames.Substring(0, bpStart)
                        + "嘤嘤"
                        + curUserNames.Substring(bpStart + bpLength);
                }
                giftVoiceTextBdr.Append(curUserNames);
                if (nameList.Count > 3)
                    giftVoiceTextBdr.Append("等观众");

                giftVoiceTextBdr.Append("的"); // 赠送的
                #endregion

                int count = 0;
                foreach (int gc in giftCounter.Values)
                {
                    count += gc;
                }
                giftVoiceTextBdr.Append(count + "个");
                int i = 0;
                foreach (string gn in giftCounter.Keys)
                {
                    giftVoiceTextBdr.Append(gn);
                    if (giftCounter.Count > 1)
                    {
                        if (i == giftCounter.Count - 2)
                            giftVoiceTextBdr.Append("和");
                        else if (i == giftCounter.Count - 1)
                        { }
                        else
                            giftVoiceTextBdr.Append("，");
                    }
                    i++;
                }
                GftBulletVoiceEnqueue(giftVoiceTextBdr.ToString());
            }
        }
        private string ShortenUserName(string userName)
        {
            userName = MultiVoice.TextHelper.Text_RemoveAllSymbols(userName);
            foreach (MultiVoice.TextMultiFragments.Fragment fg in MultiVoice.TextMultiFragments.Detach(userName))
            {
                if (fg.fgLang == MultiVoice.LanguageType.EN)
                    return fg.fgValue;
                else if (fg.fgLang == MultiVoice.LanguageType.ZH
                    || fg.fgLang == MultiVoice.LanguageType.JA)
                    return fg.fgValue.Length > 4 ? fg.fgValue.Substring(0, 4) : fg.fgValue;
            }
            return userName.Length > 4 ? userName.Substring(0, 4) : userName;
        }
        private void VoiceLoop_waitUntillDone()
        {
            while (voicer.isPlaying)
            {
                Thread.Sleep(10);
            }
        }

        public void Dispose()
        {
            db.Dispose(false);
            timeTimer.Dispose();
            VoiceLoopStop();
            voicer.Stop();
            voicer.Dispose();
            dmEngin.Dispose();
        }
    }
}
