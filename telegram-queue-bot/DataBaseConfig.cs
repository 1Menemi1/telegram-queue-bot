using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace telegram_queue_bot
{
    public class DataBaseConfig
    {
        private const string PathToConnectionString =
            "D:\\ITMO\\telegram-queue-bot\\telegram-queue-bot\\secret-information\\connection-string.txt";

        private static readonly string ConnectionString = $"{System.IO.File.ReadAllText(PathToConnectionString)}";

        public static void TestConnection()
        {
            using var connection = GetConnection();
            connection.Open();
            if (connection.State == ConnectionState.Open)
            {
                Console.WriteLine("Connected to database");
            }
        }

        public static void RegisterNewUser(TgUser tgUser)
        {
            using var connection = GetConnection();
            var query =
                @$"INSERT INTO public.users(username,tg_id,is_admin) VALUES('{tgUser.UserName}',{tgUser.UserId},false)";
            var cmd = new NpgsqlCommand(query, connection);
            connection.Open();

            var n = cmd.ExecuteNonQuery();
            if (n == 1)
            {
                Console.WriteLine("New user has been registered");
            }
        }

        public static List<TgUser> GetAllUsersInQueue()
        {
            using var connection = GetConnection();
            const string query = "SELECT * FROM public.queue";
            var cmd = new NpgsqlCommand(query, connection);
            connection.Open();

            using var reader = cmd.ExecuteReader();

            var resultList = new List<TgUser>();
            while (reader.Read())
            {
                resultList.Add(new TgUser(reader.GetString(0), reader.GetInt32(1)));
            }

            return resultList;
        }

        public static int FindUserInDataBase(int userId)
        {
            using var connection = GetConnection();
            var query = $"SELECT * FROM public.users where tg_id = {userId}";
            var cmd = new NpgsqlCommand(query, connection);
            connection.Open();

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                // вывод фулл информации о user'е
                // Console.WriteLine("id: {0}; username: {1}; user_tg_id: {2}; is_admin: {3}\n", reader.GetInt32(0), reader.GetString(1),
                //     reader.GetInt32(2), reader.GetBoolean(3));
                return reader.GetInt32(2);
            }

            return 0;
        }

        public static int FindUserInDataBase(string username)
        {
            using var connection = GetConnection();
            var query = $"SELECT * FROM public.users where username = '{username}'";
            var cmd = new NpgsqlCommand(query, connection);
            connection.Open();

            using var reader = cmd.ExecuteReader();

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

        public static void AddUserToQueue(TgUser user)
        {
            using var connection = GetConnection();
            var query = @$"INSERT INTO public.queue(username,tg_id) VALUES('{user.UserName}',{user.UserId})";
            var cmd = new NpgsqlCommand(query, connection);
            connection.Open();

            var n = cmd.ExecuteNonQuery();
            if (n == 1)
            {
                Console.WriteLine($"{user.UserName}:{user.UserId} has been added to queue");
            }
        }

        public static int FindUserInQueue(int userTgId)
        {
            using var connection = GetConnection();
            var query = $"SELECT * FROM public.queue where tg_id = {userTgId}";
            var cmd = new NpgsqlCommand(query, connection);
            connection.Open();

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return reader.GetInt32(1);
            }

            return 0;
        }

        public static int FindUserInQueue(string username)
        {
            using var connection = GetConnection();
            var query = $"SELECT * FROM public.queue where username = '{username}'";
            var cmd = new NpgsqlCommand(query, connection);
            connection.Open();

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return reader.GetInt32(1);
            }

            return 0;
        }

        public static void RemoveFromQueue(int userTgId)
        {
            using var connection = GetConnection();
            var query = @$"DELETE FROM public.queue WHERE tg_id={userTgId}";
            var cmd = new NpgsqlCommand(query, connection);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public static void ResetQueue()
        {
            using var connection = GetConnection();
            const string query = @"DELETE FROM public.queue";
            var cmd = new NpgsqlCommand(query, connection);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        private static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(ConnectionString);
        }
    }
}