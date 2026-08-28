using DigitalWalletDemo.Application.Dtos.Authentication;
using DigitalWalletDemo.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DigitalWalletDemo.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(
        IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        UserRegistrationDto request)
    {
        var result =
            await _authService.Register(request);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        UserLoginDto request)
    {
        var result =
            await _authService.Login(request);

        return Ok(result);
    }
}