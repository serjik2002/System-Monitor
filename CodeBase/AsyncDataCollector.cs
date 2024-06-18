using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal class AsyncDataCollector : IDisposable
{
    private readonly SystemMetricsCollector _metricsCollector;
    private readonly MetricsCache _metricsCache;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _disposed = false;

    public AsyncDataCollector()
    {
        _metricsCollector = new SystemMetricsCollector();
        _metricsCache = new MetricsCache();
    }

    public async Task StartCollectingAsync(CancellationToken cancellationToken = default) // Додаємо CancellationToken
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
        var token = linkedTokenSource.Token;

        try
        {
            await Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try // Додаємо try-catch для обробки винятків
                    {
                        CollectMetrics();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error collecting metrics: {ex.Message}");
                        // Додаткова обробка помилки (наприклад, логування)
                    }
                    await Task.Delay(1000, token);
                }
            }, token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Data collection canceled.");
        }
    }

    public void StopCollecting()
    {
        _cancellationTokenSource?.Cancel();
    }

    private void CollectMetrics()
    {
        float cpuUsage = _metricsCollector.GetCpuUsage();
        float cpuFrequency = _metricsCollector.GetCpuFrequency();
        float cpuTemperature = _metricsCollector.GetCpuTemperature();
        float gpuLoad = _metricsCollector.GetGpuLoad();
        float gpuMemory = _metricsCollector.GetGpuMemory();
        float gpuFrequency = _metricsCollector.GetGpuFrequancy();
        float gpuTemperature = _metricsCollector.GetGpuTemperature();
        float ramUsage = _metricsCollector.GetRamUsage();
        float totalMemory = _metricsCollector.GetTotalMemory();
        float usedMemory = _metricsCollector.GetUsedMemory();
        float sentBytesPerSecond = _metricsCollector.GetEthernetUploadSpeed();
        float receivedBytesPerSecond = _metricsCollector.GetEthernetDownloadSpeed();


        _metricsCache.UpdateMetrics(new Dictionary<string, float>
    {
        { "CPU Usage", cpuUsage },
        { "CPU Frequency", cpuFrequency },
        { "CPU Temperature", cpuTemperature },
        { "GPU Load", gpuLoad },
        { "GPU Memory", gpuMemory },
        { "GPU Frequency", gpuFrequency },
        { "GPU Temperature", gpuTemperature },
        { "RAM Usage", ramUsage },
        { "Total Memory", totalMemory },
        { "Used Memory", usedMemory },
        { "Sent Bytes Per Second", sentBytesPerSecond },
        { "Received Bytes Per Second", receivedBytesPerSecond }
    });
    }

    public MetricsCache GetMetricsCache() => _metricsCache;

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
                _metricsCollector?.Dispose();
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            }
            _disposed = true;
        }
    }
}
public class MetricsCache
{
    private readonly Dictionary<string, float> _metrics = new Dictionary<string, float>();

    public void UpdateMetrics(Dictionary<string, float> metrics)
    {
        foreach (var metric in metrics)
        {
            _metrics[metric.Key] = metric.Value;
        }
    }

    public float GetMetric(string metricName)
    {
        return _metrics.TryGetValue(metricName, out float value) ? value : -1;
    }

    public Dictionary<string, float> GetAllMetrics()
    {
        return new Dictionary<string, float>(_metrics);
    }
}