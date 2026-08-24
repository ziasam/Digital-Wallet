namespace DigitalWalletDemo.Application.Dtos.Authentication
{

    public class JwtTokenResult
    {
        public string Token { get; set; } = default!;

        public DateTime ExpiresAt { get; set; }
    }
}
