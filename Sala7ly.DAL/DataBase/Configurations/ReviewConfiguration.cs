using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.DataBase.Configurations
{


    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(x => x.Id);

            // One-to-One (Review, ServiceRequest)
            // Use Restrict to avoid circular cascade path
            builder.HasOne(x => x.ServiceRequest)
                .WithOne(x => x.Review)
                .HasForeignKey<Review>(x => x.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reviewer relationship - use Restrict to avoid multiple cascade paths
            builder.HasOne(x => x.Reviewer)
                .WithMany()
                .HasForeignKey(x => x.ReviewerID)
                .OnDelete(DeleteBehavior.Restrict);

            // Reviewee relationship - use Restrict to avoid multiple cascade paths
            builder.HasOne(x => x.Reviewee)
                .WithMany()
                .HasForeignKey(x => x.RevieweeID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }




}
