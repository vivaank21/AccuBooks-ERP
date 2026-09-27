using System;
using System.Windows;
using AccuBooks_ERP.Helper;

namespace AccuBooks_ERP
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize Serilog Structured Logger
            LoggerService.Initialize();

            // Catch unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                LoggerService.Error(ex ?? new Exception("Unknown domain exception"), "Fatal Domain Unhandled Exception");
            };

            DispatcherUnhandledException += (s, args) =>
            {
                LoggerService.Error(args.Exception, "Dispatcher Unhandled Exception");
                MessageBox.Show($"An unexpected error occurred:\n{args.Exception.Message}\n\nDetails have been logged.", "AccuBooks ERP Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LoggerService.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
