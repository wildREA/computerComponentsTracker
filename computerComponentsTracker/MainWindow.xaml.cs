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
        // Performance counters for CPU and RAM
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;

        // Hardware monitor
        private Computer computer;
        public MainWindow()
        {
            InitializeComponent();

            // Initialize performance counters
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            // Initialize LibreHardwareMonitor library
            computer = new Computer
            {
                IsCpuEnabled = true
            };
            computer.Open();

            // Set up timer to update stats every second
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += UpdateSystemStats;
            timer.Start();
        }
        private void UpdateSystemStats(object? sender, EventArgs e)
        {
            // CPU usage
            float cpuUsage = cpuCounter.NextValue();
            cpuProgressBar.Value = cpuUsage;
            cpuUsageLabel.Text = $"{cpuUsage:F1}%";

            // RAM usage
            float availableRam = ramCounter.NextValue() / 1024; // Convert to GB
            float totalRam = GetTotalRam();
            float ramUsage = totalRam - availableRam;
            ramProgressBar.Value = (ramUsage / totalRam) * 100;
            ramUsageLabel.Text = $"{ramUsage:F1} GB / {totalRam:F1} GB";

            // Disk usage
            float diskUsage = GetDiskUsage();
            diskProgressBar.Value = diskUsage;
            diskUsageLabel.Text = $"{diskUsage:F1}%";

            // Battery status
            float batteryStatus = GetBatteryStatus();
            batteryProgressBar.Value = batteryStatus;
            batteryLabel.Text = $"{batteryStatus:F0}%";

            // Network usage
            float networkUsage = GetNetworkUsage();
            batteryProgressBar.Value = networkUsage;
            batteryLabel.Text = $"{networkUsage:F2} MB/s";
        }
        private float GetTotalRam()
        {
            // Use WMI to get total RAM
            var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (var obj in searcher.Get())
            {
                return Convert.ToSingle(obj["TotalPhysicalMemory"]) / (1024 * 1024 * 1024); // Convert to GB
            }
            return 0;
        }
        private float GetDiskUsage()
        {
            // Use WMI to get disk space usage
            var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DriveType = 3");
            foreach (var disk in searcher.Get())
            {
                float freeSpace = Convert.ToSingle(disk["FreeSpace"]) / (1024 * 1024 * 1024); // Convert to GB
                float totalSpace = Convert.ToSingle(disk["Size"]) / (1024 * 1024 * 1024);     // Convert to GB
                return ((totalSpace - freeSpace) / totalSpace) * 100;                        // Usage percentage
            }
            return 0;
        }
        private int GetBatteryStatus()
        {
            // Use WMI to get battery status
            var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            foreach (var obj in searcher.Get())
            {
                // Get battery status
                var charge = obj["EstimatedChargeRemaining"];
                if (charge != null)
                {
                    return Convert.ToInt32(charge);
                }
            }
            return -1;
        }
        private float GetNetworkUsage()
        {
            try
            {
                // Get all network interfaces
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var networkInterface in networkInterfaces)
                {
                    // Skip virtual adapters (like VMware, VirtualBox, etc.)
                    if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                        !networkInterface.Name.Contains("VMnet"))
                    {
                        string interfaceName = networkInterface.Name;

                        // Use the interface name in the performance counter
                        PerformanceCounter bytesSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", interfaceName);
                        PerformanceCounter bytesReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", interfaceName);

                        // Get network stats
                        float bytesSent = bytesSentCounter.NextValue();
                        float bytesReceived = bytesReceivedCounter.NextValue();

                        return (bytesSent + bytesReceived) / (1024 * 1024); // MB/s
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting network stats: {ex.Message}");
            }
            return 0;
        }
    }
}
