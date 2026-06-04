using Sala7ly.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Entities
{
    public class ChatMessage : BaseEntity
    {

        public ChatMessage()
        {
            
        }
        public int RequestId { get; private set; }

        public string SenderId { get; private set; }

        public string? Content { get; private set; }

        public string[]? AttachmentUrls { get; private set; }

        public ChatMessageType MessageType { get; private set; }

        public int? DurationSeconds { get; private set; }

        public bool IsRead { get; private set; }

        public DateTime? ReadAt { get; private set; }

        public DateTime SentAt { get; private set; } = DateTime.UtcNow;


        // NP

        public ServiceRequest ServiceRequest { get; private set; }
        public User Sender { get; private set; }


    }

}
