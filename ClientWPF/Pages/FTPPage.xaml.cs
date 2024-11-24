using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для FTPPage.xaml
    /// </summary>
    public partial class FTPPage : Page
    {
        string CurrentDirectoryClient = @"C:\";
        public FTPPage()
        {
            InitializeComponent();
            ClientDirectory.Text = @"C:\";
        }

        private void ChangeDirectory_Server(object sender, RoutedEventArgs e)
        {
            
        }

        private void ChangeDirectory_Client(object sender, RoutedEventArgs e)
        {
            OpenDirectoryClient(ClientDirectory.Text);
        }

        private List<string> GetDirectory(string src)
        {
            try
            {
                List<string> FoldersFiles = new List<string>();
                if (Directory.Exists(src))
                {
                    string[] dirs = Directory.GetDirectories(src);
                    foreach (string dir in dirs)
                    {
                        string NameDirectory = dir.Replace(src, "");
                        FoldersFiles.Add(NameDirectory + "\\");
                    }

                    string[] files = Directory.GetFiles(src);
                    foreach (string file in files)
                    {
                        string NameFile = file.Replace(src, "");
                        FoldersFiles.Add($"{NameFile}");
                    }
                }

                return FoldersFiles;
            }
            catch (Exception e){
                MessageBox.Show($"{e.Message}", $"Ошибка");
                return null;
            }
        }

        public void OpenDirectoryClient(string dir)
        {
            ClientParent.Children.Clear();
            ClientDirectory.Text = dir;
            CurrentDirectoryClient = dir;

            var list = GetDirectory(CurrentDirectoryClient);

            if(list != null)
                foreach (var x in list)
                {
                    ClientParent.Children.Add(new Elements.ElementFTP($"{CurrentDirectoryClient}{x}", x, false, this));
                }
        }
    }
}
