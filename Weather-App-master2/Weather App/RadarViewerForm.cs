using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Weather_App
{
    public partial class RadarViewerForm : Form
    {
        // PictureBox control to display the radar image
        private PictureBox radarPictureBox;

       
        public RadarViewerForm(Image radarImage)
        {
            InitializeComponent();

            // Initialize the PictureBox control
            radarPictureBox = new PictureBox
            {
                Dock = DockStyle.Fill, // Fill the entire form with the PictureBox
                SizeMode = PictureBoxSizeMode.Zoom, // Zoom the image to fit the PictureBox
                Image = radarImage // Set the radar image to display
            };

            // Add the PictureBox to the form's controls
            Controls.Add(radarPictureBox);
        }
    }
}
