using System;

namespace BrainWave.APP.Models
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public int CollaborationId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }

    public class CreateMessageDto
    {
        public int CollaborationId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
