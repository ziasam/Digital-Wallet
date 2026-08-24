namespace DigitalWalletDemo.Application.Dtos.Wallet
{
    public class WalletResponseDto
    {
        public string WalletId { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal Balance { get; set; }

        public string Status { get; set; } = null!;

        public DateTime? LastTransactionAt { get; set; }
    }
}
