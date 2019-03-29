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
            MyLib.Class1.PostImageToGetURL(filePath.Text);
            while(MyLib.Class1.textURL.Length == 0)
            {
                System.Threading.Thread.Sleep(1000);
            }
            MyMessage.Text = "finished post";
        }

        private void GetText_Click(object sender, RoutedEventArgs e)
        {
            MyLib.Class1.GetImageText();
            MyMessage.Text = MyLib.Class1.imgText.ToString();
        }
    }
}
