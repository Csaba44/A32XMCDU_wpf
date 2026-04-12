using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO.Ports;
using System.Management;
using System.Diagnostics;

namespace A32XMCDU
{
    public partial class MainWindow : Window
    {
        private SerialPort serialPort { get; set; }
        public string[] AvailableAircraft = { "Toliss A321", "Toliss A320", "Toliss A319", "Toliss A339", "Toliss A346" };
        public string SelectedAircraft;
        public string SelectedComPort;

        public MainWindow()
        {
            InitializeComponent();
            SetSerialStatus("NOT LISTENING", Brushes.Goldenrod);
            FillComboBox();
        }

        public void StartSerialListening()
        {
            if (string.IsNullOrEmpty(SelectedComPort))
            {
                SetSerialStatus("NO PORT SELECTED", Brushes.Red);
                return;
            }

            try
            {
                serialPort = new SerialPort(SelectedComPort, 115200);
                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.Open();

                SetSerialStatus($"LISTENING {SelectedComPort}", Brushes.LimeGreen);
            }
            catch (Exception ex)
            {
                SetSerialStatus("ERROR: " + ex.Message, Brushes.Red);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadLine().Trim();
                string commandToSend = "UNMAPPED";

                string[] parts = data.Split(' ');

                if (parts.Length >= 2)
                {
                    string buttonId = parts[0];
                    string action = parts[1].ToLower();

                    if (action == "pressed" &&
                        AircraftMappingConfig.Mappings.ContainsKey(SelectedAircraft) &&
                        AircraftMappingConfig.Mappings[SelectedAircraft].ContainsKey(buttonId))
                    {
                        commandToSend = AircraftMappingConfig.Mappings[SelectedAircraft][buttonId];
                        SendToXPlane(commandToSend);
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    string timestamp = DateTime.Now.ToString("HH:mm:ss");
                    latestEventsBox.AppendText($"[{timestamp}] - {data} -> {commandToSend}\r\n");
                    latestEventsBox.ScrollToEnd();
                });
            }
            catch
            {
                Dispatcher.Invoke(() => MessageBox.Show("error on receive"));
            }
        }

        private void SendToXPlane(string dataref)
        {
            Debug.WriteLine($"XPLANE COMMAND PLACEHOLDER: {dataref}");
        }

        public void SetSerialStatus(string text, Brush color)
        {
            Dispatcher.Invoke(() =>
            {
                serialStatus.Content = text;
                serialStatus.Foreground = color;
            });
        }

        public void FillComboBox()
        {
            foreach (var ac in AvailableAircraft)
            {
                acComboBox.Items.Add(ac);
            }

            acComboBox.SelectedIndex = 0;
            SelectedAircraft = AvailableAircraft[0];

            int arduinoIndex = -1;
            int index = 0;

            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'");

            foreach (ManagementObject device in searcher.Get())
            {
                string name = device["Name"]?.ToString();

                if (name != null)
                {
                    comPortComboBox.Items.Add(name);

                    if (name.ToLower().Contains("arduino"))
                    {
                        arduinoIndex = index;
                    }

                    index++;
                }
            }

            if (comPortComboBox.Items.Count > 0)
            {
                if (arduinoIndex >= 0)
                    comPortComboBox.SelectedIndex = arduinoIndex;
                else
                    comPortComboBox.SelectedIndex = 0;
            }
        }

        private void acComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedAircraft = AvailableAircraft[acComboBox.SelectedIndex];
        }

        public void RestartSerial()
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.DataReceived -= SerialPort_DataReceived;
                    serialPort.Close();
                }

                StartSerialListening();
            }
            catch (Exception ex)
            {
                SetSerialStatus("ERROR: " + ex.Message, Brushes.Red);
            }
        }

        private void comPortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selected = comPortComboBox.SelectedItem?.ToString();

            if (selected == null) return;

            int start = selected.LastIndexOf("(COM");
            int end = selected.LastIndexOf(")");

            if (start >= 0 && end > start)
            {
                SelectedComPort = selected.Substring(start + 1, end - start - 1);
            }

            RestartSerial();
        }
    }
}