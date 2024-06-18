using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using System_Monitor.CodeBase;
using System.IO;
using System.Linq;


public class MetricsViewModel : INotifyPropertyChanged
{
    private readonly AsyncDataCollector _asyncDataCollector;
    private readonly DispatcherTimer _timer;

    private float _cpuUsage;
    private float _cpuFrequency;
    private float _cpuTemperature;
    private float _ramUsage;
    private float _usedMemory;
    private float _totalMemory;
    private float _gpuLoad;
    private float _gpuMemory;
    private float _gpuFrequancy;
    private float _uploadSpeed;
    private float _downloadSpeed;

    public ObservableCollection<DiskInfo> DiskDrives { get; set; }

    private DiskInfo _selectedDisk;
    public DiskInfo SelectedDisk
    {
        get => _selectedDisk;
        set
        {
            _selectedDisk = value;
            OnPropertyChanged(nameof(SelectedDisk));
        }
    }


    // Series collections for different metrics
    public SeriesCollection CpuLoadSeries { get; set; }
    public SeriesCollection RamLoadSeries { get; set; }
    public SeriesCollection CpuFrequencySeries { get; set; }
    public SeriesCollection CpuTemperatureSeries { get; set; }
    public SeriesCollection GpuTemperatureSeries { get; set; }
    public SeriesCollection GpuLoadSeries { get; set; }
    public SeriesCollection GpuTotalMemorySeries { get; set; }
    public SeriesCollection GpuFrequencySeries { get; set; }
    public SeriesCollection UploadSpeedSeries { get; set; }
    public SeriesCollection DownloadSpeedSeries { get; set; }

    public Func<double, string> Formatter { get; set; }


    // Properties for metrics
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

    public float UsedMemory
    {
        get => _usedMemory;
        set
        {
            _usedMemory = value;
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

    public float GpuLoad
    {
        get => _gpuLoad;
        set
        {
            _gpuLoad = value;
            OnPropertyChanged();
        }
    }

    public float GpuTotalMemory
    {
        get => _gpuMemory;
        set
        {
            _gpuMemory = value;
            OnPropertyChanged();
        }
    }

    public float GpuFrequency
    {
        get => _gpuFrequancy;
        set
        {
            _gpuFrequancy = value;
            OnPropertyChanged();
        }
    } 
    
    public float GpuTemperature
    {
        get => _gpuFrequancy;
        set
        {
            _gpuFrequancy = value;
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


    // Constructor
    public MetricsViewModel()
    {
        _asyncDataCollector = new AsyncDataCollector();
        DiskDrives = new ObservableCollection<DiskInfo>(GetAllDisks());
        InitializeSeriesCollections();

        Formatter = value => value.ToString("F");

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += UpdateMetrics;
        _timer.Start();
    }

    private void InitializeSeriesCollections()
    {
        CpuLoadSeries = new SeriesCollection
        {
            new LineSeries
            {
                Title = "CPU Usage",
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };

        RamLoadSeries = new SeriesCollection
        {
            new LineSeries
            {
                Title = "RAM Usage",
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };

        CpuFrequencySeries = new SeriesCollection
        {
            new LineSeries
            {
                Title = "CPU Frequency",
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
                Title = "GPU Load",
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };

        GpuTemperatureSeries = new SeriesCollection
        {
            new LineSeries
            {
                Title = "GPU Temperature",
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };

        GpuTotalMemorySeries = new SeriesCollection
        {
            new LineSeries
            {
                Title = "GPU Total Memory",
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };

        GpuFrequencySeries = new SeriesCollection
        {
            new LineSeries
            {
                Title = "GPU Frequency",
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };

        UploadSpeedSeries = new SeriesCollection
        {
            new LineSeries
            {
                Title = "Sent Bytes Per Second",
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };

        DownloadSpeedSeries = new SeriesCollection
        {
            new LineSeries
            {
                Title = "Received Bytes Per Second",
                Values = new ChartValues<float>(),
                PointGeometry = null
            }
        };
    }

    private ObservableCollection<DiskInfo> GetAllDisks()
    {
        var disks = new ObservableCollection<DiskInfo>();
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            disks.Add(new DiskInfo
            {
                Name = drive.Name,
                TotalSpace = drive.TotalSize / (1024 * 1024 * 1024),
                UsedSpace = (drive.TotalSize - drive.AvailableFreeSpace) / (1024 * 1024 * 1024)
            });
        }
        return disks;
    }

    private void UpdateMetrics(object sender, EventArgs e)
    {
        var metricsCache = _asyncDataCollector.GetMetricsCache().GetAllMetrics();

        // Update CPU Usage
        CpuUsage = metricsCache.TryGetValue("CPU Usage", out float cpuUsage) ? cpuUsage : -1;
        AddValueToSeries(CpuLoadSeries, CpuUsage);

        // Update RAM Usage
        RamUsage = metricsCache.TryGetValue("RAM Usage", out float ramUsage) ? ramUsage : -1;
        AddValueToSeries(RamLoadSeries, RamUsage);

        // Update Used Memory and Total Memory
        UsedMemory = metricsCache.TryGetValue("Used Memory", out float usedMemory) ? usedMemory : -1;

        TotalMemory = metricsCache.TryGetValue("Total Memory", out float totalMemory) ? totalMemory  : -1;

        // Update CPU Frequency
        CpuFrequency = metricsCache.TryGetValue("CPU Frequency", out float cpuFrequency) ? cpuFrequency : -1;
        AddValueToSeries(CpuFrequencySeries, cpuFrequency);

        // Update CPU Temperature
        CpuTemperature = metricsCache.TryGetValue("CPU Temperature", out float cpuTemperature) ? cpuTemperature : -1;
        AddValueToSeries(CpuTemperatureSeries, cpuTemperature);

        // Update GPU Load
        GpuLoad = metricsCache.TryGetValue("GPU Load", out float gpuLoad) ? gpuLoad : -1;
        AddValueToSeries(GpuLoadSeries, gpuLoad);

        // Update GPU Total Memory
        GpuTotalMemory = metricsCache.TryGetValue("GPU Total Memory", out float gpuTotalMemory) ? gpuTotalMemory : -1;
        AddValueToSeries(GpuTotalMemorySeries, gpuTotalMemory);

        // Update GPU Frequency
        GpuFrequency = metricsCache.TryGetValue("GPU Frequency", out float gpuFrequency) ? gpuFrequency : -1;
        AddValueToSeries(GpuFrequencySeries, gpuFrequency);

        GpuTemperature = metricsCache.TryGetValue("GPU Temperature", out float gpuTemperature) ? gpuTemperature : -1;
        AddValueToSeries(GpuTemperatureSeries, gpuTemperature);

        // Update Upload Speed (converted to Mbps)
        UploadSpeed = metricsCache.TryGetValue("Sent Bytes Per Second", out float uploadSpeed) ? uploadSpeed / 125000 : -1;
        AddValueToSeries(UploadSpeedSeries, uploadSpeed / 125000);

        // Update Download Speed (converted to Mbps)
        DownloadSpeed = metricsCache.TryGetValue("Received Bytes Per Second", out float downloadSpeed) ? downloadSpeed / 125000 : -1;
        AddValueToSeries(DownloadSpeedSeries, downloadSpeed / 125000);

        // Trim data in series if it exceeds a certain number of points
        TrimSeriesData(CpuLoadSeries[0].Values);
        TrimSeriesData(RamLoadSeries[0].Values);
        TrimSeriesData(CpuFrequencySeries[0].Values);
        TrimSeriesData(CpuTemperatureSeries[0].Values);
        TrimSeriesData(GpuLoadSeries[0].Values);
        TrimSeriesData(GpuTotalMemorySeries[0].Values);
        TrimSeriesData(GpuFrequencySeries[0].Values);
        TrimSeriesData(UploadSpeedSeries[0].Values);
        TrimSeriesData(DownloadSpeedSeries[0].Values);
    }

    private void AddValueToSeries(SeriesCollection series, float value)
    {
        if (series != null && series.Count > 0)
        {
            var lineSeries = (LineSeries)series[0];
            lineSeries.Values.Add(value);
        }
    }


    private void TrimSeriesData(IChartValues values)
    {
        if (values is ChartValues<float> floatValues)
        {
            if (floatValues.Count > 20)
                floatValues.RemoveAt(0);
        }
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
