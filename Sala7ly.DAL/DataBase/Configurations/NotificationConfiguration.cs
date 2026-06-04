using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.DataBase.Configurations
{

    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {

        public void Configure(EntityTypeBuilder<Notification> builder)
        {

            builder.ToTable("Notifications");

            builder.HasKey(x => x.Id);


            // UserId FK
            builder.HasOne(x => x.User)
                   .WithMany(x => x.Notifications)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);


            // ActorId FK 
            builder.HasOne(x => x.Actor)
                   .WithMany(x => x.TriggeredNotifications)
                   .HasForeignKey(x => x.ActorId)
                   .OnDelete(DeleteBehavior.SetNull);


        }

    }


}
