using System.Text.Json.Serialization;

namespace WebUI.Models;

public sealed class WeatherResponse
{
    public LocationData Location { get; set; } = new();
    public CurrentData Current { get; set; } = new();
    public ForecastData Forecast { get; set; } = new();
}

public sealed class LocationData
{
    public string Name { get; set; } = "";
    public string Country { get; set; } = "";

    [JsonPropertyName("localtime")]
    public string Localtime { get; set; } = "";
}

public sealed class CurrentData
{
    [JsonPropertyName("temp_c")]
    public double TempC { get; set; }

    [JsonPropertyName("feelslike_c")]
    public double FeelsLikeC { get; set; }

    [JsonPropertyName("wind_kph")]
    public double WindKph { get; set; }
    public ConditionData Condition { get; set; } = new();
}

public sealed class ForecastData
{
    public List<ForecastDay> Forecastday { get; set; } = new();
}

public sealed class ForecastDay
{
    public DateTime Date { get; set; }
    public DayData Day { get; set; } = new();
}

public sealed class DayData
{
    [JsonPropertyName("maxtemp_c")]
    public double MaxTempC { get; set; }

    [JsonPropertyName("mintemp_c")]
    public double MinTempC { get; set; }
    public ConditionData Condition { get; set; } = new();
}

public sealed class ConditionData
{
    public string Text { get; set; } = "";
    public string Icon { get; set; } = "";
}
