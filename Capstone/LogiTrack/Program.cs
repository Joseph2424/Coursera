using System.Text.Json.Serialization;
using LogiTrack.Data;
using LogiTrack.Endpoints;
using LogiTrack.Middleware;
using LogiTrack.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddDbContext<LogiTrackDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LogiTrack")));
builder.Services.AddScoped<IOrderManagementService, OrderManagementService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LogiTrackDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    await DatabaseSeeder.SeedAsync(dbContext);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapFulfillmentCenterEndpoints();
app.MapInventoryEndpoints();
app.MapOrderEndpoints();

app.Run();
