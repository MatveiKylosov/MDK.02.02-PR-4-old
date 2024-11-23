using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Server
{
    public static class DatabaseManager
    {
        // Класс UserDB должен быть public

        private static readonly string ConnectionString;

        // Статический конструктор для инициализации строки подключения
        static DatabaseManager()
        {
            ConnectionString = "Server=192.168.0.111;Database=PR4;User ID=root;Password=dawda6358;";
        }

        // Метод для чтения всех пользователей из таблицы users
        public static List<User> GetAllUsers()
        {
            var users = new List<User>();

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                string query = "SELECT id, login, password, home_dir FROM users";
                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new User(
                            reader.GetInt32("id"),
                            reader.GetString("login"),
                            reader.GetString("password"),
                            reader.GetString("home_dir")
                        );

                        users.Add(user);
                    }
                }
            }

            return users;
        }

        // Метод для добавления команды, выполненной пользователем
        public static void AddUserCommand(int userId, string commandText)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                string query = "INSERT INTO user_commands (user_id, command_text) VALUES (@userId, @commandText)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@commandText", commandText);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
