using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Net.NetworkInformation;
using System.Windows.Threading;
using LibreHardwareMonitor.Hardware;
using Microsoft.Windows.Themes;

namespace computerComponentsTracker
{
    public partial class ComponentUsage : UserControl
    {
        private DispatcherTimer timer;
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private PerformanceCounter bytesSentCounter;
        private PerformanceCounter bytesReceivedCounter;
        private Computer computer;
        private string selectedNetworkAdapter = "Auto"; // Default to Auto

        public ComponentUsage()
        {
            InitializeComponent();
            InitializePerformanceCounters();
            InitializeHardwareMonitor();
            StartTimer();
        }

        private void InitializePerformanceCounters()
        {
            string networkInterfaceName = GetNetworkInterfaceNameBySelection();

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            bytesSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", networkInterfaceName);
            bytesReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", networkInterfaceName);

            // Get initial network interface values
            bytesSentCounter.NextValue();
            bytesReceivedCounter.NextValue();
        }

        private void InitializeHardwareMonitor()
        {
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true
            };
            computer.Open();
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMicroseconds(1)
            };
            timer.Tick += (sender, e) => UpdateSystemStats();
            timer.Start();
        }

        private void UpdateSystemStats()
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
                if (hardware.HardwareType == HardwareType.GpuNvidia ||
                    hardware.HardwareType == HardwareType.GpuAmd ||
                    hardware.HardwareType == HardwareType.GpuIntel)
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
            // Use WMI to get disk usage
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
            // Use WMI to get battery level
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
            // Get first active network interface as initially found adapter
            var category = new PerformanceCounterCategory("Network Interface");
            string[] instanceNames = category.GetInstanceNames();

            foreach (var name in instanceNames)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }
            throw new InvalidOperationException("No active network interface found.");
        }

        private string GetNetworkInterfaceNameByType(NetworkInterfaceType type)
        {
            // Get first active network interface of specified type
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == type && ni.OperationalStatus == OperationalStatus.Up)
                {
                    return ni.Name;
                }
            }
            throw new InvalidOperationException($"No active {type} network interface found.");
        }

        private float GetNetworkUsage()
        {
            // Get network usage in kilobits per second
            if (bytesReceivedCounter == null || bytesSentCounter == null)
            {
                return 0;
            }
            float bytesSent = bytesSentCounter.NextValue();
            float bytesReceived = bytesReceivedCounter.NextValue();
            float totalBytes = bytesSent + bytesReceived;
            return (totalBytes * 8) / 1000; // Convert bytes to kilobits
        }

        private string GetNetworkInterfaceNameBySelection()
        {
            // Get network interface name based on selected adapter option
            return selectedNetworkAdapter switch
            {
                "Auto" => GetNetworkInterfaceName(),
                "Ethernet" => GetNetworkInterfaceNameByType(NetworkInterfaceType.Ethernet),
                "Wi-Fi" => GetNetworkInterfaceNameByType(NetworkInterfaceType.Wireless80211),
                _ => GetNetworkInterfaceName()
            };
        }

        public void SetNetworkAdapter(string networkAdapter)
        {
            // Set network adapter based on user selection
            selectedNetworkAdapter = networkAdapter;
            InitializePerformanceCounters();
        }

        public void SetRefreshRate(string refreshRate)
        {
            // Set timer interval based on user selection
            timer.Interval = refreshRate switch
            {
                "Millisecond" => TimeSpan.FromMilliseconds(100),
                _ => TimeSpan.FromSeconds(1)
            };
        }
    }
}
