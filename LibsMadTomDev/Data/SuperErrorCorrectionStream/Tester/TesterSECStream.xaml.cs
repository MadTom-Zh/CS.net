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

using MadTomDev.Data;

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for TesterSECStream.xaml
    /// </summary>
    public partial class TesterSECStream : Window
    {
        public TesterSECStream()
        {
            InitializeComponent();
        }



        private void btn_SEC_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(tb_source.Text))
            {
                MessageBox.Show("No source file.");
                return;
            }

            try
            {
                using (Stream sfs = File.OpenRead(tb_source.Text))
                using (Stream tfs = File.Create(tb_target.Text))
                using (SECS secs = new SECS(tfs, 2, 8))
                {
                    sfs.CopyTo(secs);
                    secs.Flush();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }

        private void btn_restore_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(tb_target.Text))
            {
                MessageBox.Show("No sec file.");
                return;
            }

            try
            {
                using (Stream tfs = File.OpenRead(tb_target.Text))
                using (Stream rfs = File.Create(tb_restore.Text))
                using (SECS secs = new SECS(tfs, 2, 8))
                {
                    secs.CopyTo(rfs);
                    rfs.Flush();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }

        private void btn_test1_Click(object sender, RoutedEventArgs e)
        {
            using (Stream sfs = File.OpenRead(tb_source.Text))
            using (SECS secs1 = new SECS(new MemoryStream(), 4, 10))
            using (SECS secs2 = new SECS(new MemoryStream(), 2, 6))
            using (Stream rfs = File.Create(@"D:\A.test1.txt"))
            {
                sfs.CopyTo(secs1);
                secs1.Position = 0;
                secs1.CopyTo_optimized(secs2);
                secs2.Position = 0;
                secs2.CopyTo_optimized(rfs);
                rfs.Flush();
            }
        }

        private void btn_test2_Click(object sender, RoutedEventArgs e)
        {
            using (Stream sfs = File.OpenRead(tb_source.Text))
            using (SECS secs1 = new SECS(new MemoryStream(), 2, 6))
            using (SECS secs2 = new SECS(new MemoryStream(), 4, 10))
            using (Stream rfs = File.Create(@"D:\A.test2.txt"))
            {
                sfs.CopyTo(secs1);
                secs1.Position = 0;
                secs1.CopyTo_optimized(secs2);
                secs2.Position = 0;
                secs2.CopyTo_optimized(rfs);
                rfs.Flush();
            }
        }
    }
}
