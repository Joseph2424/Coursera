using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Portfolio.Client;
using Portfolio.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
	BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7021/")
});
builder.Services.AddScoped<IPortfolioApiService, PortfolioApiService>();

await builder.Build().RunAsync();
