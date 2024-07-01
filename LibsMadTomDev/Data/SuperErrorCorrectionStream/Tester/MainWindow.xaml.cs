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
using Path = System.IO.Path;

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //// for test
            //Data.BlockIndexer bIdxer = new Data.BlockIndexer(3);
            //int ccIdx, c, r;
            //bool result = bIdxer.GetPosition(18, out ccIdx, out c, out r);

            InitializeComponent();
            isNotInited = false;
        }
        private bool isNotInited = true;
        private Task WaitInit()
        {
            return Task.Run(() =>
            {
                while (isNotInited)
                {
                    Task.Delay(10).Wait();
                }
            });
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            await WaitInit();
            bp_err.blockDataChangedFromBtn = ReCalBlockCorrectionAsync;
            btn_blockReinit_Click(null, null);

            RefreshBDR_BER();

            bandPanelErr.DataChangedFromBtn = BandReCurrectAsync;
            btn_bandReinit_Click(null, null);

            tb_bandDataHeight_TextChanged(null, null);
        }

        #region block test - single
        private Data.Block blockOri, blockErr;
        private Random rand = new Random((int)DateTime.Now.Ticks);
        private void btn_blockReinit_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(tb_blockInitDataWidth.Text, out int newWidth))
            {
                if (newWidth < 2)
                    return;
                if (newWidth > byte.MaxValue)
                {
                    newWidth = byte.MaxValue;
                    tb_blockInitDataWidth.Text = newWidth.ToString();
                }

                blockOri = new Data.Block((byte)newWidth);
                StaticMethods.SetBlockDataRandom(ref blockOri.data, rand);
                blockOri.GenerateCorrectingCode();

                bp_init.SetOriBlock(blockOri);

                btn_blockErrN_Click(btn_blockErr2, null);
            }
            else
            {
                tb_blockInitDataWidth.Text = "3";
                btn_blockReinit_Click(null, null);
            }
        }

        private void btn_blockErrN_Click(object sender, RoutedEventArgs e)
        {
            // 1-6
            string btnName = ((Button)sender).Name;
            int eCount = int.Parse(btnName.Substring(btnName.Length - 1));

            blockErr = blockOri.Clone();
            if (StaticMethods.SetBlockError(ref blockErr, eCount, rand))
            {

                bp_err.SetOriBlock(blockOri);
                bp_err.SetErrBlock(blockErr);
                bp_err.RefreshBlockDifferences();
                List<int> eRows = new List<int>();
                List<int> eCols = new List<int>();
                List<int> eAlts = new List<int>();
                Data.Block.CheckBlock(blockErr, ref eRows, ref eCols, ref eAlts);
                bp_err.SetError(eRows, eCols, eAlts);

                cb_blockResultSame.IsChecked = false;
                ReCalBlockCorrectionAsync();
            }
        }


        private void ReCalBlockCorrectionAsync()
        {
            Task.Run(() =>
            {
                //MessageBox.Show("to cal");
                Data.Block blockTest = blockErr.Clone();
                blockTest.TryCorrecting(true);
                List<int> eRows = new List<int>();
                List<int> eCols = new List<int>();
                List<int> eAlts = new List<int>();
                Data.Block.CheckBlock(blockTest, ref eRows, ref eCols, ref eAlts);
                Dispatcher.Invoke(() =>
                {
                    bp_cur.SetOriBlock(blockOri);
                    bp_cur.SetErrBlock(blockTest);
                    bp_cur.RefreshBlockDifferences();
                    bp_cur.SetError(eRows, eCols, eAlts);
                    cb_blockResultSame.IsChecked = blockOri == blockTest;
                });
            });
        }

        #endregion


        #region block test - batch

        private int blockBatchTest_dataHeightFrom = 3;
        private int blockBatchTest_dataHeightTo = 11;
        #region block size
        private Brush tbBackgroundOri = null;
        private Brush tbBackgroundErr = new SolidColorBrush(Colors.Orange);
        private void tb_blockHeightFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox))
                return;
            TextBox tb = (TextBox)sender;
            if (tb_TryGetIntValueNSetErr(tb, ref blockBatchTest_dataHeightFrom, byte.MaxValue))
            {
                RefreshBDR_BER();
            }
        }

        private void tb_blockHeightTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox))
                return;
            TextBox tb = (TextBox)sender;
            if (tb_TryGetIntValueNSetErr(tb, ref blockBatchTest_dataHeightTo, byte.MaxValue))
            {
                RefreshBDR_BER();
            }
        }

        private bool tb_TryGetIntValueNSetErr(TextBox tb, ref int valueToSet, int max)
        {
            if (int.TryParse(tb.Text, out int v))
            {
                tb.Background = tbBackgroundOri;
                if (v > max)
                {
                    v = max;
                    tb.Text = v.ToString();
                }
                valueToSet = v;
                return true;
            }
            else
            {
                tb.Background = tbBackgroundErr;
                return false;
            }
        }
        long GetBlockDataVol(int height)
        {
            return (long)height * height + height;
        }
        long GetBlockVol(int height)
        {
            return (long)height * height + 3 * height + 1;
        }
        private void RefreshBlockRatio(ProgressBar pbFrom, ProgressBar pbTo, Label lb, double vFrom, double vTo)
        {
            pbFrom.Value = vFrom;
            pbTo.Value = vTo;
            lb.Content = vFrom.ToString("P2") + " ~ " + vTo.ToString("P2");
        }
        private void RefreshBDR_BER()
        {
            if (pb_blockDataRatioFrom == null)
                return;

            RefreshBlockRatio(
                pb_blockDataRatioFrom, pb_blockDataRatioTo, lb_blockDataRatio,
                (double)GetBlockDataVol(blockBatchTest_dataHeightFrom) / GetBlockVol(blockBatchTest_dataHeightFrom),
                (double)GetBlockDataVol(blockBatchTest_dataHeightTo) / GetBlockVol(blockBatchTest_dataHeightTo));
            RefreshBER();
        }
        private void RefreshBER()
        {
            if (pb_blockErrRatioFrom == null)
                return;
            RefreshBlockRatio(
                pb_blockErrRatioFrom, pb_blockErrRatioTo, lb_blockErrRatio,
                (double)blockBatchTest_dataErrFrom / GetBlockVol(blockBatchTest_dataHeightTo),
                (double)blockBatchTest_dataErrTo / GetBlockVol(blockBatchTest_dataHeightFrom));
        }

        #endregion

        private int blockBatchTest_dataErrFrom = 1;
        private int blockBatchTest_dataErrTo = 5;
        #region error set
        private void tb_blockErrFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox))
                return;
            TextBox tb = (TextBox)sender;
            if (tb_TryGetIntValueNSetErr(tb, ref blockBatchTest_dataErrFrom, blockBatchTest_dataHeightTo))
            {
                RefreshBER();
            }
        }

        private void tb_blockErrTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox))
                return;
            TextBox tb = (TextBox)sender;
            if (tb_TryGetIntValueNSetErr(tb, ref blockBatchTest_dataErrTo, blockBatchTest_dataHeightTo))
            {
                RefreshBER();
            }
        }

        #endregion

        private int blockBatchTest_repeat = 1000000;
        private void tb_blockBatchCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox))
                return;
            TextBox tb = (TextBox)sender;
            tb_TryGetIntValueNSetErr(tb, ref blockBatchTest_repeat, int.MaxValue);
        }


        private void btn_blockBatchTest_Click(object sender, RoutedEventArgs e)
        {
            if (isBlockBatchTesting)
            {
                // cancel ???
                isBlockBatchCancelFlag = true;
                return;
            }

            // check tb-bg
            if (tb_blockErrFrom.Background == tbBackgroundErr
                || tb_blockErrTo.Background == tbBackgroundErr
                || tb_blockHeightFrom.Background == tbBackgroundErr
                || tb_blockHeightTo.Background == tbBackgroundErr)
            {
                System.Media.SystemSounds.Hand.Play();
                return;
            }

            // start work async
            BlockBatchTestAsync();
            isBlockBatchCancelFlag = false;
        }

        private bool isBlockBatchTesting = false;
        private bool isBlockBatchCancelFlag = false;
        private void BlockBatchTestAsync()
        {
            Task.Run(() =>
            {
                isBlockBatchTesting = true;
                SetBlockBatchTBEnable(false);

                #region cal all counts
                if (blockBatchTest_dataHeightFrom > blockBatchTest_dataHeightTo)
                {
                    Swarp(ref blockBatchTest_dataHeightFrom, ref blockBatchTest_dataHeightTo);
                }
                if (blockBatchTest_dataErrFrom > blockBatchTest_dataErrTo)
                {
                    Swarp(ref blockBatchTest_dataErrFrom, ref blockBatchTest_dataErrTo);
                }
                void Swarp(ref int a, ref int b)
                {
                    int tmp = a;
                    a = b;
                    b = tmp;
                }
                long stepTotal = (long)blockBatchTest_repeat
                    * (blockBatchTest_dataHeightTo - blockBatchTest_dataHeightFrom + 1)
                    * (blockBatchTest_dataErrTo - blockBatchTest_dataErrFrom + 1);

                #endregion


                DateTime now = DateTime.Now;
                csvWriter = new Common.CSVHelper.Writer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "result." + now.ToString("yyMMdd HHmmss") + ".csv"));
                csvWriter.WriteRow(new string[] { "Block batch test, start at " + now.ToLongDateString() + " " + now.ToLongTimeString() });
                csvWriter.WriteRow(null);
                csvWriter.WriteRow(new string[] { "blockHeight", "errCount", "totalTimes", "passCur", "passFault", "fail" });

                BlockBatchTestReport -= MainWindow_BlockBatchTestReport;
                counters.Clear();
                completeFlag.Clear();


                int i, iv;
                for (i = 0; i < processorCount; i++)
                {
                    counters.Add(0);
                    completeFlag.Add(false);
                }
                BlockBatchTestReport += MainWindow_BlockBatchTestReport;

                for (i = 0; i < processorCount; i++)
                    BlockBatchTestAsync_subTrd(i);

                long steps;
                bool allComplete;
                do
                {
                    Task.Delay(50).Wait();
                    allComplete = true;
                    for (i = 0, iv = completeFlag.Count; i < iv; i++)
                    {
                        if (!completeFlag[i])
                        {
                            allComplete = false;
                            break;
                        }
                    }
                    if (allComplete)
                        break;

                    steps = 0;
                    for (i = 0, iv = counters.Count; i < iv; i++)
                    {
                        steps += counters[i];
                    }
                    SetBlockBatchTestProgress(steps, stepTotal);
                }
                while (true);

                Dispatcher.Invoke(() =>
                {
                    csvWriter.Flush();
                    csvWriter.Dispose();
                    SetBlockBatchTBEnable(true);
                });
                if (isBlockBatchCancelFlag)
                    MessageBox.Show("Block batch test canceled");
                else
                    MessageBox.Show("Block batch test complete");
                isBlockBatchTesting = false;
                isBlockBatchCancelFlag = false;
            });
        }


        private int processorCount = Environment.ProcessorCount;
        private object locker1 = new object();
        private object locker2 = new object();
        private List<long> counters = new List<long>();
        private List<bool> completeFlag = new List<bool>();
        private delegate void BlockBatchTestReportDelegate(long blockHeight, long errCount,
            long totalTimes, long passCur, long passFault, long fail);
        private event BlockBatchTestReportDelegate BlockBatchTestReport;
        private void BlockBatchTestAsync_subTrd(int idx)
        {
            Task.Run(() =>
            {
                long idxCounter = 0;

                Random rand = new Random((int)DateTime.Now.Ticks);
                Data.Block testBlock;
                Data.Block testBlockOri;
                long counter, counterPassCur, counterPassFault, counterFail;
                bool test;
                for (int i, e, h = blockBatchTest_dataHeightFrom; h <= blockBatchTest_dataHeightTo; h++)
                {
                    if (isBlockBatchCancelFlag)
                        break;

                    testBlock = new Data.Block((byte)h);
                    for (e = blockBatchTest_dataErrFrom; e <= blockBatchTest_dataErrTo; e++)
                    {
                        test = idxCounter % processorCount != idx;
                        idxCounter++;
                        if (test)
                            continue;
                        if (isBlockBatchCancelFlag)
                            break;
                        if (e > (h * h + 3 * h))
                        {
                            lock (locker1)
                            {
                                counters[idx] += blockBatchTest_repeat;
                            }
                            continue;
                        }

                        counterFail = 0;
                        counterPassFault = 0;
                        counterPassCur = 0;
                        counter = 0;
                        for (i = 0; i < blockBatchTest_repeat; i++)
                        {
                            if (isBlockBatchCancelFlag)
                                break;
                            StaticMethods.SetBlockDataRandom(ref testBlock.data, rand);
                            testBlock.GenerateCorrectingCode();
                            testBlockOri = testBlock.Clone();
                            StaticMethods.SetBlockError(ref testBlock, e, rand);
                            if (testBlock.TryCorrecting(true))
                            {
                                if (testBlock == testBlockOri)
                                    counterPassCur++;
                                else
                                    counterPassFault++;
                            }
                            else
                            {
                                counterFail++;
                            }
                            counter++;
                            lock (locker1)
                            {
                                counters[idx]++;
                            }
                        }
                        BlockBatchTestReport?.Invoke(h, e,
                            counter, counterPassCur,
                            counterPassFault, counterFail);
                    }
                }
                lock (locker2)
                {
                    completeFlag[idx] = true;
                }
            });
        }

        Common.CSVHelper.Writer csvWriter;
        private void MainWindow_BlockBatchTestReport(
            long blockHeight, long errCount,
            long totalTimes, long passCur, long passFault, long fail)
        {
            Dispatcher.Invoke(() =>
            {
                csvWriter.WriteRow(new string[]
                {
                    blockHeight.ToString(),
                    errCount.ToString(),
                    totalTimes.ToString(), passCur.ToString(),
                    passFault.ToString(), fail.ToString()
                });
            });
        }
        //private delegate void SetBlockBatchTestProgressDelegate(long cur, long max);
        //private SetBlockBatchTestProgressDelegate SetBlockBatchTestProgressD = null;
        private void SetBlockBatchTestProgress(long cur, long max)
        {
            Dispatcher.Invoke(() =>
            {
                pb_blockBatchCompleation.Maximum = max;
                pb_blockBatchCompleation.Value = cur;
                lb_blockBatchCompleation.Content = cur.ToString() + " / " + max.ToString();
            });
        }

        private void SetBlockBatchTBEnable(bool isEnable)
        {
            Dispatcher.Invoke(() =>
            {
                tb_blockHeightFrom.IsEnabled = isEnable;
                tb_blockHeightTo.IsEnabled = isEnable;
                tb_blockErrFrom.IsEnabled = isEnable;
                tb_blockErrTo.IsEnabled = isEnable;
            });
        }


        #endregion


        #region test - band - single

        private Data.Band oriBand, errBand, curBand;


        private void btn_bandReinit_Click(object sender, RoutedEventArgs e)
        {
            if (byte.TryParse(tb_bandInitDataWidth.Text, out byte width))
            {
                if (int.TryParse(tb_bandInitLength.Text, out int length))
                {
                    oriBand = new Data.Band(width, length);

                    int dataLength = (int)oriBand.MaxBandDataLength;
                    byte[] buffer = new byte[dataLength];
                    rand.NextBytes(buffer);
                    oriBand.Write(buffer, 0, dataLength);
                    oriBand.WriteLength();
                    oriBand.GenerateCCs();

                    bandPanelOri.SetOriBand(oriBand);
                    btn_bandSetError_Click(null, null);
                }
                else
                {
                    System.Media.SystemSounds.Beep.Play();
                }
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private void btn_bandSetError_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(tb_bandErrCount.Text, out int errCount))
            {
                if (int.TryParse(tb_bandErrMaxLength.Text, out int errMaxLength))
                {
                    if (errMaxLength > errCount)
                    {
                        tb_bandErrMaxLength.Text = errCount.ToString();
                        errMaxLength = errCount;
                    }
                    errBand = new Data.Band(oriBand.DataHeight, oriBand.BandLength);
                    oriBand.FlushToBandBuffer();
                    oriBand.bandBuffer.CopyTo(errBand.bandBuffer, 0);
                    StaticMethods.SetBandError(ref errBand, errCount, errMaxLength, rand, out int memd, out int emdc);
                    lb_bandErrCount.Content = $"Err count [{emdc}], max length [{memd}]";
                    errBand.LoadBlocksFromBandBuffer();
                    bandPanelErr.SetOriBand(oriBand);
                    bandPanelErr.SetCurBand(errBand, true);

                    btn_bandCurrectRefresh_Click(null, null);
                }
                else
                {
                    System.Media.SystemSounds.Beep.Play();
                }
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private void BandReCurrectAsync()
        {
            Task.Run(() =>
            {
                if (errBand == null)
                {
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }

                curBand = new Data.Band(errBand.DataHeight, errBand.BandLength);


                errBand.FlushToBandBuffer();
                errBand.bandBuffer.CopyTo(curBand.bandBuffer, 0);
                curBand.LoadBlocksFromBandBuffer();
                bool result = curBand.TryCurrect();

                bool isSame = curBand.BlocksEqual(oriBand.blocks);
                Dispatcher.Invoke(() =>
                {
                    bandPanelCur.SetOriBand(oriBand);
                    bandPanelCur.SetCurBand(curBand, true);
                    cb_bandCurrectPass.IsChecked = result;
                    cb_bandCurrectSame.IsChecked = isSame;
                });
            });
        }
        private void btn_bandCurrectRefresh_Click(object sender, RoutedEventArgs e)
        {
            BandReCurrectAsync();
        }


        #endregion

        #region test - band - batch

        private byte bandDataHeightMin = 4, bandDataHeightMax = 16;
        private int bandLengthMin = 256, bandLengthMax = 4096;
        private void tb_bandDataHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isNotInited)
                return;
            if (TryGetIntPair(tb_bandDataHeight.Text, out int min, out int max))
            {
                bandDataHeightMin = (byte)min;
                bandDataHeightMax = (byte)max;
                RefreshBandDataRate();
                RefreshBandErrRate();
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }
        private void tb_bandLength_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isNotInited)
                return;
            if (TryGetIntPair(tb_bandLength.Text, out int min, out int max))
            {
                bandLengthMin = min;
                bandLengthMax = max;
                RefreshBandDataRate();
                RefreshBandErrRate();
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private bool TryGetIntPair(string str, out int min, out int max)
        {
            min = max = 0;
            string[] parts = str.Split(
                new string[] { " ", "　", "-", "－", ",", "，", "、" },
                StringSplitOptions.RemoveEmptyEntries
            );
            if (parts.Length != 2)
                return false;
            if (!int.TryParse(parts[0], out min))
                return false;
            if (!int.TryParse(parts[1], out max))
                return false;
            if (max < min)
            {
                int tmp = min;
                min = max;
                max = tmp;
            }
            return true;
        }


        private void RefreshBandDataRate()
        {
            long minData = bandDataHeightMin * bandDataHeightMin;
            long maxData = bandDataHeightMax * bandDataHeightMax;
            long minTotal = minData + 3 * bandDataHeightMin;
            long maxTotal = maxData + 3 * bandDataHeightMax;
            SetProgress(minData, minTotal, maxData, maxTotal,
                pgb_bandBatchDataRateBG, pgb_bandBatchDataRateFG, lb_bandBatchDataRate);
        }
        private void SetProgress(long minV, long minT, long maxV, long maxT, ProgressBar pbBG, ProgressBar pbFG, Label lb)
        {
            pbBG.Minimum = 0;
            pbFG.Minimum = 0;
            pbBG.Maximum = maxT;
            pbFG.Maximum = minT;
            pbBG.Value = maxV;
            pbFG.Value = minV;
            lb.Content
                = ((double)minV / minT).ToString("P2") + " ~ "
                + ((double)maxV / maxT).ToString("P2");
        }


        private int bandBatchErrCountMin = 512, bandBatchErrCountMax = 8192;
        private int bandBatchErrMaxLengthMin = 256, bandBatchErrMaxLengthMax = 4096;
        private void tb_bandBatchErrCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isNotInited)
                return;
            if (TryGetIntPair(tb_bandBatchErrCount.Text, out int min, out int max))
            {
                bandBatchErrCountMin = min;
                bandBatchErrCountMax = max;
                RefreshBandErrRate();
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private void tb_bandBatchErrMaxLength_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isNotInited)
                return;
            if (TryGetIntPair(tb_bandBatchErrMaxLength.Text, out int min, out int max))
            {
                bandBatchErrMaxLengthMin = min;
                bandBatchErrMaxLengthMax = max;
                RefreshBandErrRate();
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private void RefreshBandErrRate()
        {
            double eRMin = bandBatchErrCountMin
                / ((bandDataHeightMax * bandDataHeightMax + 3 * bandDataHeightMax) * bandLengthMax);
            double eRMax = bandBatchErrCountMax
                / ((bandDataHeightMin * bandDataHeightMin + 3 * bandDataHeightMin) * bandLengthMin);


            SetProgress(
                bandBatchErrCountMin,
                (bandDataHeightMax * bandDataHeightMax + 3 * bandDataHeightMax) * bandLengthMax,
                bandBatchErrCountMax,
                (bandDataHeightMin * bandDataHeightMin + 3 * bandDataHeightMin) * bandLengthMin,
                pgb_bandBatchErrRateBG,
                pgb_bandBatchErrRateFG,
                lb_bandBatchErrRate);
        }


        private void btn_bandBatchStart_Click(object sender, RoutedEventArgs e)
        {
            if (BandBatchTestAsync_pauseFlag)
            {
                BandBatchTestAsync_pauseFlag = false;
                btn_bandBatchStart.Content = "Stop";
                return;
            }
            if (BandBatchTestAsync_working)
            {
                BandBatchTestAsync_cancelFlag = true;
                btn_bandBatchStart.Content = "Start";
                return;
            }
            BandBatchTestAsync();
        }
        private void btn_bandBatchProgressPause_Click(object sender, RoutedEventArgs e)
        {
            BandBatchTestAsync_pauseFlag = true;
            btn_bandBatchStart.Content = "Start";
        }


        Common.CSVHelper.Writer csvWriterBand;
        private bool BandBatchTestAsync_working = false;
        private object locker3 = new object();
        private object locker4 = new object();
        private List<long> BandBatchCounters;


        private bool BandBatchTestAsync_cancelFlag = false;
        private bool BandBatchTestAsync_pauseFlag = false;
        private List<bool> BandBatchTestAsync_completeFlags;
        private delegate void BandBatchTestReportDelegate(byte blockHeight, int bandLength,
            long errCount, int errMaxLength,
            long totalTimes, long passCur, long passFault, long fail);
        private event BandBatchTestReportDelegate BandBatchTestReport;
        private void BandBatchTestAsync()
        {
            if (BandBatchTestAsync_working)
                return;

            SetBandBatchTBEnable(false);
            BandBatchTestAsync_working = true;
            btn_bandBatchStart.Content = "Stop";

            // 获取所有需要的值
            int steps = -1;
            int tryTimes = -1;
            if (cb_bandBatchFixedSteps.IsChecked == true)
            {
                if (!int.TryParse(tb_bandBatchFixedSteps.Text, out steps))
                    steps = 0;
            }
            if (!int.TryParse(tb_bandBatchTryTimes.Text, out tryTimes))
                tryTimes = 0;
            Task.Run(() =>
            {

                long sv;
                if (steps != 0 && tryTimes > 0)
                {
                    // steps == -1, incr = 1; steps > 0, incr = range/steps;
                    // 开始循环计算
                    byte bhInc;
                    int blInc, becInc, belInc;
                    if (steps < 0)
                    {
                        bhInc = 1;
                        blInc = 1;
                        becInc = 1;
                        belInc = 1;
                        sv = (bandDataHeightMax - bandDataHeightMin + 1)
                            * (bandLengthMax - bandLengthMin + 1)
                            * (bandBatchErrCountMax - bandBatchErrCountMin + 1)
                            * (bandBatchErrMaxLengthMax - bandBatchErrMaxLengthMax + 1);
                    }
                    else if (steps > 1)
                    {
                        int div = steps - 1;
                        bhInc = (byte)Math.Round((double)(bandDataHeightMax - bandDataHeightMin) / div);
                        bhInc = bhInc < 1 ? (byte)1 : bhInc;
                        blInc = (int)Math.Round((double)(bandLengthMax - bandLengthMin) / div);
                        blInc = blInc < 1 ? 1 : blInc;
                        becInc = (int)Math.Round((double)(bandBatchErrCountMax - bandBatchErrCountMin) / div);
                        becInc = becInc < 1 ? 1 : becInc;
                        belInc = (int)Math.Round((double)(bandBatchErrMaxLengthMax - bandBatchErrMaxLengthMin) / div);
                        belInc = belInc < 1 ? 1 : belInc;
                        sv = steps * steps * steps * steps;
                    }
                    else  //  steps == 1
                    {
                        bhInc = (byte)(bandDataHeightMax - bandDataHeightMin + 1);
                        blInc = bandLengthMax - bandLengthMin + 1;
                        becInc = bandBatchErrCountMax - bandBatchErrCountMin + 1;
                        belInc = bandBatchErrMaxLengthMax - bandBatchErrMaxLengthMin + 1;
                        sv = 1;
                    }
                    sv *= tryTimes;

                    BandBatchTestReport -= MainWindow_BandBatchTestReport;
                    BandBatchTestAsync_completeFlags = new List<bool>();
                    BandBatchCounters = new List<long>();

                    //// for test
                    //processorCount = 1;


                    for (int pi = 0; pi < processorCount; pi++)
                    {
                        BandBatchTestAsync_completeFlags.Add(false);
                        BandBatchCounters.Add(0);
                    }

                    BandBatchTestReport += MainWindow_BandBatchTestReport;
                    DateTime now = DateTime.Now;
                    csvWriterBand = new Common.CSVHelper.Writer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "result.Band." + now.ToString("yyMMdd HHmmss") + ".csv"));
                    csvWriterBand.WriteRow(new string[] { "Band batch test, start at " + now.ToLongDateString() + " " + now.ToLongTimeString() });
                    csvWriterBand.WriteRow(null);
                    csvWriterBand.WriteRow(new string[] { "blockHeight", "bandLength", "errCount", "errMaxLength", "totalTimes", "passCur", "passFault", "fail" });


                    for (int pi = 0; pi < processorCount; pi++)
                        BandBatchTestAsync_subThread(pi, bhInc, blInc, becInc, belInc, tryTimes, new Random((int)(DateTime.Now.Ticks + pi)));


                    bool allComplete;
                    long s;
                    int i;
                    int iv1 = BandBatchTestAsync_completeFlags.Count;
                    int iv2 = BandBatchCounters.Count;
                    do
                    {
                        Task.Delay(50).Wait();

                        s = 0;
                        for (i = 0; i < iv2; i++)
                        {
                            s += BandBatchCounters[i];
                        }
                        SetBandBatchTestProgress(s, sv);


                        allComplete = true;
                        for (i = 0; i < iv1; i++)
                        {
                            if (!BandBatchTestAsync_completeFlags[i])
                            {
                                allComplete = false;
                                break;
                            }
                        }
                        if (allComplete)
                            break;
                    }
                    while (true);

                    csvWriterBand.Flush();
                    csvWriterBand.Dispose();
                    if (BandBatchTestAsync_cancelFlag)
                        MessageBox.Show("Band batch test canceled");
                    else
                        MessageBox.Show("Band batch test complete");
                }
                else
                {
                    csvWriterBand.Flush();
                    csvWriterBand.Dispose();
                    MessageBox.Show("No testing.");
                }

                SetBandBatchTBEnable(true);
                BandBatchTestAsync_working = false;
                BandBatchTestAsync_cancelFlag = false;
                BandBatchTestAsync_pauseFlag = false;
            });
        }


        private void BandBatchTestAsync_subThread(int procIdx,
            byte bhInc, int blInc, int becInc, int belInc,
            int tryTimes, Random rand)
        {
            Task.Run(() =>
            {
                Data.Band testBand = null, oriBand = null;
                byte[] dataBuffer;
                long counter = 0, counterPass, counterFail, counterFault;
                byte bh;
                int bl, bec, bel, t, missing1, missing2;
                long dataCapacity;
                bool test;
                for (bh = bandDataHeightMin; bh <= bandDataHeightMax; bh += bhInc)
                {
                    if (BandBatchTestAsync_cancelFlag)
                        break;
                    for (bl = bandLengthMin; bl <= bandLengthMax; bl += blInc)
                    {
                        if (BandBatchTestAsync_cancelFlag)
                            break;
                        dataCapacity = (bh * bh + 3 * bh) * bl;

                        oriBand = new Data.Band(bh, bl);
                        testBand = new Data.Band(bh, bl);
                        dataBuffer = new byte[testBand.MaxBandDataLength];

                        for (bec = bandBatchErrCountMin; bec <= bandBatchErrCountMax; bec += becInc)
                        {
                            if (bec > dataCapacity)
                                break;
                            if (BandBatchTestAsync_cancelFlag)
                                break;

                            for (bel = bandBatchErrMaxLengthMin; bel <= bandBatchErrMaxLengthMax; bel += belInc)
                            {
                                if (BandBatchTestAsync_cancelFlag)
                                    break;
                                test = counter % processorCount != procIdx;
                                counter++;
                                if (test)
                                {
                                    continue;
                                }
                                if (bel > oriBand.MaxBandFullLength)
                                {
                                    //lock (locker3)
                                    //{
                                    BandBatchCounters[procIdx] += tryTimes;
                                    //}
                                    continue;
                                }


                                counterPass = 0; counterFail = 0; counterFault = 0;
                                for (t = 0; t < tryTimes; t++)
                                {
                                    if (BandBatchTestAsync_cancelFlag)
                                        break;

                                    while (BandBatchTestAsync_pauseFlag)
                                    {
                                        Task.Delay(100).Wait();
                                    }

                                    rand.NextBytes(dataBuffer);
                                    oriBand.Position = 0;
                                    oriBand.Write(dataBuffer, 0, dataBuffer.Length);
                                    oriBand.WriteLength();
                                    oriBand.GenerateCCs();
                                    oriBand.FlushToBandBuffer();

                                    oriBand.bandBuffer.CopyTo(testBand.bandBuffer, 0);
                                    if (StaticMethods.SetBandError(ref testBand, bec, bel, rand, out missing1, out missing2))
                                    {
                                        testBand.LoadBlocksFromBandBuffer();
                                        if (testBand.TryCurrect())
                                        {
                                            counterPass++;
                                            if (!testBand.BlocksEqual(oriBand.blocks))
                                                counterFault++;
                                        }
                                        else
                                        {
                                            counterFail++;
                                        }

                                        //lock (locker3)
                                        //{
                                        BandBatchCounters[procIdx]++;
                                        //}
                                    }
                                }
                                BandBatchTestReport?.Invoke(bh, bl, bec, bel, t, counterPass, counterFault, counterFail);
                            }
                        }
                    }
                }
                lock (locker4)
                {
                    BandBatchTestAsync_completeFlags[procIdx] = true;
                }
            });
        }
        private void MainWindow_BandBatchTestReport(byte blockHeight, int bandLength, long errCount, int errMaxLength, long totalTimes, long passCur, long passFault, long fail)
        {
            Dispatcher.Invoke(() =>
            {
                csvWriterBand.WriteRow(new string[]
                {
                    blockHeight.ToString(), bandLength.ToString(),
                    errCount.ToString(), errMaxLength.ToString(),
                    totalTimes.ToString(), passCur.ToString(),
                    passFault.ToString(), fail.ToString()
                });
            });
        }
        private void SetBandBatchTestProgress(long cur, long max)
        {
            Dispatcher.Invoke(() =>
            {
                pgb_bandBatchProgress.Maximum = max;
                pgb_bandBatchProgress.Value = cur;
                lb_bandBatchProgress.Content = cur.ToString() + " / " + max.ToString();
            });
        }
        private void SetBandBatchTBEnable(bool isEnabled)
        {
            Dispatcher.Invoke(() =>
            {
                tb_bandDataHeight.IsEnabled = isEnabled;
                tb_bandLength.IsEnabled = isEnabled;
                tb_bandBatchErrCount.IsEnabled = isEnabled;
                tb_bandBatchErrMaxLength.IsEnabled = isEnabled;
                tb_bandBatchFixedSteps.IsEnabled = isEnabled;
                cb_bandBatchFixedSteps.IsEnabled = isEnabled;
                tb_bandBatchTryTimes.IsEnabled = isEnabled;
            });
        }

        #endregion




    }
}
