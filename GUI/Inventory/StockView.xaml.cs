using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AccuBooks_ERP.Database;
using AccuBooks_ERP.GetterNSetter;
using AccuBooks_ERP.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace AccuBooks_ERP.GUI.Inventory
{
    public class StockItemDisplayDto
    {
        public int Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Unit? Unit { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal MinStockAlert { get; set; }
        public decimal StockValuation => PurchasePrice * CurrentStock;
    }

    public partial class StockView : UserControl
    {
        private List<StockItemDisplayDto> _stockList = new();

        public StockView()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadStockDataAsync();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadStockDataAsync();
        }

        public async Task LoadStockDataAsync()
        {
            try
            {
                using var context = new AppDbContext();

                var products = await context.Products
                    .Include(p => p.Unit)
                    .Where(p => p.TrackInventory)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                _stockList = products.Select(p => new StockItemDisplayDto
                {
                    Id = p.Id,
                    ItemCode = p.ItemCode,
                    Name = p.Name,
                    Unit = p.Unit,
                    PurchasePrice = p.PurchasePrice,
                    CurrentStock = p.CurrentStock,
                    MinStockAlert = p.MinStockAlert
                }).ToList();

                FilterStock();

                // Transactions
                var transactions = await context.StockTransactions
                    .Include(t => t.Product)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(100)
                    .ToListAsync();

                dgTransactions.ItemsSource = transactions;
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to load stock data.");
            }
        }

        private void FilterStock()
        {
            string query = txtSearch.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(query))
            {
                dgStockLevels.ItemsSource = _stockList;
            }
            else
            {
                dgStockLevels.ItemsSource = _stockList.Where(s =>
                    s.Name.ToLowerInvariant().Contains(query) ||
                    s.ItemCode.ToLowerInvariant().Contains(query)
                ).ToList();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterStock();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    FileName = $"Stock_Ledger_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    ExcelExportImportService.SaveExportToPath(_stockList, sfd.FileName, "Stock_Balance");
                    MessageBox.Show("Stock records exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
