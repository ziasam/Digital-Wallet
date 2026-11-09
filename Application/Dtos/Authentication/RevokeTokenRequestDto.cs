namespace DigitalWalletService.Application.Dtos.Authentication
{
    public class RevokeTokenRequestDto
    {
        public string Jti { get; set; } = null!;

        public string? UserId { get; set; }

        public int? TtlSeconds { get; set; }
    }
}
