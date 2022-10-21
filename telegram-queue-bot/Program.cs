using System;
using System.Threading;
using System.Threading.Tasks;
using telegram_queue_bot.CommandsForBot;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace telegram_queue_bot
{
    internal static class Program
    {
        private const string PathToAnecdotes = "D:\\ITMO\\telegram-queue-bot\\telegram-queue-bot\\Anecdotes.json";

        private const string PathToToken =
            "D:\\ITMO\\telegram-queue-bot\\telegram-queue-bot\\SecretInformation\\Token.txt";

        private static readonly string Token = System.IO.File.ReadAllText(PathToToken);
        private static readonly ITelegramBotClient Bot = new TelegramBotClient($"{Token}");

        private static UserManager _userManager;
        private static AdminManager _adminManager;

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            _userManager = new UserManager(botClient, cancellationToken);
            _adminManager = new AdminManager(botClient, cancellationToken);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text == null)
                {
                    await botClient.SendTextMessageAsync(message.Chat,
                        $"Ты отправил(-а) незнакомый мне тип сообщения",
                        cancellationToken: cancellationToken);
                    return;
                }

                var splitMessage = message.Text.Split(' ');

                switch (message.Text.ToLower())
                {
                    case "/register":
                    case "/start":
                    case "0":
                    {
                        await _userManager.RegisterCommand(message);
                        break;
                    }
                    case "/commands":
                    case "1":
                    {
                        await _userManager.GiveAChoice(message);
                        break;
                    }
                    case "/queue":
                    case "2":
                    {
                        await _userManager.QueueCommand(message);
                        break;
                    }
                    case "/list":
                    case "3":
                    {
                        await _userManager.ListCommand(message);
                        break;
                    }
                    case "/stop":
                    case "4":
                    {
                        await _userManager.StopCommand(message);
                        break;
                    }
                    case "/anecdote":
                    {
                        await _userManager.AnecdoteCommand(message, PathToAnecdotes);
                        break;
                    }
                    case "/admin":
                    {
                        await _adminManager.GiveAChoice(message);
                        break;
                    }
                    case "/reset":
                    {
                        await _adminManager.ResetCommand(message);
                        break;
                    }
                    case "/secret":
                    {
                        await _adminManager.SecretCommand(message);
                        break;
                    }
                    default:
                    {
                        if (splitMessage[0].ToLower() == "/pop")
                        {
                            await _adminManager.PopCommand(message, splitMessage);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat,
                                "Ты ввел(-а) какую-то незнакомую мне команду :(\n" +
                                "Список всех команд можно увидеть, если написать: `/commands`",
                                cancellationToken: cancellationToken);
                        }

                        break;
                    }
                }

                Console.WriteLine();
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
            DataBaseConfig.TestConnection();
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