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

    public async Task StartCollectingAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        try
        {
            await Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    CollectMetrics();
                    await Task.Delay(1000, token); // Затримка на 1 секунду
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
        float ramUsage = _metricsCollector.GetRamUsage();
        float freeRam = _metricsCollector.GetFreeRam();
        float freeSpaceDiskC = _metricsCollector.GetFreeSpaceDiskC();
        float freeSpaceDiskD = _metricsCollector.GetFreeSpaceDiskD();
        float sentBytesPerSecond = _metricsCollector.GetSentBytesPerSecond();
        float receivedBytesPerSecond = _metricsCollector.GetReceivedBytesPerSecond();


        _metricsCache.UpdateMetrics(new Dictionary<string, float>
        {
            { "CPU Usage", cpuUsage },
            { "CPU Frequency", cpuFrequency },
            { "CPU Temperature", cpuTemperature },
            { "RAM Usage", ramUsage },
            { "Free RAM", freeRam },
            { "Free Space Disk C", freeSpaceDiskC },
            { "Free Space Disk D", freeSpaceDiskD },
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