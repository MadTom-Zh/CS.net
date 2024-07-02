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
    /// Interaction logic for OutputWindow.xaml
    /// </summary>
    public partial class OutputWindow : Window
    {
        private static OutputWindow instance;
        public static OutputWindow Instance
        {
            get
            {
                if (instance == null || instance.isClosed)
                {
                    instance = new OutputWindow();
                }
                return instance;
            }
        }

        public static void Output(string msg, bool newLine = true, bool withTime = true)
        {
            OutputWindow wnd = Instance;
            wnd.Show();
            if (wnd.WindowState == WindowState.Minimized)
                wnd.WindowState = WindowState.Normal;
            if (wnd.Visibility != Visibility.Visible)
                wnd.Visibility = Visibility.Visible;

            wnd.WndOutput(msg, newLine, withTime);
        }

        public static bool HasBtn(string btnText)
        {
            return Instance.WndHasBtn(btnText);
        }
        public static void AddSetBtn(string btnText, Action action)
        {
            Instance.WndAddSetBtn(btnText, action);
        }
        public static void RemoveBtn(string btnText)
        {
            Instance.WndRemoveBtn(btnText);
        }



        public OutputWindow()
        {
            InitializeComponent();
        }

        public List<Button> userBtnList = new List<Button>();
        public void WndAddSetBtn(string btnText, Action action)
        {
            Button foundBtn = userBtnList.Find(b => b.Content.ToString() == btnText);
            if (foundBtn == null)
            {
                // new btn+action
                Button newBtn = new Button()
                { Content = btnText, Margin = new Thickness(0, 0, 4, 0) };
                newBtn.Tag = action;
                newBtn.Click += NewBtn_Click;
                sp_btns.Children.Add(newBtn);
                userBtnList.Add(newBtn);
            }
            else
            {
                // update action
                foundBtn.Tag = action;
            }
        }
        public bool WndHasBtn(string btnText)
        {
            return userBtnList.Find(b => b.Content.ToString() == btnText) != null;
        }
        public void WndRemoveBtn(string btnText)
        {
            Button foundBtn = userBtnList.Find(b => b.Content.ToString() == btnText);
            if (foundBtn != null)
            {
                foundBtn.Click -= NewBtn_Click;
                sp_btns.Children.Remove(foundBtn);
                userBtnList.Remove(foundBtn);
            }
        }

        private void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Tag is Action)
            {
                ((Action)btn.Tag)?.Invoke();
            }
        }


        public void WndOutput(string msg, bool newLine = true, bool withTime = true)
        {
            Dispatcher.Invoke(() =>
            {
                StringBuilder strBdr = new StringBuilder();
                if (newLine)
                    strBdr.Append(Environment.NewLine);
                if (withTime)
                {
                    strBdr.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    strBdr.Append(" ");
                }
                strBdr.Append(msg);
                tb.AppendText(strBdr.ToString());
            });
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            tb.Clear();
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public bool isClosed = false;
        private void Window_Closed(object sender, EventArgs e)
        {
            isClosed = true;
        }
    }
}
