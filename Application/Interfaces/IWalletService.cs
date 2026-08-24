using DigitalWalletDemo.Application.Dtos.Wallet;

namespace DigitalWalletDemo.Application.Interfaces;

public interface IWalletService
{
    Task<WalletResponseDto> GetWallet(
        string walletId);

    Task<TransactionResponseDto> Deposit(
        string userId,
        DepositRequestDto request,
        string idempotencyKey);

    Task<TransactionResponseDto> Withdraw(
        string userId,
        WithdrawRequestDto request,
        string idempotencyKey);

    Task<TransactionResponseDto> Transfer(
        string userId,
        TransferRequestDto request,
        string idempotencyKey);
}