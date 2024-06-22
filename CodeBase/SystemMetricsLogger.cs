using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class SystemMetricsLogger : IDisposable
{
    private SystemMetricsCollector _metricsCollector;
    private string _logFilePath;
    private const int MaxLogFileSize = 1024 * 1024; // 1 MB

    public SystemMetricsLogger(SystemMetricsCollector metricsCollector, string logFilePath = "C:\\Users\\serez\\Documents\\GitHub\\System-Monitor\\system_metrics_log.txt")
    {
        _metricsCollector = metricsCollector;
        _logFilePath = logFilePath;

        // Ensure the directory exists
        var logDirectory = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
    }

    public async Task StartLoggingAsync()
    {
        while (!_metricsCollector.IsDisposed)
        {
            string logEntry = $"{DateTime.Now:dd.MM.yyyy HH:mm}";

            var metrics = new Dictionary<string, float>
            {
                { "CPU Usage", _metricsCollector.GetCpuUsage() },
                { "CPU Frequency", _metricsCollector.GetCpuFrequency() },
                { "CPU Temperature", _metricsCollector.GetCpuTemperature() },
                { "GPU Load", _metricsCollector.GetGpuLoad() },
                { "GPU Memory", _metricsCollector.GetGpuMemory() },
                { "GPU Temperature", _metricsCollector.GetGpuTemperature() },
                { "RAM Usage", _metricsCollector.GetRamUsage() },
                { "Total Memory", _metricsCollector.GetTotalMemory() },
                { "Used Memory", _metricsCollector.GetUsedMemory() },
                { "Sent Bytes Per Second", _metricsCollector.GetEthernetUploadSpeed() },
                { "Received Bytes Per Second", _metricsCollector.GetEthernetDownloadSpeed() }
            };

            foreach (var metric in metrics)
            {
                if (!float.IsNaN(metric.Value) && metric.Value != -1)
                {
                    logEntry += $" {metric.Key}: {metric.Value}";
                }
            }

            // Create the log file if it doesn't exist
            if (!File.Exists(_logFilePath))
            {
                File.Create(_logFilePath).Dispose();
            }

            // Check file size and rotate if necessary
            if (new FileInfo(_logFilePath).Length > MaxLogFileSize)
            {
                RotateLogFile();
            }

            // Append the log entry to the file
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            await Task.Delay(5000); // Delay for 5 seconds
        }
    }

    private void RotateLogFile()
    {
        string newLogFileName = $"{Path.GetFileNameWithoutExtension(_logFilePath)}_{DateTime.Now:yyyyMMddHHmmss}.log";
        string newLogFilePath = Path.Combine(Path.GetDirectoryName(_logFilePath), newLogFileName);

        File.Move(_logFilePath, newLogFilePath);
    }

    public void Dispose()
    {
        _metricsCollector?.Dispose();
    }
}
