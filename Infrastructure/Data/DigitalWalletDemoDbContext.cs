using DigitalWalletDemo.Application.Interfaces;
using DigitalWalletDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(DigitalWalletDemoDbContext).Assembly);
        }
    }
}
