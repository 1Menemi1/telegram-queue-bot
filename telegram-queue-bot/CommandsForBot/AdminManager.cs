using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace telegram_queue_bot.CommandsForBot.Admin
{
    public class AdminManager : IOptionalChoice
    {
        private static ITelegramBotClient _botClient;
        private static CancellationToken _cancellationToken;

        private static bool IsUserAnAdmin(Message message)
        {
            var isAdmin = DataBaseConfig.IsUserAnAdmin(Convert.ToInt32(message.From.Id));
            if (!isAdmin)
            {
                PrintErrorMessage(message);
            }

            return isAdmin;
        }

        private static async Task PrintErrorMessage(Message message)
        {
            await _botClient.SendTextMessageAsync(message.Chat,
                "Ты ввёл какую-то незнакомую мне команду :(", cancellationToken: _cancellationToken);
        }

        public AdminManager(ITelegramBotClient bot, CancellationToken token)
        {
            _botClient = bot;
            _cancellationToken = token;
        }

        public async Task GiveAChoice(Message message)
        {
            if (!IsUserAnAdmin(message))
            {
                return;
            }

            await _botClient.SendTextMessageAsync(message.Chat,
                "\n1. /admin - увидеть админские команды" +
                "\n2. /list - увидеть список очереди" +
                "\n3. /pop - убрать кого-то из очереди (пример: '/pop Menemi')" +
                "\n4. /reset - очистить очередь целиком" +
                "\n5. /secret - показывает секретные команды",
                cancellationToken: _cancellationToken);
        }

        public async Task ResetCommand(Message message)
        {
            if (!IsUserAnAdmin(message))
            {
                return;
            }

            DataBaseConfig.ResetQueue();
            await _botClient.SendTextMessageAsync(message.Chat,
                $"Ты зачем мне всю очередь стёр(-ла)...", cancellationToken: _cancellationToken);
        }

        public async Task PopCommand(Message message, string[] splitMessage)
        {
            if (!IsUserAnAdmin(message))
            {
                return;
            }

            if (message.Text.ToLower().Replace(" ", "") == "/pop")
            {
                await _botClient.SendTextMessageAsync(message.Chat,
                    "Надо вписать того, кого хочешь удалить",
                    cancellationToken: _cancellationToken);
                return;
            }

            var usernameToRemove = splitMessage[1].Replace("@", "");
            var memberId = DataBaseConfig.FindUserInQueue(usernameToRemove);
            if (memberId == 0)
            {
                await _botClient.SendTextMessageAsync(message.Chat,
                    "Этого человека нет в очереди",
                    cancellationToken: _cancellationToken);
            }
            else
            {
                DataBaseConfig.RemoveFromQueue(memberId);
                await _botClient.SendTextMessageAsync(message.Chat,
                    $"{memberId}:@{usernameToRemove} удален(-а) из очереди",
                    cancellationToken: _cancellationToken);
            }
        }

        public async Task SecretCommand(Message message)
        {
            if (!IsUserAnAdmin(message))
            {
                return;
            }

            await _botClient.SendTextMessageAsync(message.Chat,
                "Секретные команды:" +
                "\n1. /anecdote", cancellationToken: _cancellationToken);
        }
    }
}