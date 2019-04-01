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
using System.Timers;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MyLib.Class1.Init();
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    filePath.Text = file;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    filePath.Text = string.Empty;
                    break;
            }
        }

        private void PostImage_Click(object sender, RoutedEventArgs e)
        {
            if(!System.IO.File.Exists(filePath.Text))
            {
                MessageBox.Show("Please check the image file path.");
                return;
            }
            MyLib.Class1.PostImageToGetURL(filePath.Text);
            System.Threading.Thread.Sleep(10000);
        }

        private void GetText_Click(object sender, RoutedEventArgs e)
        {
            MyLib.Class1.GetImageText();
            string text = MyLib.Class2.ScanText(MyLib.Class1.imgText.ToString()).ToString();
            if (text.Length < 20)
            {
                MessageBox.Show("Please try again.");
                return;
            }
            string fileName = text.Substring(0, 3).Replace('\t', '_') + ".txt";
            MyMessage.Text = "OK, please check the text file " + fileName;
            try
            {
                System.IO.File.WriteAllText(fileName, text);
            }
            catch(System.IO.IOException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
