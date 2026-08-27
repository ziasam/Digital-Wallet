using DigitalWalletDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DigitalWalletDemo.Application.Interfaces
{
    public interface IDigitalWalletDemoDbContext
    {
        DbSet<User> Users { get; }

        DbSet<Wallet> Wallets { get; }

        DbSet<WalletTransaction> WalletTransactions { get; }

        DbSet<TransactionRequest> TransactionRequests { get; }

        public DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
