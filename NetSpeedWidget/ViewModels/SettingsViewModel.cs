using System;
using System.Windows;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using NetSpeedWidget.Resources;

namespace NetSpeedWidget.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private const string StartupRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "NetSpeedWidget";
        private const string SettingsRegistryKey = @"SOFTWARE\NetSpeedWidget";

        private readonly Dictionary<string, string> _languageCodes = new()
        {
            { "English", "en-US" },
            { "Türkçe", "tr-TR" },
            { "Deutsch", "de-DE" },
            { "Español", "es-ES" },
            { "Français", "fr-FR" },
            { "Italiano", "it-IT" },
            { "日本語", "ja-JP" },
            { "한국어", "ko-KR" },
            { "中文", "zh-CN" }
        };

        [ObservableProperty]
        private bool _startWithWindows;

        [ObservableProperty]
        private string _selectedLanguage;

        public List<string> AvailableLanguages { get; } = new List<string>
        {
            "English",
            "Türkçe",
            "Deutsch",
            "Español",
            "Français",
            "Italiano",
            "日本語",
            "한국어",
            "中文"
        };

        public SettingsViewModel()
        {
            // Check if app is in startup registry
            using var startupKey = Registry.CurrentUser.OpenSubKey(StartupRegistryKey);
            _startWithWindows = startupKey?.GetValue(AppName) != null;

            // Load language setting
            using var settingsKey = Registry.CurrentUser.OpenSubKey(SettingsRegistryKey);
            _selectedLanguage = (settingsKey?.GetValue("Language") as string) ?? "English";

            // Create settings key if it doesn't exist
            if (settingsKey == null)
            {
                using var newKey = Registry.CurrentUser.CreateSubKey(SettingsRegistryKey);
                newKey.SetValue("Language", _selectedLanguage);
            }

            // Apply current language
            ApplyLanguage(_selectedLanguage);
        }

        private void ApplyLanguage(string language)
        {
            try
            {
                if (_languageCodes.TryGetValue(language, out string? cultureCode))
                {
                    var culture = new CultureInfo(cultureCode);
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture = culture;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Strings.LanguageError, ex.Message),
                    Strings.Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        partial void OnStartWithWindowsChanged(bool value)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);
                if (key == null)
                {
                    MessageBox.Show(
                        Strings.RegistryAccessError,
                        Strings.Error,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if (value)
                {
                    // Add to startup
                    var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (exePath != null)
                    {
                        key.SetValue(AppName, exePath);
                    }
                }
                else
                {
                    // Remove from startup
                    key.DeleteValue(AppName, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Strings.StartupError, ex.Message),
                    Strings.Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        partial void OnSelectedLanguageChanged(string value)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(SettingsRegistryKey);
                key.SetValue("Language", value);

                // Apply language immediately
                ApplyLanguage(value);

                MessageBox.Show(
                    Strings.RestartRequired,
                    Strings.LanguageChanged,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Strings.SaveLanguageError, ex.Message),
                    Strings.Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}