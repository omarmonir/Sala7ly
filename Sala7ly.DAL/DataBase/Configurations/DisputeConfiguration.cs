using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.DataBase.Configurations
{
    public class DisputeConfiguration : IEntityTypeConfiguration<Dispute>
    {
        public void Configure(EntityTypeBuilder<Dispute> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.EscrowTransactionId).IsRequired();
            builder.Property(d => d.ServiceRequestId).IsRequired();
            builder.Property(d => d.InitiatedByUserId).IsRequired();
            builder.Property(d => d.ResolvedByAdminId).IsRequired(false);

            builder.Property(d => d.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasDefaultValue(DisputeStatus.Open);

            builder.Property(d => d.Reason).HasMaxLength(1000);
            builder.Property(d => d.Resolution).IsRequired(false).HasMaxLength(2000);
            builder.Property(d => d.RefundAmount).HasPrecision(18, 2);

            // relationships
            builder.HasOne(d => d.EscrowTransaction)
                   .WithMany()
                   .HasForeignKey(d => d.EscrowTransactionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.ServiceRequest)
                   .WithMany()
                   .HasForeignKey(d => d.ServiceRequestId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
