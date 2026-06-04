using Sala7ly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.DataBase.Configurations
{

    //public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    //{
    //    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    //    {
    //        builder.ToTable("ChatMessages");

    //        builder.HasKey(x => x.Id);


    //        // One  to Many ( ServiceRequest  ,  ChatMessages )
    //        builder.HasOne(x => x.ServiceRequest)
    //            .WithMany(x => x.ChatMessages)
    //            .HasForeignKey(x => x.RequestId)
    //            .OnDelete(DeleteBehavior.Cascade);
    //    }

    //}


}
