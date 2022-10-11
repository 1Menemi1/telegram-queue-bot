using System;
using System.Data;
using Npgsql;

namespace telegram_queue_bot
{
    public class WevSecurityConfig
    {
        private static readonly string ConnectionString =

        public static void TestConnection()
        {
            using var connection = GetConnection();
            connection.Open();
            if (connection.State == ConnectionState.Open)
            {
                Console.WriteLine("Connected to database");
            }
        }

        public static void AddNewUser(TgUser tgUser)
        {
            using var connection = GetConnection();
            var username = tgUser.UserName;
            var tg_id = tgUser.UserId;
            var query = @$"insert into public.users(username,tg_id,is_admin)values('{username}',{tg_id},false)";
            var cmd = new NpgsqlCommand(query, connection);
            connection.Open();
            var n = cmd.ExecuteNonQuery();
            if (n == 1)
            {
                Console.WriteLine("New user has been registered");
            }
        }

        public void GetAllUsers()
        {
            using var connection = GetConnection();
            const string query = "SELECT * FROM public.users";
            var cmd = new NpgsqlCommand(query, connection);
            connection.Open();

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine("id: {0}; username: {1}; user_tg_id: {2}\n", reader.GetInt32(0), reader.GetString(1),
                    reader.GetInt32(2));
            }
        }

        public static int FindUserById(int userId)
        {
            using NpgsqlConnection connection = GetConnection();
            var query = $"SELECT * FROM public.users where tg_id = {userId}";
            var cmd = new NpgsqlCommand(query, connection);
            connection.Open();

            using NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                // вывод фулл информации о user'е
                // Console.WriteLine("id: {0}; username: {1}; user_tg_id: {2}; is_admin: {3}\n", reader.GetInt32(0), reader.GetString(1),
                //     reader.GetInt32(2), reader.GetBoolean(3));
                return reader.GetInt32(2);
            }

            return 0;
        }

        public static int FindUserByUsername(string username)
        {
            using var connection = GetConnection();
            var query = $"SELECT * FROM public.users where username = '{username}'";
            var cmd = new NpgsqlCommand(query, connection);
            connection.Open();

            using NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                // вывод фулл информации о user'е
                // Console.WriteLine("id: {0}; username: {1}; user_tg_id: {2}; is_admin: {3}\n", reader.GetInt32(0), reader.GetString(1),
                //     reader.GetInt32(2), reader.GetBoolean(3));
                return reader.GetInt32(2);
            }

            return 0;
        }

        public static bool IsUserAnAdmin(int userId)
        {
            using var connection = GetConnection();
            var query = $"SELECT * FROM public.users where tg_id = {userId}";
            var cmd = new NpgsqlCommand(query, connection);
            connection.Open();

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return reader.GetBoolean(3);
            }

            return false;
        }

        private static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(ConnectionString);
        }
    }
}