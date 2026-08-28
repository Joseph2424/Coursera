namespace InventoryManagement.Web.Services;

public static class AppRoutes
{
    public static string Category(string? id)
    {
        return string.IsNullOrWhiteSpace(id)
            ? "/category"
            : $"/category/{Uri.EscapeDataString(id.Trim())}";
    }

    public static string Product(string? id)
    {
        return string.IsNullOrWhiteSpace(id)
            ? "/product"
            : $"/product/{Uri.EscapeDataString(id.Trim())}";
    }
}
