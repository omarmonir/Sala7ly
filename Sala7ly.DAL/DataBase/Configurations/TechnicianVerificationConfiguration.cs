using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.DataBase.Configurations
{
    public class TechnicianVerificationConfiguration : IEntityTypeConfiguration<TechnicianVerification>
    {
        public void Configure(EntityTypeBuilder<TechnicianVerification> builder)
        {
            builder.HasKey(v => v.Id);

            
            //builder.Property(v => v.DocType)
            //       .HasConversion<string>()
            //       .HasMaxLength(20);

            builder.Property(v => v.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasDefaultValue(VerificationStatus.Pending);

            builder.Property(v => v.RejectionReason).HasMaxLength(500);
            builder.Property(v => v.ReviewedByAdminId).IsRequired(false); 

            
        }
    }
}