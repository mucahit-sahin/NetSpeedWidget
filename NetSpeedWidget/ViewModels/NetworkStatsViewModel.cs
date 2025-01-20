using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using NetSpeedWidget.Services;

namespace NetSpeedWidget.ViewModels
{
    public partial class NetworkStatsViewModel : ObservableObject
    {
        private readonly NetworkStatsService _statsService;
        private readonly Dispatcher _dispatcher;

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        [ObservableProperty]
        private string _selectedPeriod = "Daily";

        [ObservableProperty]
        private ObservableCollection<ISeries> _downloadSeries = new();

        [ObservableProperty]
        private ObservableCollection<ISeries> _uploadSeries = new();

        [ObservableProperty]
        private ObservableCollection<AppUsageStats> _applicationStats = new();

        [ObservableProperty]
        private ObservableCollection<Axis> _xAxes;

        [ObservableProperty]
        private ObservableCollection<Axis> _yAxes;

        [ObservableProperty]
        private string[] _xLabels = Array.Empty<string>();

        [ObservableProperty]
        private string _totalDownload = "0 B";

        [ObservableProperty]
        private string _totalUpload = "0 B";

        private string _currentSortColumn = "Download";
        private bool _isAscending = false;

        public ObservableCollection<string> Periods { get; } = new()
        {
            "Daily",
            "Weekly",
            "Monthly",
            "Yearly"
        };

        public NetworkStatsViewModel()
        {
            _statsService = new NetworkStatsService();
            _dispatcher = Dispatcher.CurrentDispatcher;

            // Initialize axes
            _xAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = "Time",
                    TextSize = 12,
                    NameTextSize = 14,
                    ShowSeparatorLines = false,
                    LabelsPaint = new SolidColorPaint { Color = SKColors.LightGray },
                    NamePaint = new SolidColorPaint { Color = SKColors.LightGray }
                }
            };

            _yAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = "MB/s",
                    TextSize = 12,
                    NameTextSize = 14,
                    ShowSeparatorLines = false,
                    LabelsPaint = new SolidColorPaint { Color = SKColors.LightGray },
                    NamePaint = new SolidColorPaint { Color = SKColors.LightGray }
                }
            };

            UpdateStatsCommand.Execute(null);
        }

        private System.Windows.Media.ImageSource? GetIconFromProcessName(string processName)
        {
            try
            {
                var processes = System.Diagnostics.Process.GetProcessesByName(processName);
                if (processes.Length == 0) return null;

                using var process = processes[0];
                string? fileName = process.MainModule?.FileName;
                if (string.IsNullOrEmpty(fileName)) return null;

                using var icon = System.Drawing.Icon.ExtractAssociatedIcon(fileName);
                if (icon == null) return null;

                using var bitmap = icon.ToBitmap();
                var handle = bitmap.GetHbitmap();
                try
                {
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        handle,
                        System.IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    DeleteObject(handle);
                }
            }
            catch
            {
                return null;
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(System.IntPtr hObject);

        [RelayCommand]
        private async Task UpdateStats()
        {
            try
            {
                var (startDate, endDate) = GetDateRange();
                var stats = SelectedPeriod switch
                {
                    "Daily" => await _statsService.GetDailyStatsAsync(SelectedDate),
                    "Weekly" => await _statsService.GetWeeklyStatsAsync(startDate),
                    "Monthly" => await _statsService.GetMonthlyStatsAsync(SelectedDate.Year, SelectedDate.Month),
                    "Yearly" => await _statsService.GetYearlyStatsAsync(SelectedDate.Year),
                    _ => throw new ArgumentException("Invalid period")
                };

                var appTotals = await _statsService.GetApplicationTotalsAsync(startDate, endDate);

                _dispatcher.Invoke(() =>
                {
                    // Calculate total usage
                    double totalDownloadBytes = stats.Sum(s => s.TotalDownloadBytes);
                    double totalUploadBytes = stats.Sum(s => s.TotalUploadBytes);
                    TotalDownload = FormatBytes(totalDownloadBytes);
                    TotalUpload = FormatBytes(totalUploadBytes);

                    // Group data by hour/day/month based on selected period
                    var groupedStats = SelectedPeriod switch
                    {
                        "Daily" => stats.GroupBy(s => s.Timestamp.Hour)
                                      .Select(g => new
                                      {
                                          Time = g.Key.ToString("00") + ":00",
                                          Download = g.Sum(x => x.TotalDownloadBytes) / (1024.0 * 1024.0), // MB
                                          Upload = g.Sum(x => x.TotalUploadBytes) / (1024.0 * 1024.0) // MB
                                      })
                                      .OrderBy(x => x.Time),
                        "Weekly" => stats.GroupBy(s => s.Timestamp.Date)
                                       .Select(g => new
                                       {
                                           Time = g.Key.ToString("MM/dd"),
                                           Download = g.Sum(x => x.TotalDownloadBytes) / (1024.0 * 1024.0),
                                           Upload = g.Sum(x => x.TotalUploadBytes) / (1024.0 * 1024.0)
                                       })
                                       .OrderBy(x => x.Time),
                        "Monthly" => stats.GroupBy(s => s.Timestamp.Date)
                                        .Select(g => new
                                        {
                                            Time = g.Key.ToString("MM/dd"),
                                            Download = g.Sum(x => x.TotalDownloadBytes) / (1024.0 * 1024.0),
                                            Upload = g.Sum(x => x.TotalUploadBytes) / (1024.0 * 1024.0)
                                        })
                                        .OrderBy(x => x.Time),
                        "Yearly" => stats.GroupBy(s => s.Timestamp.Month)
                                       .Select(g => new
                                       {
                                           Time = new DateTime(2000, g.Key, 1).ToString("MMMM"),
                                           Download = g.Sum(x => x.TotalDownloadBytes) / (1024.0 * 1024.0),
                                           Upload = g.Sum(x => x.TotalUploadBytes) / (1024.0 * 1024.0)
                                       })
                                       .OrderBy(x => x.Time),
                        _ => throw new ArgumentException("Invalid period")
                    };

                    var data = groupedStats.ToList();
                    var timeLabels = data.Select(x => x.Time).ToArray();

                    // Update X axis labels
                    XAxes[0].Labels = timeLabels;

                    // Create download series
                    var downloadSeries = new LineSeries<double>
                    {
                        Values = data.Select(x => x.Download).ToList(),
                        Name = "Download",
                        Stroke = new SolidColorPaint(SKColors.LightGreen, 2),
                        GeometrySize = 8,
                        GeometryStroke = new SolidColorPaint(SKColors.LightGreen, 2),
                        GeometryFill = new SolidColorPaint(SKColors.DarkGreen),
                        Fill = null
                    };

                    // Create upload series
                    var uploadSeries = new LineSeries<double>
                    {
                        Values = data.Select(x => x.Upload).ToList(),
                        Name = "Upload",
                        Stroke = new SolidColorPaint(SKColors.Orange, 2),
                        GeometrySize = 8,
                        GeometryStroke = new SolidColorPaint(SKColors.Orange, 2),
                        GeometryFill = new SolidColorPaint(SKColors.DarkOrange),
                        Fill = null
                    };

                    // Update chart series
                    DownloadSeries = new ObservableCollection<ISeries> { downloadSeries };
                    UploadSeries = new ObservableCollection<ISeries> { uploadSeries };

                    // Update application stats
                    ApplicationStats = new ObservableCollection<AppUsageStats>(
                        appTotals.Select(kvp => new AppUsageStats
                        {
                            ProcessName = kvp.Key,
                            TotalDownload = FormatBytes(kvp.Value.TotalDownload),
                            TotalUpload = FormatBytes(kvp.Value.TotalUpload),
                            Icon = GetIconFromProcessName(kvp.Key)
                        })
                        .OrderByDescending(x => ParseBytes(x.TotalDownload)));
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error updating stats: {ex.Message}");
            }
        }

        private (DateTime startDate, DateTime endDate) GetDateRange()
        {
            return SelectedPeriod switch
            {
                "Daily" => (SelectedDate.Date, SelectedDate.Date.AddDays(1)),
                "Weekly" => (SelectedDate.Date.AddDays(-(int)SelectedDate.DayOfWeek), SelectedDate.Date.AddDays(7 - (int)SelectedDate.DayOfWeek)),
                "Monthly" => (new DateTime(SelectedDate.Year, SelectedDate.Month, 1), new DateTime(SelectedDate.Year, SelectedDate.Month, 1).AddMonths(1)),
                "Yearly" => (new DateTime(SelectedDate.Year, 1, 1), new DateTime(SelectedDate.Year + 1, 1, 1)),
                _ => throw new ArgumentException("Invalid period")
            };
        }

        private string FormatBytes(double bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes = bytes / 1024;
            }
            return $"{bytes:0.##} {sizes[order]}";
        }

        private double ParseBytes(string formattedBytes)
        {
            var parts = formattedBytes.Split(' ');
            if (parts.Length != 2 || !double.TryParse(parts[0], out double value))
                return 0;

            return parts[1] switch
            {
                "KB" => value * 1024,
                "MB" => value * 1024 * 1024,
                "GB" => value * 1024 * 1024 * 1024,
                "TB" => value * 1024 * 1024 * 1024 * 1024,
                _ => value
            };
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            UpdateStatsCommand.Execute(null);
        }

        partial void OnSelectedPeriodChanged(string value)
        {
            UpdateStatsCommand.Execute(null);
        }

        [RelayCommand]
        private void Sort(string columnName)
        {
            if (_currentSortColumn == columnName)
            {
                _isAscending = !_isAscending;
            }
            else
            {
                _currentSortColumn = columnName;
                _isAscending = false;
            }

            var sortedItems = _currentSortColumn switch
            {
                "ProcessName" => _isAscending
                    ? ApplicationStats.OrderBy(x => x.ProcessName).AsEnumerable()
                    : ApplicationStats.OrderByDescending(x => x.ProcessName).AsEnumerable(),
                "Download" => _isAscending
                    ? ApplicationStats.OrderBy(x => ParseBytes(x.TotalDownload)).AsEnumerable()
                    : ApplicationStats.OrderByDescending(x => ParseBytes(x.TotalDownload)).AsEnumerable(),
                "Upload" => _isAscending
                    ? ApplicationStats.OrderBy(x => ParseBytes(x.TotalUpload)).AsEnumerable()
                    : ApplicationStats.OrderByDescending(x => ParseBytes(x.TotalUpload)).AsEnumerable(),
                _ => ApplicationStats.AsEnumerable()
            };

            ApplicationStats = new ObservableCollection<AppUsageStats>(sortedItems);
        }
    }

    public class AppUsageStats
    {
        public string ProcessName { get; set; } = string.Empty;
        public string TotalDownload { get; set; } = string.Empty;
        public string TotalUpload { get; set; } = string.Empty;
        public System.Windows.Media.ImageSource? Icon { get; set; }
    }
}