using MLW_Succubus_Storys.Classes;
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
using System.Windows.Threading;
using static MLW_Succubus_Storys.Classes.SuccuNode;

namespace MLW_Succubus_Storys.Ctrls
{
    /// <summary>
    /// Interaction logic for BtnHeart.xaml
    /// </summary>
    public partial class BtnNode : UserControl
    {
        public BtnNode()
        {
            InitializeComponent();
            if (Core.localizeDict.ContainsKey("isChecked"))
                cmBtnNode_isBordered.Header = Core.localizeDict["isChecked"];
            btn.Background = null;
        }

        public List<ArrowLine> arrowLinesFrom = new List<ArrowLine>();
        public List<ArrowLine> arrowLinesTo = new List<ArrowLine>();

        private static Thickness btnBorderThickness = new Thickness(2);
        private static Thickness btnBorderThickness0 = new Thickness(0);

        public event Action<BtnNode> IsBorderedChanged;
        private bool _IsBordered = false;
        public bool IsBordered
        {
            get => btn.BorderThickness.Left > 0;
            set
            {
                Dispatcher.Invoke(() =>
                {
                    if (value)
                        btn.BorderThickness = btnBorderThickness;
                    else
                        btn.BorderThickness = btnBorderThickness0;
                });
                if (_IsBordered != value)
                {
                    _IsBordered = value;
                    if (isStoryLink)
                    {
                        if (arrowLinesTo.Count > 0)
                        {
                            arrowLinesTo[0].btnEnd.IsBordered = value;
                        }
                    }
                    else
                    {
                        IsBorderedChanged?.Invoke(this);
                    }
                }
            }
        }

        internal bool isStoryLink;
        public enum IconTypes
        {
            HeartEmpty, HeartHalf, HeartFull,
            HeartEnding,
            Choice,
        }

        private IconTypes _IconType;
        public IconTypes IconType
        {
            get => _IconType;
            set
            {
                Dispatcher.BeginInvoke(() =>
                {
                    _IconType = value;
                    string key = null;
                    switch (_IconType)
                    {
                        case IconTypes.HeartEmpty:
                            key = "HeartEmpty"; break;
                        case IconTypes.HeartHalf:
                            key = "HeartHalf"; break;
                        case IconTypes.HeartFull:
                            key = "HeartFull"; break;

                        case IconTypes.HeartEnding:
                            key = "HeartEnding"; break;

                        case IconTypes.Choice:
                            key = "Choice"; break;
                    }
                    if (Core.imageDict.ContainsKey(key))
                        img.Source = Core.imageDict[key];
                });
            }
        }
        public void SetIconType(SuccuNode.StoryNode storyNode)
        {
            if (storyNode.name.StartsWith("I-"))
                IconType = IconTypes.HeartEmpty;
            else if (storyNode.name.StartsWith("II-"))
                IconType = IconTypes.HeartHalf;
            else // (storyNode.name.StartsWith("III-"))
                IconType = IconTypes.HeartFull;
        }
        public void SetIconType(object dataNode)
        {
            if (dataNode is SuccuNode.ChoiceNode)
            {
                IconType = BtnNode.IconTypes.Choice;
            }
            else if (dataNode is SuccuNode.StoryNode)
            {
                SetIconType((SuccuNode.StoryNode)dataNode);
            }
            else if (dataNode is SuccuNode.EndingNode)
            {
                IconType = BtnNode.IconTypes.HeartEnding;
            }
        }

        public void SetText(int no)
        {
            Dispatcher.BeginInvoke(() =>
            {
                tb.Text = GetSymNo(no);
            });
        }
        public static string GetSymNo(string no)
        {
            if (int.TryParse(no, out int v))
                return GetSymNo(v);
            else
                return GetSymNo(-1);
        }
        public static string GetSymNo(int no)
        {
            if (no <= 0) return "O";
            else if (no == 1) return "I";
            else if (no == 2) return "II";
            else if (no == 3) return "III";
            else if (no == 4) return "IV";
            else if (no == 5) return "V";
            else if (no == 6) return "VI";
            else if (no == 7) return "VII";
            else if (no == 8) return "VIII";
            else if (no == 9) return "IX";
            else if (no == 10) return "X";
            else return ">X";
        }
        public void SetText(StoryNode story)
        {
            SetText(int.Parse(story.name.Substring(story.name.IndexOf('-') + 1)));
        }
        public void SetText(bool isEndingAorB)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (isEndingAorB)
                    tb.Text = "A";
                else
                    tb.Text = "B";
            });
        }
        public void SetText(string tx)
        {
            Dispatcher.BeginInvoke(() =>
            {
                tb.Text = tx;
            });
        }

        public Action<BtnNode, bool> ActionClicked;

        private void btn_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //e.Handled = true;
            if (e.ChangedButton == MouseButton.Left)
                ActionClicked?.Invoke(this, true);
            else if (e.ChangedButton == MouseButton.Right)
                ActionClicked?.Invoke(this, false);
        }

        private void cmBtnNode_Opened(object sender, RoutedEventArgs e)
        {
            cmBtnNode_isBordered.IsChecked = IsBordered;
        }

        private void cmBtnNode_isBordered_Click(object sender, RoutedEventArgs e)
        {
            IsBordered = !IsBordered;
        }

    }
}
