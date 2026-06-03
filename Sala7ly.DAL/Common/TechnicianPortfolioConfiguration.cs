using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Configurations
{
    public class TechnicianPortfolioConfiguration : IEntityTypeConfiguration<TechnicianPortfolio>
    {
        public void Configure(EntityTypeBuilder<TechnicianPortfolio> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.ImageUrl).IsRequired();
            builder.Property(p => p.Caption).HasMaxLength(255);
            builder.Property(p => p.Type).HasMaxLength(20); // before, after, general
            builder.Property(p => p.RequestId).IsRequired(false); // nullable

            // Technician relationship is configured from TechnicianProfile side.
            // RequestId → ServiceRequest: configure once that entity exists.
        }
    }
}