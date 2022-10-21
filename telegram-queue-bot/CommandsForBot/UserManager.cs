using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace telegram_queue_bot.CommandsForBot
{
    public class UserManager : IOptionalChoice
    {
        private static ITelegramBotClient _botClient;
        private static CancellationToken _cancellationToken;

        private static int GetPositionInQueue(int userId)
        {
            var count = 0;
            foreach (var member in DataBaseConfig.GetAllUsersInQueue())
            {
                count++;
                if (userId.Equals(member.UserId))
                {
                    return count;
                }
            }

            return 0;
        }

        public UserManager(ITelegramBotClient bot, CancellationToken token)
        {
            _botClient = bot;
            _cancellationToken = token;
        }

        public async Task GiveAChoice(Message message)
        {
            await _botClient.SendTextMessageAsync(message.Chat,
                $"Привет, @{message.From.Username}, чтобы воспользоваться ботом, ты можешь написать команды (или просто цифру):" +
                "\n0. /register - зарегистрироваться (внезапно)" +
                "\n1. /commands - увидеть все команды" +
                "\n2. /queue - встать в очередь" +
                "\n3. /list - увидеть список очереди" +
                "\n4. /stop - выйти из очереди", cancellationToken: _cancellationToken);
        }

        public async Task RegisterCommand(Message message)
        {
            var user = new TgUser(message.From.Username, Convert.ToInt32(message.From.Id));
            if (DataBaseConfig.FindUserInDataBase(user.UserId) == 0)
            {
                DataBaseConfig.RegisterNewUser(user);
                await _botClient.SendTextMessageAsync(message.Chat, "Регистрация прошла успешно",
                    cancellationToken: _cancellationToken);
                return;
            }

            await _botClient.SendTextMessageAsync(message.Chat, "Ты уже был(-а) зарегистрирован(-а) 🤡",
                cancellationToken: _cancellationToken);
        }

        public async Task QueueCommand(Message message)
        {
            var user = new TgUser(message.From.Username, Convert.ToInt32(message.From.Id));
            var userId = DataBaseConfig.FindUserInDataBase(user.UserId);
            if (userId != 0)
            {
                if (DataBaseConfig.FindUserInQueue(userId) != 0)
                {
                    await _botClient.SendTextMessageAsync(message.Chat,
                        $"Ты уже есть в очереди\nТвой номер в ней: {GetPositionInQueue(userId)}",
                        cancellationToken: _cancellationToken);
                    return;
                }
            }

            DataBaseConfig.AddUserToQueue(user);
            await _botClient.SendTextMessageAsync(message.Chat,
                $"Твой номер в очереди: {GetPositionInQueue(userId)}",
                cancellationToken: _cancellationToken);
        }

        public async Task ListCommand(Message message)
        {
            var list = "";
            var count = 0;
            foreach (var member in DataBaseConfig.GetAllUsersInQueue())
            {
                count++;
                list += $"{count}. @{member.UserName}\n";
            }

            await _botClient.SendTextMessageAsync(message.Chat, $"Список очереди:\n{list}",
                cancellationToken: _cancellationToken);
        }

        public async Task StopCommand(Message message)
        {
            var user = new TgUser(message.From.Username, Convert.ToInt32(message.From.Id));
            var userIdToRemove = DataBaseConfig.FindUserInDataBase(user.UserId);
            if (userIdToRemove != 0)
            {
                var memberId = DataBaseConfig.FindUserInQueue(userIdToRemove);
                if (memberId == 0)
                {
                    await _botClient.SendTextMessageAsync(message.Chat,
                        "Тебя и так не было в очереди, не переживай :)",
                        cancellationToken: _cancellationToken);
                    return;
                }

                var positionOfUserToRemove = GetPositionInQueue(memberId);
                DataBaseConfig.RemoveFromQueue(memberId);
                await _botClient.SendTextMessageAsync(message.Chat,
                    "Ты вышел(-ла) из очереди, чтобы заново в неё встать, напиши команду: `/queue`",
                    cancellationToken: _cancellationToken);

                if (positionOfUserToRemove.Equals(1))
                {
                    var allUsersInQueue = DataBaseConfig.GetAllUsersInQueue();
                    if (allUsersInQueue.Count.Equals(0))
                    {
                        return;
                    }
                    await _botClient.SendTextMessageAsync(allUsersInQueue[0].UserId,
                        $"@{allUsersInQueue[0].UserName}, очередь подошла к тебе, ты сдаёшь следующий(-ей)",
                        cancellationToken: _cancellationToken);
                }
            }
        }

        public async Task AnecdoteCommand(Message message, string PathToAnecdotes)
        {
            var rnd = new Random();
            var number = rnd.Next(0, 37);
            var json = System.IO.File.ReadAllText(PathToAnecdotes);
            dynamic stuff = JsonConvert.DeserializeObject(json);
            await _botClient.SendTextMessageAsync(message.Chat, $"{stuff[number].anecdote}",
                cancellationToken: _cancellationToken);
        }
    }
}