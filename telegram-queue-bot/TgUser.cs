namespace telegram_queue_bot
{
    public class TgUser
    {
        public string UserName { get; }
        public int UserId { get; }

        public TgUser(string name, int id)
        {
            UserName = name;
            UserId = id;
        }
    }
}