using DigitalWallet.Application.Dtos.Wallet;

namespace DigitalWallet.Application.Interfaces;

public interface ITransactionService
{
    Task<List<TransactionResponseDto>> GetHistory(
        Guid userId,
        Guid walletId);

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