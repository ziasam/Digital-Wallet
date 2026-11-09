using DigitalWalletService.Application.Dtos.Authentication;
using DigitalWalletService.Application.Interfaces;
using System.Net.Http.Json;

namespace DigitalWalletService.Application.Services
{
    public class TokenRevocationClient : ITokenRevocationClient
    {
        private readonly HttpClient _httpClient;

        public TokenRevocationClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task RevokeAsync(
            RevokeTokenRequestDto tokenRequest,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/tokens/revoke",
                new
                {
                    tokenRequest.Jti,
                    tokenRequest.UserId,
                    tokenRequest.TtlSeconds
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> IsRevokedAsync(
            string jti,
            string? userId = null,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/tokens/check",
                new { Jti = jti, UserId = userId },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<CheckTokenResponseDto>(cancellationToken: cancellationToken);

            return result?.IsRevoked ?? false;
        }
    }
}