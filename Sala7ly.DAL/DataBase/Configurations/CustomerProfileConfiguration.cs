using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.DataBase.Configurations
{
    public class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
    {
        public void Configure(EntityTypeBuilder<CustomerProfile> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.UserId).IsRequired();
            builder.Property(c => c.TotalSpent).HasPrecision(18, 2);
            //builder.Property(c => c.IsBusinessAccount).HasDefaultValue(false);

            // Configure relationship with User
            builder.HasOne(c => c.User)
                   .WithOne(u => u.CustomerProfile)
                   .HasForeignKey<CustomerProfile>(c => c.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany<Address>()
                   .WithOne(a => a.Customer)
                   .HasForeignKey(a => a.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}