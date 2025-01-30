using System;
using System.Windows;
using System.Threading;
using System.Globalization;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using static App;

namespace computerComponentsTracker
{
    public partial class MainWindow : Window
    {
        public static ComponentUsage componentUsage;
        public Settings settings;
        private readonly IServiceProvider _serviceProvider;

        //public MainWindow(IServiceProvider serviceProvider)
        public MainWindow(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
            InitializePages(_serviceProvider);
            InitializeComponentPage(); // Run as default
        }

        private void InitializePages(IServiceProvider serviceProvider)
        {
            // Create a shared Settings instance
            settings = serviceProvider.GetRequiredService<Settings>();

            // Pass the shared Settings instance to ComponentUsage
            componentUsage = serviceProvider.GetRequiredService<ComponentUsage>();
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
