using DigitalWalletDemo.Application.Interfaces;
using DigitalWalletDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DigitalWalletDemo.Infrastructure.Data
{
    public class DigitalWalletDemoDbContext : DbContext, IDigitalWalletDemoDbContext
    {
        public DigitalWalletDemoDbContext(
            DbContextOptions<DigitalWalletDemoDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

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
                typeof(DigitalWalletDemoDbContext).Assembly);


            modelBuilder.HasSequence<long>("UserIdSequence")
                .StartsAt(1001)
                .IncrementsBy(1);

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
