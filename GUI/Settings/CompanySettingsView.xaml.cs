using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AccuBooks_ERP.Database;
using AccuBooks_ERP.GetterNSetter;
using AccuBooks_ERP.Helper;
using Microsoft.EntityFrameworkCore;

namespace AccuBooks_ERP.GUI.Settings
{
    public partial class CompanySettingsView : UserControl
    {
        public CompanySettingsView()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadCompanyProfileAsync();
        }

        private async Task LoadCompanyProfileAsync()
        {
            try
            {
                using var context = new AppDbContext();
                var company = await context.CompanyProfiles.FirstOrDefaultAsync();
                if (company != null)
                {
                    txtCompanyName.Text = company.CompanyName;
                    txtLegalName.Text = company.LegalName;
                    txtGSTIN.Text = company.GSTIN;
                    txtPAN.Text = company.PAN;
                    txtPhone.Text = company.Phone;
                    txtEmail.Text = company.Email;
                    txtAddress.Text = company.AddressLine1;
                    txtCity.Text = company.City;
                    txtState.Text = company.State;
                    txtCurrencySymbol.Text = company.CurrencySymbol;
                    txtTerms.Text = company.TermsAndConditions;
                }
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to load company profile.");
            }
        }

        private async void BtnSaveCompany_Click(object sender, RoutedEventArgs e)
        {
            string name = txtCompanyName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Company Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new AppDbContext();
                var company = await context.CompanyProfiles.FirstOrDefaultAsync();
                if (company == null)
                {
                    company = new CompanyProfile();
                    await context.CompanyProfiles.AddAsync(company);
                }

                company.CompanyName = name;
                company.LegalName = txtLegalName.Text.Trim();
                company.GSTIN = txtGSTIN.Text.Trim().ToUpperInvariant();
                company.PAN = txtPAN.Text.Trim().ToUpperInvariant();
                company.Phone = txtPhone.Text.Trim();
                company.Email = txtEmail.Text.Trim();
                company.AddressLine1 = txtAddress.Text.Trim();
                company.City = txtCity.Text.Trim();
                company.State = txtState.Text.Trim();
                company.CurrencySymbol = txtCurrencySymbol.Text.Trim();
                company.TermsAndConditions = txtTerms.Text.Trim();

                await context.SaveChangesAsync();
                SessionManager.Instance.ActiveCompany = company;

                MessageBox.Show("Company profile updated successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to save company profile.");
                MessageBox.Show($"Error saving settings: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnUpdatePassword_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = txtCurrentPassword.Password;
            string newPassword = txtNewPassword.Password;

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Please enter both current and new password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("New password must be at least 6 characters.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var userSession = SessionManager.Instance.CurrentUser;
            if (userSession == null)
                return;

            try
            {
                using var context = new AppDbContext();
                var user = await context.Users.FindAsync(userSession.Id);
                if (user == null || !SecurityHelper.VerifyPassword(currentPassword, user.PasswordHash))
                {
                    MessageBox.Show("Current password is incorrect.", "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                user.PasswordHash = SecurityHelper.HashPassword(newPassword);
                await context.SaveChangesAsync();

                txtCurrentPassword.Clear();
                txtNewPassword.Clear();
                MessageBox.Show("Password updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to update password.");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDarkTheme_Click(object sender, RoutedEventArgs e)
        {
            ApplyTheme("DarkTheme.xaml");
        }

        private void BtnLightTheme_Click(object sender, RoutedEventArgs e)
        {
            ApplyTheme("LightTheme.xaml");
        }

        private void ApplyTheme(string themeFileName)
        {
            var appResources = Application.Current.Resources;
            var oldTheme = appResources.MergedDictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme.xaml"));
            if (oldTheme != null)
            {
                appResources.MergedDictionaries.Remove(oldTheme);
            }

            var newTheme = new ResourceDictionary
            {
                Source = new Uri($"Resources/Themes/{themeFileName}", UriKind.Relative)
            };
            appResources.MergedDictionaries.Insert(0, newTheme);
        }
    }
}
