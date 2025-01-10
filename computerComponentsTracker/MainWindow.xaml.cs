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
        private PerformanceCounter gpuCounter;
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
            gpuCounter = new PerformanceCounter("GPU Engine", "Utilization Percentage", "engtype_3D_0");
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

            // Battery status
            float batteryStatus = GetBatteryStatus();
            batteryProgressBar.Value = batteryStatus;
            batteryLabel.Text = $"{batteryStatus:F0}%";

            // Network usage
            float networkUsage = GetNetworkUsage();
            networkProgressBar.Value = networkUsage;
            networkLabel.Text = $"{networkUsage:F2} KB/s";
            networkInterfaceNameActive.Text = $"Network Interface: {GetNetworkInterfaceName()}";
        }
        private float GetGpuUsage()
        {
            // First, check for NVIDIA GPU usage
            try
            {
                PerformanceCounter gpuCounter = new PerformanceCounter("GPU Engine", "Utilization Percentage", "engtype_3D_0");
                float gpuUsage = gpuCounter.NextValue(); // Get NVIDIA GPU usage percentage
                return gpuUsage;
            }
            catch (InvalidOperationException)
            {
                // If NVIDIA GPU is not found, try to get Intel or AMD GPU usage
                try
                {
                    // Query Intel GPU usage from WMI
                    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController WHERE AdapterCompatibility = 'Intel'");
                    foreach (ManagementObject gpu in searcher.Get())
                    {
                        // Intel GPUs have no LoadPercentage, but might still query related properties
                        // Intel GPUs might provide processor clock or other indicators of activity
                        var load = gpu["CurrentClockSpeed"]; // Example: Using clock speed as indicator (not utilization directly)
                        if (load != null)
                        {
                            return 0; // Intel GPUs might not directly expose load percentage, return 0 or estimated value
                        }
                    }
                }
                catch (Exception)
                {
                    // If querying Intel GPU or the WMI query fails, return 0
                    return 0;
                }
                // Finally, if AMD GPU is found, try fetching its load percentage from WMI
                try
                {
                    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController WHERE AdapterCompatibility = 'AMD'");
                    foreach (ManagementObject gpu in searcher.Get())
                    {
                        // Similar to Intel, check for available properties ("LoadPercentage" or similar)
                        var load = gpu["LoadPercentage"];
                        if (load != null)
                        {
                            return Convert.ToSingle(load);
                        }
                    }
                }
                catch (Exception)
                {
                    // If AMD GPU is not found, return 0
                    return 0;
                }
            }
            // If no GPU found or unable to retrieve usage, return 0
            Console.WriteLine("No GPUs drivers (NVidia, AMD, Intel) could be found on the system.");
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
        private int GetBatteryStatus()
        {
            // Use WMI to get battery status
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            foreach (var level in searcher.Get())
            {
                // Get battery status
                var charge = level["EstimatedChargeRemaining"];
                if (charge != null)
                {
                    return Convert.ToInt32(charge);
                }
            }
            return -1;
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
            float totalKbps = (totalBytes) / 1000; // Convert bytes to kilobits
            return totalKbps;
        }
    }
}
