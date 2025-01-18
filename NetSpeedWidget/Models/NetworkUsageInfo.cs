using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NetSpeedWidget.Models
{
    public partial class NetworkUsageInfo : ObservableObject
    {
        [ObservableProperty]
        private string _processName = string.Empty;

        [ObservableProperty]
        private int _processId;

        [ObservableProperty]
        private BitmapSource? _icon;

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

        public static BitmapSource? GetIconFromProcess(int processId)
        {
            try
            {
                using var process = System.Diagnostics.Process.GetProcessById(processId);
                string? fileName = process.MainModule?.FileName;
                if (string.IsNullOrEmpty(fileName)) return null;

                using var icon = System.Drawing.Icon.ExtractAssociatedIcon(fileName);
                if (icon == null) return null;

                using var bitmap = icon.ToBitmap();
                var handle = bitmap.GetHbitmap();
                try
                {
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        handle,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    DeleteObject(handle);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting icon for process {processId}: {ex.Message}");
                return null;
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}