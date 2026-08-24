using DigitalWalletDemo.Application.Dtos.Authentication;
using DigitalWalletDemo.Domain.Entities;

namespace DigitalWalletDemo.Application.Interfaces
{
    public interface IJwtTokenService
    {
        JwtTokenResult GenerateToken(User user);
    }
}
