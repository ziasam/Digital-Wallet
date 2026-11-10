using DigitalWallet.Api.Middleware;
using DigitalWallet.Application.Interfaces;
using DigitalWallet.Application.Services;
using DigitalWallet.Infrastructure.Data;
using DigitalWalletService.Application.Interfaces;
using DigitalWalletService.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<DigitalWalletDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddHttpClient<ITokenRevocationClient, TokenRevocationClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["TokenRevocation:BaseUrl"]
        ?? "http://localhost:5005");
});

// JWT validation against Authentication_Service (RSA public key via JWKS)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSection = builder.Configuration.GetSection("Jwt");

        options.MetadataAddress =
            jwtSection["MetadataAddress"]
            ?? throw new InvalidOperationException(
                "Jwt:MetadataAddress is not configured.");

        options.RequireHttpsMetadata =
            bool.Parse(jwtSection["RequireHttpsMetadata"] ?? "false");

        options.MapInboundClaims = false;

        options.RefreshOnIssuerKeyNotFound = true;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSection["Issuer"],

                ValidateAudience = true,
                ValidAudience = jwtSection["Audience"],

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidAlgorithms = new[]
                {
                    SecurityAlgorithms.RsaSha256
                },

                ClockSkew = TimeSpan.FromMinutes(1)
            };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?
                    .FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?
                    .Value;

                var userId = context.Principal?
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
                    .Value
                    ?? context.Principal?.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(jti))
                {
                    context.Fail("Token is missing jti claim.");
                    return;
                }

                var client = context.HttpContext.RequestServices
                    .GetRequiredService<ITokenRevocationClient>();

                var isRevoked = await client.IsRevokedAsync(jti, userId);

                if (isRevoked)
                {
                    context.Fail("Token has been revoked.");
                }
            }
        };
    });

builder.Services.AddScoped<
    IDigitalWalletDbContext,
    DigitalWalletDbContext>();

builder.Services.AddScoped<
    IWalletService,
    WalletService>();

builder.Services.AddScoped<
    ITransactionService,
    TransactionService>();

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital Wallet API v1"));
}

app.UseMiddleware<CustomExceptionHandler>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
