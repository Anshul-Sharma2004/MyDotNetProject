using System.Collections.Generic;

namespace RoleBasedJWTMVC.Models
{
    public class ChatBoxViewModel
    {
        public string SenderId { get; set; }
        public string SenderName { get; set; }   // Added
        public string ReceiverId { get; set; }
        public string ReceiverName { get; set; }

        public List<ChatMessage> Messages { get; set; }   // Fixed type
    }
}
