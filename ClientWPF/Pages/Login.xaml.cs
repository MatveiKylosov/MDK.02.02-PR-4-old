using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

namespace ClientWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Connection(object sender, RoutedEventArgs e)
        {

            if (!IPAddress.TryParse(IPTB.Text, out MainWindow.mainWindow.IpAddress))
            {
                MessageBox.Show("Указан не правильный ip адрес", "Ошибка");
                return;
            }


            if (!int.TryParse(PortTB.Text, out MainWindow.mainWindow.Port))
            {
                MessageBox.Show("Указан не правильный port адрес", "Ошибка");
                return;
            }

            IPEndPoint endPoint = new IPEndPoint(MainWindow.mainWindow.IpAddress, MainWindow.mainWindow.Port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endPoint);

            if(!ConnectToServer(socket, LoginTB.Text, PasswordTB.Password))
            {
                MessageBox.Show("Не удалось подключиться", "Ошибка");
            }
            else
            {
                //ToDo: Open page with use main
            }
        }


        public bool ConnectToServer(Socket socket, string login, string password)
        {
            try
            {
                // Создание команды connect
                string command = $"connect {login} {password}";
                var viewModelSend = new ViewModelSend(command, -1); // -1, так как ID неизвестен до авторизации

                // Сериализация объекта в JSON и отправка серверу
                byte[] messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelSend));
                socket.Send(messageBytes);

                // Получение ответа от сервера
                byte[] buffer = new byte[1024]; // Буфер для чтения ответа
                int bytesReceived = socket.Receive(buffer);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

                // Десериализация ответа
                ViewModelMessage viewModelMessage = JsonConvert.DeserializeObject<ViewModelMessage>(response);

                // Обработка команды авторизации
                if (viewModelMessage.Command == "autorization")
                {
                    int userId = int.Parse(viewModelMessage.Data); // ID пользователя
                                                                   //Program.Id = userId; // Сохранение ID в глобальной переменной
                    MessageBox.Show($"Успешная авторизация. Ваш ID: {userId}", "Успех");
                    return true;
                }
                else
                {
                    MessageBox.Show($"Ошибка авторизации: {viewModelMessage.Data}", "Ошибка");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении: {ex.Message}", "Ошибка");
                return false;
            }
        }
    }
}