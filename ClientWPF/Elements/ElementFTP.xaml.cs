using ClientWPF.Pages;
using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        bool Fileb = false;

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
                Fileb = true;
            }

            NameElement.Text = name;

            this.Server = server;
            this.FTPPage = ftpPage;
        }

        private void OpenOrSend_Click(object sender, MouseButtonEventArgs e)
        {
            if (Server)
            {
                ServerOpenOrSend();
            }
            else
            {
                ClientOpenOrSend();
            }
        }

        void ClientOpenOrSend()
        {
            if (Fileb)
            {
                SendFileToServer(Path);
            }
            else
            {
                FTPPage.OpenDirectoryClient(Path);
            }
        }

        void ServerOpenOrSend()
        {
            if (Fileb)
            {
                //ToDo: отправка на сервер.
            }
            else
            {
                FTPPage.OpenDirectoryServer(NameElement.Text);
            }
        }


        private void SendFileToServer(string filePath)
        {
            try
            {
                var socket = MainWindow.mainWindow.ConnectToServer();
                var userId = MainWindow.mainWindow.Id;

                // Проверяем подключение к серверу
                if (socket == null)
                {
                    MessageBox.Show("Не удалось подключиться к серверу.", "Ошибка подключения");
                    return;
                }

                // Проверяем, существует ли файл
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Указанный файл не существует.", "Ошибка");
                    return;
                }

                // Получаем данные файла
                FileInfo fileInfo = new FileInfo(filePath);
                FileInfoFTP fileInfoFTP = new FileInfoFTP(File.ReadAllBytes(filePath), fileInfo.Name);

                // Создаем объект для отправки
                ViewModelSend viewModelSend = new ViewModelSend(JsonConvert.SerializeObject(fileInfoFTP), userId);

                // Сериализуем и отправляем данные
                byte[] messageByte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelSend));
                socket.Send(messageByte);

                // Получаем ответ от сервера
                byte[] buffer = new byte[10485760];
                int bytesReceived = socket.Receive(buffer);
                string serverResponse = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

                // Обрабатываем ответ сервера
                ViewModelMessage responseMessage = JsonConvert.DeserializeObject<ViewModelMessage>(serverResponse);
                if (responseMessage.Command == "message")
                {
                    MessageBox.Show(responseMessage.Data, "Ответ сервера");
                }
                else
                {
                    MessageBox.Show("Неизвестный ответ от сервера.", "Ошибка");
                }
                socket.Close();
                FTPPage.UpdateDir();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке файла: {ex.Message}", "Ошибка");
            }
        }
    }
}
