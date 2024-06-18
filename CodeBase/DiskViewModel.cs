using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System_Monitor.CodeBase
{
    internal class DiskViewModel : INotifyPropertyChanged
    {
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

        public DiskViewModel()
        {
            DiskDrives = new ObservableCollection<DiskInfo>(GetAllDisks());
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
