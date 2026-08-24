using DigitalWalletDemo.Domain.Enums;

namespace DigitalWalletDemo.Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; set; }

        public string WalletId { get; set; } = null!;

        public Guid UserId { get; set; }

        public string Currency { get; set; } = null!;

        public decimal Balance { get; set; }

        public UserStatus Status { get; set; }

        public DateTime? LastTransactionAt { get; set; }

        public long Version { get; set; }

        public User User { get; set; } = null!;
        public List<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    }
}
