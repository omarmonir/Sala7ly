using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.DataBase.Configurations
{
    public class FavoriteTechnicianConfiguration : IEntityTypeConfiguration<FavoriteTechnician>
    {
        public void Configure(EntityTypeBuilder<FavoriteTechnician> builder)
        {
            builder.HasKey(f => f.Id);

            // Create a unique composite key to prevent duplicates
            builder.HasIndex(f => new { f.CustomerId, f.TechnicianId }).IsUnique();

            builder.Property(f => f.CustomerId).IsRequired();
            builder.Property(f => f.TechnicianId).IsRequired();
            builder.Property(f => f.AddedAt).HasDefaultValueSql("GETUTCDATE()");

            // relationships
            builder.HasOne(f => f.Customer)
                   .WithMany(c => c.FavoriteTechnicians)
                   .HasForeignKey(f => f.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.Technician)
                   .WithMany(t => t.FavoritedByCustomers)
                   .HasForeignKey(f => f.TechnicianId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
