﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System_Monitor.CodeBase;

namespace System_Monitor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MetricsViewModel ViewModel { get; }
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MetricsViewModel();
            DataContext = ViewModel;
            var dataCollector = new AsyncDataCollector();
            var logger = new SystemMetricsLogger(dataCollector);
            _ = logger.StartLoggingAsync();
            ViewModel.StartAsync();
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ViewModel.Stop();
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.ShowDialog();
        }

    }
}
