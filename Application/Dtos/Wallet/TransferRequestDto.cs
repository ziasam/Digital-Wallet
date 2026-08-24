namespace DigitalWalletDemo.Application.Dtos.Wallet
{
    public class TransferRequestDto
    {
        public string FromWalletId { get; set; } = null!;

        public string ToWalletId { get; set; } = null!;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = null!;

        public string? Reference { get; set; }
    }
}
