namespace DigitalWalletDemo.Application.Dtos.Wallet
{
    public class TransactionResponseDto
    {
        public string TransactionId { get; set; } = null!;

        public string Type { get; set; } = null!;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = null!;

        public string Status { get; set; } = null!;

        public string? Counterparty { get; set; }

        public string? Reference { get; set; }

        public DateTime CreatedAt { get; set; }

        public decimal Balance { get; set; }
    }
}
