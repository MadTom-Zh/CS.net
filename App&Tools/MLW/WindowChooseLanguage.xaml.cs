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
using System.Windows.Shapes;

using System.IO;
using System.Formats.Asn1;
using CSVReader = MadTomDev.Common.CSVHelper.Reader;
using System.Security.Cryptography.X509Certificates;

namespace MLW_Succubus_Storys
{
    /// <summary>
    /// Interaction logic for WindowChooseLanguage.xaml
    /// </summary>
    public partial class WindowChooseLanguage : Window
    {
        public WindowChooseLanguage()
        {
            InitializeComponent();
            Core.luncherWindow = this;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            spLangBtns.Children.Add(NewLangButton("Please wait...", null));
            LoadLangsAsync();
        }


        private void LoadLangsAsync()
        {
            Dispatcher.BeginInvoke(() =>
            {
                List<Button> btnList = new List<Button>();
                string newLine = Environment.NewLine;
                foreach (FileInfo fi in new DirectoryInfo("StoryPkg").GetFiles("*.csv"))
                {
                    btnList.Add(
                        NewLangButton(
                            fi.Name.Substring(0, fi.Name.Length - 4).Replace("_", newLine),
                            fi
                            ));
                }
                spLangBtns.Children.Clear();
                foreach (Button btn in btnList)
                {
                    spLangBtns.Children.Add(btn);
                }
            });
        }

        private Thickness btnPadding = new Thickness(12);
        private Button NewLangButton(string? tx, FileInfo? pkgFile)
        {
            Button ui = new Button()
            {
                MinWidth = 160,
                MinHeight = 60,
                FontSize = 20,
                Padding = btnPadding,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Content = tx,
                Tag = pkgFile,
            };

            ui.Click += Ui_Click;
            return ui;
        }

        private void Ui_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                Button btn = (Button)sender;
                if (btn.Tag is FileInfo)
                {
                    this.Hide();

                    Core.storyPkgFile = (FileInfo)btn.Tag;
                    bool acceptWarning = false;
                    string warningInfo = null;
                    using (CSVReader csvReader = new CSVReader(Core.storyPkgFile.FullName))
                    {
                        string[] row;
                        string no;
                        do
                        {
                            row = csvReader.ReadRow();
                            if (row == null || row.Length == 0)
                                continue;
                            no = row[0].Trim();
                            if (no.Length == 1)
                            {
                                if (no == "0")
                                {
                                    warningInfo = row[5];
                                }
                                break;
                            }
                        }
                        while (!csvReader.IsEoF);
                    }

                    if (warningInfo != null)
                    {
                        bool isLoaded = false;
                        WindowStartWarning wDlg = new WindowStartWarning()
                        { WarningText = warningInfo };
                        wDlg.Activated += async (s, e) => 
                        {
                            if (isLoaded)
                                return;

                            await Task.Delay(100);

                            Core.LoadAllImages();
                            Core.LoadCSV();
                            wDlg.TrySetLocalization();
                            wDlg.spBtns.IsEnabled = true;
                            isLoaded = true;
                        };
                        if (wDlg.ShowDialog() == true)
                            acceptWarning = true;
                    }

                    if (acceptWarning)
                    {
                        Core.mainWindow = new MainWindow();
                        Core.LoadSettings();
                        Core.mainWindow.Show();
                    }
                    else
                    {
                        Core.Exit();
                    }
                }
            }
        }
    }
}
