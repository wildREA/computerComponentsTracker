using System.Windows;
using System.Threading;
using System.Globalization;
using System.Windows.Controls;

namespace computerComponentsTracker
{
    public partial class MainWindow : Window
    {
        public static ComponentUsage componentUsage;
        public Settings settings;

        public MainWindow()
        {
            InitializeComponent();
            InitializePages();
            InitializeComponentPage(); // Run as default
        }

        private void InitializePages()
        {
            // Create a shared Settings instance
            settings = new Settings();

            // Pass the shared Settings instance to ComponentUsage
            componentUsage = new ComponentUsage();
        }

        private void InitializeComponentPage()
        {
            MainContent.Content = componentUsage;
        }

        private void MonitorButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = componentUsage;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = settings;
        }
    }
}
