using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetSpeedWidget.Data;
using NetSpeedWidget.Models;

namespace NetSpeedWidget.Services
{
    public class NetworkStatsService
    {
        private readonly NetworkDbContext _dbContext;

        public NetworkStatsService()
        {
            _dbContext = new NetworkDbContext();
            _dbContext.Database.EnsureCreated();
        }

        public async Task SaveNetworkUsageAsync(Dictionary<int, NetworkUsageInfo> usageData)
        {
            var networkUsage = new NetworkUsageData
            {
                Timestamp = DateTime.Now,
                TotalDownloadBytes = usageData.Values.Sum(x => x.DownloadBytesPerSecond),
                TotalUploadBytes = usageData.Values.Sum(x => x.UploadBytesPerSecond),
                ApplicationUsages = usageData.Values.Select(x => new ApplicationUsageData
                {
                    ProcessName = x.ProcessName,
                    DownloadBytes = x.DownloadBytesPerSecond,
                    UploadBytes = x.UploadBytesPerSecond
                }).ToList()
            };

            _dbContext.NetworkUsages.Add(networkUsage);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<NetworkUsageData>> GetDailyStatsAsync(DateTime date)
        {
            return await _dbContext.NetworkUsages
                .Include(n => n.ApplicationUsages)
                .Where(n => n.Timestamp.Date == date.Date)
                .OrderBy(n => n.Timestamp)
                .ToListAsync();
        }

        public async Task<List<NetworkUsageData>> GetWeeklyStatsAsync(DateTime startDate)
        {
            var endDate = startDate.AddDays(7);
            return await _dbContext.NetworkUsages
                .Include(n => n.ApplicationUsages)
                .Where(n => n.Timestamp >= startDate && n.Timestamp < endDate)
                .OrderBy(n => n.Timestamp)
                .ToListAsync();
        }

        public async Task<List<NetworkUsageData>> GetMonthlyStatsAsync(int year, int month)
        {
            return await _dbContext.NetworkUsages
                .Include(n => n.ApplicationUsages)
                .Where(n => n.Timestamp.Year == year && n.Timestamp.Month == month)
                .OrderBy(n => n.Timestamp)
                .ToListAsync();
        }

        public async Task<List<NetworkUsageData>> GetYearlyStatsAsync(int year)
        {
            return await _dbContext.NetworkUsages
                .Include(n => n.ApplicationUsages)
                .Where(n => n.Timestamp.Year == year)
                .OrderBy(n => n.Timestamp)
                .ToListAsync();
        }

        public async Task<Dictionary<string, (double TotalDownload, double TotalUpload)>> GetApplicationTotalsAsync(DateTime startDate, DateTime endDate)
        {
            var appStats = await _dbContext.ApplicationUsages
                .Where(a => a.NetworkUsageData.Timestamp >= startDate && a.NetworkUsageData.Timestamp < endDate)
                .GroupBy(a => a.ProcessName)
                .Select(g => new
                {
                    ProcessName = g.Key,
                    TotalDownload = g.Sum(a => a.DownloadBytes),
                    TotalUpload = g.Sum(a => a.UploadBytes)
                })
                .ToListAsync();

            return appStats.ToDictionary(
                x => x.ProcessName,
                x => (x.TotalDownload, x.TotalUpload));
        }
    }
}