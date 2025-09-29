using System;

namespace BrainWave.APP.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public int CollaborationId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
