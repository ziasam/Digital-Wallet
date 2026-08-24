using DigitalWalletDemo.Application.Dtos.Wallet;

namespace DigitalWalletDemo.Application.Interfaces;

public interface ITransactionService
{
    Task<List<TransactionResponseDto>> GetHistory(
        string userId,
        string walletId);
}