using DigitalWalletDemo.Application.Exceptions;

namespace DigitalWalletDemo.Api.Middleware
{
    public class CustomExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionHandler> _logger;

        public CustomExceptionHandler(
            RequestDelegate next,
            ILogger<CustomExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (InsufficientBalanceException ex)
            {
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    "INSUFFICIENT_BALANCE",
                    ex.Message);
            }
            catch (TransactionCooldownException ex)
            {
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status409Conflict,
                    "TRANSACTION_COOLDOWN",
                    ex.Message);
            }
            catch (WalletException ex)
            {
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    "WALLET_ERROR",
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An unhandled exception occurred.");

                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "INTERNAL_ERROR",
                    "An unexpected error occurred.");
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            int statusCode,
            string error,
            string message)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.StatusCode = statusCode;

            context.Response.ContentType =
                "application/json";

            await context.Response.WriteAsJsonAsync(
                new
                {
                    error,
                    message
                });
        }
    }
}

