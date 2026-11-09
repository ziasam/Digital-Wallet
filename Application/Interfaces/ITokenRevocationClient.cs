using DigitalWalletService.Application.Dtos.Authentication;

namespace DigitalWalletService.Application.Interfaces
{
    public interface ITokenRevocationClient
    {
        Task RevokeAsync(
            RevokeTokenRequestDto tokenRequest,
            CancellationToken cancellationToken = default);

        Task<bool> IsRevokedAsync(
            string jti,
            string? userId = null,
            CancellationToken cancellationToken = default);
    }
}