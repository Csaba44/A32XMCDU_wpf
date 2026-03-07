using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Management;

namespace A32XMCDU
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
                string data = serialPort.ReadLine();

                Dispatcher.Invoke(() =>
                {
                    string timestamp = DateTime.Now.ToString("HH:mm:ss");
                    latestEventsBox.AppendText($"[{timestamp}] - {data}");
                    latestEventsBox.ScrollToEnd();
                });
            }
            catch
            {
                MessageBox.Show("error on receive");
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

            var searcher = new ManagementObjectSearcher(
                "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'");

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