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

            builder.Property(p => p.ImageUrlBefore).IsRequired();
            builder.Property(p => p.ImageUrlAfter).IsRequired();
            builder.Property(p => p.Caption).HasMaxLength(255);
            builder.Property(p => p.ServiceRequestId).IsRequired(false);
        }
    }
}