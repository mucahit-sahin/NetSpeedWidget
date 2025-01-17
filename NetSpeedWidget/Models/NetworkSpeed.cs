using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetSpeedWidget.Models
{
    public class NetworkSpeed : INotifyPropertyChanged
    {
        private double _downloadSpeed;
        private double _uploadSpeed;

        public double DownloadSpeed
        {
            get => _downloadSpeed;
            set
            {
                if (_downloadSpeed != value)
                {
                    _downloadSpeed = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DownloadSpeedFormatted));
                }
            }
        }

        public double UploadSpeed
        {
            get => _uploadSpeed;
            set
            {
                if (_uploadSpeed != value)
                {
                    _uploadSpeed = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(UploadSpeedFormatted));
                }
            }
        }

        public string DownloadSpeedFormatted => FormatSpeed(DownloadSpeed);
        public string UploadSpeedFormatted => FormatSpeed(UploadSpeed);

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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}