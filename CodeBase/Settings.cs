using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.ComponentModel;
using Microsoft.Win32;

using System.Drawing;


namespace System_Monitor.CodeBase
{
    public partial class Settings : INotifyPropertyChanged
    {
        private string _selectedMemoryUnit;
        private string _selectedSpeedUnit;

        private bool _autoStart;
        private bool _showInSystemTray;

        public string[] MemoryUnits { get; } = { "GB", "MB" };
        public string[] SpeedUnits { get; } = { "Mbps", "MB/s" };

        public string SelectedMetric { get; set; }
        public string SelectedFrequency { get; set; }
        public double NotificationThreshold { get; set; }
        public string SelectedNotificationType { get; set; }
        public string LogFilePath { get; set; }

        public string SelectedMemoryUnit
        {
            get { return _selectedMemoryUnit; }
            set
            {
                _selectedMemoryUnit = value;
                OnPropertyChanged(nameof(SelectedMemoryUnit));
            }
        }

        public string SelectedSpeedUnit
        {
            get { return _selectedSpeedUnit; }
            set
            {
                _selectedSpeedUnit = value;
                OnPropertyChanged(nameof(SelectedSpeedUnit));
            }
        }

        public bool AutoStart
        {
            get => _autoStart;
            set
            {
                if (_autoStart != value)
                {
                    _autoStart = value;
                    OnPropertyChanged(nameof(AutoStart));
                    SetAutoStart(value);
                }
            }
        }

        public bool ShowInSystemTray
        {
            get => _showInSystemTray;
            set
            {
                if (_showInSystemTray != value)
                {
                    _showInSystemTray = value;
                    OnPropertyChanged(nameof(ShowInSystemTray));
                }
            }
        }

        private void SetAutoStart(bool autoStart)
        {
            string appName = "SystemMonitor"; // Название вашего приложения
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (autoStart)
                {
                    key.SetValue(appName, $"\"{appPath}\"");
                }
                else
                {
                    key.DeleteValue(appName, false);
                }
            }
        }

        // Сохранение настроек в JSON файл
        public void Save(string filePath)
        {
            try
            {
                string json = JsonSerializer.Serialize(this);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                // Обработка ошибок сохранения
                Console.WriteLine($"Ошибка сохранения настроек: {ex.Message}");
            }
        }

        // Загрузка настроек из JSON файла
        public static Settings Load(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<Settings>(json);
                }
                else
                {
                    return new Settings(); // Возвращаем новый экземпляр настроек, если файл не найден
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок загрузки
                Console.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
                return new Settings(); // Возвращаем новый экземпляр настроек в случае ошибки
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
