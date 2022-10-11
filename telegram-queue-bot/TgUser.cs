using System;

namespace telegram_queue_bot
{
    public class TgUser
    {
        public String UserName { get; }
        public int UserId { get; }

        public TgUser(String Name, int Id)
        {
            UserName = Name;
            UserId = Id;
        }
    }
}