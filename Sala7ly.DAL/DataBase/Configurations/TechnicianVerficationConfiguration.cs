using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Configurations
{
    public class TechnicianVerificationConfiguration : IEntityTypeConfiguration<TechnicianVerification>
    {
        public void Configure(EntityTypeBuilder<TechnicianVerification> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.DocumentUrlFront)
                   .IsRequired();

            builder.Property(v => v.DocumentUrlBack)
                   .IsRequired();

            builder.Property(v => v.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasDefaultValue(VerificationStatus.Pending);

            builder.Property(v => v.RejectionReason)
                   .HasMaxLength(500)
                   .IsRequired(false);          // nullable

            builder.Property(v => v.ReviewedByAdminId)
                   .IsRequired(false);          // nullable

            // relationship: many verifications → one technician
            builder.HasOne(v => v.Technician)
                   .WithMany(t => t.Verifications)
                   .HasForeignKey(v => v.TechnicianId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}