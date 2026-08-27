using DigitalWalletDemo.Application.Dtos.Wallet;

namespace DigitalWalletDemo.Application.Interfaces;

public interface ITransactionService
{
    Task<List<TransactionResponseDto>> GetHistory(
        string userId,
        string walletId);

    Task<TransactionResponseDto> Deposit(
    Guid userId,
    DepositRequestDto request);

    Task<TransactionResponseDto> Withdraw(
        Guid userId,
        WithdrawRequestDto request);

    Task<TransactionResponseDto> Transfer(
        Guid userId,
        TransferRequestDto request);
}