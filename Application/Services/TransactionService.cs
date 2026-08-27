using DigitalWalletDemo.Application.Dtos.Wallet;
using DigitalWalletDemo.Application.Interfaces;
using DigitalWalletDemo.Domain.Entities;
using DigitalWalletDemo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DigitalWalletDemo.Application.Services;

public class TransactionService
    : ITransactionService
{
    private readonly IDigitalWalletDemoDbContext _db;
    private readonly IConfiguration _configuration;

    public TransactionService(
        IDigitalWalletDemoDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }
    
    public async Task<List<TransactionResponseDto>> GetHistory(
        Guid userId,
        Guid walletId)
    {
        var wallet = await _db.Wallets
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Id == walletId &&
                     x.UserId == userId);

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

    public async Task<TransactionResponseDto> Deposit(
    Guid userId,
    DepositRequestDto request)
    {
        await using var transaction =
            await _db.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted);

        try
        {
            // 1. Idempotency check
            var existing =
                await _db.TransactionRequests
                    .SingleOrDefaultAsync(
                        x => x.IdempotencyKey ==
                             request.IdempotencyKey);

            if (existing != null)
            {
                return await GetExistingResult(existing);
            }

            // 2. Find wallet
            var walletId = await _db.Wallets
                .Where(x => x.UserId == userId)
                .Select(x => x.Id)
                .SingleAsync();

            // 3. Lock wallet
            var wallet =
                await GetWalletForUpdate(walletId);

            if (wallet == null)
                throw new Exception("Wallet not found.");

            ValidateWallet(wallet);

            // 4. Validate amount
            if (request.Amount <= 0)
                throw new Exception(
                    "Amount must be greater than zero.");

            // 5. Currency
            if (wallet.Currency != request.Currency)
                throw new Exception(
                    "Currency mismatch.");

            // 6. Cooldown
            ValidateTransactionGap(wallet);

            // 7. Update balance
            wallet.Balance += request.Amount;

            wallet.LastTransactionAt =
                DateTime.UtcNow;

            // 8. Transaction record
            var transactionRecord =
                new WalletTransaction
                {
                    Id = Guid.NewGuid(),

                    TransactionId =
                        await GenerateTransactionId(),

                    Type = TransactionType.Deposit,

                    Amount = request.Amount,

                    Status =
                        TransactionStatus.Completed,

                    CreatedAt = DateTime.UtcNow,

                    ToWalletId = wallet.Id,

                    Currency = request.Currency,

                    Reference = request.Reference,

                    IdempotencyKey =
                        request.IdempotencyKey
                };

            _db.WalletTransactions.Add(
                transactionRecord);

            // 9. Idempotency record
            var requestRecord =
                new TransactionRequest
                {
                    Id = Guid.NewGuid(),

                    IdempotencyKey =
                        request.IdempotencyKey,

                    ToWalletId = wallet.Id,

                    Amount = request.Amount,

                    Currency = request.Currency,

                    Reference = request.Reference,

                    Status =
                        TransactionStatus.Completed,

                    CreatedAt = DateTime.UtcNow,

                    CompletedAt = DateTime.UtcNow
                };

            _db.TransactionRequests.Add(
                requestRecord);

            // 10. Save everything
            await _db.SaveChangesAsync();

            // 11. Commit
            await transaction.CommitAsync();

            return new TransactionResponseDto
            {
                TransactionId =
                    transactionRecord.TransactionId,

                Status =
                    TransactionStatus.Completed.ToString(),

                Amount = request.Amount,

                Balance = wallet.Balance
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<long> GetNextTransactionSequence()
    {
        return await _db.Database
            .SqlQuery<long>(
                $"""SELECT nextval('"TransactionIdSequence"') AS "Value" """)
            .SingleAsync();
    }

    private async Task<string> GenerateTransactionId()
    {
        var sequence = await GetNextTransactionSequence();
        return $"TXN-{sequence:D4}";
    }

    private static void ValidateWallet(
        Wallet wallet)
    {
        if (wallet.Status.Equals(WalletStatus.Active))
        {
            throw new Exception(
                "Wallet is inactive.");
        }

        if (wallet.Status.Equals(WalletStatus.Frozen))
        {
            throw new Exception(
                "Wallet is frozen.");
        }
    }

    private async Task<TransactionResponseDto> GetExistingResult(
        TransactionRequest existing)
    {
        var transaction =
            await _db.WalletTransactions
                .SingleOrDefaultAsync(
                    x => x.IdempotencyKey ==
                         existing.IdempotencyKey);

        if (transaction == null)
        {
            throw new Exception(
                "Existing transaction record was found, but its transaction details are missing.");
        }

        decimal balance = 0;

        if (existing.ToWalletId.HasValue)
        {
            var wallet =
                await _db.Wallets
                    .SingleOrDefaultAsync(
                        x => x.Id ==
                             existing.ToWalletId.Value);

            if (wallet != null)
            {
                balance = wallet.Balance;
            }
        }

        return new TransactionResponseDto
        {
            TransactionId =
                transaction.TransactionId,

            Status =
                transaction.Status.ToString(),

            Amount =
                transaction.Amount,

            Balance =
                balance
        };
    }

    private async Task<Wallet?> GetWalletForUpdate(
    Guid walletId)
    {
        return await _db.Wallets
            .FromSqlInterpolated($@"
            SELECT *
            FROM ""Wallets""
            WHERE ""Id"" = {walletId}
            FOR UPDATE")
            .SingleOrDefaultAsync();
    }

    private void ValidateTransactionGap(
    Wallet wallet)
    {
        var _minimumTransactionGapSeconds =
            _configuration.GetValue<int>(
                "Wallet:MinimumTransactionGapSeconds");
        if (wallet.LastTransactionAt == null)
            return;

        var gapSeconds =
            (DateTime.UtcNow -
             wallet.LastTransactionAt.Value)
            .TotalSeconds;

        if (gapSeconds <
            _minimumTransactionGapSeconds)
        {
            throw new Exception(
                $"Please wait {_minimumTransactionGapSeconds} seconds between transactions.");
        }
    }

    public async Task<TransactionResponseDto> Withdraw(
    Guid userId,
    WithdrawRequestDto request)
    {
        await using var transaction =
            await _db.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted);

        try
        {
            // 1. Idempotency check
            var existing =
                await _db.TransactionRequests
                    .SingleOrDefaultAsync(
                        x => x.IdempotencyKey ==
                             request.IdempotencyKey);

            if (existing != null)
            {
                return await GetExistingResult(existing);
            }

            // 2. Find wallet
            var walletId =
                await _db.Wallets
                    .Where(x => x.UserId == userId)
                    .Select(x => x.Id)
                    .SingleOrDefaultAsync();

            if (walletId == Guid.Empty)
            {
                throw new Exception(
                    "Wallet not found.");
            }

            // 3. Lock wallet
            var wallet =
                await GetWalletForUpdate(walletId);

            if (wallet == null)
            {
                throw new Exception(
                    "Wallet not found.");
            }

            // 4. Validate wallet
            ValidateWallet(wallet);

            // 5. Validate amount
            if (request.Amount <= 0)
            {
                throw new Exception(
                    "Amount must be greater than zero.");
            }

            // 6. Validate currency
            if (wallet.Currency != request.Currency)
            {
                throw new Exception(
                    "Currency mismatch.");
            }

            // 7. Validate transaction gap
            ValidateTransactionGap(wallet);

            // 8. Check balance
            if (wallet.Balance < request.Amount)
            {
                throw new Exception(
                    "Insufficient wallet balance.");
            }

            // 9. Update balance
            wallet.Balance -= request.Amount;

            wallet.LastTransactionAt =
                DateTime.UtcNow;

            // 10. Create transaction record
            var transactionRecord =
                new WalletTransaction
                {
                    Id = Guid.NewGuid(),

                    TransactionId =
                        await GenerateTransactionId(),

                    Type =
                        TransactionType.Withdrawal,

                    Amount =
                        request.Amount,

                    Status =
                        TransactionStatus.Completed,

                    FromWalletId =
                        wallet.Id,

                    Currency =
                        request.Currency,

                    Reference =
                        request.Reference,

                    IdempotencyKey =
                        request.IdempotencyKey,

                    CreatedAt =
                        DateTime.UtcNow
                };

            _db.WalletTransactions.Add(
                transactionRecord);

            // 11. Create idempotency record
            var requestRecord =
                new TransactionRequest
                {
                    Id = Guid.NewGuid(),

                    IdempotencyKey =
                        request.IdempotencyKey,

                    FromWalletId =
                        wallet.Id,

                    Amount =
                        request.Amount,

                    Currency =
                        request.Currency,

                    Reference =
                        request.Reference,

                    Status =
                        TransactionStatus.Completed,

                    CreatedAt =
                        DateTime.UtcNow,

                    CompletedAt =
                        DateTime.UtcNow
                };

            _db.TransactionRequests.Add(
                requestRecord);

            // 12. Save everything
            await _db.SaveChangesAsync();

            // 13. Commit
            await transaction.CommitAsync();

            // 14. Return response
            return new TransactionResponseDto
            {
                TransactionId =
                    transactionRecord.TransactionId,

                Status =
                    TransactionStatus.Completed
                        .ToString(),

                Amount =
                    request.Amount,

                Balance =
                    wallet.Balance
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TransactionResponseDto> Transfer(
    Guid userId,
    TransferRequestDto request)
    {
        await using var dbTransaction =
            await _db.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted);

        try
        {
            // Idempotency
            var existing =
                await _db.TransactionRequests
                    .SingleOrDefaultAsync(
                        x => x.IdempotencyKey ==
                             request.IdempotencyKey);

            if (existing != null)
                return await GetExistingResult(existing);

            // Find source wallet
            var sourceWallet =
                await _db.Wallets
                    .SingleAsync(
                        x => x.UserId == userId);

            // Find destination
            var destinationWallet =
                await _db.Wallets
                    .SingleAsync(
                        x => x.WalletId ==
                             request.ToWalletId);

            if (sourceWallet.Id ==
                destinationWallet.Id)
            {
                throw new Exception(
                    "Cannot transfer to the same wallet.");
            }

            // Lock in deterministic order
            var walletIds = new[]
            {
            sourceWallet.Id,
            destinationWallet.Id
        }
            .OrderBy(x => x)
            .ToArray();

            var wallets =
                await LockWallets(walletIds);

            var source =
                wallets.Single(x =>
                    x.Id == sourceWallet.Id);

            var destination =
                wallets.Single(x =>
                    x.Id == destinationWallet.Id);

            // Validate
            ValidateWallet(source);
            ValidateWallet(destination);

            if (source.Currency != request.Currency ||
                destination.Currency != request.Currency)
            {
                throw new Exception(
                    "Currency mismatch.");
            }

            ValidateTransactionGap(source);
            ValidateTransactionGap(destination);

            if (source.Balance < request.Amount)
            {
                throw new Exception(
                    "Insufficient balance.");
            }

            if (request.Amount <= 0)
            {
                throw new Exception(
                    "Invalid amount.");
            }

            // Debit
            source.Balance -= request.Amount;

            // Credit
            destination.Balance += request.Amount;

            var now = DateTime.UtcNow;

            source.LastTransactionAt = now;
            destination.LastTransactionAt = now;

            // Transaction OUT
            var transferOut =
                new WalletTransaction
                {
                    Id = Guid.NewGuid(),

                    TransactionId =
                        await GenerateTransactionId(),

                    Type =
                        TransactionType.TransferOut,

                    Amount = request.Amount,

                    Status =
                        TransactionStatus.Completed,

                    FromWalletId = source.Id,

                    ToWalletId = destination.Id,

                    Currency = request.Currency,

                    Reference = request.Reference,

                    IdempotencyKey =
                        request.IdempotencyKey,

                    CreatedAt = now
                };

            // Transaction IN
            var transferIn =
                new WalletTransaction
                {
                    Id = Guid.NewGuid(),

                    TransactionId =
                        await GenerateTransactionId(),

                    Type =
                        TransactionType.TransferIn,

                    Amount = request.Amount,

                    Status =
                        TransactionStatus.Completed,

                    FromWalletId = source.Id,

                    ToWalletId = destination.Id,

                    Currency = request.Currency,

                    Reference = request.Reference,

                    IdempotencyKey =
                        request.IdempotencyKey,

                    CreatedAt = now
                };

            _db.WalletTransactions.Add(
                transferOut);

            _db.WalletTransactions.Add(
                transferIn);

            // Idempotency record
            _db.TransactionRequests.Add(
                new TransactionRequest
                {
                    Id = Guid.NewGuid(),

                    IdempotencyKey =
                        request.IdempotencyKey,

                    FromWalletId = source.Id,

                    ToWalletId = destination.Id,

                    Amount = request.Amount,

                    Currency = request.Currency,

                    Reference = request.Reference,

                    Status =
                        TransactionStatus.Completed,

                    CreatedAt = now,

                    CompletedAt = now
                });

            await _db.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            return new TransactionResponseDto
            {
                TransactionId =
                    transferOut.TransactionId,

                Status =
                    TransactionStatus.Completed.ToString(),

                Amount = request.Amount,

                Balance = source.Balance
            };
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    private async Task<List<Wallet>> LockWallets(
    Guid[] walletIds)
    {
        if (walletIds == null || walletIds.Length == 0)
        {
            return new List<Wallet>();
        }

        var wallets =
            await _db.Wallets
                .FromSqlInterpolated(
                    $"""
                SELECT *
                FROM "Wallets"
                WHERE "Id" = ANY(
                    ARRAY[
                        {string.Join(",", walletIds.Select(x => $"'{x}'::uuid"))}
                    ]
                )
                ORDER BY "Id"
                FOR UPDATE
                """)
                .ToListAsync();

        return wallets;
    }
}