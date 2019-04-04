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
        string fileName = "fileName1.txt";

        public MainWindow()
        {
            InitializeComponent();
            MyLib.OCRSocket.Init();
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    filePath.Text = fileDialog.FileName;
                    fileName = System.IO.Path.GetFileName(filePath.Text) + ".txt";
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
            MyLib.OCRSocket.PostImageToGetURL(filePath.Text);
            System.Threading.Thread.Sleep(10000);
            GetText_Click(null, null);
        }

        private void GetText_Click(object sender, RoutedEventArgs e)
        {
            if (MyLib.OCRSocket.textURL.Length == 0)
            {
                MyMessage.Text = MyMessage.Text + "\ntext URL response is null";
                return;
            }
            string text = string.Empty;
            while(text.Length < 20)
            {
                System.Threading.Thread.Sleep(5000);
                MyLib.OCRSocket.GetImageText();
                if (MyLib.OCRSocket.imgText.Length == 0)
                    return;
                text = MyLib.PostOCRTextProcessor.ScanText(MyLib.OCRSocket.imgText.ToString()).ToString();
            }
            while (System.IO.File.Exists(fileName))
                fileName = fileName + ".txt";
            try
            {
                System.IO.File.WriteAllText(fileName, text);
            }
            catch(System.IO.IOException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            MyMessage.Text = MyMessage.Text + "\nOK, please check the text file " + fileName;
        }

        private void PostAll_Click(object sender, RoutedEventArgs e)
        {
            if(filePath.Text.Length == 0)
            {
                MessageBox.Show("File path is empty");
                return;
            }
            filePath.Text = System.IO.Path.GetDirectoryName(filePath.Text);
            if(!System.IO.Directory.Exists(filePath.Text))
            {
                MessageBox.Show("Folder path doesn't exist");
                return;
            }
            string[] paths = System.IO.Directory.GetFiles(filePath.Text);
            foreach(string i in paths)
            {
                filePath.Text = i;
                fileName = System.IO.Path.GetFileNameWithoutExtension(i) + ".txt";
                while (System.IO.File.Exists(fileName))
                    fileName = fileName + ".txt";
                MyLib.OCRSocket.PostImageToGetURL(filePath.Text);
                System.Threading.Thread.Sleep(10000);
                GetText_Click(null, null);
            }
        }
    }
}
