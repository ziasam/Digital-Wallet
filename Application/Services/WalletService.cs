using DigitalWalletDemo.Application.Dtos.Wallet;
using DigitalWalletDemo.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalWalletDemo.Application.Services;

public class WalletService : IWalletService
{
    private readonly IDigitalWalletDemoDbContext _db;

    public WalletService(
        IDigitalWalletDemoDbContext db)
    {
        _db = db;
    }

    public async Task<WalletResponseDto> GetWallet(
        string walletId)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(x => x.WalletId == walletId);

        if (wallet == null)
        {
            throw new Exception(
                "Wallet not found.");
        }

        return new WalletResponseDto
        {
            WalletId = wallet.WalletId,
            Currency = wallet.Currency,
            Balance = wallet.Balance,
            Status = wallet.Status.ToString(),
            LastTransactionAt =
                wallet.LastTransactionAt
        };
    }

    public Task<TransactionResponseDto> Deposit(
        string userId,
        DepositRequestDto request,
        string idempotencyKey)
    {
        throw new NotImplementedException();
    }

    public Task<TransactionResponseDto> Withdraw(
        string userId,
        WithdrawRequestDto request,
        string idempotencyKey)
    {
        throw new NotImplementedException();
    }

    public Task<TransactionResponseDto> Transfer(
        string userId,
        TransferRequestDto request,
        string idempotencyKey)
    {
        throw new NotImplementedException();
    }
}