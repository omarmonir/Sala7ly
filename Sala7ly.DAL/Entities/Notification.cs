using Sala7ly.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Entities
{
    public class Notification : BaseEntity
    {

        public string UserId { get; set; }   
        public string? ActorId { get; set; }  

        public NotificationType Type { get; set; }

        public string Title { get; set; } = null!;

        public string Body { get; set; } = null!;

        public string DeepLink { get; set; }

        public string Metadata { get; set; } 

        public bool IsRead { get; set; }

        public bool IsPushed { get; set; }

        public DateTime SentAt { get; set; }

        public DateTime? ReadAt { get; set; }


        // NP
        public User User { get; set; } = null;
        public User? Actor { get; set; }


    }
}
