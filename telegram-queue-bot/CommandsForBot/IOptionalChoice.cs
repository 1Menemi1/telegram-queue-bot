using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace telegram_queue_bot.CommandsForBot
{
    public interface IOptionalChoice
    {
        Task GiveAChoice(Message message);
    }
}