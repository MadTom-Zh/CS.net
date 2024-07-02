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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for PinchableGroupBox.xaml
    /// </summary>
    public partial class PinchableGroupBox : UserControl
    {
        public PinchableGroupBox()
        {
            InitializeComponent();
            DataContext = this;

            IsChecked = true;

            //checkBox.Checked += Cb_Checked;
            //checkBox.Unchecked += Cb_Unchecked;
        }

        public bool IsCheckboxEnabled
        {
            get => checkBox.IsEnabled;
            set => checkBox.IsEnabled = value;
        }


        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }

        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register("HeaderText", typeof(string), typeof(PinchableGroupBox), new PropertyMetadata("Header"));



        public new object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set
            {
                SetValue(ContentProperty, value);
                //if (checkBox.IsChecked == true)
                //    Cb_Checked(null, null);
                //else
                //    Cb_Unchecked(null, null);
            }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly new DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(PinchableGroupBox), new PropertyMetadata("Content here"));



        public bool IsChecked
        {
            get => checkBox.IsChecked == true;
            set => checkBox.IsChecked = value;
        }



        public event RoutedEventHandler CheckChanged
        {
            add { AddHandler(CheckChangedProperty, value); }
            remove { RemoveHandler(CheckChangedProperty, value); }
        }
        public static readonly RoutedEvent CheckChangedProperty =
            EventManager.RegisterRoutedEvent(nameof(CheckChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PinchableGroupBox));

        public enum CheckPinchModes
        {
            CheckedExpand, CheckedPinch, CheckedDoNothing,
        }
        public CheckPinchModes CheckPinchMode { set; get; } = CheckPinchModes.CheckedExpand;
        public enum CheckEnableModes
        {
            CheckedEnable, CheckDisable, CheckedDoNothing,
        }
        public CheckEnableModes CheckEnableMode { set; get; } = CheckEnableModes.CheckedEnable;


        private void Cb_Checked(object sender, RoutedEventArgs e)
        {
            switch (CheckPinchMode)
            {
                case CheckPinchModes.CheckedPinch:
                    contentControl.Visibility = Visibility.Collapsed;
                    break;
                case CheckPinchModes.CheckedExpand:
                    contentControl.Visibility = Visibility.Visible;
                    break;
                default:
                case CheckPinchModes.CheckedDoNothing:
                    break;
            }
            switch (CheckEnableMode)
            {
                case CheckEnableModes.CheckedEnable:
                    contentControl.IsEnabled = true;
                    break;
                case CheckEnableModes.CheckDisable:
                    contentControl.IsEnabled = false;
                    break;
                default:
                case CheckEnableModes.CheckedDoNothing:
                    break;
            }
            RaiseEvent(new RoutedEventArgs(CheckChangedProperty, this));
        }
        private void Cb_Unchecked(object sender, RoutedEventArgs e)
        {
            switch (CheckPinchMode)
            {
                case CheckPinchModes.CheckedPinch:
                    contentControl.Visibility = Visibility.Visible;
                    break;
                case CheckPinchModes.CheckedExpand:
                    contentControl.Visibility = Visibility.Collapsed;
                    break;
                default:
                case CheckPinchModes.CheckedDoNothing:
                    break;
            }
            switch (CheckEnableMode)
            {
                case CheckEnableModes.CheckedEnable:
                    contentControl.IsEnabled = false;
                    break;
                case CheckEnableModes.CheckDisable:
                    contentControl.IsEnabled = true;
                    break;
                default:
                case CheckEnableModes.CheckedDoNothing:
                    break;
            }
            RaiseEvent(new RoutedEventArgs(CheckChangedProperty, this));
        }




        //public new bool IsEnabled
        //{
        //    get { return (bool)GetValue(IsEnabledProperty); }
        //    set { SetValue(IsEnabledProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for IsEnabled.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty IsEnabledProperty =
        //    DependencyProperty.Register("IsEnabled", typeof(bool), typeof(PinchableGroupBox), new PropertyMetadata(0));


    }
}
