using DigitalWallet.Api.Middleware;
using DigitalWallet.Application.Interfaces;
using DigitalWallet.Application.Services;
using DigitalWallet.Infrastructure.Data;
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

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ClockSkew = TimeSpan.FromMinutes(1)
        };

        // Map "sub" → ClaimTypes.NameIdentifier
        options.MapInboundClaims = true;
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
