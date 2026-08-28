using InventoryManagement.Web;
using InventoryManagement.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient<InventoryApiClient>(client =>
    //TODO: should retrieve this endpoint from the configuration file
    client.BaseAddress = new Uri("http://localhost:5035/api/")
);

await builder.Build().RunAsync();
