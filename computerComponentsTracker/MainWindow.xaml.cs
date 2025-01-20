using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using LibreHardwareMonitor.Hardware;
using Microsoft.Windows.Themes;

namespace computerComponentsTracker
{
    public partial class MainWindow : Window
    {
        private ComponentUsage componentUsage;

        public MainWindow()
        {
            InitializeComponent();
            InitializeComponentPage(); // Run as default
        }

        private void InitializeComponentPage()
        {
            componentUsage = new ComponentUsage();
            MainContent.Content = componentUsage;
        }

        private void MonitorButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = componentUsage;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsPage = new Settings();
            settingsPage.NetworkAdapterChanged += SettingsPage_NetworkAdapterChanged;
            settingsPage.RefreshRateChanged += SettingsPage_RefreshRateChanged;
            MainContent.Content = settingsPage;
        }

        private void SettingsPage_NetworkAdapterChanged(object sender, string networkAdapter)
        {
            componentUsage.SetNetworkAdapter(networkAdapter);
        }

        private void SettingsPage_RefreshRateChanged(object sender, string refreshRate)
        {
            componentUsage.SetRefreshRate(refreshRate);
        }
    }
}
