using System;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Threading;
using LibreHardwareMonitor.Hardware;

namespace computerComponentsTracker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeComponentPage(); // Run as default
        }

        // Performance counters for CPU and RAM
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private PerformanceCounter bytesSentCounter;
        private PerformanceCounter bytesReceivedCounter;

        // Hardware monitor
        private Computer computer;
        private void InitializeComponentPage()
        {
            ComponentUsage ComponentUsage = new ComponentUsage();

            // Get network interface name
            string networkInterfaceName = GetNetworkInterfaceName();


            // Initialize performance counters
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            bytesSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", networkInterfaceName);
            bytesReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", networkInterfaceName);

            // Get network interface values
            bytesSentCounter.NextValue();
            bytesReceivedCounter.NextValue();

            // Initialize LibreHardwareMonitor library
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true
            };
            computer.Open();
            MainContent.Content = ComponentUsage;

            // Set up timer to update stats every second
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => UpdateSystemStats(ComponentUsage);
            timer.Start();
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeSettingsPage();
        }
        private void InitializeSettingsPage()
        {
            Settings settingsPage = new Settings();
            MainContent.Content = settingsPage;
        }
        private void UpdateSystemStats(ComponentUsage componentUsage)
        {
            if (MainContent.Content == componentUsage) // For optimization
            {
                // CPU usage
                float cpuUsage = cpuCounter.NextValue();
                componentUsage.cpuProgressBar.Value = cpuUsage;
                componentUsage.cpuUsageLabel.Text = $"{cpuUsage:F1}%";

                // GPU usage
                float gpuUsage = GetGpuUsage();
                componentUsage.gpuProgressBar.Value = gpuUsage;
                componentUsage.gpuUsageLabel.Text = $"{gpuUsage:F1}%";

                // RAM usage
                float availableRam = ramCounter.NextValue() / 1024; // Convert to GB
                float totalRam = GetTotalRam();
                float ramUsage = totalRam - availableRam;
                componentUsage.ramProgressBar.Value = (ramUsage / totalRam) * 100;
                componentUsage.ramUsageLabel.Text = $"{ramUsage:F1} GB / {totalRam:F1} GB";

                // Disk usage
                float diskUsage = GetDiskUsage();
                componentUsage.diskProgressBar.Value = diskUsage;
                componentUsage.diskUsageLabel.Text = $"{diskUsage:F1}%";

                // Battery level
                var batteryInfo = GetBatteryLevel();
                componentUsage.batteryProgressBar.Value = (int)batteryInfo.Level; // Battery level (percentage)
                componentUsage.batteryLabel.Text = $"{batteryInfo.Level:F0}% {batteryInfo.Status}";

                // Network usage
                float networkUsage = GetNetworkUsage();
                componentUsage.networkProgressBar.Value = networkUsage;
                componentUsage.networkLabel.Text = $"{networkUsage:F2} KB/s";
                componentUsage.networkInterfaceNameActive.Text = $"Network Interface: {GetNetworkInterfaceName()}";
            }
        }
        private float GetGpuUsage()
        {
            // Get GPU usage in percentage
            foreach (var hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuIntel)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load)
                        {
                            return sensor.Value.GetValueOrDefault();
                        }
                    }
                }
            }
            return 0;
        }
        private float GetTotalRam()
        {
            // Use WMI to get total RAM
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (var ram in searcher.Get())
            {
                return Convert.ToSingle(ram["TotalPhysicalMemory"]) / (1024 * 1024 * 1024); // Convert to GB
            }
            return 0;
        }
        private float GetDiskUsage()
        {
            // Use WMI to get disk space usage
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DriveType = 3");
            foreach (var disk in searcher.Get())
            {
                float freeSpace = Convert.ToSingle(disk["FreeSpace"]) / (1024 * 1024 * 1024); // Convert to GB
                float totalSpace = Convert.ToSingle(disk["Size"]) / (1024 * 1024 * 1024);     // Convert to GB
                return ((totalSpace - freeSpace) / totalSpace) * 100;                        // Usage percentage
            }
            return 0;
        }
        private (float Level, string Status) GetBatteryLevel()
        {
            // Use WMI to get battery status
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            foreach (var battery in searcher.Get())
            {
                float level = Convert.ToSingle(battery["EstimatedChargeRemaining"] ?? 0);
                bool isCharging = battery["BatteryStatus"]?.ToString() == "2"; // 2 indicates charging

                string status = isCharging ? "(Charging)" : string.Empty;
                return (level, status);
            }
            return (0, "No battery found");
        }
        private string GetNetworkInterfaceName()
        {
            // Get all valid network interface names
            var category = new PerformanceCounterCategory("Network Interface");
            string[] instanceNames = category.GetInstanceNames();

            // Use first valid network interface
            foreach (string name in instanceNames)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }
            throw new InvalidOperationException("No active network interface found.");
        }
        private float GetNetworkUsage()
        {
            // Get network usage in bytes per second
            float bytesSent = bytesSentCounter.NextValue();
            float bytesReceived = bytesReceivedCounter.NextValue();

            // Convert to kbps (kilobits per second)
            float totalBytes = bytesSent + bytesReceived;
            float totalKbps = (totalBytes * 8) / 1000; // Convert bytes to kilobits
            return totalKbps;
        }
    }
}
