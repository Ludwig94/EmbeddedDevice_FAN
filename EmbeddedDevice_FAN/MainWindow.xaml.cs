using System.Windows;
using System.Windows.Media.Animation;

namespace EmbeddedDevice_FAN
{
    public partial class MainWindow : Window
    {
        private bool isRunning = false;
        private Storyboard spinStoryboard;

        public MainWindow()
        {
            InitializeComponent();

            // Get storyboard from resources
            spinStoryboard = (Storyboard)this.Resources["SpinFan"];
        }

        private void Btn_OnOff_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
            {
                spinStoryboard.Begin(this, true); // Start animation
                Btn_OnOff.Content = "STOP";
                isRunning = true;
            }
            else
            {
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
            }
        }
    }
}