using DigitalWallet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class TransactionRequestConfiguration :
    IEntityTypeConfiguration<TransactionRequest>
    {
        public void Configure(EntityTypeBuilder<TransactionRequest> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.IdempotencyKey)
                .IsUnique();

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2);

            builder.Property(x => x.Currency)
                .HasMaxLength(3)
                .IsRequired();
        }
    }
}
