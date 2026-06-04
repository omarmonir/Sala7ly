using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.DataBase.Configurations
{
    public class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
    {
        public void Configure(EntityTypeBuilder<ServiceCategory> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.NameAr).IsRequired().HasMaxLength(150);
            builder.Property(c => c.IsActive).HasDefaultValue(true);
            builder.Property(c => c.ParentCategoryId).IsRequired(false); 

           
            builder.HasOne(c => c.ParentCategory)
                   .WithMany(c => c.SubCategories)
                   .HasForeignKey(c => c.ParentCategoryId)
                   .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}