using System.Net;
using System.Text;
using System.Text.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Runtime;

namespace EmbeddedDevice_FAN
{

    public partial class MainWindow : Window
    {
        private string _logFilePath = "";
        private decimal _pendingSpeed;
        private bool isRunning = false;
        private Storyboard spinStoryboard;
        private DispatcherTimer? _speedTimer;
        private ObservableCollection<string>? _eventLog;
        private DeviceSettings _settings;
        private string _settingsPath = "";

        public MainWindow()
        {
            InitializeComponent();
            InitializeFeatures();
            StartRestServer();
        }

        private void LoadSettings()
        {
            var appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EmbeddedDevice");
            Directory.CreateDirectory(appDir);
            _settingsPath = Path.Combine(appDir, "settings.json");

            if (File.Exists(_settingsPath))
            {
                string json = File.ReadAllText(_settingsPath);
                _settings = JsonSerializer.Deserialize<DeviceSettings>(json) ?? new DeviceSettings();
            }
            else
            {
                _settings = new DeviceSettings();
                SaveSettings();
            }

            Slider_Speed.Value = (double)_settings.DefaultSpeed;
        }

        private void SaveSettings()
        {
            _settings.DefaultSpeed = _pendingSpeed;
            string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }

        private void InitializeFeatures()
        {
            // Get storyboard from resources
            spinStoryboard = (Storyboard)this.Resources["SpinFan"];
            _speedTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(500),
            };
            _speedTimer.Tick += (_, _) =>
            {
                _speedTimer.Stop();
                LogMessage($"Speed set to {_pendingSpeed:0.00}");
            };

            _eventLog = [];
            LB_EventLog.ItemsSource = _eventLog; //ska ta bort den

            var appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EmbeddedDevice");
            Directory.CreateDirectory(appDir);
            _logFilePath = Path.Combine(appDir, "eventlog.log");
            //C: \Users\klope\AppData\Local\EmbeddedDevice här finns log filen
        }

        private void LogMessage(string message)
        {
            string line = $@"{DateTime.Now:yyyy-MM-dd HH:mm:ss} : {message}";
            _eventLog?.Add(line);

            try
            {
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
            }
            catch
            {

            }

            if (LB_EventLog.Items.Count > 0)
            {
                LB_EventLog.ScrollIntoView(LB_EventLog.Items[LB_EventLog.Items.Count - 1]);
            }
        }

        private void Btn_OnOff_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
            {
                LogMessage("Started");
                spinStoryboard.Begin(this, true); // Start animation
                Btn_OnOff.Content = "STOP";
                isRunning = true;
            }
            else
            {
                LogMessage("Stopped");
                spinStoryboard.Stop(this); // Stop animation
                Btn_OnOff.Content = "START";
                isRunning = false;
            }
        }

        private void Slider_Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (spinStoryboard != null)
            {
                double speed = e.NewValue;

                // Prevent division by zero
                if (speed <= 0.1) speed = 0.1;

                // Faster slider value = shorter duration
                double secondsPerRotation = 1.0 / speed;

                foreach (var timeline in spinStoryboard.Children)
                {
                    if (timeline is DoubleAnimation anim)
                    {
                        anim.Duration = new Duration(System.TimeSpan.FromSeconds(secondsPerRotation));
                    }
                }

                if (isRunning)
                {
                    // Restart storyboard with new speed
                    spinStoryboard.Begin(this, true);
                }

                _pendingSpeed = (decimal)e.NewValue; // Explicit conversion to fix CS0266
                _speedTimer.Stop();
                _speedTimer.Start();
            }
        }
       private async void StartRestServer()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/");
            listener.Start();
            LogMessage("REST server running at http://localhost:5000/");

            _ = Task.Run(async () =>
            {
                while (listener.IsListening)
                {
                    var context = await listener.GetContextAsync();
                    string responseMessage = "";

                    string path = context.Request.Url.AbsolutePath.ToLower();

                    switch (path)
                    {
                        case "/start":
                            Dispatcher.Invoke(() =>
                            {
                                if (!isRunning) Btn_OnOff_Click(null, null);
                            });
                            responseMessage = "Fan started.";
                            break;

                        case "/stop":
                            Dispatcher.Invoke(() =>
                            {
                                if (isRunning) Btn_OnOff_Click(null, null);
                            });
                            responseMessage = "Fan stopped.";
                            break;

                        case "/speed":
                            using (var reader = new StreamReader(context.Request.InputStream))
                            {
                                string body = await reader.ReadToEndAsync();
                                if (decimal.TryParse(body, out var newSpeed))
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        Slider_Speed.Value = (double)newSpeed;
                                    });
                                    responseMessage = $"Speed set to {newSpeed}";
                                }
                                else
                                {
                                    responseMessage = "Invalid speed value.";
                                }
                            }
                            break;

                        case "/status":
                            var status = new
                            {
                                running = isRunning,
                                speed = _pendingSpeed
                            };
                            responseMessage = JsonSerializer.Serialize(status);
                            break;

                        default:
                            responseMessage = "Unknown command.";
                            break;
                    }

                    byte[] buffer = Encoding.UTF8.GetBytes(responseMessage);
                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength64 = buffer.Length;
                    await context.Response.OutputStream.WriteAsync(buffer);
                    context.Response.Close();
                }
            });

        }
    }
}