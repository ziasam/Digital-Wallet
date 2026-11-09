namespace DigitalWalletService.Application.Dtos.Authentication
{
    public class CheckTokenResponseDto
    {
        public bool IsRevoked { get; set; }
        public string? Reason { get; set; }
    }
}
