using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;

namespace Sala7ly.DAL.DataBase.Configurations
{
    public class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
    {
        public void Configure(EntityTypeBuilder<ServiceRequest> builder)
        {
            builder.HasKey(sr => sr.Id);

            builder.Property(sr => sr.Title).IsRequired().HasMaxLength(500);
            builder.Property(sr => sr.Description).IsRequired().HasMaxLength(5000);
            builder.Property(sr => sr.ImageUrls)
                   .HasConversion(
                       v => string.Join(",", v ?? new List<string>()),
                       v => new List<string>(v.Split(",", System.StringSplitOptions.RemoveEmptyEntries)))
                   .HasColumnType("nvarchar(max)");

            builder.Property(sr => sr.Urgency)
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(sr => sr.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasDefaultValue(Enums.Status.open);

            builder.Property(sr => sr.BookingMode)
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(sr => sr.AiPriceMin).HasPrecision(18, 2);
            builder.Property(sr => sr.AiPriceMax).HasPrecision(18, 2);
            builder.Property(sr => sr.IsEmergency).HasDefaultValue(false);
            builder.Property(sr => sr.SurgeMultiplier).HasPrecision(5, 2).HasDefaultValue(1.0m);

            builder.Property(sr => sr.CustomerId).IsRequired();
            builder.Property(sr => sr.AddressId).IsRequired();
            builder.Property(sr => sr.CategoryId).IsRequired();
            builder.Property(sr => sr.SelectedBidId).IsRequired(false);

            // relationships
            builder.HasOne(sr => sr.Profile)
                   .WithMany(c => c.ServiceRequests)
                   .HasForeignKey(sr => sr.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.Category)
                   .WithMany(s => s.ServiceRequests)
                   .HasForeignKey(sr => sr.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.Address)
                   .WithMany(a => a.ServiceRequests)
                   .HasForeignKey(sr => sr.AddressId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.SelectedBid)
                   .WithOne()
                   .HasForeignKey<ServiceRequest>(sr => sr.SelectedBidId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(sr => sr.Bids)
                   .WithOne(b => b.ServiceRequest)
                   .HasForeignKey(b => b.ServiceRequestId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(sr => sr.ChatMessages)
                   .WithOne(x => x.ServiceRequest)
                   .HasForeignKey(x => x.RequestId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(sr=>sr.TechnicianPortfolios)
                .WithOne(tp => tp.ServiceRequest)
                .HasForeignKey(tp => tp.ServiceRequestId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
