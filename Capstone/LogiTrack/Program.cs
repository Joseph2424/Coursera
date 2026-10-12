using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LogiTrack.Controllers;
using LogiTrack.Data;
using LogiTrack.Middleware;
using LogiTrack.Security;
using LogiTrack.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddMemoryCache();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

var signingKeyText = builder.Configuration["Authentication:SigningKey"];
if (string.IsNullOrWhiteSpace(signingKeyText))
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("Configure Authentication:SigningKey before running outside Development.");

    signingKeyText = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}

if (Encoding.UTF8.GetByteCount(signingKeyText) < 32)
    throw new InvalidOperationException("Authentication:SigningKey must be at least 32 bytes.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKeyText));
var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
var tokenIssuer = builder.Configuration["Authentication:Issuer"] ?? "LogiTrack";
var tokenAudience = builder.Configuration["Authentication:Audience"] ?? "LogiTrack.Api";
var tokenSettings = new JwtTokenSettings(
    signingCredentials,
    tokenIssuer,
    tokenAudience,
    builder.Configuration.GetValue("Authentication:AccessTokenMinutes", 60));

builder.Services.AddDbContext<LogiTrackDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LogiTrack")));
builder.Services.AddDbContext<LogiTrackIdentityDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Identity")));
builder.Services.AddIdentityCore<IdentityUser>(options =>
    {
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<LogiTrackIdentityDbContext>()
    .AddSignInManager();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = tokenSettings.SigningCredentials.Key,
            ValidateIssuer = true,
            ValidIssuer = tokenSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = tokenSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton(tokenSettings);
builder.Services.AddScoped<IdentitySeeder>();
builder.Services.AddScoped<IOrderManagementService, OrderManagementService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LogiTrackDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    await DatabaseSeeder.SeedAsync(dbContext);

    var identityDbContext = scope.ServiceProvider.GetRequiredService<LogiTrackIdentityDbContext>();
    await identityDbContext.Database.EnsureCreatedAsync();
    var identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
    await identitySeeder.SeedAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
