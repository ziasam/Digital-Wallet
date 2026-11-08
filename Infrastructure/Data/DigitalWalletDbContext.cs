using DigitalWallet.Application.Interfaces;
using DigitalWallet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DigitalWallet.Infrastructure.Data
{
    public class DigitalWalletDbContext : DbContext, IDigitalWalletDbContext
    {
        public DigitalWalletDbContext(
            DbContextOptions<DigitalWalletDbContext> options)
            : base(options)
        {
        }

        public DbSet<Wallet> Wallets => Set<Wallet>();

        public DbSet<WalletTransaction> WalletTransactions =>
            Set<WalletTransaction>();

        public DbSet<TransactionRequest> TransactionRequests =>
            Set<TransactionRequest>();

        public new DatabaseFacade Database => base.Database;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(DigitalWalletDbContext).Assembly);

            modelBuilder.HasSequence<long>("WalletIdSequence")
                .StartsAt(1001)
                .IncrementsBy(1);

            modelBuilder.HasSequence<long>("TransactionIdSequence")
                .StartsAt(1001)
                .IncrementsBy(1);

            modelBuilder.Entity<TransactionRequest>()
                .HasIndex(x => x.IdempotencyKey)
                .IsUnique();
        }
    }
}
