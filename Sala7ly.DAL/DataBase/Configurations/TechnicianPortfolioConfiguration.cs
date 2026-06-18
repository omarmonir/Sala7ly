using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.DataBase.Configurations
{
    public class TechnicianPortfolioConfiguration : IEntityTypeConfiguration<TechnicianPortfolio>
    {
        public void Configure(EntityTypeBuilder<TechnicianPortfolio> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Title).IsRequired().HasMaxLength(150);
            builder.Property(p => p.Description).HasMaxLength(1000);
            builder.Property(p => p.ImageUrlBefore).IsRequired();
            builder.Property(p => p.ImageUrlAfter).IsRequired();
        }
    }
}