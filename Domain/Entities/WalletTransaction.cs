using DigitalWalletDemo.Domain.Enums;
using System.Transactions;

namespace DigitalWalletDemo.Domain.Entities
{
    public class WalletTransaction
    {
        public Guid Id { get; set; }

        public string TransactionId { get; set; } = null!;

        public TransactionType Type { get; set; }

        public decimal Amount { get; set; }

        public string? Counterparty { get; set; }

        public TransactionStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? FromWalletId { get; set; }

        public Guid? ToWalletId { get; set; }

        public string Currency { get; set; } = null!;

        public string? Reference { get; set; }

        public string IdempotencyKey { get; set; } = null!;

        public string? FailureReason { get; set; }

        public Wallet? FromWallet { get; set; }

        public Wallet? ToWallet { get; set; }
    }
}
