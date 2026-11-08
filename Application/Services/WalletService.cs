using DigitalWallet.Application.Dtos.Wallet;
using DigitalWallet.Application.Exceptions;
using DigitalWallet.Application.Interfaces;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DigitalWallet.Application.Services;

public class WalletService : IWalletService
{
    private readonly IDigitalWalletDbContext _db;

    public WalletService(IDigitalWalletDbContext db)
    {
        _db = db;
    }

    public async Task<WalletResponseDto> CreateWallet(
        Guid userId,
        CreateWalletRequestDto request)
    {
        var currency = string.IsNullOrWhiteSpace(request.Currency)
            ? "BDT"
            : request.Currency.Trim().ToUpperInvariant();

        if (currency.Length != 3)
        {
            throw new WalletException(
                "Currency must be a 3-letter ISO code.");
        }

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            WalletId = await GenerateNextWalletId(),
            UserId = userId,
            Currency = currency,
            Balance = 0m,
            Status = WalletStatus.Active,
            Version = 1
        };

        _db.Wallets.Add(wallet);
        await _db.SaveChangesAsync();

        return Map(wallet);
    }

    public async Task<List<WalletResponseDto>> GetMyWallets(Guid userId)
    {
        var wallets = await _db.Wallets
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.WalletId)
            .ToListAsync();

        return wallets.Select(Map).ToList();
    }

    public async Task<WalletResponseDto> GetWallet(
        Guid userId,
        string walletId)
    {
        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(x =>
                x.WalletId == walletId &&
                x.UserId == userId);

        if (wallet == null)
        {
            throw new WalletException("Wallet not found.");
        }

        return Map(wallet);
    }

    private static WalletResponseDto Map(Wallet wallet) =>
        new()
        {
            WalletId = wallet.WalletId,
            Currency = wallet.Currency,
            Balance = wallet.Balance,
            Status = wallet.Status.ToString(),
            LastTransactionAt = wallet.LastTransactionAt
        };

    private async Task<long> GetNextWalletSequence()
    {
        return await _db.Database
            .SqlQuery<long>(
                $"""SELECT nextval('"WalletIdSequence"') AS "Value" """)
            .SingleAsync();
    }

    private async Task<string> GenerateNextWalletId()
    {
        var sequence = await GetNextWalletSequence();
        return $"WLT-{sequence:D4}";
    }
}
