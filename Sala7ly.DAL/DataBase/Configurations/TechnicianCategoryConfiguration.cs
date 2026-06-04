using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.DataBase.Configurations
{
    public class TechnicianCategoryConfiguration : IEntityTypeConfiguration<TechnicianCategory>
    {
        public void Configure(EntityTypeBuilder<TechnicianCategory> builder)
        {
            builder.HasKey(tc => tc.Id);

            builder.Property(tc => tc.IsPrimary).HasDefaultValue(false);

            // prevents a tech being linked to the same category twice
            builder.HasIndex(tc => new { tc.TechnicianId, tc.CategoryId }).IsUnique();

            // Technician side configured from TechnicianProfile.
            // Category side: Restrict so deleting a category doesn't create
            // a second cascade path into this table.
            builder.HasOne(tc => tc.Category)
                   .WithMany(c => c.TechnicianCategories)
                   .HasForeignKey(tc => tc.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}