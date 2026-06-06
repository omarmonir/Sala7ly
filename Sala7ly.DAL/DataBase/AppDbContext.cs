using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sala7ly.DAL.Entities;


namespace Sala7ly.DAL.DataBase
{
    public class AppDbContext : IdentityDbContext<User>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

   

       

        public DbSet<CustomerProfile> CustomerProfiles { get; set; }
        public DbSet<TechnicianProfile> TechnicianProfiles { get; set; }
        public DbSet<TechnicianVerification> TechnicianVerifications { get; set; }
        public DbSet<TechnicianPortfolio> TechnicianPortfolios { get; set; }
        public DbSet<TechnicianCategory> TechnicianCategories { get; set; }
        public DbSet<FavoriteTechnician> FavoriteTechnicians { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Ai_Interaction> AiInteractions { get; set; }
        public DbSet<EscrowTransaction> EscrowTransactions { get; set; }
        public DbSet<Dispute> Disputes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}