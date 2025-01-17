using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NetSpeedWidget.Models
{
    public partial class NetworkUsageInfo : ObservableObject
    {
        [ObservableProperty]
        private string _processName = string.Empty;

        [ObservableProperty]
        private int _processId;

        private double _downloadBytesPerSecond;
        public double DownloadBytesPerSecond
        {
            get => _downloadBytesPerSecond;
            set
            {
                if (SetProperty(ref _downloadBytesPerSecond, value))
                {
                    OnPropertyChanged(nameof(DownloadSpeedFormatted));
                    OnPropertyChanged(nameof(TotalBytesPerSecond));
                }
            }
        }

        private double _uploadBytesPerSecond;
        public double UploadBytesPerSecond
        {
            get => _uploadBytesPerSecond;
            set
            {
                if (SetProperty(ref _uploadBytesPerSecond, value))
                {
                    OnPropertyChanged(nameof(UploadSpeedFormatted));
                    OnPropertyChanged(nameof(TotalBytesPerSecond));
                }
            }
        }

        public double TotalBytesPerSecond => DownloadBytesPerSecond + UploadBytesPerSecond;

        public string DownloadSpeedFormatted => FormatSpeed(DownloadBytesPerSecond);
        public string UploadSpeedFormatted => FormatSpeed(UploadBytesPerSecond);

        private string FormatSpeed(double speed)
        {
            if (speed > 1024 * 1024)
                return $"{speed / (1024 * 1024):F1}M/s";
            else if (speed > 1024)
            {
                var kbSpeed = speed / 1024;
                return kbSpeed < 1024 ? $"{kbSpeed:F0}K/s" : $"{kbSpeed:F1}K/s";
            }
            else
                return $"{speed:F0}B/s";
        }
    }
}