using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

public class MetricsViewModel : INotifyPropertyChanged
{
    private readonly AsyncDataCollector _asyncDataCollector;
    private readonly DispatcherTimer _timer;

    private float _cpuUsage;
    private float _cpuFrequency;
    private float _cpuTemperature;
    private float _ramUsage;
    private float _totalMemory;
    private float _usedMemory;
    private float _gpuLoad;
    private float _gpuTotalMemory;
    private float _gpuFrequency;
    private float _uploadSpeed;
    private float _downloadSpeed;

    public SeriesCollection CpuLoadSeries { get; set; }
    public SeriesCollection RamLoadSeries { get; set; }
    public SeriesCollection CpuFrequencySeries { get; set; }
    public SeriesCollection CpuTemperatureSeries { get; set; }
    public SeriesCollection GpuLoadSeries { get; set; }
    public SeriesCollection GpuTotalMemorySeries { get; set; }
    public SeriesCollection GpuFrequencySeries { get; set; }
    public SeriesCollection UploadSpeedSeries { get; set; }
    public SeriesCollection DownloadSpeedSeries { get; set; }

    public Func<double, string> Formatter { get; set; }

    // Properties for accessing data
    public float CpuUsage
    {
        get => _cpuUsage;
        set
        {
            _cpuUsage = value;
            OnPropertyChanged();
        }
    }

    public float CpuFrequency
    {
        get => _cpuFrequency;
        set
        {
            _cpuFrequency = value;
            OnPropertyChanged();
        }
    }

    public float CpuTemperature
    {
        get => _cpuTemperature;
        set
        {
            _cpuTemperature = value;
            OnPropertyChanged();
        }
    }

    public float RamUsage
    {
        get => _ramUsage;
        set
        {
            _ramUsage = value;
            OnPropertyChanged();
        }
    }

    public float TotalMemory
    {
        get => _totalMemory;
        set
        {
            _totalMemory = value;
            OnPropertyChanged();
        }
    }

    public float UsedMemory
    {
        get => _usedMemory;
        set
        {
            _usedMemory = value;
            OnPropertyChanged();
        }
    }

    public float GpuLoad
    {
        get => _gpuLoad;
        set
        {
            _gpuLoad = value;
            OnPropertyChanged();
            GpuLoadSeries[0].Values.Add(value);
        }
    }

    public float GpuTotalMemory
    {
        get => _gpuTotalMemory;
        set
        {
            _gpuTotalMemory = value;
            OnPropertyChanged();
        }
    }

    public float GpuFrequency
    {
        get => _gpuFrequency;
        set
        {
            _gpuFrequency = value;
            OnPropertyChanged();
        }
    }

    public float UploadSpeed
    {
        get => _uploadSpeed;
        set
        {
            _uploadSpeed = value;
            OnPropertyChanged();
        }
    }

    public float DownloadSpeed
    {
        get => _downloadSpeed;
        set
        {
            _downloadSpeed = value;
            OnPropertyChanged();
        }
    }

    public MetricsViewModel()
    {
        _asyncDataCollector = new AsyncDataCollector();
        CpuLoadSeries = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };
        RamLoadSeries = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };
        RamLoadSeries = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };
        RamLoadSeries = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };
        CpuFrequencySeries = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };
        CpuTemperatureSeries = new SeriesCollection
        {
            new LineSeries
            {
                Title = "CPU Temperature",
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };

        GpuLoadSeries = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };
        GpuTotalMemorySeries = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };
        GpuFrequencySeries = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };
        UploadSpeedSeries = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };

        DownloadSpeedSeries = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };

        Formatter = value => value.ToString("F");

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
        _timer.Tick += UpdateMetrics;
        _timer.Start();
    }

    private void UpdateMetrics(object sender, EventArgs e)
    {
        var metricsCache = _asyncDataCollector.GetMetricsCache().GetAllMetrics();

        // Update CPU Usage
        CpuUsage = metricsCache.TryGetValue("CPU Usage", out float cpuUsage) ? cpuUsage : -1;
        CpuLoadSeries[0].Values.Add(CpuUsage);

        // Update RAM Usage
        RamUsage = metricsCache.TryGetValue("RAM Usage", out float ramUsage) ? ramUsage / 8 : -1;
        RamLoadSeries[0].Values.Add(RamUsage);

        // Update CPU Frequency
        CpuFrequency = metricsCache.TryGetValue("CPU Frequency", out float cpuFrequency) ? cpuFrequency : -1;
        CpuFrequencySeries[0].Values.Add(CpuFrequency);

        // Update CPU Temperature
        CpuTemperature = metricsCache.TryGetValue("CPU Temperature", out float cpuTemperature) ? cpuTemperature : -1;
        CpuTemperatureSeries[0].Values.Add(CpuTemperature);

        // Update GPU Load, Total Memory, and Frequency (if applicable)
        GpuLoad = metricsCache.TryGetValue("GPU Load", out float gpuLoad) ? gpuLoad : -1;
        GpuLoadSeries[0].Values.Add(GpuLoad);

        GpuTotalMemory = metricsCache.TryGetValue("GPU Total Memory", out float gpuTotalMemory) ? gpuTotalMemory : -1;
        GpuTotalMemorySeries[0].Values.Add(GpuTotalMemory);

        GpuFrequency = metricsCache.TryGetValue("GPU Frequency", out float gpuFrequency) ? gpuFrequency : -1;
        GpuFrequencySeries[0].Values.Add(GpuFrequency);

        // Update Network Upload and Download Speeds (converted to Mbps)
        UploadSpeed = metricsCache.TryGetValue("Sent Bytes Per Second", out float uploadSpeed) ? uploadSpeed / 125000 : -1;
        UploadSpeedSeries[0].Values.Add(UploadSpeed);

        DownloadSpeed = metricsCache.TryGetValue("Received Bytes Per Second", out float downloadSpeed) ? downloadSpeed / 125000 : -1;
        DownloadSpeedSeries[0].Values.Add(DownloadSpeed);

        // Remove old values if collection exceeds 20 elements
        
        
    }

    private void TrimSeriesData(ChartValues<float> values)
    {
        if (values.Count > 20)
            values.RemoveAt(0);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task StartAsync()
    {
        await _asyncDataCollector.StartCollectingAsync();
    }

    public void Stop()
    {
        _asyncDataCollector.StopCollecting();
    }
}
