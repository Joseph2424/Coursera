namespace InventoryManagement.Web.Services;

public sealed class InventoryApiResult<T>
{
    private InventoryApiResult(T? value, bool isSuccess, bool isNotFound, bool isServiceUnavailable)
    {
        Value = value;
        IsSuccess = isSuccess;
        IsNotFound = isNotFound;
        IsServiceUnavailable = isServiceUnavailable;
    }

    public T? Value { get; }

    public bool IsSuccess { get; }

    public bool IsNotFound { get; }

    public bool IsServiceUnavailable { get; }

    public static InventoryApiResult<T> Success(T value)
    {
        return new(value, isSuccess: true, isNotFound: false, isServiceUnavailable: false);
    }

    public static InventoryApiResult<T> NotFound()
    {
        return new(default, isSuccess: false, isNotFound: true, isServiceUnavailable: false);
    }

    public static InventoryApiResult<T> ServiceUnavailable()
    {
        return new(default, isSuccess: false, isNotFound: false, isServiceUnavailable: true);
    }
}
