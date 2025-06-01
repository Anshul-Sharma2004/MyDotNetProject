using System.Collections.Generic;

namespace RoleBasedJWTMVC.Models
{
    public class ChatBoxViewModel
    {
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string ReceiverName { get; set; }

        public List<Message> Messages { get; set; }
    }

    public class Message
    {
        public string SenderId { get; set; }
        public string Text { get; set; }
    }
}
