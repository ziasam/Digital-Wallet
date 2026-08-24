using DigitalWalletDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWalletDemo.Infrastructure.Data.Configurations
{
    public class WalletConfiguration :
    IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.WalletId)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(x => x.WalletId)
                .IsUnique();

            builder.Property(x => x.Currency)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.Balance)
                .HasPrecision(18, 2);

            builder.Property(x => x.Version)
                .IsConcurrencyToken();
        }
    }
}
