using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Portfolio")));
builder.Services.AddControllers();
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("PortfolioUsersRead", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("portfolio-users"));
    options.AddPolicy("ProjectsRead", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .SetVaryByQuery("portfolioUserId")
        .Tag("projects"));
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
app.UseOutputCache();
app.UseHttpsRedirection();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var database = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
    await database.Database.EnsureCreatedAsync();
}

app.Run();
