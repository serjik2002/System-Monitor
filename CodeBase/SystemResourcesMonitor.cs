using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management; // Для доступа к WMI
using OpenHardwareMonitor.Hardware;
using System.Diagnostics; // Библиотека OpenHardwareMonitor

namespace System_Monitor.CodeBase
{

    public class SystemResourcesMonitor
    {
        private PerformanceCounter downloadCounter;
        private PerformanceCounter uploadCounter;

        private Computer computer;
        private ManagementObjectSearcher cpuFrequencySearcher;
        private ManagementObjectSearcher cpuTemperatureSearcher;
        private ManagementObjectSearcher gpuMemorySearcher;
        private ManagementObjectSearcher gpuFrequencySearcher;
        private ManagementObjectSearcher ramTotalSearcher;
        private ManagementObjectSearcher ramUsedSearcher;

        public SystemResourcesMonitor(string networkInterfaceName)
        {
            downloadCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", networkInterfaceName);
            uploadCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", networkInterfaceName);

            computer = new Computer();
            computer.CPUEnabled = true;
            computer.GPUEnabled = true;
            computer.Open();

            cpuFrequencySearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT MaxClockSpeed FROM Win32_Processor");
            cpuTemperatureSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT CurrentTemperature FROM Win32_Processor");
            gpuMemorySearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT AdapterRAM FROM Win32_VideoController");
            gpuFrequencySearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT MaxClockSpeed FROM Win32_VideoController");
            ramTotalSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
            ramUsedSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT FreePhysicalMemory FROM Win32_OperatingSystem");
        }

        public async Task<float> GetCurrentCpuUsageAsync()
        {
            float cpuUsage = 0f;

            foreach (var hardwareItem in computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.CPU)
                {
                    hardwareItem.Update();
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("CPU Total"))
                        {
                            cpuUsage = await Task.Run(() => sensor.Value ?? 0f);
                        }
                    }
                }
            }

            return cpuUsage;
        }

        public async Task<int> GetCurrentCpuFrequencyAsync()
        {
            int cpuFrequency = 0;

            foreach (ManagementObject queryObj in cpuFrequencySearcher.Get())
            {
                cpuFrequency = await Task.Run(() => Convert.ToInt32(queryObj["MaxClockSpeed"]));
            }

            return cpuFrequency;
        }

        public async Task<float> GetCurrentCpuTemperatureAsync()
        {
            float cpuTemperature = 0f;

            foreach (ManagementObject queryObj in cpuTemperatureSearcher.Get())
            {
                cpuTemperature = await Task.Run(() => Convert.ToSingle(queryObj["CurrentTemperature"]) / 10);
            }

            return cpuTemperature;
        }

        public async Task<float> GetCurrentGpuLoadAsync()
        {
            float gpuLoad = 0f;

            foreach (var hardwareItem in computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAti)
                {
                    hardwareItem.Update();
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("GPU Core"))
                        {
                            gpuLoad = await Task.Run(() => sensor.Value ?? 0f);
                        }
                    }
                }
            }

            return gpuLoad;
        }

        public async Task<ulong> GetTotalGpuMemoryAsync()
        {
            ulong gpuMemory = 0;

            foreach (ManagementObject queryObj in gpuMemorySearcher.Get())
            {
                gpuMemory = await Task.Run(() => Convert.ToUInt64(queryObj["AdapterRAM"]));
            }

            return gpuMemory;
        }

        public async Task<int> GetCurrentGpuFrequencyAsync()
        {
            int gpuFrequency = 0;

            foreach (ManagementObject queryObj in gpuFrequencySearcher.Get())
            {
                gpuFrequency = await Task.Run(() => Convert.ToInt32(queryObj["MaxClockSpeed"]));
            }

            return gpuFrequency;
        }

        public async Task<ulong> GetTotalRamLoadAsync()
        {
            ulong totalRam = 0;

            foreach (ManagementObject queryObj in ramTotalSearcher.Get())
            {
                totalRam = await Task.Run(() => Convert.ToUInt64(queryObj["TotalVisibleMemorySize"]));
            }

            return totalRam;
        }

        public async Task<ulong> GetUsedRamLoadAsync()
        {
            ulong usedRam = 0;

            foreach (ManagementObject queryObj in ramUsedSearcher.Get())
            {
                usedRam = await Task.Run(() => Convert.ToUInt64(queryObj["FreePhysicalMemory"]));
            }

            // Convert to used RAM
            ulong totalRam = await GetTotalRamLoadAsync();
            usedRam = totalRam - usedRam;

            return usedRam;
        }

        public async Task<float> GetEthernetDownloadSpeedAsync()
        {
            return await Task.Run(() => downloadCounter.NextValue());
        }

        public async Task<float> GetEthernetUploadSpeedAsync()
        {
            return await Task.Run(() => uploadCounter.NextValue());
        }
    }
}
