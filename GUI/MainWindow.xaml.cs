﻿using System;
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
            System.Threading.Thread.Sleep(15000);
            GetText_Click(null, null);
        }

        private void GetText_Click(object sender, RoutedEventArgs e)
        {
            string text = string.Empty;
            while(text.Length < 20)
            {
                System.Threading.Thread.Sleep(10000);
                MyLib.OCRSocket.GetImageText();
                text = MyLib.PostOCRTextProcessor.ScanText(MyLib.OCRSocket.imgText.ToString()).ToString();
            }
            while (System.IO.File.Exists(fileName))
                fileName = fileName + ".txt";
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
