using DigitalWallet.Application.Dtos.Wallet;
using DigitalWallet.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalWallet.Api.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _service;

    public TransactionController(ITransactionService service)
    {
        _service = service;
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetTransactionHistory(
        [FromQuery] Guid walletId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(InvalidUserResponse());
        }

        var result = await _service.GetHistory(userId, walletId);
        return Ok(result);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(TransferRequestDto request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(InvalidUserResponse());
        }

        var result = await _service.Transfer(userId, request);
        return Ok(result);
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(DepositRequestDto request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(InvalidUserResponse());
        }

        var result = await _service.Deposit(userId, request);
        return Ok(result);
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw(WithdrawRequestDto request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(InvalidUserResponse());
        }

        var result = await _service.Withdraw(userId, request);
        return Ok(result);
    }

    private bool TryGetUserId(out Guid userId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub");

        return Guid.TryParse(claim, out userId);
    }

    private static object InvalidUserResponse() => new
    {
        error = "INVALID_USER_ID",
        message = "Invalid or missing user ID."
    };
}
