using Sala7ly.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Entities
{
    public class ChatMessage
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RequestId { get; set; }

        public Guid SenderId { get; set; }

        public string? Content { get; set; }

        public string[]? AttachmentUrls { get; set; }

        public ChatMessageType MessageType { get; set; }

        public int? DurationSeconds { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;


        // NP

        // public ServiceRequest ServiceRequest { get; set; }
       // public User Sender { get; set; }


    }

}
