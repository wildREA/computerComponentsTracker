using System.Windows;
using System.Threading;
using System.Globalization;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace computerComponentsTracker
{
    public partial class MainWindow : Window
    {
        public static ComponentUsage componentUsage;
        public Settings settings;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            InitializePages(serviceProvider);
            InitializeComponentPage(); // Run as default
        }

        private void InitializePages(IServiceProvider serviceProvider)
        {
            // Create a shared Settings instance
            settings = serviceProvider.GetRequiredService<Settings>();

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
