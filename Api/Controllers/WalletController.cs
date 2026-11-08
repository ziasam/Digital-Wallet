using DigitalWallet.Application.Dtos.Wallet;
using DigitalWallet.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalWallet.Api.Controllers;

[ApiController]
[Route("api/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>
    /// Create a wallet for the authenticated user (sub claim from Auth Service).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWallet(
        [FromBody] CreateWalletRequestDto? request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                error = "INVALID_USER_ID",
                message = "Invalid or missing user ID."
            });
        }

        var result = await _walletService.CreateWallet(
            userId,
            request ?? new CreateWalletRequestDto());

        return Ok(result);
    }

    /// <summary>
    /// List all wallets owned by the authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyWallets()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                error = "INVALID_USER_ID",
                message = "Invalid or missing user ID."
            });
        }

        var wallets = await _walletService.GetMyWallets(userId);
        return Ok(wallets);
    }

    /// <summary>
    /// Get a specific wallet by business WalletId (must belong to caller).
    /// </summary>
    [HttpGet("{walletId}")]
    public async Task<IActionResult> GetWallet(string walletId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                error = "INVALID_USER_ID",
                message = "Invalid or missing user ID."
            });
        }

        var wallet = await _walletService.GetWallet(userId, walletId);
        return Ok(wallet);
    }

    private bool TryGetUserId(out Guid userId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub");

        return Guid.TryParse(claim, out userId);
    }
}
