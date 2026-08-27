using DigitalWalletDemo.Application.Dtos.Wallet;

namespace DigitalWalletDemo.Application.Interfaces;

public interface IWalletService
{
    Task<WalletResponseDto> GetWallet(
        string walletId);
}