using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http; // Add this for HttpClient
using System.Threading.Tasks; // Add this for Task
using Newtonsoft.Json.Linq; // Add this for JObject and JArray
using System.IO;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using static WeatherService;
using System.Collections.Generic;
using System.Text;

namespace Weather_App
{
    public partial class Form1 : Form
    {
        // Variables to store the full-size radar image and initial gradient colors
        private Image fullSizeRadarImage;
        private Color initialColor = ColorTranslator.FromHtml("#173253");
        private Color intialEndColor = ColorTranslator.FromHtml("#6889a8");
        private string currentTemperatureUnit = "Fahrenheit"; // Default temperature unit

       
        public Form1()
        {
            InitializeComponent();
            PopulateStatesComboBox(); // Populate the states combo box with US states
            this.BackColor = Color.White;

            // Optimize form rendering for better performance
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;

            // Initially hide controls
            RadarPictureBox.Visible = false;
            RadarBTN.Visible = false;
            groupBox1.Visible = false;
            templabel1.Visible = false;
            ShortForcastLabel.Visible = false;
            detailForecastlbl.Visible = false;
        }

        // Override the OnPaint method to draw a gradient background
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Create a gradient brush for the background
            Rectangle gradientRect = new Rectangle(0, 0, this.Width, this.Height);
            using (LinearGradientBrush brush = new LinearGradientBrush(
                gradientRect,
                initialColor, // Start color
                intialEndColor, // End color
                LinearGradientMode.Vertical)) // Gradient direction
            {
                // Set background and text colors for specific controls
                ColorBox.BackColor = Color.FromArgb(180, ColorTranslator.FromHtml("#1a1d3c"));
                Citylabel.BackColor = Color.FromArgb(180, ColorTranslator.FromHtml("#1a1d3c"));
                Citylabel.ForeColor = Color.White;
                Statelabel.BackColor = Color.FromArgb(180, ColorTranslator.FromHtml("#1a1d3c"));
                Statelabel.ForeColor = Color.White;

                // Fill the form with the gradient
                e.Graphics.FillRectangle(brush, gradientRect);
            }
        }

        // Event handler for form load (currently empty)
        private void Form1_Load(object sender, EventArgs e)
        {
        }

        // Populate the states combo box with US states
        private void PopulateStatesComboBox()
        {
            string[] states = {
                "Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware",
                "Florida", "Georgia", "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky",
                "Louisiana", "Maine", "Maryland", "Massachusetts", "Michigan", "Minnesota", "Mississippi",
                "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire", "New Jersey", "New Mexico",
                "New York", "North Carolina", "North Dakota", "Ohio", "Oklahoma", "Oregon", "Pennsylvania",
                "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont",
                "Virginia", "Washington", "West Virginia", "Wisconsin", "Wyoming"
            };

            StateComboBox.Items.AddRange(states); // Add states to the combo box
            StateComboBox.SelectedIndex = 0; // Set the default selected state
        }

        // Event handler for the search button click
        private async void Searchbutton_Click(object sender, EventArgs e)
        {
            string city = CityTextBox.Text; // Get the city from the text box
            string state = StateComboBox.SelectedItem.ToString(); // Get the selected state

            WeatherService weatherService = new WeatherService();

            // Fetch latitude and longitude for the city and state
            var (lat, lon) = await weatherService.GetLatLonFromOpenWeatherMap(city, state);

            // Fetch weather data for the location
            var weatherData = await weatherService.GetWeatherDataFromOpenWeather(lat, lon);

            if (lat != 0 && lon != 0)
            {
                // Display the weather icon
                var Info = weatherData.ToObject<WeatherInfo>();

                // Fetch sunrise and sunset times
                var (sunriseTime, sunsetTime) = await weatherService.GetSunriseSunsetTimes(lat, lon);

                // Fetch weather data from the National Weather Service (NWS)
                var combinedForecastData = await weatherService.FetchWeatherDataFromNWS(lat, lon);

                if (combinedForecastData != null)
                {
                    // Display hourly forecast
                    var hourlyForecast = combinedForecastData["hourlyForecast"] as JObject;
                    if (hourlyForecast != null)
                    {
                        DisplayWeatherData(hourlyForecast);
                    }

                    // Display daily forecast
                    var forecast = combinedForecastData["forecast"] as JObject;
                    if (forecast != null)
                    {
                        DisplayForecast(forecast);
                        var dailyForecasts = weatherService.ProcessDailyForecastData(forecast);
                        DisplayDailyForecast(dailyForecasts);
                    }

                    // Update the background based on the time of day
                    UpdateBackground(sunriseTime, sunsetTime);

                    // Display sunrise and sunset times
                    DisplaySunriseSunsetTimes(sunriseTime, sunsetTime);
                }
                else
                {
                    MessageBox.Show("Failed to fetch weather data.");
                }

                // Fetch and display the radar image
                string radarStation = await weatherService.GetRadarStation(lat, lon);
                if (!string.IsNullOrEmpty(radarStation))
                {
                    string radarUrl = $"https://radar.weather.gov/ridge/standard/{radarStation}_0.gif";
                    await LoadRadarImage(radarUrl);

                    // Show the radar image and button
                    RadarPictureBox.Visible = true;
                    RadarBTN.Visible = true;
                }
                else
                {
                    MessageBox.Show("Radar station not found.");
                }

                // Display weather alerts
                await DisplayWeatherAlerts(lat, lon);
            }
            else
            {
                MessageBox.Show("City not found.");
            }
        }

        // Display the forecast data in the UI
        private async void DisplayForecast(JObject forecast)
        {
            var periods = forecast["properties"]["periods"] as JArray;
            if (periods == null || periods.Count == 0)
            {
                MessageBox.Show("No forecast data available.");
                return;
            }

            // Extract data for the first period
            var period = periods[0];
            string detailForecast = period["detailedForecast"].ToString();
            string precipitation = "0";
            var probabilityOfPrecipitation = period["probabilityOfPrecipitation"];
            if (probabilityOfPrecipitation != null)
            {
                var precipitationValue = probabilityOfPrecipitation["value"];
                if (precipitationValue != null && precipitationValue.Type != JTokenType.Null)
                {
                    precipitation = precipitationValue.ToString();
                }
            }

            // Update UI labels with forecast data
            pertilabel.Text = "Precipitation: " + precipitation + "%";
            detailForecastlbl.Text = detailForecast;
            detailForecastlbl.Visible = true;
        }

        // Display hourly weather data in the UI
        private async void DisplayWeatherData(JObject hourlyForecast)
        {
            var periods = hourlyForecast["properties"]["periods"] as JArray;

            if (periods != null && periods.Count > 0)
            {
                flowLayoutPanel1.Controls.Clear(); // Clear existing controls

                // Display data for the next 24 periods
                int maxPeriods = Math.Min(24, periods.Count);
                for (int i = 0; i < maxPeriods; i++)
                {
                    // Create labels and PictureBox for each period
                    Label timeLabel = new Label();
                    Label tempLabel = new Label();
                    PictureBox pictureBox = new PictureBox();

                    // Display data for the current period
                    await DisplayPeriodData(periods[i], timeLabel, tempLabel, pictureBox, i);

                    // Add the period's data to the flowLayoutPanel
                    AddPeriodToFlowLayoutPanel(periods[i], pictureBox.Image, timeLabel.Text, tempLabel.Text, i);
                }
            }
            else
            {
                MessageBox.Show("No weather data available.");
            }
        }

        // Helper method to add a period's data to the flowLayoutPanel
        private void AddPeriodToFlowLayoutPanel(JToken period, Image icon, string time, string temperature, int periodIndex)
        {
            // Create a panel to hold the controls for this period
            Panel periodPanel = new Panel();
            periodPanel.Size = new Size(100, 150);

            // Create and configure the time label
            Label timeLabel = new Label();
            timeLabel.TextAlign = ContentAlignment.MiddleCenter;
            timeLabel.AutoSize = false;
            timeLabel.Size = new Size(80, 20);
            timeLabel.Location = new Point((periodPanel.Width - timeLabel.Width) / 2, 10);
            timeLabel.Text = time;
            timeLabel.BackColor = Color.Transparent;
            timeLabel.ForeColor = Color.White;

            // Create and configure the PictureBox for the weather icon
            PictureBox pictureBox = new PictureBox();
            pictureBox.Size = new Size(60, 60);
            pictureBox.Location = new Point((periodPanel.Width - pictureBox.Width) / 2, timeLabel.Bottom + 5);
            pictureBox.Image = icon;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Create and configure the temperature label
            Label tempLabel = new Label();
            tempLabel.TextAlign = ContentAlignment.MiddleCenter;
            tempLabel.AutoSize = false;
            tempLabel.Size = new Size(80, 20);
            tempLabel.Location = new Point((periodPanel.Width - tempLabel.Width) / 2, pictureBox.Bottom + 5);
            tempLabel.Text = temperature;
            tempLabel.BackColor = Color.Transparent;
            tempLabel.ForeColor = Color.White;

            // Add controls to the panel
            periodPanel.Controls.Add(timeLabel);
            periodPanel.Controls.Add(pictureBox);
            periodPanel.Controls.Add(tempLabel);

            // Add the panel to the flowLayoutPanel
            flowLayoutPanel1.Controls.Add(periodPanel);
        }

        // Display data for a specific period
        private async Task DisplayPeriodData(JToken period, Label timeLabel, Label tempLabel, PictureBox pictureBox, int periodIndex)
        {
            string iconUrl = period["icon"].ToString();
            await LoadImageAsync(iconUrl, pictureBox); // Load the weather icon

            string temperature = period["temperature"].ToString();
            string shortForecast = period["shortForecast"].ToString();
            string windSpeed = period["windSpeed"].ToString();
            string humidity = period["relativeHumidity"]["value"].ToString();

            // Parse the start time with the time zone
            DateTimeOffset periodStartTime = DateTimeOffset.Parse(period["startTime"].ToString());

            templabel1.Visible = true;
            ShortForcastLabel.Visible = true;

            // Special handling for the first period (current weather)
            if (periodIndex == 0)
            {
                timeLabel.Text = "Now";
                tempLabel.Text = temperature + "\u00B0";
                ShortForcastLabel.Text = shortForecast;
                templabel1.Text = temperature + "\u00B0";
                windlabel1.Text = "Wind Speed: " + windSpeed;
                humlabel.Text = "Humidity: " + humidity + "%";
            }
            else
            {
                timeLabel.Text = periodStartTime.ToString("h:mm tt");
                tempLabel.Text = temperature + "\u00B0";
            }
        }

        // Load an image from a URL asynchronously
        private async Task LoadImageAsync(string url, PictureBox pictureBox)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Add a User-Agent header to the request
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("YourAppName/1.0 (your-email@example.com)");

                    // Fetch the image data
                    byte[] imageData = await client.GetByteArrayAsync(url);

                    // Convert the byte array to an Image object
                    using (var ms = new System.IO.MemoryStream(imageData))
                    {
                        var image = Image.FromStream(ms);
                        pictureBox.Image = image; // Assign the image to the PictureBox
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load image: " + ex.Message);
            }
        }

        // Display weather alerts for the location
        private async Task DisplayWeatherAlerts(double lat, double lon)
        {
            WeatherService weatherService = new WeatherService();

            // Fetch weather alerts
            var alerts = await weatherService.GetWeatherAlerts(lat, lon);

            if (alerts != null && alerts.Count > 0)
            {
                // Process and display alerts
                var weatherAlerts = weatherService.ProcessWeatherAlerts(alerts);
                StringBuilder alertMessage = new StringBuilder();
                foreach (var alert in weatherAlerts)
                {
                    alertMessage.AppendLine($"** {alert.Headline} **");
                    alertMessage.AppendLine(alert.Description.Substring(0, Math.Min(100, alert.Description.Length)) + "...");
                    alertMessage.AppendLine();
                }

                // Show a MessageBox with all alerts
                DialogResult result = MessageBox.Show(
                    alertMessage.ToString(),
                    "Weather Alerts",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning
                );

                // Show detailed alert form if the user clicks "OK"
                if (result == DialogResult.OK)
                {
                    AlertDetailForm detailForm = new AlertDetailForm(weatherAlerts[0]);
                    detailForm.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("No active weather alerts for this location.", "Weather Alerts", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Update the background gradient based on the time of day
        private void UpdateBackground(DateTime sunriseTime, DateTime sunsetTime)
        {
            try
            {
                DateTime now = DateTime.Now;

                if (now >= sunriseTime && now < sunsetTime)
                {
  

                    // Daytime gradient colors
                    initialColor = ColorTranslator.FromHtml("#4184f0");
                    intialEndColor = ColorTranslator.FromHtml("#9ecbff");

                    // Update UI colors for daytime
                    groupBox1.ForeColor = Color.White;
                    templabel1.ForeColor = Color.White;
                    ShortForcastLabel.ForeColor = Color.White;
                    detailForecastlbl.ForeColor = Color.White;
                    flowLayoutPanel1.BackColor = Color.FromArgb(30, ColorTranslator.FromHtml("#41718c"));
                }
                else
                {
                    // Nighttime gradient colors
                    initialColor = ColorTranslator.FromHtml("#14213a");
                    intialEndColor = ColorTranslator.FromHtml("#4c5c6b");

                    // Update UI colors for nighttime
                    groupBox1.ForeColor = Color.White;
                    templabel1.ForeColor = Color.White;
                    ShortForcastLabel.ForeColor = Color.White;
                    detailForecastlbl.ForeColor = Color.White;
                    flowLayoutPanel1.BackColor = Color.FromArgb(60, Color.Black);
                }

                // Force the form to redraw the background
                this.Invalidate();
                groupBox1.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating background: " + ex.Message);
            }
        }

        // Display sunrise and sunset times in the UI
        private void DisplaySunriseSunsetTimes(DateTime sunriseTime, DateTime sunsetTime)
        {
            SunriseLabel.Text = "Sunrise: " + sunriseTime.ToString("h:mm tt");
            SunsetLabel.Text = "Sunset: " + sunsetTime.ToString("h:mm tt");
        }

        // Convert Fahrenheit to Celsius
        private double FahrenheitToCelsius(double fahrenheit)
        {
            return (fahrenheit - 32) * 5 / 9;
        }

        // Convert Celsius to Fahrenheit
        private double CelsiusToFahrenheit(double celsius)
        {
            return (celsius * 9 / 5) + 32;
        }

        // Event handler for Celsius button click
        private void Celsiusbtn_Click(object sender, EventArgs e)
        {
            if (currentTemperatureUnit != "Celsius")
            {
                UpdateTemperatures(toCelsius: true);
                currentTemperatureUnit = "Celsius";
            }
        }

        // Event handler for Fahrenheit button click
        private void Fahrenheitbtn_Click(object sender, EventArgs e)
        {
            if (currentTemperatureUnit != "Fahrenheit")
            {
                UpdateTemperatures(toCelsius: false);
                currentTemperatureUnit = "Fahrenheit";
            }
        }

        // Update temperature displays based on the selected unit
        private void UpdateTemperatures(bool toCelsius)
        {
            // Update hourly forecast temperatures
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is Panel periodPanel)
                {
                    foreach (Control panelControl in periodPanel.Controls)
                    {
                        if (panelControl is Label tempLabel)
                        {
                            string text = tempLabel.Text.Replace("°", "").Trim();
                            if (double.TryParse(text, out double tempValue))
                            {
                                double convertedTemp = toCelsius ? FahrenheitToCelsius(tempValue) : CelsiusToFahrenheit(tempValue);
                                tempLabel.Text = $"{Math.Round(convertedTemp, 1)}°";
                            }
                        }
                    }
                }
            }

            // Update daily forecast temperatures
            foreach (Control control in flowLayoutPanel2.Controls)
            {
                if (control is Panel dayPanel)
                {
                    foreach (Control panelControl in dayPanel.Controls)
                    {
                        if (panelControl is Label tempLabel && tempLabel.Tag is DailyForecast forecast)
                        {
                            double minTemp = toCelsius ? FahrenheitToCelsius(forecast.TempMin) : forecast.TempMin;
                            double maxTemp = toCelsius ? FahrenheitToCelsius(forecast.TempMax) : forecast.TempMax;
                            tempLabel.Text = $"H:{Math.Round(maxTemp)}° L:{Math.Round(minTemp)}°";
                        }
                    }
                }
            }

            // Update the main temperature display
            string mainTempText = templabel1.Text.Replace("°", "").Trim();
            if (double.TryParse(mainTempText, out double mainTempValue))
            {
                double convertedMainTemp = toCelsius ? FahrenheitToCelsius(mainTempValue) : CelsiusToFahrenheit(mainTempValue);
                templabel1.Text = $"{Math.Round(convertedMainTemp, 1)}°";
            }
        }

        // Display the daily forecast in the UI
        private async void DisplayDailyForecast(List<DailyForecast> dailyForecasts)
        {
            foreach (var forecast in dailyForecasts)
            {
                // Create a panel for each day's forecast
                Panel dayPanel = new Panel
                {
                    Size = new Size(120, 150),
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.FromArgb(100, 255, 255, 255),
                    Padding = new Padding(5),
                    Margin = new Padding(5)
                };

                // Add gradient background to the panel
                dayPanel.Paint += (sender, e) =>
                {
                    using (var brush = new LinearGradientBrush(dayPanel.ClientRectangle, Color.FromArgb(100, 173, 216, 230), Color.FromArgb(100, 135, 206, 250), LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(brush, dayPanel.ClientRectangle);
                    }
                };

                // Add day of the week label
                Label dayLabel = new Label
                {
                    Text = forecast.DayOfWeek,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = false,
                    Size = new Size(110, 20),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };

                // Add weather icon
                PictureBox iconBox = new PictureBox
                {
                    Size = new Size(50, 50),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.Transparent
                };

                // Load the weather icon
                await LoadImageAsync(forecast.Icon, iconBox);

                // Add temperature range label
                Label tempLabel = new Label
                {
                    Text = $"H:{Math.Round(forecast.TempMax)}° L: {Math.Round(forecast.TempMin)}°",
                    Font = new Font("Arial", 9),
                    ForeColor = Color.White,
                    AutoSize = false,
                    Size = new Size(110, 20),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Tag = forecast // Store forecast data for temperature conversion
                };

                // Position controls within the panel
                dayLabel.Location = new Point((dayPanel.Width - dayLabel.Width) / 2, 10);
                iconBox.Location = new Point((dayPanel.Width - iconBox.Width) / 2, 40);
                tempLabel.Location = new Point((dayPanel.Width - tempLabel.Width) / 2, 100);

                // Add controls to the panel
                dayPanel.Controls.Add(dayLabel);
                dayPanel.Controls.Add(iconBox);
                dayPanel.Controls.Add(tempLabel);

                // Add the panel to the flowLayoutPanel
                flowLayoutPanel2.Controls.Add(dayPanel);
            }

            // Adjust the width of the flowLayoutPanel to fit all panels
            int totalPanelWidth = (120 + 10) * dailyForecasts.Count;
            int containerWidth = flowLayoutPanel2.Parent.ClientSize.Width;
            flowLayoutPanel2.Width = totalPanelWidth;
            flowLayoutPanel2.Left = (containerWidth - totalPanelWidth) / 2;
        }

        // Load the radar image from a URL
        private async Task LoadRadarImage(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Download the image as a byte array
                    byte[] imageData = await client.GetByteArrayAsync(url);

                    // Convert the byte array into a MemoryStream
                    using (var ms = new System.IO.MemoryStream(imageData))
                    {
                        // Store the full-size image
                        fullSizeRadarImage = Image.FromStream(ms);

                        // Scale the image for display in the PictureBox
                        RadarPictureBox.Image = new Bitmap(fullSizeRadarImage, new Size(200, 150));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load radar image: " + ex.Message);
            }
        }

        // Event handler for the radar button click
        private void RadarBTN_Click(object sender, EventArgs e)
        {
            if (fullSizeRadarImage != null)
            {
                // Open a new form to display the full-size radar image
                RadarViewerForm viewer = new RadarViewerForm(fullSizeRadarImage);
                viewer.Show();
            }
            else
            {
                MessageBox.Show("Radar image not loaded yet.");
            }
        }
    }
}