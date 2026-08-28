using DigitalWalletDemo.Application.Dtos.Wallet;
using DigitalWalletDemo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _service;

    public TransactionController(
        ITransactionService service)
    {
        _service = service;
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetTransactionHistory(Guid walletId)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                error = "INVALID_USER_ID",
                message = "Invalid or missing user ID."
            });
        }
        var result =
            await _service.GetHistory(
                userId,
                walletId);
        return Ok(result);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(
    TransferRequestDto request)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                error = "INVALID_USER_ID",
                message = "Invalid or missing user ID."
            });
        }

        var result =
            await _service.Transfer(
                userId,
                request);

        return Ok(result);
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(
        DepositRequestDto request)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                error = "INVALID_USER_ID",
                message = "Invalid or missing user ID."
            });
        }

        var result =
            await _service.Deposit(
                userId!,
                request);

        return Ok(result);
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw(
        WithdrawRequestDto request)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                error = "INVALID_USER_ID",
                message = "Invalid or missing user ID."
            });
        }

        var result =
            await _service.Withdraw(
                userId!,
                request);

        return Ok(result);
    }
}