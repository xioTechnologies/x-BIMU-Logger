using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace x_BIMU_Logger
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Array of connect buttons in form.
        /// </summary>
        private Button[] connectButtons;

        /// <summary>
        /// Array of battery progress bars in form.
        /// </summary>
        private ProgressBar[] batteryProgressBars;

        /// <summary>
        /// Array of xBimuInterfaces objects.
        /// </summary>
        private XBimuInterface[] xBimuInterfaces;

        /// <summary>
        /// Flag to indicate if logging.
        /// </summary>
        private bool isLogging;

        /// <summary>
        /// Start time of logging used to 
        /// </summary>
        private DateTime loggingStartTime;

        /// <summary>
        /// Form update timer to periodically update form controls.
        /// </summary>
        private Timer formUpdateTimer = new System.Windows.Forms.Timer();

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Form1 load event to intialise class memebrs.
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            // Set initial form values
            this.Text = Assembly.GetExecutingAssembly().GetName().Name;
            textBoxDirectory.Text = Directory.GetCurrentDirectory();
            textBoxFileName.Text = "LoggedData";
            buttonStartLogging.Text = "Start Logging";
            isLogging = false;
            loggingStartTime = DateTime.MinValue;

            // Create connectButtons array
            List<Button> connectButtonList = new List<Button>();
            foreach (Button button in this.Controls.OfType<Button>())
            {
                if (Regex.Replace(button.Name, "[0-9]", "") == "buttonConnect")
                {
                    connectButtonList.Add(button);
                    button.Text = "Connect With XStick";
                }
            }
            connectButtons = connectButtonList.OrderBy(x => x.Name).ToArray();

            // Create batteryProgressBars array
            List<ProgressBar> batteryProgressBarList = new List<ProgressBar>();
            foreach (ProgressBar progressBar in this.Controls.OfType<ProgressBar>())
            {
                if (Regex.Replace(progressBar.Name, "[0-9]", "") == "progressBarBattery")
                {
                    batteryProgressBarList.Add(progressBar);
                }
            }
            batteryProgressBars = batteryProgressBarList.OrderBy(x => x.Name).ToArray();

            // Create XBimuInterfaces array
            xBimuInterfaces = new XBimuInterface[connectButtons.Length];
            for (int i = 0; i < xBimuInterfaces.Length; i++)
            {
                xBimuInterfaces[i] = new XBimuInterface();
            }

            // Setup form update timer
            formUpdateTimer.Interval = 20;
            formUpdateTimer.Tick += new EventHandler(formUpdateTimer_Tick);
            formUpdateTimer.Start();
        }

        /// <summary>
        /// Form closing event to stop logging and disconct forms  x-BIMUs.
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < xBimuInterfaces.Length; i++)
            {
                xBimuInterfaces[i].StopLogging();
                xBimuInterfaces[i].Disconnect();
            }
        }

        /// <summary>
        /// formUpdateTimer Tick event to update form controls.
        /// </summary>
        void formUpdateTimer_Tick(object sender, EventArgs e)
        {
            // Update button text if connected
            for (int i = 0; i < connectButtons.Length; i++)
            {
                if (xBimuInterfaces[i].XStickChannel != -1)
                {
                    connectButtons[i].Text = "Channel " + xBimuInterfaces[i].XStickChannel.ToString() + Environment.NewLine +
                                             xBimuInterfaces[i].PacketCounter.PacketsReceived.ToString() + " packets" + Environment.NewLine +
                                             xBimuInterfaces[i].PacketCounter.PacketRate.ToString() + " packets/s" + Environment.NewLine;
                    int batteryPercentage = (int)((xBimuInterfaces[i].BatteryVoltage - 3.0f) / (4.1f - 3.0f) * 100.0f);
                    if (batteryPercentage <= 0)
                    {
                        batteryProgressBars[i].Value = 0;
                    }
                    else if (batteryPercentage > 100)
                    {
                        batteryProgressBars[i].Value = 100;
                    }
                    else
                    {
                        batteryProgressBars[i].Value = batteryPercentage;
                    }
                }
            }  

            // Update logging time if logging
            if (isLogging)
            {
                TimeSpan t = DateTime.Now - loggingStartTime;
                buttonStartLogging.Text = "Stop Logging" + Environment.NewLine + string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D3}", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
            }
        }

        /// <summary>
        /// buttonConnect Click event to connect or diconenct from x-BIMUs.
        /// </summary>
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            int i = Array.IndexOf(connectButtons, (Button)sender);
            if (xBimuInterfaces[i].XStickChannel == -1)
            {
                xBimuInterfaces[i].AutoConnect();
                if (xBimuInterfaces[i].XStickChannel != -1)
                {
                    switch (xBimuInterfaces[i].XStickChannel)
                    {
                        case 12: connectButtons[i].BackColor = Color.FromArgb(0x00, 0x00, 0xFF); break;  /* Blue         */
                        case 13: connectButtons[i].BackColor = Color.FromArgb(0x55, 0x00, 0xAA); break;  /* Violet       */
                        case 14: connectButtons[i].BackColor = Color.FromArgb(0x7F, 0x00, 0x7F); break;  /* Magenta      */
                        case 15: connectButtons[i].BackColor = Color.FromArgb(0xAA, 0x00, 0x55); break;  /* Rose         */
                        case 16: connectButtons[i].BackColor = Color.FromArgb(0xFF, 0x00, 0x00); break;  /* Red          */
                        case 17: connectButtons[i].BackColor = Color.FromArgb(0xAA, 0x55, 0x00); break;  /* Orange       */
                        case 18: connectButtons[i].BackColor = Color.FromArgb(0x7F, 0x7F, 0x00); break;  /* Yellow       */
                        case 19: connectButtons[i].BackColor = Color.FromArgb(0x55, 0xAA, 0x00); break;  /* Chartreuse   */
                        case 20: connectButtons[i].BackColor = Color.FromArgb(0x00, 0xFF, 0x00); break;  /* Green        */
                        case 21: connectButtons[i].BackColor = Color.FromArgb(0x00, 0xAA, 0x55); break;  /* Spring green */
                        case 22: connectButtons[i].BackColor = Color.FromArgb(0x00, 0x7F, 0x7F); break;  /* Cyan         */
                        case 23: connectButtons[i].BackColor = Color.FromArgb(0x00, 0x55, 0xAA); break;  /* Azure        */
                        default: connectButtons[i].BackColor = System.Drawing.Color.Transparent; break;
                    }
                }
            }
            else
            {
                xBimuInterfaces[i].Disconnect();
                connectButtons[i].Text = "Connect With XStick";
                batteryProgressBars[i].Value = 0;
                connectButtons[i].BackColor = System.Drawing.Color.Transparent;
            }
        }

        /// <summary>
        /// buttonDirectory Click event to open directory browser dialog.
        /// </summary>
        private void buttonDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (Directory.Exists(textBoxDirectory.Text))
            {
                folderBrowserDialog.SelectedPath = textBoxDirectory.Text;
            }
            else
            {
                folderBrowserDialog.SelectedPath = Directory.GetCurrentDirectory();
            }
            folderBrowserDialog.ShowDialog();
            if (folderBrowserDialog.SelectedPath != "")
            {
                textBoxDirectory.Text = folderBrowserDialog.SelectedPath;
            }
        }

        /// <summary>
        /// buttonStartLogging Click event to start or stop logging.
        /// </summary>
        private void buttonStartLogging_Click(object sender, EventArgs e)
        {
            if (isLogging)
            {
                for (int i = 0; i < xBimuInterfaces.Length; i++)
                {
                    xBimuInterfaces[i].StopLogging();
                }
                foreach (Control control in this.Controls.OfType<Control>())
                {
                    control.Enabled = true;
                }
                isLogging = false;
                buttonStartLogging.Text = "StartLogging";
            }
            else
            {
                if (Directory.Exists(textBoxDirectory.Text))
                {
                    string filePath = Path.Combine(textBoxDirectory.Text, textBoxFileName.Text);
                    for (int i = 0; i < xBimuInterfaces.Length; i++)
                    {
                        if (xBimuInterfaces[i].XStickChannel != -1)
                        {
                            xBimuInterfaces[i].StartLogging(Path.Combine(textBoxDirectory.Text, textBoxFileName.Text));
                        }
                    }
                    foreach (Control control in this.Controls.OfType<Control>())
                    {
                        control.Enabled = false;
                    }
                    buttonStartLogging.Enabled = true;
                    loggingStartTime = DateTime.Now;
                    isLogging = true;
                }
                else
                {
                    MessageBox.Show("Specified Directory does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
