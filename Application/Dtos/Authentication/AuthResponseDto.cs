using DigitalWalletDemo.Domain.Entities;

namespace DigitalWalletDemo.Application.Dtos.Authentication
{
    public class AuthResponseDto
    {
        public string UserId { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Token { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }
    }
}
