using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public class WeatherService
{
    // API Key for OpenWeatherMap (replace with your own key)
    private const string OpenWeatherMapApiKey = "11232c228a24553297f18317cb4dc098";

    // Get latitude and longitude from OpenWeatherMap for a given city and state
    public async Task<(double lat, double lon)> GetLatLonFromOpenWeatherMap(string city, string state)
    {
        using (HttpClient client = new HttpClient())
        {
            // Construct the URL for the OpenWeatherMap Geocoding API
            string url = $"http://api.openweathermap.org/geo/1.0/direct?q={city},{state},US&limit=1&appid={OpenWeatherMapApiKey}";
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Parse the JSON response
                string json = await response.Content.ReadAsStringAsync();
                var locations = JArray.Parse(json);

                // Extract latitude and longitude from the first result
                if (locations.Count > 0)
                {
                    double lat = locations[0]["lat"].ToObject<double>();
                    double lon = locations[0]["lon"].ToObject<double>();
                    return (lat, lon);
                }
            }
        }

        // Return default values if the request fails or no data is found
        return (0, 0);
    }

    // Get sunrise and sunset times from OpenWeatherMap and convert them to local time
    public async Task<(DateTime sunriseTime, DateTime sunsetTime)> GetSunriseSunsetTimes(double lat, double lon)
    {
        using (HttpClient client = new HttpClient())
        {
            // Construct the URL for the OpenWeatherMap Current Weather API
            string url = $"http://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={OpenWeatherMapApiKey}";
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Parse the JSON response
                string json = await response.Content.ReadAsStringAsync();
                var weatherData = JObject.Parse(json);

                if (weatherData != null)
                {
                    // Extract sunrise and sunset times (in Unix timestamp format)
                    long sunriseUnix = weatherData["sys"]["sunrise"].ToObject<long>();
                    long sunsetUnix = weatherData["sys"]["sunset"].ToObject<long>();

                    // Convert Unix timestamps to DateTime (UTC)
                    DateTime sunriseUtc = DateTimeOffset.FromUnixTimeSeconds(sunriseUnix).UtcDateTime;
                    DateTime sunsetUtc = DateTimeOffset.FromUnixTimeSeconds(sunsetUnix).UtcDateTime;

                    // Convert UTC times to local time
                    DateTime sunriseLocal = sunriseUtc.ToLocalTime();
                    DateTime sunsetLocal = sunsetUtc.ToLocalTime();

                    return (sunriseLocal, sunsetLocal);
                }
            }
        }

        // Return default values if the request fails
        return (DateTime.MinValue, DateTime.MinValue);
    }

    // Fetch weather data from OpenWeatherMap (currently used for weather icons)
    public async Task<JObject> GetWeatherDataFromOpenWeather(double lat, double lon)
    {
        using (HttpClient client = new HttpClient())
        {
            // Construct the URL for the OpenWeatherMap Current Weather API
            string url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={OpenWeatherMapApiKey}";
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Parse the JSON response
                string json = await response.Content.ReadAsStringAsync();
                return JObject.Parse(json);
            }
        }

        // Return null if the request fails
        return null;
    }

    // Fetch weather data from the National Weather Service (NWS) for forecasts
    public async Task<JObject> FetchWeatherDataFromNWS(double lat, double lon)
    {
        using (HttpClient client = new HttpClient())
        {
            // Add a User-Agent header (required by NWS API)
            client.DefaultRequestHeaders.UserAgent.ParseAdd("YourAppName/1.0 (your@email.com)");

            // Step 1: Get the points data (to retrieve forecast URLs)
            string pointsUrl = $"https://api.weather.gov/points/{lat},{lon}";
            HttpResponseMessage pointsResponse = await client.GetAsync(pointsUrl);

            if (!pointsResponse.IsSuccessStatusCode)
            {
                return null; // Return null if the points request fails
            }

            // Parse the points data
            string pointsJson = await pointsResponse.Content.ReadAsStringAsync();
            var pointsData = JObject.Parse(pointsJson);

            // Fetch the hourly forecast data
            string hourlyForecastUrl = pointsData["properties"]["forecastHourly"].ToString();
            HttpResponseMessage hourlyForecastResponse = await client.GetAsync(hourlyForecastUrl);

            // Fetch the regular forecast data
            string forecastUrl = pointsData["properties"]["forecast"].ToString();
            HttpResponseMessage forecastResponse = await client.GetAsync(forecastUrl);

            if (!hourlyForecastResponse.IsSuccessStatusCode || !forecastResponse.IsSuccessStatusCode)
            {
                return null; // Return null if either forecast request fails
            }

            // Parse the hourly forecast
            string hourlyForecastJson = await hourlyForecastResponse.Content.ReadAsStringAsync();
            var hourlyForecastData = JObject.Parse(hourlyForecastJson);

            // Parse the regular forecast
            string forecastJson = await forecastResponse.Content.ReadAsStringAsync();
            var forecastData = JObject.Parse(forecastJson);

            // Extract sunrise and sunset times from the forecast data
            var periods = forecastData["properties"]["periods"] as JArray;
            DateTime sunriseTime = DateTime.MinValue;
            DateTime sunsetTime = DateTime.MinValue;

            if (periods != null)
            {
                foreach (var period in periods)
                {
                    if (period["name"].ToString().Contains("Sunrise"))
                    {
                        sunriseTime = DateTime.Parse(period["startTime"].ToString());
                    }
                    else if (period["name"].ToString().Contains("Sunset"))
                    {
                        sunsetTime = DateTime.Parse(period["startTime"].ToString());
                    }
                }
            }

            // Combine all data into a single JObject
            var combinedData = new JObject
            {
                ["hourlyForecast"] = hourlyForecastData,
                ["forecast"] = forecastData,
                ["sunriseTime"] = sunriseTime,
                ["sunsetTime"] = sunsetTime
            };

            return combinedData;
        }
    }

    // Class to represent weather alerts
    public class WeatherAlert
    {
        public string Headline { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
        public DateTime Effective { get; set; }
        public DateTime Ends { get; set; }
        public string Instruction { get; set; }
    }

    // Fetch weather alerts for a given latitude and longitude
    public async Task<JArray> GetWeatherAlerts(double lat, double lon)
    {
        using (HttpClient client = new HttpClient())
        {
            // Add a User-Agent header (required by NWS API)
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WeatherApp/1.0 (your-email@example.com)");

            // Fetch active alerts for the given location
            string alertsUrl = $"https://api.weather.gov/alerts/active?point={lat},{lon}";
            HttpResponseMessage response = await client.GetAsync(alertsUrl);

            if (response.IsSuccessStatusCode)
            {
                // Parse the JSON response
                string json = await response.Content.ReadAsStringAsync();
                var alertsData = JObject.Parse(json);

                // Return the alerts array
                return alertsData["features"] as JArray;
            }
        }

        // Return null if the request fails
        return null;
    }

    // Process weather alerts and convert them into a list of WeatherAlert objects
    public List<WeatherAlert> ProcessWeatherAlerts(JArray alerts)
    {
        var weatherAlerts = new List<WeatherAlert>();

        if (alerts != null)
        {
            foreach (var alert in alerts)
            {
                var properties = alert["properties"];
                if (properties != null)
                {
                    weatherAlerts.Add(new WeatherAlert
                    {
                        Headline = properties["headline"]?.ToString(),
                        Description = properties["description"]?.ToString(),
                        Severity = properties["severity"]?.ToString(),
                        Effective = DateTime.Parse(properties["effective"]?.ToString()),
                        Ends = DateTime.Parse(properties["ends"]?.ToString()),
                        Instruction = properties["instruction"]?.ToString(),
                    });
                }
            }
        }

        return weatherAlerts;
    }

    // Process daily forecast data from NWS and convert it into a list of DailyForecast objects
    public List<DailyForecast> ProcessDailyForecastData(JObject forecastData)
    {
        if (forecastData == null)
        {
            Console.WriteLine("Forecast data is null.");
            return new List<DailyForecast>();
        }

        var dailyForecasts = new Dictionary<string, DailyForecast>();
        var periods = forecastData["properties"]["periods"] as JArray;

        if (periods == null)
        {
            Console.WriteLine("Periods data is null.");
            return new List<DailyForecast>();
        }

        foreach (var period in periods)
        {
            // Parse the date from the startTime field
            string startTime = period["startTime"]?.ToString();
            if (string.IsNullOrEmpty(startTime))
            {
                Console.WriteLine("startTime is null or empty.");
                continue;
            }

            DateTime forecastTime = DateTime.Parse(startTime);
            string dateKey = forecastTime.ToString("yyyy-MM-dd");

            // Extract temperature, description, and icon
            double temp = period["temperature"]?.ToObject<double>() ?? 0;
            string description = period["shortForecast"]?.ToString() ?? "N/A";
            string iconUrl = period["icon"]?.ToString() ?? "N/A";

            // If we don't have this day in our dictionary yet, create a new entry
            if (!dailyForecasts.ContainsKey(dateKey))
            {
                dailyForecasts[dateKey] = new DailyForecast
                {
                    Date = forecastTime.Date,
                    DayOfWeek = forecastTime.DayOfWeek.ToString(),
                    TempMin = temp,
                    TempMax = temp,
                    Description = description,
                    Icon = iconUrl // Set the NWS icon URL
                };
            }
            else
            {
                // Update min/max temperatures
                var daily = dailyForecasts[dateKey];
                if (temp < daily.TempMin) daily.TempMin = temp;
                if (temp > daily.TempMax) daily.TempMax = temp;

                // Use the mid-day forecast (around noon) for the main description and icon
                if (forecastTime.Hour >= 11 && forecastTime.Hour <= 14)
                {
                    daily.Description = description;
                    daily.Icon = iconUrl;
                }
            }
        }

        // Convert the dictionary to a list, sort by date, and take the first 7 days
        return new List<DailyForecast>(dailyForecasts.Values)
            .OrderBy(d => d.Date)
            .Take(7)
            .ToList();
    }

    // Fetch daily forecast data for a given latitude and longitude
    public async Task<List<DailyForecast>> GetDailyForecast(double lat, double lon)
    {
        var weatherData = await FetchWeatherDataFromNWS(lat, lon);
        if (weatherData == null)
        {
            Console.WriteLine("Failed to fetch weather data.");
            return new List<DailyForecast>();
        }

        var forecastData = weatherData["forecast"] as JObject;
        return ProcessDailyForecastData(forecastData);
    }

    // Class to represent weather information from OpenWeatherMap
    public class WeatherInfo
    {
        public List<Weather> weather { get; set; }
    }

    // Class to represent weather conditions (used for icons)
    public class Weather
    {
        public string icon { get; set; }  // Icons for weather conditions
    }

    // Class to represent daily forecast data
    public class DailyForecast
    {
        public DateTime Date { get; set; }
        public string DayOfWeek { get; set; }
        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }

        public override string ToString()
        {
            return $"{Date.ToString("ddd")}: {Math.Round(TempMin)}°F - {Math.Round(TempMax)}°F";
        }
    }

    // Retrieve the nearest radar station for a given latitude and longitude
    public async Task<string> GetRadarStation(double lat, double lon)
    {
        using (HttpClient client = new HttpClient())
        {
            // Add a User-Agent header (required by NWS API)
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WeatherApp/1.0 (your-email@example.com)");

            // Get the points data (to retrieve radar station ID)
            string pointsUrl = $"https://api.weather.gov/points/{lat},{lon}";
            HttpResponseMessage response = await client.GetAsync(pointsUrl);

            if (response.IsSuccessStatusCode)
            {
                // Parse the JSON response
                string json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json);

                // Extract radar station ID
                return data["properties"]["radarStation"]?.ToString();
            }
        }

        // Return null if the request fails
        return null;
    }
}

    
