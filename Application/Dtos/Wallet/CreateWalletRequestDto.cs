namespace DigitalWallet.Application.Dtos.Wallet
{
    public class CreateWalletRequestDto
    {
        /// <summary>
        /// ISO 4217 currency code. Defaults to BDT if omitted.
        /// </summary>
        public string Currency { get; set; } = "BDT";
    }
}
