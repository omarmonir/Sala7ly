using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Sala7ly.DAL.Entities
{
    public class User : IdentityUser
    {
        public User()
        {
            
        }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string ImageUrl { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public CustomerProfile CustomerProfile { get; set; }
        public TechnicianProfile TechnicianProfile { get; set; }
        public Wallet Wallet { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Notification> TriggeredNotifications { get; set; } = new List<Notification>();
        public ICollection<Ai_Interaction> AiInteractions { get; set; } = new List<Ai_Interaction>();
    }
}
