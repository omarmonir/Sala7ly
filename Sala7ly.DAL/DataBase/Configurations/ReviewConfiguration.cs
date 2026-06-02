using Sala7ly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.DataBase.Configurations
{
   
    
    //public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    //{
    //    public void Configure(EntityTypeBuilder<Review> builder)
    //    {
    //        builder.ToTable("Reviews");

    //        builder.HasKey(x => x.Id);


    //        // One-to-One   ( Review , ServiceRequest )

    //        builder.HasOne(x => x.ServiceRequest)
    //            .WithOne(x => x.Review)
    //            .HasForeignKey<Review>(x => x.RequestId)
    //            .OnDelete(DeleteBehavior.Cascade);


    //    }
    //}




}
