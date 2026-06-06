using System.ComponentModel.DataAnnotations;

namespace Sala7ly.DAL.Entities
{
    public class RefreshToken
    {
        public RefreshToken()
        {
            
        }

        [Key]
        public int Id { get; private set; }
        public string Token { get; private set; }
        public DateTime Expires { get; private set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created { get; private set; } = DateTime.UtcNow;
        public DateTime? Revoked { get; private set; }
        public bool IsRevoked => Revoked != null;

        public bool IsActive => !IsRevoked  && !IsExpired;
        public string UserId { get; private set; } = null!;
        public virtual User User { get; private set; }
        public string? ReplacedByToken { get; private set; }

        public static RefreshToken Create(string userId, string token, int validityDays)
        {
            return new RefreshToken
            {
                Token = token,
                Expires = DateTime.UtcNow.AddDays(validityDays),
                UserId = userId
            };
        }
        public void Revoke(string? replacedByToken = null)
        {
            Revoked = DateTime.Now;
            ReplacedByToken = replacedByToken;
        }
    }
}
