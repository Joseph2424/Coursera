using Microsoft.AspNetCore.Authentication.Cookies;
using SecurePortal.Web.Handlers;
using SecurePortal.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Set your desired timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Essential for sessions to work without explicit GDPR consent
});

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<JwtAuthorizationHandler>();

builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress =
        new Uri("http://localhost:5100/");
})
.AddHttpMessageHandler<JwtAuthorizationHandler>();

builder
    .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Identity/Account/Login";
        options.AccessDeniedPath = "/Identity/Account/Denied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession(); 

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
