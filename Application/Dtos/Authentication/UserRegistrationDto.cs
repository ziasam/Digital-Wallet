namespace DigitalWalletDemo.Application.Dtos.Authentication
{
    public class UserRegistrationDto
    {
        public string Email { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}
