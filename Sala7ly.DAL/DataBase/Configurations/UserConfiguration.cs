using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;
namespace Sala7ly.DAL.DataBase.Configurations
{

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {

            builder.Property(u => u.Name)
                   .HasMaxLength(256)
                   .IsRequired();

            builder.Property(u => u.Email)
                   .HasMaxLength(256);

            builder.Property(u => u.ImageUrl)
                   .HasMaxLength(500)
                   .IsRequired(false);

            builder.Property(u => u.LastLoginAt)
                   .IsRequired(false);

            builder.Property(u => u.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(u => u.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(u => u.IsActive)
                   .HasDefaultValue(true);

            builder.HasOne(u =>u.CustomerProfile)
                   .WithOne(c => c.User)
                   .HasForeignKey<CustomerProfile>(c => c.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u =>u.TechnicianProfile)
                   .WithOne(t => t.User)
                   .HasForeignKey<TechnicianProfile>(t => t.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(u => u.RefreshTokens)
                   .WithOne(r => r.User)
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(u => u.UserName)
                   .HasMaxLength(256);

            builder.Property(u => u.NormalizedUserName)
                   .HasMaxLength(256);

            builder.Property(u => u.NormalizedEmail)
                   .HasMaxLength(256);

            builder.Property(u => u.PhoneNumber)
                   .HasMaxLength(20)
                   .IsRequired(false);

            builder.HasIndex(u => u.UserName).IsUnique();
            builder.HasIndex(u => u.Email);
            builder.HasIndex(u => u.NormalizedUserName).IsUnique();
            builder.HasIndex(u => u.NormalizedEmail);
        }
    }
}
