using DigitalWalletDemo.Domain.Enums;

namespace DigitalWalletDemo.Domain.Entities
{
    public class TransactionRequest
    {
        public Guid Id { get; set; }

        public string IdempotencyKey { get; set; } = null!;

        public Guid? FromWalletId { get; set; }

        public Guid? ToWalletId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = null!;

        public string? Reference { get; set; }

        public TransactionStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
