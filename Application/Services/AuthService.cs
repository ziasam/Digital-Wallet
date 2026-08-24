using DigitalWalletDemo.Application.Dtos.Authentication;
using DigitalWalletDemo.Application.Interfaces;
using DigitalWalletDemo.Domain.Entities;
using DigitalWalletDemo.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DigitalWalletDemo.Application.Services;

public class AuthService : IAuthService
{
    private readonly IDigitalWalletDemoDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IDigitalWalletDemoDbContext db,
        IJwtTokenService jwtTokenService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Register(
        UserRegistrationDto request)
    {
        var email = request.Email.Trim()
            .ToLowerInvariant();

        var exists = await _db.Users
            .AnyAsync(x => x.Email == email);

        if (exists)
        {
            throw new Exception(
                "Email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),

            UserId = await GenerateNextUserId(),

            Email = email,

            Name = request.Name.Trim(),

            UserStatus = UserStatus.Active,

            RegisteredAt = DateTime.UtcNow,

            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    request.Password)
        };

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),

            WalletId = await GenerateNextWalletId(),

            UserId = user.Id,

            Currency = "BDT",

            Balance = 0m,

            Status = WalletStatus.Active,

            Version = 1
        };

        _db.Users.Add(user);
        _db.Wallets.Add(wallet);

        await _db.SaveChangesAsync();

        var token =
            _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            UserId = user.UserId,

            Email = user.Email,

            Name = user.Name,

            Token = token.Token,

            ExpiresAt = token.ExpiresAt,

            Wallets = user.Wallets
        };
    }

    public async Task<AuthResponseDto> Login(
        UserLoginDto request)
    {
        var email = request.Email.Trim()
            .ToLowerInvariant();

        var user = await _db.Users
            .Include(x => x.Wallets)
            .SingleOrDefaultAsync(
                x => x.Email == email);

        if (user == null)
        {
            throw new Exception(
                "Invalid email or password.");
        }

        if (user.UserStatus != UserStatus.Active)
        {
            throw new Exception(
                "User account is inactive.");
        }

        var passwordValid =
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);

        if (!passwordValid)
        {
            throw new Exception(
                "Invalid email or password.");
        }

        if (user.Wallets == null)
        {
            throw new Exception(
                "User wallet was not found.");
        }

        var token =
            _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            UserId = user.UserId,

            Email = user.Email,

            Name = user.Name,

            Token = token.Token,

            ExpiresAt = token.ExpiresAt,

            Wallets = user.Wallets
        };
    }

    private async Task<string> GenerateNextUserId()
    {
        // PostgreSQL sequence will be implemented here.
        throw new NotImplementedException();
    }

    private async Task<string> GenerateNextWalletId()
    {
        // PostgreSQL sequence will be implemented here.
        throw new NotImplementedException();
    }
}