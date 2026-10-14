using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Api.Data;
using Portfolio.Api.Models;
using Portfolio.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Portfolio")));
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 10;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
    .AddEntityFrameworkStores<PortfolioDbContext>()
    .AddSignInManager();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException("Configure Jwt:Key with a secret of at least 32 bytes before starting outside Development.");
    }

    jwtKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:Key must be at least 32 bytes.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Portfolio.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Portfolio.Client";
var tokenLifetimeMinutes = int.TryParse(builder.Configuration["Jwt:LifetimeMinutes"], out var configuredLifetime)
    ? configuredLifetime
    : 60;

builder.Services.Configure<JwtTokenOptions>(options =>
{
    options.Key = jwtKey;
    options.Issuer = jwtIssuer;
    options.Audience = jwtAudience;
    options.LifetimeMinutes = tokenLifetimeMinutes;
});
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("PortfolioUsersRead", policy => policy
        .Cache()
        .Expire(TimeSpan.FromSeconds(30))
        .SetCacheKeyPrefix(context => context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous")
        .Tag("portfolio-users"),
        excludeDefaultPolicy: true);
    options.AddPolicy("ProjectsRead", policy => policy
        .Cache()
        .Expire(TimeSpan.FromSeconds(30))
        .SetVaryByQuery("portfolioUserId")
        .SetCacheKeyPrefix(context => context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous")
        .Tag("projects"),
        excludeDefaultPolicy: true);
});
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins("https://localhost:7261", "http://localhost:5138")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var database = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
    await PortfolioDatabaseInitializer.MigrateAsync(database);
}

app.Run();
