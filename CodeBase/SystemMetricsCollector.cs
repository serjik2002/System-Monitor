using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using LibreHardwareMonitor.Hardware;



internal class SystemMetricsCollector : IDisposable
{
    private bool _disposed = false;
    private readonly Computer _computer;

    public SystemMetricsCollector()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true, // Enable CPU monitoring
            IsGpuEnabled = true, // Enable GPU monitoring
            IsMemoryEnabled = true, // Enable RAM monitoring
            IsNetworkEnabled = true // Enable network monitoring
        };
        _computer.Open();
    }


    public float GetCpuUsage() => GetHardwareSensorValue(HardwareType.Cpu, SensorType.Load);

    public float GetCpuFrequency()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT CurrentClockSpeed FROM Win32_Processor"))
            {
                foreach (var obj in searcher.Get())
                {
                    return Convert.ToSingle(obj["CurrentClockSpeed"]) / 1000; // Convert from MHz to GHz
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting CPU frequency: {ex.Message}");
        }
        return -1; // Or handle error differently
    }

    public float GetCpuTemperature() => GetHardwareSensorValue(HardwareType.Cpu, SensorType.Temperature);

    public float GetGpuLoad()
    {
        var computer = new OpenHardwareMonitor.Hardware.Computer();
        computer.Open();
        computer.GPUEnabled = true;

        foreach (var hardware in computer.Hardware)
        {
            if (hardware.HardwareType == OpenHardwareMonitor.Hardware.HardwareType.GpuNvidia || hardware.HardwareType == OpenHardwareMonitor.Hardware.HardwareType.GpuAti)
            {
                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == OpenHardwareMonitor.Hardware.SensorType.Load && sensor.Name.Contains("Core"))
                    {
                        return sensor.Value.GetValueOrDefault();
                    }
                }
            }
        }
        computer.Close();
        return -1;
    }

    public float GetGpuMemory()
    {
        var totalMemory = GetHardwareSensorValue(HardwareType.GpuAmd, SensorType.SmallData); // Replace with GpuAmd if you have an AMD GPU
        if (totalMemory == -1)
        {
            return -1; // Handle sensor not found or unavailable
        }
        return totalMemory / (1024 * 1024); // Convert to MB 
    }

    public float GetGpuFrequancy()
    {
        var computer = new OpenHardwareMonitor.Hardware.Computer();
        computer.Open();
        computer.GPUEnabled = true;

        foreach (var hardware in computer.Hardware)
        {
            if (hardware.HardwareType == OpenHardwareMonitor.Hardware.HardwareType.GpuNvidia || hardware.HardwareType == OpenHardwareMonitor.Hardware.HardwareType.GpuAti)
            {
                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    // Шукаємо сенсор з назвою "GPU Core" або подібним, який має тип Clock
                    if (sensor.SensorType == OpenHardwareMonitor.Hardware.SensorType.Clock && sensor.Name.Contains("Core"))
                    {
                        return sensor.Value.GetValueOrDefault(); // Повертаємо значення частоти
                    }
                }
            }
        }
        computer.Close();
        return -1; // Повертаємо -1, якщо сенсор не знайдено
    }

    public float GetGpuTemperature() => GetHardwareSensorValue(HardwareType.GpuAmd, SensorType.Temperature);

    public float GetRamUsage() => GetHardwareSensorValue(HardwareType.Memory, SensorType.Data);
    
    public float GetTotalMemory()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
            {
                foreach (var obj in searcher.Get())
                {
                    return Convert.ToSingle(obj["TotalVisibleMemorySize"]) / 1024; // Convert from KB to MB
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting total memory: {ex.Message}");
        }
        return -1; // Or handle error differently
    }

    public float GetUsedMemory()=> GetHardwareSensorValue(HardwareType.Memory, SensorType.Data);


    private float GetNetworkSpeedValue(string adapterName, string propertyName)
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher(
                $"SELECT * FROM Win32_PerfFormattedData_Tcpip_NetworkInterface WHERE Name LIKE '%{adapterName}%'"))
            {
                foreach (var obj in searcher.Get())
                {
                    return Convert.ToSingle(obj[propertyName]);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting network speed: {ex.Message}");
        }
        return -1;
    }

    public float GetEthernetDownloadSpeed()
    {
        return GetNetworkSpeedValue("Realtek PCIe GbE Family Controller", "BytesReceivedPerSec");
    }

    public float GetEthernetUploadSpeed()
    {
        return GetNetworkSpeedValue("Realtek PCIe GbE Family Controller", "BytesSentPerSec");
    }

    private float GetHardwareSensorValue(HardwareType hardwareType, SensorType sensorType)
    {
        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType != hardwareType) continue;

            hardware.Update();

            return hardware
                .Sensors
                .Where(s => s.SensorType == sensorType && s.Value.HasValue)
                .Select(s => s.Value.Value)
                .FirstOrDefault();
        }

        return -1; // Or handle the case where no sensor is found differently
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
                _computer.Close();
            }
            _disposed = true;
        }
    }
}
