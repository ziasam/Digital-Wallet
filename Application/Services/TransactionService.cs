using DigitalWalletDemo.Application.Dtos.Wallet;
using DigitalWalletDemo.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalWalletDemo.Application.Services;

public class TransactionService
    : ITransactionService
{
    private readonly IDigitalWalletDemoDbContext _db;

    public TransactionService(
        IDigitalWalletDemoDbContext db)
    {
        _db = db;
    }

    public async Task<List<TransactionResponseDto>> GetHistory(
        string userId,
        string walletId)
    {
        var wallet = await _db.Wallets
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.WalletId == walletId &&
                     x.User.UserId == userId);

        if (wallet == null)
        {
            throw new Exception(
                "Wallet not found.");
        }

        var transactions = await _db.WalletTransactions
            .Where(x =>
                x.FromWalletId == wallet.Id ||
                x.ToWalletId == wallet.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new TransactionResponseDto
            {
                TransactionId = x.TransactionId,

                Type = x.Type.ToString(),

                Amount = x.Amount,

                Currency = x.Currency,

                Status = x.Status.ToString(),

                Counterparty = x.Counterparty,

                Reference = x.Reference,

                CreatedAt = x.CreatedAt,

                Balance = wallet.Balance
            })
            .ToListAsync();

        return transactions;
    }
}