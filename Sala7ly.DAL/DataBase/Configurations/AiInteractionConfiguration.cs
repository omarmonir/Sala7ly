using Sala7ly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.DataBase.Configurations
{

    //public class AiInteractionConfiguration : IEntityTypeConfiguration<Ai_Interaction>
    //{
       
    //    public void Configure(EntityTypeBuilder<Ai_Interaction> builder)
    //    {

    //        builder.ToTable("AI_INTERACTIONS");

    //        builder.HasKey(x => x.Id);

    //        builder.Property(x => x.ModelUsed)
    //               .IsRequired();

    //        builder.Property(x => x.PromptSnapshot)
    //               .IsRequired();

    //        builder.Property(x => x.ResponseSnapshot)
    //               .IsRequired();

    //        // One to many ( ServiceRequest , AiInteractions )

    //        builder.HasOne(x => x.ServiceRequest)
    //               .WithMany(x => x.AiInteractions)
    //               .HasForeignKey(x => x.RequestId)
    //               .OnDelete(DeleteBehavior.SetNull);

    //    }

    //}


}
