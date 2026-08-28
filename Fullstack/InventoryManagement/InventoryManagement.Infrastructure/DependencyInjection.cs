using InventoryManagement.Domain.Contracts;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>((options) =>
                {
                    options.UseSqlServer(
                        config.GetConnectionString("DefaultConnection"));
                });

        services.AddScoped<IProductReadRepository, ProductReadRepository>();

        return services;
    }
}
