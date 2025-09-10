using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EmbeddedDevice_FAN
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isRunning = false;
        private Storyboard? _rotatingFan;
        public MainWindow()
        {
            InitializeComponent();
            _rotatingFan = ((BeginStoryboard)FindResource("sb-rotate-fan")).Storyboard;
        }

        private void Btn_OnOff_Click(object sender, RoutedEventArgs e)
        {
            ToggleRunningState();

            if (_isRunning)
            {
                _rotatingFan?.Begin(this, true); // Start the fan animation
            }
            else
            {
                _rotatingFan?.Stop(); // Stop the fan animation
            }
        }

        private void ToggleRunningState()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Btn_OnOff.Content = "STOP";
            }
            else
            {
                _isRunning = false;
                Btn_OnOff.Content = "START";
            }
        }
    }
}