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

namespace AccuBooks_ERP.GUI.Masters.Supplier
{
    public partial class SupplierMasterView : UserControl
    {
        private List<AccuBooks_ERP.GetterNSetter.Supplier> _allSuppliers = new();

        public SupplierMasterView()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadSuppliersAsync();
        }

        public async Task LoadSuppliersAsync()
        {
            try
            {
                using var context = new AppDbContext();
                _allSuppliers = await context.Suppliers
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                FilterSuppliers();
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to load suppliers.");
            }
        }

        private void FilterSuppliers()
        {
            string query = txtSearch.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(query))
            {
                dgSuppliers.ItemsSource = _allSuppliers;
            }
            else
            {
                dgSuppliers.ItemsSource = _allSuppliers.Where(s =>
                    s.Name.ToLowerInvariant().Contains(query) ||
                    (s.CompanyName != null && s.CompanyName.ToLowerInvariant().Contains(query)) ||
                    (s.Phone != null && s.Phone.Contains(query)) ||
                    (s.GSTIN != null && s.GSTIN.ToLowerInvariant().Contains(query)) ||
                    (s.City != null && s.City.ToLowerInvariant().Contains(query))
                ).ToList();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterSuppliers();
        }

        private async void BtnAddSupplier_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SupplierEditDialog();
            if (dialog.ShowDialog() == true)
            {
                await LoadSuppliersAsync();
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AccuBooks_ERP.GetterNSetter.Supplier supplier)
            {
                var dialog = new SupplierEditDialog(supplier.Id);
                if (dialog.ShowDialog() == true)
                {
                    await LoadSuppliersAsync();
                }
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AccuBooks_ERP.GetterNSetter.Supplier supplier)
            {
                var result = MessageBox.Show($"Are you sure you want to delete supplier '{supplier.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    using var context = new AppDbContext();
                    var entity = await context.Suppliers.FindAsync(supplier.Id);
                    if (entity != null)
                    {
                        entity.IsDeleted = true;
                        await context.SaveChangesAsync();
                        await LoadSuppliersAsync();
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
                    FileName = $"Suppliers_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    ExcelExportImportService.SaveExportToPath(_allSuppliers, sfd.FileName, "Suppliers");
                    MessageBox.Show("Supplier records exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
