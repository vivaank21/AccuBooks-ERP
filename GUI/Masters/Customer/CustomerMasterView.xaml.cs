using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AccuBooks_ERP.Database;
using AccuBooks_ERP.GetterNSetter;
using AccuBooks_ERP.GUI.CustomDialogs;
using AccuBooks_ERP.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace AccuBooks_ERP.GUI.Masters.Customer
{
    public partial class CustomerMasterView : UserControl
    {
        private List<AccuBooks_ERP.GetterNSetter.Customer> _allCustomers = new();

        public CustomerMasterView()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadCustomersAsync();
        }

        public async Task LoadCustomersAsync()
        {
            try
            {
                using var context = new AppDbContext();
                _allCustomers = await context.Customers
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                FilterCustomers();
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to load customers.");
            }
        }

        private void FilterCustomers()
        {
            string query = txtSearch.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(query))
            {
                dgCustomers.ItemsSource = _allCustomers;
            }
            else
            {
                dgCustomers.ItemsSource = _allCustomers.Where(c =>
                    c.Name.ToLowerInvariant().Contains(query) ||
                    (c.CompanyName != null && c.CompanyName.ToLowerInvariant().Contains(query)) ||
                    (c.Phone != null && c.Phone.Contains(query)) ||
                    (c.GSTIN != null && c.GSTIN.ToLowerInvariant().Contains(query)) ||
                    (c.City != null && c.City.ToLowerInvariant().Contains(query))
                ).ToList();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterCustomers();
        }

        private async void BtnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CustomerEditDialog();
            if (dialog.ShowDialog() == true)
            {
                await LoadCustomersAsync();
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AccuBooks_ERP.GetterNSetter.Customer customer)
            {
                var dialog = new CustomerEditDialog(customer.Id);
                if (dialog.ShowDialog() == true)
                {
                    await LoadCustomersAsync();
                }
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AccuBooks_ERP.GetterNSetter.Customer customer)
            {
                var result = MessageBox.Show($"Are you sure you want to delete customer '{customer.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    using var context = new AppDbContext();
                    var entity = await context.Customers.FindAsync(customer.Id);
                    if (entity != null)
                    {
                        entity.IsDeleted = true;
                        await context.SaveChangesAsync();
                        await LoadCustomersAsync();
                    }
                }
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    FileName = $"Customers_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    ExcelExportImportService.SaveExportToPath(_allCustomers, sfd.FileName, "Customers");
                    MessageBox.Show("Customer records exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
