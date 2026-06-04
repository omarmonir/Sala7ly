using Sala7ly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.DataBase.Configurations
{

    //public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    //{
    //    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    //    {

    //        builder.ToTable("WalletTransactions");

    //        builder.HasKey(x => x.Id);

    //        // One-to-Many   ( Wallet , WalletTransactions )

    //        builder.HasOne(x => x.Wallet)
    //               .WithMany(x => x.Transactions)
    //               .HasForeignKey(x => x.WalletId)
    //               .OnDelete(DeleteBehavior.Cascade);

    //    }

    //}



}
