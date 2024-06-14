using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace System_Monitor.CodeBase
{
    internal class UiUpdater : INotifyPropertyChanged
    {
        private readonly ResourcesMonitorService resourcesMonitorService;
        private readonly ObservableCollection<ObservablePoint> cpuUsageValues = new ObservableCollection<ObservablePoint>();
        private readonly ObservableCollection<ObservablePoint> gpuLoadValues = new ObservableCollection<ObservablePoint>();

        public SeriesCollection CpuUsageValues { get; }
        public SeriesCollection GpuLoadValues { get; }

        public UiUpdater(ResourcesMonitorService monitorService)
        {
            resourcesMonitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));

            // Привязка данных для графиков
            CpuUsageValues = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "CPU Usage",
                    Values = cpuUsageValues
                }
            };
            GpuLoadValues = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "GPU Load",
                    Values = gpuLoadValues
                }
            };

            // Подписываемся на события обновления данных
            resourcesMonitorService.CpuUsageUpdated += ResourcesMonitorService_CpuUsageUpdated;
            resourcesMonitorService.GpuLoadUpdated += ResourcesMonitorService_GpuLoadUpdated;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ResourcesMonitorService_CpuUsageUpdated(object sender, float cpuUsage)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                cpuUsageValues.Add(new ObservablePoint { X = DateTime.Now.Ticks, Y = cpuUsage });
                TrimSeries(cpuUsageValues);
                OnPropertyChanged(nameof(CpuUsageValues));
            });
        }

        private void ResourcesMonitorService_GpuLoadUpdated(object sender, float gpuLoad)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                gpuLoadValues.Add(new ObservablePoint { X = DateTime.Now.Ticks, Y = gpuLoad });
                TrimSeries(gpuLoadValues);
                OnPropertyChanged(nameof(GpuLoadValues));
            });
        }

        private void TrimSeries(ObservableCollection<ObservablePoint> series)
        {
            while (series.Count > 10)
            {
                series.RemoveAt(0);
            }
        }

        public void StartMonitoring()
        {
            resourcesMonitorService.StartMonitoring();
        }

        public void StopMonitoring()
        {
            resourcesMonitorService.StopMonitoring();
        }
    }
}
