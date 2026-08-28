using AutoMapper;
using InventoryManagement.Application.DTOs.Category;
using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Mapping;

public class MappingConfig : Profile
{
    public MappingConfig()
    {
        this.CreateMap<Category, GetCategory>();
        this.CreateMap<Product, GetProduct>();
    }
}
