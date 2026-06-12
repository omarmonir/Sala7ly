using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.DataBase.Configurations
{
    public class TechnicianProfileConfiguration : IEntityTypeConfiguration<TechnicianProfile>
    {
        public void Configure(EntityTypeBuilder<TechnicianProfile> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.UserId).IsRequired();
            builder.Property(t => t.AvgResponseTime).HasMaxLength(12);
            builder.Property(t => t.IsApproved).HasDefaultValue(false);
            builder.Property(t => t.IsFeatured).HasDefaultValue(false);

            // Configure relationship with User
            builder.HasOne(t => t.User)
                   .WithOne(u => u.TechnicianProfile)
                   .HasForeignKey<TechnicianProfile>(t => t.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // enum stored as readable text — capped so it isn't nvarchar(max)
            builder.Property(t => t.SubscriptionTier)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasDefaultValue(SubscriptionTier.Free);

            // SQL Server: store the embedding as JSON text.
            // Not searchable in-DB — for similarity search, compare in app
            // or move to a dedicated vector store later.
            builder.Property(t => t.EmbeddingVector)
                   .HasConversion(
                       v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                       v => JsonSerializer.Deserialize<float[]>(v, (JsonSerializerOptions)null))
                   .HasColumnType("nvarchar(max)");

            builder.HasMany(t => t.Verifications)
                   .WithOne(v => v.Technician)
                   .HasForeignKey(v => v.TechnicianId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Portfolio)
                   .WithOne(p => p.Technician)
                   .HasForeignKey(p => p.TechnicianId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Categories)
                   .WithOne(tc => tc.Technician)
                   .HasForeignKey(tc => tc.TechnicianId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}