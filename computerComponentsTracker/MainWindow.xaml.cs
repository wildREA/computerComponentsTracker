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
        private PerformanceCounter bytesSentCounter;
        private PerformanceCounter bytesReceivedCounter;

        // Hardware monitor
        private Computer computer;
        public MainWindow()
        {
            InitializeComponent();

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

            // GPU usage
            float gpuUsage = GetGpuUsage();
            gpuProgressBar.Value = gpuUsage;
            gpuUsageLabel.Text = $"{gpuUsage:F1}%";

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

            // Battery level
            var batteryInfo = GetBatteryLevel();
            batteryProgressBar.Value = (int)batteryInfo.Level; // Battery level (percentage)
            batteryLabel.Text = $"{batteryInfo.Level:F0}% {batteryInfo.Status}";

            // Network usage
            float networkUsage = GetNetworkUsage();
            networkProgressBar.Value = networkUsage;
            networkLabel.Text = $"{networkUsage:F2} KB/s";
            networkInterfaceNameActive.Text = $"Network Interface: {GetNetworkInterfaceName()}";
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
            try
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
            }
            catch (Exception ex)
            {
                // Log or handle exception here
                Console.WriteLine($"Error retrieving battery status: {ex.Message}");
            }
            return (0, "No battery found");
        }
        // GetBatteryLevel METHOD HERE (charging, not charging, has even battery (if desktop)?)
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
            float totalKbps = (totalBytes) / 1000; // Convert bytes to kilobits
            return totalKbps;
        }
    }
}
