using DigitalWallet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DigitalWallet.Application.Interfaces
{
    public interface IDigitalWalletDbContext
    {
        DbSet<Wallet> Wallets { get; }

        DbSet<WalletTransaction> WalletTransactions { get; }

        DbSet<TransactionRequest> TransactionRequests { get; }

        DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
