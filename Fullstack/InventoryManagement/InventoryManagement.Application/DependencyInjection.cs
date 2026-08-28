using InventoryManagement.Application.Mapping;
using InventoryManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingConfig>());

        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
