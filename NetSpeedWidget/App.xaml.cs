using System;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;

namespace NetSpeedWidget
{
    public partial class App : Application
    {
        public App()
        {
            // Add handler for unhandled exceptions
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Debug.WriteLine("Application constructor initialized");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                Debug.WriteLine("Application starting up");
                base.OnStartup(e);

                // Set the shutdown mode to when the last window is closed
                ShutdownMode = ShutdownMode.OnLastWindowClose;

                Debug.WriteLine("Application startup completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Critical error during startup: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Application startup failed: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Critical Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(-1);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Unhandled exception: {e.Exception.Message}\n{e.Exception.StackTrace}");
            MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Debug.WriteLine($"Critical error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"A critical error occurred: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Critical Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}