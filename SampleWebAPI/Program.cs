using SampleWebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpLogging();

builder.Services.AddScoped<ILogService, LogService>();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseHttpLogging();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    DateTime startTime = DateTime.UtcNow;
    await next();
    TimeSpan elapsedTime = DateTime.UtcNow - startTime;
    ILogService logService = context.RequestServices.GetRequiredService<ILogService>();

    logService.Log($"Request to {context.Request.Path} took {elapsedTime.TotalMilliseconds} ms");
});

app.Use((context, next) =>
{
    ILogService logService = context.RequestServices.GetRequiredService<ILogService>();
    logService.Log($"Request: {context.Request.Method} {context.Request.Path}");
    return next();
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/log", (ILogService logService) =>
{
    logService.Log("This is a log message.");
    return Results.Ok("Log message sent.");
})
.WithName("GetLog");

app.Run();
