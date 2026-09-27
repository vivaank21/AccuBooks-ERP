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

namespace AccuBooks_ERP.GUI.Masters.Product
{
    public partial class ProductMasterView : UserControl
    {
        private List<AccuBooks_ERP.GetterNSetter.Product> _allProducts = new();

        public ProductMasterView()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadProductsAsync();
        }

        public async Task LoadProductsAsync()
        {
            try
            {
                using var context = new AppDbContext();
                _allProducts = await context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Unit)
                    .Include(p => p.TaxRate)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                FilterProducts();
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to load products.");
            }
        }

        private void FilterProducts()
        {
            string query = txtSearch.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(query))
            {
                dgProducts.ItemsSource = _allProducts;
            }
            else
            {
                dgProducts.ItemsSource = _allProducts.Where(p =>
                    p.Name.ToLowerInvariant().Contains(query) ||
                    p.ItemCode.ToLowerInvariant().Contains(query) ||
                    (p.Barcode != null && p.Barcode.Contains(query)) ||
                    (p.Category != null && p.Category.Name.ToLowerInvariant().Contains(query))
                ).ToList();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterProducts();
        }

        private async void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProductEditDialog();
            if (dialog.ShowDialog() == true)
            {
                await LoadProductsAsync();
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AccuBooks_ERP.GetterNSetter.Product product)
            {
                var dialog = new ProductEditDialog(product.Id);
                if (dialog.ShowDialog() == true)
                {
                    await LoadProductsAsync();
                }
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AccuBooks_ERP.GetterNSetter.Product product)
            {
                var result = MessageBox.Show($"Are you sure you want to delete product '{product.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    using var context = new AppDbContext();
                    var entity = await context.Products.FindAsync(product.Id);
                    if (entity != null)
                    {
                        entity.IsDeleted = true;
                        await context.SaveChangesAsync();
                        await LoadProductsAsync();
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
                    FileName = $"Products_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    ExcelExportImportService.SaveExportToPath(_allProducts, sfd.FileName, "Products");
                    MessageBox.Show("Product catalog exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
