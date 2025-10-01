using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WeatherService; // Import the WeatherService namespace for WeatherAlert

namespace Weather_App
{
    public partial class AlertDetailForm : Form
    {
       
        public AlertDetailForm(WeatherAlert alert)
        {
            InitializeComponent(); 

            // Populate the form with alert details
            HeadlineLabel.Text = alert.Headline; // Set the headline of the alert
            SeverityLabel.Text = $"Severity: {alert.Severity}"; // Display the severity of the alert
            DescriptionrichTextBox.Text = $"Description: {alert.Description}"; // Set the detailed description of the alert
            EffectiveLabel.Text = $"Effective: {alert.Effective}"; // Display the effective time of the alert
            endsLabel.Text = $"Ends: {alert.Ends}"; // Display the end time of the alert
        }
    }
}
