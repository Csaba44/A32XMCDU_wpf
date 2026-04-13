using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO.Ports;
using System.Management;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using XPlaneConnectorCore;

namespace A32XMCDU
{
    public partial class MainWindow : Window
    {
        private SerialPort serialPort { get; set; }
        private XPlaneConnector xplaneConnector { get; set; }

        public string SelectedAircraft;
        public string SelectedComPort;
        public string SelectedSide = "1";

        private readonly int debounceMs = 50;
        private readonly int holdDelayMs = 1000;
        private readonly int repeatIntervalMs = 100;

        private HashSet<string> repeatableButtons = new HashSet<string> { "R8C11", "R8C4", "R8C5" };
        private Dictionary<string, bool> buttonPressedStates = new Dictionary<string, bool>();
        private Dictionary<string, DateTime> lastPressTimes = new Dictionary<string, DateTime>();

        private float[] mcduBrightness = new float[] { 0.8f, 0.8f, 0.8f };

        public MainWindow()
        {
            InitializeComponent();
            InitializeXPlane();
            SetSerialStatus("NOT LISTENING", Brushes.Goldenrod);
            FillComboBoxes();
        }

        private void InitializeXPlane()
        {
            try
            {
                xplaneConnector = new XPlaneConnector();
                xplaneConnector.Start();
                Debug.WriteLine("X-Plane Connector started successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start X-Plane Connector: {ex.Message}", "X-Plane Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                    if (!string.IsNullOrEmpty(SelectedAircraft) &&
                        AircraftMappingConfig.Mappings.ContainsKey(SelectedAircraft) &&
                        AircraftMappingConfig.Mappings[SelectedAircraft].ContainsKey(buttonId))
                    {
                        if (action == "pressed")
                        {
                            bool allowPress = true;

                            if (lastPressTimes.ContainsKey(buttonId))
                            {
                                if ((DateTime.Now - lastPressTimes[buttonId]).TotalMilliseconds < debounceMs)
                                {
                                    allowPress = false;
                                }
                            }

                            if (allowPress)
                            {
                                lastPressTimes[buttonId] = DateTime.Now;
                                buttonPressedStates[buttonId] = true;

                                if (buttonPressedStates.ContainsKey("R3C1") && buttonPressedStates["R3C1"])
                                {
                                    bool sideChanged = false;
                                    if (buttonId == "R1C8") { SelectedSide = "1"; sideChanged = true; }
                                    else if (buttonId == "R2C8") { SelectedSide = "2"; sideChanged = true; }
                                    else if (buttonId == "R3C8") { SelectedSide = "3"; sideChanged = true; }

                                    if (sideChanged)
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            mcduSideComboBox.SelectedIndex = int.Parse(SelectedSide) - 1;
                                            string timestamp = DateTime.Now.ToString("HH:mm:ss");
                                            latestEventsBox.AppendText($"[{timestamp}] - SHORTCUT: MCDU Side Switched to {SelectedSide}\r\n");
                                            latestEventsBox.ScrollToEnd();
                                        });
                                        return;
                                    }
                                }

                                string rawCommand = AircraftMappingConfig.Mappings[SelectedAircraft][buttonId];
                                commandToSend = rawCommand.Replace("{side}", SelectedSide);

                                int brightIndex = SelectedSide == "1" ? 6 : (SelectedSide == "2" ? 7 : 8);
                                commandToSend = commandToSend.Replace("{bright}", brightIndex.ToString());

                                ExecuteXPlaneAction(commandToSend);

                                if (repeatableButtons.Contains(buttonId))
                                {
                                    _ = HandleButtonHoldAsync(buttonId, commandToSend);
                                }
                            }
                        }
                        else if (action == "released")
                        {
                            if (buttonPressedStates.ContainsKey(buttonId))
                            {
                                buttonPressedStates[buttonId] = false;
                            }
                        }
                    }
                }

                if (data.Contains("pressed"))
                {
                    Dispatcher.Invoke(() =>
                    {
                        string timestamp = DateTime.Now.ToString("HH:mm:ss");
                        latestEventsBox.AppendText($"[{timestamp}] - {data} -> {commandToSend}\r\n");
                        latestEventsBox.ScrollToEnd();
                    });
                }
            }
            catch
            {
                Dispatcher.Invoke(() => MessageBox.Show("error on receive"));
            }
        }

        private void ExecuteXPlaneAction(string mappedCommand)
        {
            try
            {
                if (xplaneConnector == null || mappedCommand == "UNMAPPED") return;

                if (mappedCommand.StartsWith("DREF_UP:"))
                {
                    string dref = mappedCommand.Substring(8);
                    int sideIndex = int.Parse(SelectedSide) - 1;
                    mcduBrightness[sideIndex] = Math.Min(1.0f, mcduBrightness[sideIndex] + 0.1f);
                    xplaneConnector.SetDataRefValueAsync(dref, mcduBrightness[sideIndex]);
                }
                else if (mappedCommand.StartsWith("DREF_DN:"))
                {
                    string dref = mappedCommand.Substring(8);
                    int sideIndex = int.Parse(SelectedSide) - 1;
                    mcduBrightness[sideIndex] = Math.Max(0.0f, mcduBrightness[sideIndex] - 0.1f);
                    xplaneConnector.SetDataRefValueAsync(dref, mcduBrightness[sideIndex]);
                }
                else
                {
                    var xplaneCmd = new XPlaneCommand(mappedCommand, mappedCommand);
                    xplaneConnector.SendCommandAsync(xplaneCmd);
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    latestEventsBox.AppendText($"[XPLANE ERROR] - {ex.Message}\r\n");
                    latestEventsBox.ScrollToEnd();
                });
            }
        }

        private async Task HandleButtonHoldAsync(string buttonId, string command)
        {
            await Task.Delay(holdDelayMs);

            while (buttonPressedStates.ContainsKey(buttonId) && buttonPressedStates[buttonId])
            {
                ExecuteXPlaneAction(command);

                Dispatcher.Invoke(() =>
                {
                    string timestamp = DateTime.Now.ToString("HH:mm:ss");
                    latestEventsBox.AppendText($"[{timestamp}] - (HOLD REPEAT) -> {command}\r\n");
                    latestEventsBox.ScrollToEnd();
                });

                await Task.Delay(repeatIntervalMs);
            }
        }

        public void SetSerialStatus(string text, Brush color)
        {
            Dispatcher.Invoke(() =>
            {
                serialStatus.Content = text;
                serialStatus.Foreground = color;
            });
        }

        public void FillComboBoxes()
        {
            foreach (var ac in AircraftMappingConfig.Mappings.Keys)
            {
                acComboBox.Items.Add(ac);
            }

            if (acComboBox.Items.Count > 0)
            {
                acComboBox.SelectedIndex = 0;
                SelectedAircraft = acComboBox.Items[0].ToString();
            }

            mcduSideComboBox.Items.Add("CM1");
            mcduSideComboBox.Items.Add("CM2");
            mcduSideComboBox.Items.Add("Center (3)");
            mcduSideComboBox.SelectedIndex = 0;
            SelectedSide = "1";

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
            SelectedAircraft = acComboBox.SelectedItem?.ToString();
        }

        private void mcduSideComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mcduSideComboBox.SelectedIndex == 0) SelectedSide = "1";
            else if (mcduSideComboBox.SelectedIndex == 1) SelectedSide = "2";
            else SelectedSide = "3";
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

        protected override void OnClosed(EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }

            if (xplaneConnector != null)
            {
                xplaneConnector.Dispose();
            }

            base.OnClosed(e);
        }
    }
}