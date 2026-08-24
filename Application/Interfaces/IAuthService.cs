using DigitalWalletDemo.Application.Dtos.Authentication;
using Microsoft.AspNetCore.Identity.Data;

namespace DigitalWalletDemo.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(UserRegistrationDto request);

        Task<AuthResponseDto> Login(UserLoginDto request);
    }
}
