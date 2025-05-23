﻿using Common;
using Newtonsoft.Json;
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
        string CurrentDirectoryServer = @"";
        public FTPPage()
        {
            InitializeComponent();
            ClientDirectory.Text = CurrentDirectoryClient;

            OpenDirectoryClient(CurrentDirectoryClient);
            OpenDirectoryServer();
        }

        private void ChangeDirectory_Server(object sender, RoutedEventArgs e)
        {
            OpenDirectoryServer();
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
            {
                foreach (var x in list)
                {
                    ClientParent.Children.Add(new Elements.ElementFTP($"{CurrentDirectoryClient}{x}", x, false, this));
                }
            }
        }

        public void OpenDirectoryServer(string dir = "", bool update = false)
        {
            try
            {
                ServerParent.Children.Clear();

                if(!update)
                    CurrentDirectoryServer = !string.IsNullOrEmpty(dir) ? CurrentDirectoryServer + "\\" + dir : dir;

                var socket = MainWindow.mainWindow.ConnectToServer();
                var userId = MainWindow.mainWindow.Id;

                if (socket == null)
                {
                    MessageBox.Show("Не удалось подключиться к серверу.", "Ошибка подключения");
                    return;
                }

                string command = $"cd{(string.IsNullOrEmpty(dir) ? "" : " " + dir)}";
                ViewModelSend viewModelSend = new ViewModelSend(command, userId);

                byte[] messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelSend));
                socket.Send(messageBytes);

                byte[] buffer = new byte[10485760];
                int bytesReceived = socket.Receive(buffer);
                string serverResponse = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

                ViewModelMessage responseMessage = JsonConvert.DeserializeObject<ViewModelMessage>(serverResponse);

                socket.Close();
                if (responseMessage.Command == "cd")
                {
                    List<string> directoryContents = JsonConvert.DeserializeObject<List<string>>(responseMessage.Data);
                    foreach (var x in directoryContents)
                    {
                        ServerParent.Children.Add(new Elements.ElementFTP($"{CurrentDirectoryServer}{x}", x, true, this));
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось перейти в указанную директорию. Либо она пустая либо не существует", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при переходе по директориям: {ex.Message}", "Ошибка");
            }
        }


        public void UpdateDir()
        {
            try
            {
                OpenDirectoryServer("", true);
                ServerParent.Children.Clear();

                var socket = MainWindow.mainWindow.ConnectToServer();
                var userId = MainWindow.mainWindow.Id;

                if (socket == null)
                {
                    MessageBox.Show("Не удалось подключиться к серверу.", "Ошибка подключения");
                    return;
                }

                string command = $"cd {CurrentDirectoryServer}";
                ViewModelSend viewModelSend = new ViewModelSend(command, userId);

                byte[] messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelSend));
                socket.Send(messageBytes);

                byte[] buffer = new byte[10485760];
                int bytesReceived = socket.Receive(buffer);
                string serverResponse = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

                ViewModelMessage responseMessage = JsonConvert.DeserializeObject<ViewModelMessage>(serverResponse);
                socket.Close();
                if (responseMessage.Command == "cd")
                {
                    List<string> directoryContents = JsonConvert.DeserializeObject<List<string>>(responseMessage.Data);
                    foreach (var x in directoryContents)
                    {
                        ServerParent.Children.Add(new Elements.ElementFTP($"{CurrentDirectoryServer}{x}", x, true, this));
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось перейти в указанную директорию. Либо она пустая либо не существует", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при переходе по директориям: {ex.Message}", "Ошибка");
            }
        }

        public void DownloadFileFromServer(string serverFilePath)
        {
            try
            {
                var localSavePath = (CurrentDirectoryClient[0] != '\\' ? CurrentDirectoryClient + "\\" : CurrentDirectoryClient) + serverFilePath;
                var socket = MainWindow.mainWindow.ConnectToServer();
                var userId = MainWindow.mainWindow.Id;

                // Проверяем подключение к серверу
                if (socket == null)
                {
                    MessageBox.Show("Не удалось подключиться к серверу.", "Ошибка подключения");
                    return;
                }

                // Формируем команду "get" для запроса файла
                string command = $"get {serverFilePath}";
                ViewModelSend viewModelSend = new ViewModelSend(command, userId);

                // Сериализуем и отправляем запрос
                byte[] messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelSend));
                socket.Send(messageBytes);

                // Получаем ответ от сервера
                byte[] buffer = new byte[10485760]; // Буфер для получения файла
                int bytesReceived = socket.Receive(buffer);
                string serverResponse = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

                // Обрабатываем ответ сервера
                ViewModelMessage responseMessage = JsonConvert.DeserializeObject<ViewModelMessage>(serverResponse);
                socket.Close();

                if (responseMessage.Command == "file")
                {
                    // Десериализуем данные файла
                    byte[] fileData = JsonConvert.DeserializeObject<byte[]>(responseMessage.Data);

                    // Сохраняем файл на локальной машине
                    File.WriteAllBytes(localSavePath, fileData);
                    OpenDirectoryClient(CurrentDirectoryClient);
                    MessageBox.Show($"Файл успешно скачан и сохранён в: {localSavePath}", "Скачивание завершено");
                }
                else
                {
                    MessageBox.Show("Не удалось получить файл. Проверьте путь на сервере.", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при скачивании файла: {ex.Message}", "Ошибка");
            }
        }
    }
}
