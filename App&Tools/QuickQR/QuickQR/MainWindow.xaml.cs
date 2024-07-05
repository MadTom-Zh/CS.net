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

using AForge.Video.DirectShow;
using MadTomDev.UI;
using Bitmap = System.Drawing.Bitmap;
using BitmapData = System.Drawing.Imaging.BitmapData;

namespace QuickQR
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

        #region cameras
        FilterInfoCollection cams;
        private void ReloadCams()
        {
            cams = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        }
        private void UnloadCams()
        {
            StopCurCam(false);
            cams = null;
            curCamIdx = -1;
            tb_camIdx.Text = $"";
        }
        int curCamIdx = -1;
        bool curCamWorking = false;
        VideoCaptureDevice curCam;
        private void StopCurCam(bool unLoadImage = true)
        {
            if (curCam == null)
                return;
            curCam.NewFrame -= CurCam_NewFrame;
            curCam.SignalToStop();
            curCam = null;
            curCamWorking = false;
            Dispatcher.Invoke(() =>
            {
                btn_screen.IsEnabled = true;
                btn_clipboard.IsEnabled = true;
                btn_file.IsEnabled = true;
                btn_cam.Content = "摄像头";
                if (unLoadImage)
                    img_in.Source = null;
            });
        }
        private void StartNextCam()
        {
            if (cams == null || cams.Count <= 0)
                return;
            StopCurCam();
            curCamIdx++;
            if (curCamIdx >= cams.Count)
                curCamIdx = 0;
            curCam = new VideoCaptureDevice(cams[curCamIdx].MonikerString);
            curCam.NewFrame += CurCam_NewFrame;
            curCam.Start();
            curCamWorking = true;
            Dispatcher.Invoke(() =>
            {
                btn_screen.IsEnabled = false;
                btn_clipboard.IsEnabled = false;
                btn_file.IsEnabled = false;
                btn_cam.Content = "截图";
                tb_camIdx.Text = $"{curCamIdx + 1} / {cams.Count}  {cams[curCamIdx].Name}";
            });
        }
        private void CurCam_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            Dispatcher.Invoke(() =>
            {
                if (curCamWorking)
                    SetUIImage(eventArgs.Frame, img_in);
            });
        }
        private async void btn_camNext_Click(object sender, RoutedEventArgs e)
        {
            if (curCamWorking)
            {
                btn_cam.IsEnabled = false;
                btn_camNext.IsEnabled = false;
                await Task.Run(() =>
                {
                    StartNextCam();
                });
                btn_cam.IsEnabled = true;
                btn_camNext.IsEnabled = true;
            }
        }
        private async void btn_cam_Click(object sender, RoutedEventArgs e)
        {
            btn_cam.IsEnabled = false;
            await Task.Run(async () =>
            {

                if (curCamWorking)
                {
                    // 截图
                    ImageSource imageTmp = img_in.Source.Clone();
                    UnloadCams();

                    // 格式转换
                    MemoryStream ms = new MemoryStream();
                    BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imageTmp));
                    encoder.Save(ms);
                    Bitmap img = new Bitmap(ms);

                    // 解码
                    Decode(img);
                    UnloadCams();
                    tb_camIdx.Text = "";
                }
                else
                {
                    // 启动摄像头
                    SetResultTextAsync("正在获取摄像头……", Cursors.AppStarting);
                    await Task.Run(() =>
                    {
                        ReloadCams();
                        Dispatcher.Invoke(() =>
                        {
                            this.Cursor = Cursors.Arrow;
                            if (cams.Count > 0)
                            {
                                StartNextCam();
                            }
                            else
                            {
                                UnloadCams();
                                MessageBox.Show("没找到摄像头");
                            }
                        });
                    });
                }
                Dispatcher.Invoke(() =>
                { btn_cam.IsEnabled = true; });
            });
        }
        #endregion

        #region show image, decode

        ImageSourceConverter imageSourceConverter = new ImageSourceConverter();
        MemoryStream stream = new MemoryStream();
        private void SetUIImage(Bitmap img, Image toUI)
        {
            stream?.Dispose();
            stream = new MemoryStream();
            img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            toUI.Source = (ImageSource)imageSourceConverter.ConvertFrom(stream);
        }
        private void SetUIImage(BitmapSource img, Image toUI)
        {
            toUI.Source = img;
        }
        private void Decode(Bitmap img)
        {
            SetResultTextAsync("正在解码……");
            tb_decode.InvalidateVisual();

            ZXing.MultiFormatReader reader = new ZXing.MultiFormatReader();
            ZXing.BitmapLuminanceSource source = new ZXing.BitmapLuminanceSource(img);
            ZXing.Common.HybridBinarizer bizer = new ZXing.Common.HybridBinarizer(source);
            ZXing.BinaryBitmap bitmap = new ZXing.BinaryBitmap(bizer);
            ZXing.Result result = reader.decodeWithState(bitmap);

            StringBuilder strBdr = new StringBuilder();
            if (result == null)
            {
                strBdr.Append("解析无内容");
            }
            else
            {
                strBdr.Append(result.Text);
                strBdr.Append(Environment.NewLine + Environment.NewLine);
                strBdr.Append("========== 附加信息 ==========");
                strBdr.Append(Environment.NewLine);
                strBdr.Append("编码格式： ");
                strBdr.Append(result.BarcodeFormat);
                strBdr.Append(Environment.NewLine);
                strBdr.Append("位点数量： ");
                strBdr.Append(result.NumBits);
                strBdr.Append(Environment.NewLine);
                strBdr.Append("时间戳： ");
                strBdr.Append(result.Timestamp);
            }
            tb_decode.Text = strBdr.ToString();
        }
        private void Decode(BitmapSource img)
        {
            Decode(MadTomDev.UI.QuickGraphics.BitmapSourceToBitmap(img));
        }
        private void SetResultTextAsync(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                tb_decode.Text = msg;
            });
        }
        private void SetResultTextAsync(string msg, Cursor cursor)
        {
            this.Dispatcher.Invoke(() =>
            {
                tb_decode.Text = msg;
                this.Cursor = cursor;
            });
        }
        #endregion

        #region open image file

        Microsoft.Win32.OpenFileDialog openfileDlg = new Microsoft.Win32.OpenFileDialog()
        { Filter = "Supported|*.bmp;*.jpg;*.jpeg;*.png|All|*.*", Multiselect = false, };
        private void btn_file_Click(object sender, RoutedEventArgs e)
        {
            if (openfileDlg.ShowDialog() == true)
            {
                using (Bitmap img
                    = (Bitmap)Bitmap.FromFile(openfileDlg.FileName))
                {
                    SetUIImage(img, img_in);
                    Decode(img);
                }
            }
        }

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //ssCore?.Cancel();
            UnloadCams();
        }

        #region screen shot

        private void btn_screen_Click(object sender, RoutedEventArgs e)
        {
            SetResultTextAsync("正在启动截屏程序，请不要动鼠标、键盘……", Cursors.AppStarting);
            ScreenSelectorSimple sss = new ScreenSelectorSimple()
            { Owner = this, };
            if (sss.ShowDialog() == true)
            {
                BitmapSource img = sss.SelectedImage;
                SetUIImage(img, img_in);
                Decode(img);
            }
            this.Cursor = Cursors.Arrow;
        }

        #endregion

        #region image from clipboard

        private void btn_clipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Bitmap img = QuickGraphics.Image.FromClipboard_Bitmap();
                if (img != null)
                {
                    SetUIImage(img, img_in);
                    Decode(img);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }
        #endregion

        #region copy image, save image

        private void btn_sourceImgCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyImage(img_in);
        }
        private void CopyImage(Image fromUI)
        {
            if (fromUI.Source == null)
            {
                MessageBox.Show("没有图片");
                return;
            }

            Clipboard.SetImage((BitmapSource)fromUI.Source);
        }

        private void btn_sourceImgSave_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(img_in);
        }
        Microsoft.Win32.SaveFileDialog saveFileDlg;
        private void SaveImage(Image fromUI)
        {
            if (fromUI.Source == null)
            {
                MessageBox.Show("没有图片");
                return;
            }

            if (saveFileDlg == null)
                saveFileDlg = new Microsoft.Win32.SaveFileDialog();

            saveFileDlg.Filter = "PNG|*.png";
            if (saveFileDlg.ShowDialog() == true)
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)fromUI.Source));
                using (Stream fileStream = File.OpenWrite(saveFileDlg.FileName))
                    encoder.Save(fileStream);
            }
        }

        #endregion

        #region qr encode
        private void btn_encode_Click(object sender, RoutedEventArgs e)
        {
            ZXing.BarcodeWriter encoder = new ZXing.BarcodeWriter()
            { Format = ZXing.BarcodeFormat.QR_CODE, };
            SetUIImage(encoder.Write(tb_input.Text), img_out);
        }

        private void btn_outImgCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyImage(img_out);
        }

        private void btn_outImgSave_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(img_out);
        }
        #endregion
    }
}
