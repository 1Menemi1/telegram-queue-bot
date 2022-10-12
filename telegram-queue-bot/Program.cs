using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace telegram_queue_bot
{
    internal static class Program
    {
        private const string PathToAnecdotes = "D:\\ITMO\\telegram-queue-bot\\telegram-queue-bot\\anecdotes.json";
        private const string PathToToken = "D:\\ITMO\\telegram-queue-bot\\telegram-queue-bot\\secret-information\\token.txt";

        private static readonly string Token = System.IO.File.ReadAllText(PathToToken);
        private static readonly ITelegramBotClient Bot = new TelegramBotClient($"{Token}");
        private static readonly List<TgUser> Queue = new List<TgUser>();
        private static readonly WevSecurityConfig Config = new WevSecurityConfig();

        private static TgUser FindMemberById(int userId)
        {
            return Queue.FirstOrDefault(user => userId.Equals(user.UserId));
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text == null)
                {
                    await botClient.SendTextMessageAsync(message.Chat,
                        $"Ты отправил незнакомый мне тип сообщения",
                        cancellationToken: cancellationToken);
                    return;
                }

                var splitMessage = message.Text.Split(' ');
                switch (message.Text.ToLower())
                {
                    case "/register":
                    case "0":
                    {
                        var user = new TgUser(message.From.Username, Convert.ToInt32(message.From.Id));
                        if (WevSecurityConfig.FindUserById(user.UserId) == 0)
                        {
                            WevSecurityConfig.AddNewUser(user);
                            await botClient.SendTextMessageAsync(message.Chat, $"Регистрация прошла успешно",
                                cancellationToken: cancellationToken);
                            break;
                        }

                        await botClient.SendTextMessageAsync(message.Chat, $"Ты уже был зарегистрирован 🤡",
                            cancellationToken: cancellationToken);
                        break;
                    }
                    case "/start":
                    case "/commands":
                    case "1":
                    {
                        var user = new TgUser(message.From.Username, Convert.ToInt32(message.From.Id));
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"Привет, @{user.UserName}, чтобы воспользоваться ботом, ты можешь написать команды (или просто цифру):" +
                            "\n0. /register - зарегистрироваться (внезапно)" +
                            "\n1. /commands - увидеть все команды" +
                            "\n2. /queue - встать в очередь" +
                            "\n3. /list - увидеть список очереди" +
                            "\n4. /stop - выйти из очереди", cancellationToken: cancellationToken);
                        break;
                    }
                    case "/queue":
                    case "2":
                    {
                        var user = new TgUser(message.From.Username, Convert.ToInt32(message.From.Id));
                        var userId = WevSecurityConfig.FindUserById(user.UserId);
                        if (userId != 0)
                        {
                            if (FindMemberById(userId) != null)
                            {
                                await botClient.SendTextMessageAsync(message.Chat, $"Ты уже есть в очереди" +
                                    $"\nТвой номер в ней: {Queue.Count}",
                                    cancellationToken: cancellationToken);
                                break;
                            }
                        }

                        Queue.Add(user);
                        await botClient.SendTextMessageAsync(message.Chat, $"Твой номер в очереди: {Queue.Count}",
                            cancellationToken: cancellationToken);

                        break;
                    }
                    case "/list":
                    case "3":
                    {
                        var list = "";
                        var count = 0;
                        foreach (TgUser member in Queue)
                        {
                            count++;
                            list += $"{count}. @{member.UserName}\n";
                        }

                        await botClient.SendTextMessageAsync(message.Chat, $"Список очереди:\n{list}",
                            cancellationToken: cancellationToken);
                        break;
                    }
                    case "/stop":
                    case "4":
                    {
                        TgUser user = new TgUser(message.From.Username, Convert.ToInt32(message.From.Id));
                        var userIdToRemove = WevSecurityConfig.FindUserById(user.UserId);
                        if (userIdToRemove != 0)
                        {
                            var member = FindMemberById(userIdToRemove);
                            if (member == null)
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                    $"Тебя и так не было в очереди, не переживай :)",
                                    cancellationToken: cancellationToken);
                                break;
                            }


                            Queue.Remove(member);
                            await botClient.SendTextMessageAsync(message.Chat,
                                $"Ты вышел из очереди, чтобы заново в неё встать напиши команду: `/queue`",
                                cancellationToken: cancellationToken);
                        }

                        break;
                    }
                    case "/anecdote":
                    {
                        var rnd = new Random();
                        var number = rnd.Next(0, 37);
                        var json = System.IO.File.ReadAllText(PathToAnecdotes);
                        dynamic stuff = JsonConvert.DeserializeObject(json);
                        await botClient.SendTextMessageAsync(message.Chat, $"{stuff[number].anecdote}",
                            cancellationToken: cancellationToken);
                        break;
                    }
                    case "/admin":
                    {
                        var user = new TgUser(message.From.Username, Convert.ToInt32(message.From.Id));
                        if (WevSecurityConfig.IsUserAnAdmin(Convert.ToInt32(message.From.Id)))
                        {
                            await botClient.SendTextMessageAsync(message.Chat,
                                $"Привет, @{user.UserName}, чтобы воспользоваться ботом, ты можешь написать команды:" +
                                $"\n1. /admin - увидеть админские команды" +
                                $"\n2. /list - увидеть список очереди" +
                                $"\n3. /pop - убрать кого-то из очереди (пример: '/pop Menemi')",
                                cancellationToken: cancellationToken);
                        }

                        break;
                    }
                    case "/users":
                    {
                        if (WevSecurityConfig.IsUserAnAdmin(Convert.ToInt32(message.From.Id)))
                        {
                            WevSecurityConfig.FindUserById(Convert.ToInt32(message.From.Id));
                        }

                        break;
                    }
                    default:
                    {
                        if (splitMessage[0].ToLower() == "/pop")
                        {
                            if (WevSecurityConfig.IsUserAnAdmin(Convert.ToInt32(message.From.Id)))
                            {
                                if (message.Text.ToLower().Replace(" ", "") == "/pop")
                                {
                                    await botClient.SendTextMessageAsync(message.Chat,
                                        "Надо вписать того, кого хочешь удалить",
                                        cancellationToken: cancellationToken);
                                    break;
                                }

                                TgUser userToRemove =
                                    FindMemberById(WevSecurityConfig.FindUserByUsername(splitMessage[1]));
                                if (userToRemove is null)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat,
                                        "Этого человека нет в очереди",
                                        cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    Queue.Remove(userToRemove);
                                    await botClient.SendTextMessageAsync(message.Chat,
                                        $"{userToRemove.UserId}:{userToRemove.UserName} удалён из очереди",
                                        cancellationToken: cancellationToken);
                                }
                            }
                        }
                        else
                        {
                            TgUser user = new TgUser(message.From.Username, Convert.ToInt32(message.From.Id));
                            await botClient.SendTextMessageAsync(message.Chat,
                                "Ты ввёл какую-то незнакомую мне команду :(", cancellationToken: cancellationToken);
                            await botClient.SendTextMessageAsync(message.Chat,
                                $"Привет, @{user.UserName}, чтобы воспользоваться ботом, ты можешь написать команды (или просто цифру):" +
                                "\n0. /register - зарегистрироваться (внезапно)" +
                                "\n1. /commands - увидеть все команды" +
                                "\n2. /queue - встать в очередь" +
                                "\n3. /list - увидеть список очереди" +
                                "\n4. /stop - выйти из очереди", cancellationToken: cancellationToken);
                        }

                        break;
                    }
                }
            }
        }

        private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        // TODO: start
        private static void Main(string[] args)
        {
            WevSecurityConfig.TestConnection();
            Console.WriteLine("Bot started its work " + Bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };
            Bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}