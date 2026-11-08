using DigitalWallet.Application.Dtos.Wallet;

namespace DigitalWallet.Application.Interfaces;

public interface IWalletService
{
    Task<WalletResponseDto> CreateWallet(
        Guid userId,
        CreateWalletRequestDto request);

    Task<List<WalletResponseDto>> GetMyWallets(Guid userId);

    Task<WalletResponseDto> GetWallet(
        Guid userId,
        string walletId);
}
