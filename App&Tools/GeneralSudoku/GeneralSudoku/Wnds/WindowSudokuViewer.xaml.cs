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

namespace MadTomDev.App.Wnds
{
    /// <summary>
    /// Interaction logic for WindowSudokuViewer.xaml
    /// </summary>
    public partial class WindowSudokuViewer : Window
    {
        public WindowSudokuViewer()
        {
            InitializeComponent();
        }

        Core core = Core.Instance;
        public int[][]? boardData;
        public string boardName = "";
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(boardName))
            {
                // one time view
            }
            else
            {
                // from core
                core.InitBoardDataChanged += AfterInitBoardDataChanged;
                core.InitBoardTemplateChanged += AfterInitBoardTemplateChanged;
                GetBoardDataFromCore();
                ReLoadDataToBoard();
            }
        }
        private void GetBoardDataFromCore()
        {
            if (boardName == nameof(core.initBoardDataNormal))
            {
                boardData = core.initBoardDataNormal;
            }
            else if (boardName == nameof(core.initBoardDataRand))
            {
                boardData = core.initBoardDataRand;
            }
            else if (boardName == nameof(core.initBoardTemplate))
            {
                boardData = core.initBoardTemplate;
            }
        }
        private void AfterInitBoardDataChanged(Core sender)
        {
            GetBoardDataFromCore();
            ReLoadDataToBoard();
        }
        private void AfterInitBoardTemplateChanged(Core sender)
        {
            GetBoardDataFromCore();
            ReLoadDataToBoard();
        }
        private void ReLoadDataToBoard()
        {
            board.SetData(boardData, core.NumberCharList);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            core.InitBoardDataChanged -= AfterInitBoardDataChanged;
            core.InitBoardTemplateChanged -= AfterInitBoardTemplateChanged;
        }
    }
}
