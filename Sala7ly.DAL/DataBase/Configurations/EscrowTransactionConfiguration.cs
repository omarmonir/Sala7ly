using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.DataBase.Configurations
{
    public class EscrowTransactionConfiguration : IEntityTypeConfiguration<EscrowTransaction>
    {
        public void Configure(EntityTypeBuilder<EscrowTransaction> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.ServiceRequestId).IsRequired();
            builder.Property(e => e.CustomerId).IsRequired();
            builder.Property(e => e.TechnicianId).IsRequired();
            builder.Property(e => e.DisputeId).IsRequired(false);

            builder.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            builder.Property(e => e.PlatformFee).HasPrecision(18, 2).IsRequired();
            builder.Property(e => e.TechnicianPayout).HasPrecision(18, 2).IsRequired();

            builder.Property(e => e.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasDefaultValue(EscrowStatus.PendingDeposit);

            builder.Property(e => e.ProviderRef).HasMaxLength(255);
            builder.Property(e => e.ProviderReceiptUrl).IsRequired(false);

            // relationships
            builder.HasOne(e => e.ServiceRequest)
                   .WithOne(sr => sr.EscrowTransaction)
                   .HasForeignKey<EscrowTransaction>(e => e.ServiceRequestId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Customer)
                   .WithMany()
                   .HasForeignKey(e => e.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Technician)
                   .WithMany()
                   .HasForeignKey(e => e.TechnicianId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Dispute)
                   .WithMany()
                   .HasForeignKey(e => e.DisputeId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired(false);

            builder.HasMany(e => e.WalletTransactions)
                   .WithOne(wt => wt.EscrowTransaction)
                   .HasForeignKey(wt => wt.EscrowTransactionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
