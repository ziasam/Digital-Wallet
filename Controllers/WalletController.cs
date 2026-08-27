using DigitalWalletDemo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(
        IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWallet()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        var wallet =
            await _walletService.GetWallet(userId!);

        return Ok(wallet);
    }
}