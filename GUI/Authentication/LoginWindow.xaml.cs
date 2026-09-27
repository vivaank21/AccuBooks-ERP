using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AccuBooks_ERP.Database;
using AccuBooks_ERP.Helper;
using Microsoft.EntityFrameworkCore;

namespace AccuBooks_ERP.GUI.Authentication
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter both username and password.");
                return;
            }

            btnLogin.IsEnabled = false;
            btnLogin.Content = "Verifying...";
            txtError.Visibility = Visibility.Collapsed;

            try
            {
                using var context = new AppDbContext();

                var user = await context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                            .ThenInclude(r => r.Permissions)
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() || (u.Email != null && u.Email.ToLower() == username.ToLower()));

                if (user == null || !SecurityHelper.VerifyPassword(password, user.PasswordHash))
                {
                    ShowError("Invalid username or password.");
                    btnLogin.IsEnabled = true;
                    btnLogin.Content = "Sign In";
                    return;
                }

                if (!user.IsActive)
                {
                    ShowError("Your account has been deactivated. Contact Admin.");
                    btnLogin.IsEnabled = true;
                    btnLogin.Content = "Sign In";
                    return;
                }

                // Update login timestamp
                user.LastLoginAt = DateTime.UtcNow;
                user.FailedLoginAttempts = 0;
                await context.SaveChangesAsync();

                // Load roles & permissions
                var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                var permissions = user.UserRoles.SelectMany(ur => ur.Role.Permissions).ToList();
                var company = await context.CompanyProfiles.FirstOrDefaultAsync();
                var fy = await context.FinancialYears.FirstOrDefaultAsync(f => f.IsCurrent);

                // Set session
                SessionManager.Instance.SetSession(user, roles, permissions, company, fy);
                LoggerService.Info($"User '{user.Username}' logged in successfully.");

                // Launch MainWindow
                var mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Error during authentication.");
                ShowError($"Login error: {ex.Message}");
                btnLogin.IsEnabled = true;
                btnLogin.Content = "Sign In";
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}
