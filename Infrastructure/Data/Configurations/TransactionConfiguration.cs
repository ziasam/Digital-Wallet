using DigitalWalletDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWalletDemo.Infrastructure.Data.Configurations
{
    public class WalletTransactionConfiguration
    : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TransactionId)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(x => x.TransactionId)
                .IsUnique();

            builder.HasIndex(x => x.IdempotencyKey)
                .IsUnique();

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2);

            builder.Property(x => x.Currency)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.Reference)
                .HasMaxLength(200);

            builder.Property(x => x.FailureReason)
                .HasMaxLength(500);

            // FromWallet relationship
            builder.HasOne(x => x.FromWallet)
                .WithMany(x => x.OutgoingTransactions)
                .HasForeignKey(x => x.FromWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            // ToWallet relationship
            builder.HasOne(x => x.ToWallet)
                .WithMany(x => x.IncomingTransactions)
                .HasForeignKey(x => x.ToWalletId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
