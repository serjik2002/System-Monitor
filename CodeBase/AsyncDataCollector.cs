using System;
using System.Threading;
using System.Threading.Tasks;

public class AsyncDataCollector : IDisposable
{
    private readonly SystemMetricsCollector _metricsCollector;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _disposed = false;

    public bool IsDisposed => _disposed;

    public AsyncDataCollector()
    {
        _metricsCollector = new SystemMetricsCollector();
    }

    public async Task StartCollectingAsync(CancellationToken cancellationToken = default)
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
                    try
                    {
                        // Метрики собираются внутри логгера, поэтому здесь мы не делаем ничего
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error collecting metrics: {ex.Message}");
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

    public SystemMetricsCollector GetMetricsCollector() => _metricsCollector;

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
