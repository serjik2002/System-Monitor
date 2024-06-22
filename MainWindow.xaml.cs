using System;
using System.Threading.Tasks;
using System.Windows;
using System_Monitor.CodeBase;

namespace System_Monitor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MetricsViewModel ViewModel { get; }
        private AsyncDataCollector _dataCollector;
        private SystemMetricsLogger _logger;

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MetricsViewModel();
            DataContext = ViewModel;

            // Инициализация и запуск сборщика данных и логгера
            _dataCollector = new AsyncDataCollector();
            _logger = new SystemMetricsLogger(_dataCollector.GetMetricsCollector());
            StartLoggingAndDataCollection();
        }

        private async void StartLoggingAndDataCollection()
        {
            // Запуск логирования и сбора данных параллельно
            var viewModelTask = ViewModel.StartAsync();
            var loggerTask = _logger.StartLoggingAsync();

            await Task.WhenAll(viewModelTask, loggerTask);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Остановка сбора данных при закрытии окна
            ViewModel.Stop();
            _dataCollector.Dispose();
            _logger.Dispose();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.ShowDialog();
        }
    }
}
