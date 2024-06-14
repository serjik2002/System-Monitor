using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using OpenHardwareMonitor.Hardware;

internal class SystemMetricsCollector : IDisposable
{
    private bool _disposed = false;
    private readonly Computer _computer;
    public SystemMetricsCollector()
    {
        _computer = new Computer { CPUEnabled = true, GPUEnabled = true };
        _computer.Open();
    }

    public float GetCpuUsage()
    {
        return GetWmiValue("Win32_Processor", "LoadPercentage");
    }

    public float GetCpuFrequency()
    {
        return GetWmiValue("Win32_Processor", "MaxClockSpeed") / 1000; // Convert from MHz to GHz
    }

    public float GetCpuTemperature()
    {
        return GetHardwareTemperature(HardwareType.CPU);
    }

    public float GetGpuTemperature()
    {
        return GetHardwareTemperature(HardwareType.GpuAti); // Or HardwareType.GpuNvidia for Nvidia GPUs
    }

    private float GetHardwareTemperature(HardwareType hardwareType)
    {
        try
        {
            foreach (IHardware hardware in _computer.Hardware)
            {
                if (hardware.HardwareType != hardwareType) continue;

                hardware.Update(); // Ensure hardware data is up-to-date

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        // Check if the sensor has a valid value
                        if (sensor.Value.HasValue)
                        {
                            return (float)sensor.Value;
                        }
                    }
                }

                // If no valid sensor value found
                Console.WriteLine($"No valid temperature sensor found for {hardwareType}.");
                return -1; // Or handle differently if needed
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting {hardwareType} temperature: {ex.Message}");
        }

        return -1; // Return -1 if an error occurs or sensor not found
    }

    public float GetRamUsage()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
            {
                foreach (var obj in searcher.Get())
                {
                    float totalVisibleMemory = Convert.ToSingle(obj["TotalVisibleMemorySize"]);
                    float freePhysicalMemory = Convert.ToSingle(obj["FreePhysicalMemory"]);
                    return ((totalVisibleMemory - freePhysicalMemory) / totalVisibleMemory) * 100;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error getting RAM usage: " + ex.Message);
        }
        return -1;
    }

    public float GetFreeRam()
    {
        return GetWmiValue("Win32_OperatingSystem", "FreePhysicalMemory") / 1024; // Convert from KB to MB
    }

    public float GetFreeSpaceDiskC()
    {
        return GetFreeSpaceForDrive("C:");
    }

    public float GetFreeSpaceDiskD()
    {
        return GetFreeSpaceForDrive("D:");
    }

    private float GetFreeSpaceForDrive(string driveLetter)
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher($"SELECT FreeSpace, Size FROM Win32_LogicalDisk WHERE DeviceID='{driveLetter}'"))
            {
                foreach (var obj in searcher.Get())
                {
                    float freeSpace = Convert.ToSingle(obj["FreeSpace"]);
                    float size = Convert.ToSingle(obj["Size"]);
                    return (freeSpace / size) * 100; // Percentage of free space
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting free space for drive {driveLetter}: " + ex.Message);
        }
        return -1;
    }


    public float GetSentBytesPerSecond()
    {
        float totalSentBytesPerSec = 0;
        try
        {
            using (var searcher = new ManagementObjectSearcher("Win32_PerfFormattedData_Tcpip_NetworkInterface", "BytesSentPerSec"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    float sentBytesPerSec = Convert.ToSingle(obj["BytesSentPerSec"]);
                    if (sentBytesPerSec >= 0)
                    {
                        totalSentBytesPerSec += sentBytesPerSec;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error getting sent bytes per second: " + ex.Message);
        }
        return totalSentBytesPerSec;
    }

    public float GetReceivedBytesPerSecond()
    {
        float totalReceivedBytesPerSec = 0;
        try
        {
            using (var searcher = new ManagementObjectSearcher("Win32_PerfFormattedData_Tcpip_NetworkInterface", "BytesReceivedPerSec"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    float receivedBytesPerSec = Convert.ToSingle(obj["BytesReceivedPerSec"]);
                    if (receivedBytesPerSec >= 0)
                    {
                        totalReceivedBytesPerSec += receivedBytesPerSec;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error getting received bytes per second: " + ex.Message);
        }
        return totalReceivedBytesPerSec;
    }

    private float GetWmiValue(string tableName, string fieldName)
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher($"SELECT {fieldName} FROM {tableName}"))
            {
                foreach (var obj in searcher.Get())
                {
                    return Convert.ToSingle(obj[fieldName]);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting {fieldName} from {tableName}: " + ex.Message);
        }
        return -1;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
            }
            _disposed = true;
        }
    }
}
