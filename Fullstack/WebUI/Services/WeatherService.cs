using System.Net.Http.Json;
using WebUI.Models;

namespace WebUI.Services;

public interface IWeatherApiService
{
    WeatherResponse? CurrentWeather { get; }
    event Func<WeatherResponse?, Task>? WeatherChanged;

    Task<WeatherResponse?> GetForecastAsync(string location, int days = 3);
}

public sealed class WeatherApiService : IWeatherApiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public WeatherResponse? CurrentWeather { get; private set; }

    public event Func<WeatherResponse?, Task>? WeatherChanged;

    public WeatherApiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<WeatherResponse?> GetForecastAsync(string location, int days = 3)
    {
        var apiKey = _config["WeatherApi:Key"] ?? _config["WeatherApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Weather API key is missing.");

        var query = Uri.EscapeDataString(location.Trim());
        var key = Uri.EscapeDataString(apiKey);

        var url =
            $"http://api.weatherapi.com/v1/forecast.json?key={key}&q={query}&days={days}&aqi=no&alerts=no";

        var result = await _http.GetFromJsonAsync<WeatherResponse>(url);

        CurrentWeather = result;

        if (WeatherChanged is not null)
            await WeatherChanged.Invoke(CurrentWeather);

        return result;
    }
}
