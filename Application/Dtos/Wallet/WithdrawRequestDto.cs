namespace DigitalWalletDemo.Application.Dtos.Wallet
{
    public class WithdrawRequestDto
    {
        public string WalletId { get; set; } = null!;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = null!;

        public string? Counterparty { get; set; }

        public string? Reference { get; set; }
    }
}
