using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sala7ly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.DataBase.Configurations
{

    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("ChatMessages");

            builder.HasKey(x => x.Id);

            // One to Many (ServiceRequest, ChatMessages)
            // Use Restrict to avoid cascade path issues (cascade is configured from ServiceRequest side)
            builder.HasOne(x => x.ServiceRequest)
                .WithMany(x => x.ChatMessages)
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sender relationship - use Restrict to avoid cascade path issues
            builder.HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }


}
