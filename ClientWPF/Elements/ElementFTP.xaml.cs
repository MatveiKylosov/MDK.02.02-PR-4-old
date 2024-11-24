using ClientWPF.Pages;
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

namespace ClientWPF.Elements
{
    /// <summary>
    /// Логика взаимодействия для ElementFTP.xaml
    /// </summary>
    public partial class ElementFTP : UserControl
    {
        string Path;
        bool Server = false;
        bool File = false;

        FTPPage FTPPage;

        public ElementFTP(string path, string name, bool server, FTPPage ftpPage)
        {
            InitializeComponent();
            this.Path = path;

            if (path[path.Length -1] == '\\')
            {
                FileIcon.Visibility = Visibility.Hidden;
                name = name.Replace("\\", "");
            }
            else
            {
                FolderIcon.Visibility = Visibility.Hidden;
                File = true;
            }

            NameElement.Text = name;

            this.Server = server;
            this.FTPPage = ftpPage;
        }

        private void OpenOrSend_Click(object sender, MouseButtonEventArgs e)
        {
            if (File)
            {
                //ToDo: отправка на сервер.
            }
            else 
            {
                   FTPPage.OpenDirectoryClient(Path);
            }
        }
    }
}
