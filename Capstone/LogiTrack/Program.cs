using LogiTrack.Data;
using LogiTrack.Middleware;
using LogiTrack.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
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

app.MapControllers();

app.Run();
