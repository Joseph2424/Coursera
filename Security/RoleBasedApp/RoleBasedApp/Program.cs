using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoleBasedApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("AuthDb")
);

builder
    .Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder
    .Services.AddAuthorizationBuilder()
    .AddPolicy("RequireITDepartment", policy => policy.RequireClaim("Department", "IT"));

var app = builder.Build();

app.UseHttpsRedirection();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.Run();
