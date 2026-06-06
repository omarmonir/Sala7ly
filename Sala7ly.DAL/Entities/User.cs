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
        public string Name { get;  set; }
        public string ImageUrl { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
        public string? ResetOtp { get; private set; }
        public DateTime? ResetOtpExpiry { get; private set; }
        public int FailedOtpAttempts { get; private set; }

        public DateTime? DeactivationDate { get; private set; }
        public bool IsActive { get; private set; } = true;


        public void Activate()
        {
            IsActive = true;
            DeactivationDate = null;
            UpdatedAt = DateTime.UtcNow;
        }
        public void Deactivate()
        {
            IsActive = false;
            DeactivationDate = DateTime.UtcNow;
        }
        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetOtp(string otp, DateTime expiry)
        {
            ResetOtp = otp;
            ResetOtpExpiry = expiry;
            FailedOtpAttempts = 0;
        }

        public void ClearOtp()
        {
            ResetOtp = null;
            ResetOtpExpiry = null;
            FailedOtpAttempts = 0;
        }
        public void IncrementFailedOtpAttempts()
        {
            FailedOtpAttempts++;
            UpdatedAt = DateTime.UtcNow;
        }

        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
        public CustomerProfile CustomerProfile { get; private set; }
        public TechnicianProfile TechnicianProfile { get; private set; }
        public Wallet Wallet { get; private set; }
        public ICollection<Notification> Notifications { get; private set; } = new List<Notification>();
        public ICollection<Notification> TriggeredNotifications { get; private set; } = new List<Notification>();
        public ICollection<Ai_Interaction> AiInteractions { get; private set; } = new List<Ai_Interaction>();
    }
}
