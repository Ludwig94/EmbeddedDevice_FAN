using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace EmbeddedDevice_FAN
{
    public partial class MainWindow : Window
    {
        private decimal _pendingSpeed;
        private bool isRunning = false;
        private Storyboard spinStoryboard;
        private DispatcherTimer _speedTimer;

        public MainWindow()
        {
            InitializeComponent();

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
        }

        private void LogMessage(string message)
        {
            string line = $@"{DateTime.Now:yyyy-MM-dd HH:mm:ss} : {message}";
            LB_EventLog.Items.Add(line);

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
    }
}