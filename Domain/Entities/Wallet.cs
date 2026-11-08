using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; set; }

        public string WalletId { get; set; } = null!;

        /// <summary>
        /// User identity from Authentication_Service JWT "sub" claim.
        /// No local Users table — ownership is external.
        /// </summary>
        public Guid UserId { get; set; }

        public string Currency { get; set; } = null!;

        public decimal Balance { get; set; }

        public WalletStatus Status { get; set; }

        public DateTime? LastTransactionAt { get; set; }

        public long Version { get; set; }

        public List<WalletTransaction> OutgoingTransactions { get; set; }
            = new List<WalletTransaction>();

        public List<WalletTransaction> IncomingTransactions { get; set; }
            = new List<WalletTransaction>();
    }
}
