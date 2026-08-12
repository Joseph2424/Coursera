var builder = WebApplication.CreateBuilder(args);

// Configure to listen on HTTP only for simplicity
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5294);
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.Use((context, next) =>
{
    if (context.Request.Query.ContainsKey("secure") == false)
    {
        context.Response.StatusCode = 403;
        return context.Response.WriteAsync("Access denied. Please provide the 'secure' query parameter.");
    }
    return next();
});

app.Use((context, next) =>
{
    if (context.Request.Query.ContainsKey("authenticated") == false)
    {
        context.Response.StatusCode = 401;
        return context.Response.WriteAsync("Unauthorized. Please provide the 'authenticated' query parameter.");
    }
    return next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
