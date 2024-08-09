using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MadTomDev.App.BiliDmEnginInterface;
using MadTomDev.Common;
using System.IO;
using Microsoft.Web.WebView2.Core;

namespace MadTomDev.App.BiliDMEngin20240731
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, EnginInterface
    {
        public MainWindow()
        {
            InitializeComponent();

            logger = new Logger()
            {
                BaseFileNamePre = "EnginErr",
            };


            timer = new Timer(new TimerCallback(TimerTick));
            timer.Change(300, 300);

        }
        private string urlFile = "liveRoomAddress.txt";
        private string urlFileContent;

        private void CoreWebView2_NavigationStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            Disconnect(new Exception("Re-Navigating to URI."));
        }
        private void CoreWebView2_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            Connect();
        }

        private void CoreWebView2_NewWindowRequested(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
        }

        public event EnginInterface.NewBulletDelegate NewBullet;
        public event EnginInterface.ConnectedDelegate Connected;
        public event EnginInterface.DisconnectedDelegate Disconnected;

        private bool isConnected = false;
        public void Connect()
        {
            if (!isConnected)
            {
                isConnected = true;
                Connected.Invoke(this);
            }
        }
        public void Disconnect(Exception? err = null)
        {
            isTimerBusy = false;
            if (isConnected)
            {
                isConnected = false;
                Disconnected?.Invoke(this, err);
            }
        }
        public void Dispose()
        {
            this.Close();
        }

        private void tb_url_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_go_Click(sender, new RoutedEventArgs());
            }
        }

        private void btn_go_Click(object sender, RoutedEventArgs e)
        {
            if (urlFileContent != tb_url.Text)
            {
                urlFileContent = tb_url.Text;
                File.WriteAllText(urlFile, urlFileContent);
            }
            webView2.CoreWebView2.Navigate(urlFileContent);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Disconnect();
        }


        Timer timer;
        bool isWebViewFirstInit = true;
        bool isTimerBusy = false;
        Common.Logger logger;
        private void TimerTick(object? o)
        {
            if (isWebViewFirstInit)
            {
                Dispatcher.Invoke( async() =>
                {
                    if (webView2 != null )
                    {
                        isWebViewFirstInit = false;
                        var env = await CoreWebView2Environment.CreateAsync(null, "C:\\temp");
                        await webView2.EnsureCoreWebView2Async(env);
                        webView2.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
                        webView2.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
                        webView2.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                        if (File.Exists(urlFile))
                        {
                            urlFileContent = File.ReadAllText(urlFile);
                            tb_url.Text = urlFileContent;
                            btn_go_Click(null, null);
                        }
                    }
                });
                return;
            }
            if (!isConnected)
            {
                return;
            }
            if (isTimerBusy)
            {
                return;
            }

            if (NewBullet == null)
            {
                return;
            }
            isTimerBusy = true;
            Dispatcher.Invoke(async () =>
            {
                CoreWebView2 wb = webView2.CoreWebView2;
                string html = await GetDivHtml(wb, flag_divId_chatItems);
                if (html != null && html.Length > 65)
                {
                    await ClearDivChildren(wb, flag_divId_chatItems);
                    if (!TryFireDanmakus(ref html, out Exception err))
                    {
                        logger.Log(err + Environment.NewLine + html + Environment.NewLine);
                    }
                }

                html = await GetDivHtml(wb, flag_divId_brushPrompt);
                if (html != null && html.Length > 100)
                {
                    // 有进入，也有点赞
                    await ClearDivChildren(wb, flag_divId_brushPrompt);
                    HtmlBasicProcess(ref html);
                    //int strIdx1 = html.IndexOf("interact-name") + 13;
                    //strIdx1 = html.IndexOf(">", strIdx1)+1;
                    //int strIdx2 = html.IndexOf("<", strIdx1);
                    Bullet newBullet = new Bullet(Bullet.BulletType.Sys)
                    {
                        sysMsg = HtmlRemoveAllTags(html, true),
                    };
                    NewBullet.Invoke(this, newBullet);
                }


                html = await GetDivHtml(wb, flag_divId_penuryGiftMsg);
                if (html != null && html.Length > 105)
                {
                    await ClearDivChildren(wb, flag_divId_brushPrompt);
                    logger.Log("礼物列表有内容" + Environment.NewLine + html + Environment.NewLine);
                    Bullet newBullet = new Bullet(Bullet.BulletType.Sys)
                    {
                        sysMsg = "出现礼物",
                    };
                    NewBullet.Invoke(this, newBullet);
                }

                html = await GetDivHtml(wb, flag_divId_lotteryGiftToast);
                if (html != null && html.Length > 112)
                {
                    await ClearDivChildren(wb, flag_divId_lotteryGiftToast);
                    logger.Log("干杯列表有内容" + Environment.NewLine + html + Environment.NewLine);
                    Bullet newBullet = new Bullet(Bullet.BulletType.Sys)
                    {
                        sysMsg = "出现干杯",
                    };
                    NewBullet.Invoke(this, newBullet);
                }


                isTimerBusy = false;
            });
        }

        private string flag_divId_chatItems = "chat-items";
        private string flag_divId_brushPrompt = "brush-prompt";
        private string flag_divId_penuryGiftMsg = "penury-gift-msg";
        private string flag_divId_lotteryGiftToast = "lottery-gift-toast";

        string htmlPre_chatItems = "";
        string htmlPre_brushPrompt = "";
        string htmlPre_penuryGiftMsg = "";
        string htmlPre_lotteryGiftToast = "";

        private Task<string> GetDivHtml(Microsoft.Web.WebView2.Core.CoreWebView2 wb, string divId)
        {
            return wb.ExecuteScriptAsync("document.getElementById('" + divId + "').outerHTML");
        }
        private Task<string> ClearDivChildren(Microsoft.Web.WebView2.Core.CoreWebView2 wb, string divId)
        {
            return wb.ExecuteScriptAsync("document.getElementById('" + divId + "').innerHTML = '';");
        }
        private string Unicode2String(string source)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(source,
                x => Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)).ToString());
        }

        private void HtmlBasicProcess(ref string str)
        {
            str = Unicode2String(str);
            if (str.StartsWith("\"") && str.EndsWith("\""))
            {
                str = str.Substring(1, str.Length - 2);
            }
            str = str.Replace("\\n", Environment.NewLine);
            str = str.Replace("\\\"", "\"");
        }
        private List<string> HtmlSplitByTagStr(string str, string tagStr)
        {
            List<string> result = new List<string>();
            int idx1 = str.IndexOf(tagStr);
            if (idx1 < 0)
            {
                return result;
            }
            int idx2, idx3;
            while (true)
            {
                idx2 = FindBackwardIndexOf(ref str, '<',  idx1);
                idx1 = str.IndexOf(tagStr, idx1 + 1);
                if (idx1 < 0)
                {
                    result.Add(str.Substring(idx2));
                    break;
                }
                idx3 = FindBackwardIndexOf(ref str, '<', idx1);
                result.Add(str.Substring(idx2, idx3 - idx2));
            }
            return result;

            int FindBackwardIndexOf(ref string str,char findC, int idx)
            {
                if (idx < 0)
                {
                    return -1;
                }
                while ( idx>=0)
                {
                    if (str[idx--] == findC)
                    {
                        return ++idx;
                    }
                }
                return -1;
            }
        }
        private string? HtmlRemoveAllTags(string? str, bool addSpace = false)
        {
            if (str == null)
            {
                return null;
            }
            int idx1 = str.IndexOf("<"),
                idx2 = str.IndexOf(">");
            int idxCur = 0;
            if (idx1 < 0 && idx2 < 0)
            {
                return str;
            }
            else if (idx1 < 0)
            {
                return str.Substring(idx2 + 1);
            }
            else if (idx2 < 0)
            {
                return str.Substring(0, idx1);
            }
            else if (idx2 < idx1)
            {
                idxCur = idx2 + 1;
            }
            else
            {
                idxCur = 0;
            }
            StringBuilder strBdr = new StringBuilder();
            while (true)
            {
                idx1 = str.IndexOf("<", idxCur);
                if (idx1 < 0)
                {
                    if (idxCur < str.Length - 1)
                    {
                        strBdr.Append(str.Substring(idxCur));
                    }
                    break;
                }
                else if (idx1 == idxCur)
                {
                    idxCur = str.IndexOf(">", idxCur) + 1;
                }
                else
                {
                    strBdr.Append(str.Substring(idxCur, idx1 - idxCur));
                    if (addSpace)
                    {
                        strBdr.Append(" ");
                    }
                    idxCur = str.IndexOf(">", idx1) + 1;
                }
            }
            return strBdr.ToString();
        }
        private int GetNumber(string str)
        {
            StringBuilder strBdr = new StringBuilder();
            char c;
            for (int i = 0, iv = str.Length; i < iv; ++i)
            {
                c = str[i];
                if ('0' <= c && c <= '9')
                {
                    strBdr.Append(c);
                }
            }
            return int.Parse(strBdr.ToString());
        }
        private bool TryFireDanmakus(ref string html, out Exception err)
        {
            err = null;
            bool result = true;
            HtmlBasicProcess(ref html);
            int strIdx1, strIdx2;
            try
            {
                // 每条弹幕开头标记为 chat-item
                List<string> dmItems = HtmlSplitByTagStr(html, "chat-item ");
                foreach (string dmItem in dmItems)
                {
                    // 普通用户弹幕
                    if (dmItem.Contains("danmaku-item"))
                    {
                        // 普通用户弹幕，开头为 danmaku-item，会包含如下信息
                        //data-uid, data-uname, data-danmaku, data-timestamp

                        strIdx1 = dmItem.IndexOf("data-uid=") + 10;
                        strIdx2 = dmItem.IndexOf("\"", strIdx1+1);
                        Bullet newBullet = new Bullet(Bullet.BulletType.Msg)
                        {
                            userID = dmItem.Substring(strIdx1, strIdx2 - strIdx1),
                        };
                        strIdx1 = dmItem.IndexOf("data-uname=") + 12;
                        strIdx2 = dmItem.IndexOf("\"", strIdx1);
                        newBullet.userName = dmItem.Substring(strIdx1, strIdx2 - strIdx1);
                        strIdx1 = dmItem.IndexOf("data-danmaku=") + 14;
                        strIdx2 = dmItem.IndexOf("\"", strIdx1);
                        newBullet.userMsg = dmItem.Substring(strIdx1, strIdx2 - strIdx1);


                        strIdx1 = dmItem.IndexOf("fans-medal-content");
                        if (strIdx1 >= 0)
                        {
                            // 如果该用户有徽章，则徽章的标记为 fans-medal-content ，随后跟徽章等级 fans-medal-level
                            strIdx1 = dmItem.IndexOf(">", strIdx1) + 1;
                            strIdx2 = dmItem.IndexOf("<", strIdx1);
                            string medalTx = dmItem.Substring(strIdx1, strIdx2 - strIdx1);

                            strIdx1 = dmItem.IndexOf("fans-medal-level");
                            strIdx1 = dmItem.IndexOf("</", strIdx1) - 10;
                            strIdx1 = dmItem.IndexOf(">", strIdx1) + 1;
                            strIdx2 = dmItem.IndexOf("<", strIdx1);
                            string medalLv = dmItem.Substring(strIdx1, strIdx2 - strIdx1);
                            newBullet.userTitle = medalLv + "级 " + medalTx;
                        }

                        NewBullet.Invoke(this, newBullet);
                    }
                    // 系统消息
                    else if (dmItem.Contains("convention-msg"))
                    {
                        // 系统消息，开头 convention-msg

                        strIdx1 = dmItem.IndexOf("convention-msg");
                        strIdx1 = dmItem.IndexOf(">", strIdx1) + 1;
                        strIdx2 = dmItem.IndexOf("<", strIdx1);
                        Bullet newBullet = new Bullet(Bullet.BulletType.Sys)
                        {
                            sysMsg = dmItem.Substring(strIdx1, strIdx2 - strIdx1),
                        };

                        NewBullet.Invoke(this, newBullet);
                    }
                    // 入前三消息
                    else if (dmItem.Contains("top3-notice"))
                    {
                        // top3-notice
                        Bullet newBullet = new Bullet(Bullet.BulletType.Sys)
                        {
                            sysMsg = HtmlRemoveAllTags(dmItem),
                        };

                        NewBullet.Invoke(this, newBullet);
                    }
                    // 礼物
                    else if (dmItem.Contains("gift-item"))
                    {
                        strIdx1 = dmItem.IndexOf("data-uid=") + 10;
                        strIdx2 = dmItem.IndexOf("\"", strIdx1);
                        Bullet newBullet = new Bullet(Bullet.BulletType.Msg)
                        {
                            userID = dmItem.Substring(strIdx1, strIdx2 - strIdx1),
                        };
                        strIdx1 = dmItem.IndexOf("data-uname=") + 12;
                        strIdx2 = dmItem.IndexOf("\"", strIdx1);
                        newBullet.userName = dmItem.Substring(strIdx1, strIdx2 - strIdx1);

                        strIdx1 = dmItem.IndexOf("gift-name") + 9;
                        strIdx1 = dmItem.IndexOf(">", strIdx1) + 1;
                        strIdx2 = dmItem.IndexOf("<", strIdx1);
                        newBullet.giftName = dmItem.Substring(strIdx1, strIdx2 - strIdx1);

                        strIdx1 = dmItem.IndexOf("gift-total-count") + 16;
                        strIdx1 = dmItem.IndexOf(">", strIdx1) + 1;
                        strIdx2 = dmItem.IndexOf("<", strIdx1);
                        newBullet.giftCount = GetNumber(dmItem.Substring(strIdx1, strIdx2 - strIdx1));
                    }
                    else
                    {
                        err = new Exception("无法处理");
                        result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                err = ex;
            }


            return result;
        }
    }
}