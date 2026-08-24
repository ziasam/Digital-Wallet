using DigitalWalletDemo.Domain.Enums;

namespace DigitalWalletDemo.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string UserId { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Name { get; set; } = null!;

        public UserStatus UserStatus { get; set; }

        public DateTime RegisteredAt { get; set; }

        public string PasswordHash { get; set; } = null!;
        public List<Wallet> Wallets { get; set; } = new List<Wallet>();
    }
}
