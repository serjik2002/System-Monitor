using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SystemMetricsLogger
{
    private AsyncDataCollector _dataCollector;
    private string _logFilePath;
    private const int MaxLogFileSize = 1024 * 1024;

    public SystemMetricsLogger(AsyncDataCollector dataCollector, string logFilePath = "system_metrics_log.txt")
    {
        _dataCollector = dataCollector;
        _logFilePath = logFilePath;
    }



    public async Task StartLoggingAsync()
    {
        while (!_dataCollector.IsDisposed)
        {
            var metrics = _dataCollector.GetMetricsCache().GetAllMetrics();
            string logEntry = $"{DateTime.Now:dd.MM.yyyy HH:mm}";

            foreach (var metric in metrics)
            {
                if (!float.IsNaN(metric.Value)) 
                {
                    logEntry += $" {metric.Key}: {metric.Value}";
                }
            }
            if (new FileInfo(_logFilePath).Length > MaxLogFileSize)
            {
                RotateLogFile();
            }

            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            await Task.Delay(5000); 
        }
    }

    private void RotateLogFile()
    {
        string newLogFileName = $"{Path.GetFileNameWithoutExtension(_logFilePath)}_{DateTime.Now:yyyyMMddHHmmss}.log";
        string newLogFilePath = Path.Combine(Path.GetDirectoryName(_logFilePath), newLogFileName);

        File.Move(_logFilePath, newLogFilePath); 
    }
}

