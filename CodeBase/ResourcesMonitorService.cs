using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System_Monitor.CodeBase
{
    public class ResourcesMonitorService
    {
        private readonly SystemResourcesMonitor resourcesMonitor;
        private CancellationTokenSource cancellationTokenSource;

        public ResourceMonitorService(string networkInterfaceName)
        {
            resourcesMonitor = new SystemResourcesMonitor(networkInterfaceName);
        }

        public event EventHandler<float> CpuUsageUpdated;
        public event EventHandler<float> GpuLoadUpdated;

        public void StartMonitoring()
        {
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => MonitorResourcesAsync(cancellationTokenSource.Token));
        }

        public void StopMonitoring()
        {
            cancellationTokenSource?.Cancel();
        }

        private async Task MonitorResourcesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    float cpuUsage = await resourcesMonitor.GetCurrentCpuUsageAsync();
                    float gpuLoad = await resourcesMonitor.GetCurrentGpuLoadAsync();

                    CpuUsageUpdated?.Invoke(this, cpuUsage);
                    GpuLoadUpdated?.Invoke(this, gpuLoad);

                    await Task.Delay(1000, cancellationToken); // Пауза между измерениями
                }
                catch (OperationCanceledException)
                {
                    break; // Операция отменена, выходим из цикла
                }
                catch (Exception ex)
                {
                    // Обработка ошибок
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}

