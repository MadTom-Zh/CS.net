using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for MessageBoxEx.xaml
    /// </summary>
    public partial class MessageBoxEx : Window
    {
        public MessageBoxEx()
        {
            InitializeComponent();
        }

        public string Message
        {
            set => Dispatcher.Invoke(() => tb_msg.Text = value);
            get => tb_msg.Text;
        }


        #region icons
        public int IconNo
        {
            set
            {
                if (value < 0)
                    return;
                Dispatcher.Invoke(() =>
                {
                    img.Source = Common.IconHelper.Shell32Icons.Instance.GetIcon(value);
                });
            }
        }

        public enum CommonIcons
        {
            InfoRound = 277,
            Setting = 316,
            Picture = 325,
            Clock = 265,
            QuestionRound = 23,
            Keys = 104,
            Star = 43,
            StarTilt = 208,
            StarShift = 320,
            Stop = 109,
            Tree = 41,
            CrossRed = 131,
            CheckRed = 144,
            ExclamationTriangle = 77,
            ExclamationShield = 244,
            Search = 268,
        }

        #endregion


        #region buttons
        public string[] Buttons
        {
            set
            {
                Dispatcher.Invoke(() =>
                {
                    wPanelBtns.Children.Clear();
                    if (value == null || value.Length == 0)
                        return;
                    string b;
                    Button newBtn = null;
                    double txFontSize = tb_msg.FontSize * 1.5;
                    for (int i = 0, iv = value.Length; i < iv; i++)
                    {
                        b = value[i];
                        newBtn = new Button()
                        {
                            Content = b,
                            Tag = i,
                            FontSize = txFontSize,
                            Height = txFontSize * 2,
                            Width = txFontSize * 8,
                            Margin = new Thickness(4),
                        };
                        newBtn.Click += Btn_Click;
                        newBtn.GotFocus += NewBtn_GotFocus;
                        newBtn.LostFocus += NewBtn_LostFocus;
                        wPanelBtns.Children.Add(newBtn);
                    }
                    btnBrushDefault = newBtn.Background;
                });
            }
        }

        private Brush btnBrushDefault;
        private void NewBtn_GotFocus(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            btn.Background = SystemColors.ControlBrush;
        }

        private void NewBtn_LostFocus(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            btn.Background = btnBrushDefault;
        }

        public int DefaultButton { set; private get; }

        private bool inited = false;
        private void Window_Activated(object sender, EventArgs e)
        {
            if (inited)
                return;
            inited = true;
            if (DefaultButton < 0)
                return;
            if (wPanelBtns.Children.Count >= DefaultButton + 1)
            {
                if (wPanelBtns.Children[DefaultButton] is Button)
                {
                     wPanelBtns.Children[DefaultButton].Focus();
                }
            };
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            List<int> checks = new List<int>();
            CheckBox cb;
            foreach (UIElement ui in wPanelChecks.Children)
            {
                if (ui is CheckBox)
                {
                    cb = (CheckBox)ui;
                    if (cb.IsChecked == true)
                        checks.Add((int)cb.Tag);
                }
            }
            checks.Sort();
            SelectedCheckOptions = checks.ToArray();


            RadioButton rb;
            foreach (UIElement ui in wPanelRadios.Children)
            {
                if (ui is RadioButton)
                {
                    rb = (RadioButton)ui;
                    if (rb.IsChecked == true)
                    {
                        SelectedRadioOption = (int)rb.Tag;
                        break;
                    }
                }
            }


            SelectedButton = (int)((Button)sender).Tag;


            this.Close();
        }

        public int SelectedButton { private set; get; } = -1;


        #endregion


        #region check boxes
        public string[] CheckOptions
        {
            set
            {
                wPanelChecks.Children.Clear();
                Dispatcher.Invoke(() =>
                {
                    string b;
                    CheckBox newCB;
                    for (int i = 0, iv = value.Length; i < iv; i++)
                    {
                        b = value[i];
                        newCB = new CheckBox()
                        {
                            Content = b,
                            Tag = i,
                            Margin = new Thickness(16, 5, 0, 0),
                        };
                        wPanelChecks.Children.Add(newCB);
                    }
                });
            }
        }

        public int[] DefaultCheckOptions
        {
            set
            {
                if (value == null)
                    return;
                Dispatcher.Invoke(() =>
                {
                    foreach (int i in value)
                    {
                        if (wPanelChecks.Children.Count >= i + 1)
                        {
                            if (wPanelChecks.Children[i] is CheckBox)
                                ((CheckBox)wPanelChecks.Children[i]).IsChecked = true;
                        }
                    }
                });
            }
        }

        public int[] SelectedCheckOptions { private set; get; }


        #endregion


        #region radios
        public string[] RadioOptions
        {
            set
            {
                wPanelRadios.Children.Clear();
                Dispatcher.Invoke(() =>
                {
                    string b;
                    RadioButton newRB;
                    for (int i = 0, iv = value.Length; i < iv; i++)
                    {
                        b = value[i];
                        newRB = new RadioButton()
                        {
                            Content = b,
                            Tag = i,
                            Margin = new Thickness(16, 5, 0, 0),
                        };
                        wPanelRadios.Children.Add(newRB);
                    }
                });
            }
        }

        public int DefaultRadioOption
        {
            set
            {
                if (value < 0)
                    return;
                Dispatcher.Invoke(() =>
                {
                    if (wPanelRadios.Children.Count >= value + 1)
                    {
                        if (wPanelRadios.Children[value] is RadioButton)
                            ((RadioButton)wPanelRadios.Children[value]).IsChecked = true;
                    }
                });
            }
        }

        public int SelectedRadioOption { private set; get; } = -1;

        #endregion


        #region static show dialog

        /// <summary>
        /// 显示消息窗口，带单选项和多选项
        /// </summary>
        /// <param name="parent">父窗口，继承置顶属性</param>
        /// <param name="msg">消息主题</param>
        /// <param name="title">窗口标题</param>
        /// <param name="btns">按钮</param>
        /// <param name="defaultBtnIdx">默认按钮索引</param>
        /// <param name="iconNo">图标序号</param>
        /// <param name="radios">单选按钮</param>
        /// <param name="defaultRadioIdx">默认选中的单选按钮</param>
        /// <param name="selectedRadioIdx">返回用户选中的单选按钮索引</param>
        /// <param name="checks">多选按钮</param>
        /// <param name="defaultCheckIdxes">默认选中的多选按钮</param>
        /// <param name="selectedCheckIdxes">返回用户选中的多选按钮的索引数组</param>
        /// <returns>-1直接关闭，其他对应按钮索引</returns>
        public static int ShowDialog(Window parent, string msg, string title, string[] btns, int defaultBtnIdx, int iconNo,
            string[] radios, int defaultRadioIdx, out int selectedRadioIdx,
            string[] checks, int[] defaultCheckIdxes, out int[] selectedCheckIdxes)
        {

            MessageBoxEx dlg = new MessageBoxEx()
            {
                Topmost = parent == null ? false : parent.Topmost,

                Title = title,
                Message = msg,
                IconNo = iconNo,

                CheckOptions = checks,

                RadioOptions = radios,

                Buttons = btns,
            };
            dlg.DefaultCheckOptions = defaultCheckIdxes;
            dlg.DefaultRadioOption = defaultRadioIdx;
            dlg.DefaultButton = defaultBtnIdx;

            dlg.ShowDialog();


            selectedCheckIdxes = dlg.SelectedCheckOptions;
            selectedRadioIdx = dlg.SelectedRadioOption;

            return dlg.SelectedButton;
        }


        public static int ShowDialog(Window parent, string msg, string title, string[] btns, int defaultBtnIdx, CommonIcons iconEnum,
            string[] radios, int defaultRadioIdx, out int selectedRadioIdx,
            string[] checks, int[] defaultCheckIdxes, out int[] selectedCheckIdxes
            )
        {
            return ShowDialog(parent, msg, title, btns, defaultBtnIdx, (int)iconEnum,
                radios, defaultRadioIdx, out selectedRadioIdx,
                checks, defaultCheckIdxes, out selectedCheckIdxes);
        }


        public static int ShowDialog(string msg, string title, string[] btns, int defaultBtnIdx, int iconNo,
            string[] radios, int defaultRadioIdx, out int selectedRadioIdx,
            string[] checks, int[] defaultCheckIdxes, out int[] selectedCheckIdxes
            )
        {
            return ShowDialog(null, msg, title, btns, defaultBtnIdx, iconNo,
                radios, defaultRadioIdx, out selectedRadioIdx,
                checks, defaultCheckIdxes, out selectedCheckIdxes);
        }
        public static int ShowDialog(string msg, string title, string[] btns, int defaultBtnIdx, CommonIcons iconEnum,
            string[] radios, int defaultRadioIdx, out int selectedRadioIdx,
            string[] checks, int[] defaultCheckIdxes, out int[] selectedCheckIdxes)
        {
            return ShowDialog(null, msg, title, btns, defaultBtnIdx, (int)iconEnum,
                radios, defaultRadioIdx, out selectedRadioIdx,
                checks, defaultCheckIdxes, out selectedCheckIdxes);
        }




        public static int ShowDialog(Window parent, string msg, string title, string[] btns, int defaultBtnIdx, int iconNo,
            string[] radios, int defaultRadioIdx, out int selectedRadioIdx)
        {
            return ShowDialog(parent, msg, title, btns, defaultBtnIdx, iconNo,
                radios, defaultRadioIdx, out selectedRadioIdx,
                null, null, out int[] missing);
        }
        public static int ShowDialog(Window parent, string msg, string title, string[] btns, int defaultBtnIdx, CommonIcons iconEnum,
            string[] radios, int defaultRadioIdx, out int selectedRadioIdx)
        {
            return ShowDialog(parent, msg, title, btns, defaultBtnIdx, (int)iconEnum,
                radios, defaultRadioIdx, out selectedRadioIdx,
                null, null, out int[] missing);
        }

        public static int ShowDialog(string msg, string title, string[] btns, int defaultBtnIdx, int iconNo,
            string[] radios, int defaultRadioIdx, out int selectedRadioIdx)
        {
            return ShowDialog(msg, title, btns, defaultBtnIdx, iconNo,
                radios, defaultRadioIdx, out selectedRadioIdx,
                null, null, out int[] missing);
        }
        public static int ShowDialog(string msg, string title, string[] btns, int defaultBtnIdx, CommonIcons iconEnum,
            string[] radios, int defaultRadioIdx, out int selectedRadioIdx)
        {
            return ShowDialog(msg, title, btns, defaultBtnIdx, (int)iconEnum,
                radios, defaultRadioIdx, out selectedRadioIdx,
                null, null, out int[] missing);
        }




        public static int ShowDialog(Window parent, string msg, string title, string[] btns, int defaultBtnIdx, int iconNo,
            string[] checks, int[] defaultCheckIdxes, out int[] selectedCheckIdxes)
        {
            return ShowDialog(parent, msg, title, btns, defaultBtnIdx, iconNo,
                null, -1, out int missing,
                checks, defaultCheckIdxes, out selectedCheckIdxes);
        }
        public static int ShowDialog(Window parent, string msg, string title, string[] btns, int defaultBtnIdx, CommonIcons iconEnum,
            string[] checks, int[] defaultCheckIdxes, out int[] selectedCheckIdxes)
        {
            return ShowDialog(parent, msg, title, btns, defaultBtnIdx, (int)iconEnum,
                null, -1, out int missing,
                checks, defaultCheckIdxes, out selectedCheckIdxes);
        }

        public static int ShowDialog(string msg, string title, string[] btns, int defaultBtnIdx, int iconNo,
            string[] checks, int[] defaultCheckIdxes, out int[] selectedCheckIdxes)
        {
            return ShowDialog(msg, title, btns, defaultBtnIdx, iconNo,
                null, -1, out int missing,
                checks, defaultCheckIdxes, out selectedCheckIdxes);
        }
        public static int ShowDialog(string msg, string title, string[] btns, int defaultBtnIdx, CommonIcons iconEnum,
            string[] checks, int[] defaultCheckIdxes, out int[] selectedCheckIdxes)
        {
            return ShowDialog(msg, title, btns, defaultBtnIdx, (int)iconEnum,
                null, -1, out int missing,
                checks, defaultCheckIdxes, out selectedCheckIdxes);
        }



        public static int ShowDialog(Window parent, string msg, string title, string[] btns, int defaultBtnIdx, int iconNo)
        {
            return ShowDialog(parent, msg, title, btns, defaultBtnIdx, iconNo,
                null, -1, out int missing,
                null, null, out int[] missing2);
        }
        public static int ShowDialog(Window parent, string msg, string title, string[] btns, int defaultBtnIdx, CommonIcons iconEnum)
        {
            return ShowDialog(parent, msg, title, btns, defaultBtnIdx, (int)iconEnum,
                null, -1, out int missing,
                null, null, out int[] missing2);
        }
        public static int ShowDialog(string msg, string title, string[] btns, int defaultBtnIdx, int iconNo)
        {
            return ShowDialog(msg, title, btns, defaultBtnIdx, iconNo,
                null, -1, out int missing,
                null, null, out int[] missing2);
        }
        public static int ShowDialog(string msg, string title, string[] btns, int defaultBtnIdx, CommonIcons iconEnum)
        {
            return ShowDialog(msg, title, btns, defaultBtnIdx, (int)iconEnum,
                null, -1, out int missing,
                null, null, out int[] missing2);
        }



        public static int ShowDialog(Window parent, string msg, string title, string[] btns, int defaultBtnIdx)
        {
            return ShowDialog(parent, msg, title, btns, defaultBtnIdx, -1,
                null, -1, out int missing,
                null, null, out int[] missing2);
        }
        public static int ShowDialog(Window parent, string msg, string title, string[] btns)
        {
            return ShowDialog(parent, msg, title, btns, -1, -1,
                null, -1, out int missing,
                null, null, out int[] missing2);
        }
        public static int ShowDialog(string msg, string title, string[] btns, int defaultBtnIdx)
        {
            return ShowDialog(msg, title, btns, defaultBtnIdx, -1,
                null, -1, out int missing,
                null, null, out int[] missing2);
        }
        public static int ShowDialog(string msg, string title, string[] btns)
        {
            return ShowDialog(msg, title, btns, -1, -1,
                null, -1, out int missing,
                null, null, out int[] missing2);
        }




        public static void ShowDialog(Window parent, string msg, string title)
        {
            ShowDialog(parent, msg, title, null, -1);
        }
        public static void ShowDialog(string msg, string title)
        {
            ShowDialog(msg, title, null, -1);
        }

        public static void ShowDialog(Window parent, string msg)
        {
            ShowDialog(parent, msg, "");
        }
        public static void ShowDialog(string msg)
        {
            ShowDialog(msg, "");
        }


        #endregion

    }
}
