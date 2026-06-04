using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;

namespace Sala7ly.DAL.DataBase.Configurations
{
    public class BidConfiguration : IEntityTypeConfiguration<Bid>
    {
        public void Configure(EntityTypeBuilder<Bid> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.TechnicianId).IsRequired();
            builder.Property(b => b.ServiceRequestId).IsRequired();
            builder.Property(b => b.Price).HasPrecision(18, 2).IsRequired();
            builder.Property(b => b.ProposalMessage).HasMaxLength(1000);
            builder.Property(b => b.EstimatedDurationMinutes).IsRequired();

            builder.Property(b => b.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasDefaultValue(Enums.BidStatus.pending);

            builder.Property(b => b.ValidUntil).IsRequired();
            builder.Property(b => b.SubmittedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(b => b.RespondedAt).IsRequired(false);
            builder.Property(b => b.IsAccepted).HasDefaultValue(false);

            // relationships
            builder.HasOne(b => b.Technician)
                   .WithMany(t => t.Bids)
                   .HasForeignKey(b => b.TechnicianId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ServiceRequest relationship is configured from ServiceRequest side to avoid cascade path issues
            builder.HasOne(b => b.ServiceRequest)
                   .WithMany(sr => sr.Bids)
                   .HasForeignKey(b => b.ServiceRequestId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
