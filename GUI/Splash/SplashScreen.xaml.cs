using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AccuBooks_ERP.Database;
using AccuBooks_ERP.GUI.Authentication;
using AccuBooks_ERP.Helper;

namespace AccuBooks_ERP.GUI.Splash
{
    public partial class SplashScreen : Window
    {
        private const double ProgressMaxWidth = 780;

        public SplashScreen()
        {
            InitializeComponent();
            Loaded += SplashScreen_Loaded;
        }

        private async void SplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await UpdateProgress(15, "Initializing local database engine...");
                
                using (var context = new AppDbContext())
                {
                    await UpdateProgress(40, "Verifying database schema & tables...");
                    await DbInitializer.InitializeAsync(context);
                }

                await UpdateProgress(75, "Loading system masters & security configurations...");
                await System.Threading.Tasks.Task.Delay(300);

                await UpdateProgress(90, "Preparing interface...");
                await System.Threading.Tasks.Task.Delay(200);

                await UpdateProgress(100, "Ready.");
                await System.Threading.Tasks.Task.Delay(300);

                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to initialize application startup.");
                MessageBox.Show($"Startup Error: {ex.Message}", "AccuBooks Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        public System.Threading.Tasks.Task UpdateProgress(double percent, string? stageText = null)
        {
            percent = Math.Max(0, Math.Min(100, percent));

            if (!string.IsNullOrEmpty(stageText))
                txtStage.Text = stageText;

            double targetWidth = ProgressMaxWidth * (percent / 100.0);

            var widthAnimation = new DoubleAnimation
            {
                To = targetWidth,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            widthAnimation.Completed += (s, e) => tcs.SetResult(true);

            ProgressFill.BeginAnimation(WidthProperty, widthAnimation);
            AnimatePercentageText(percent);

            return tcs.Task;
        }

        private void AnimatePercentageText(double target)
        {
            double current = 0;
            double.TryParse(txtPercentage.Text.TrimEnd('%'), out current);

            var timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            DateTime start = DateTime.Now;
            double from = current;
            double duration = 400; // ms

            timer.Tick += (s, e) =>
            {
                double elapsed = (DateTime.Now - start).TotalMilliseconds;
                double t = Math.Min(1, elapsed / duration);
                double eased = 1 - Math.Pow(1 - t, 2); // ease-out
                double value = from + (target - from) * eased;

                txtPercentage.Text = $"{(int)Math.Round(value)}%";

                if (t >= 1)
                    timer.Stop();
            };

            timer.Start();
        }
    }
}