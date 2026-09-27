using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AccuBooks_ERP.Database;
using AccuBooks_ERP.Helper;
using Microsoft.EntityFrameworkCore;

namespace AccuBooks_ERP.GUI.Dashboard
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadDashboardDataAsync();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDashboardDataAsync();
        }

        public async Task LoadDashboardDataAsync()
        {
            try
            {
                using var context = new AppDbContext();

                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);

                // 1. Sales Metric
                var monthlySales = await context.SalesInvoices
                    .Where(s => s.InvoiceDate >= startOfMonth)
                    .ToListAsync();

                decimal totalSalesAmount = monthlySales.Sum(s => s.GrandTotal);
                int totalSalesCount = monthlySales.Count;

                // 2. Purchases Metric
                var totalPurchasesAmount = await context.PurchaseInvoices
                    .Where(p => p.InvoiceDate >= startOfMonth)
                    .SumAsync(p => (decimal?)p.GrandTotal) ?? 0;
                int totalPurchaseCount = await context.PurchaseInvoices
                    .Where(p => p.InvoiceDate >= startOfMonth)
                    .CountAsync();

                // 3. Accounts Receivable
                var totalReceivables = await context.Customers
                    .SumAsync(c => (decimal?)c.CurrentBalance) ?? 0;

                // 4. Low Stock Products
                var lowStockProducts = await context.Products
                    .Where(p => p.TrackInventory && p.CurrentStock <= p.MinStockAlert)
                    .Take(15)
                    .ToListAsync();

                // 5. Recent Invoices
                var recentInvoices = await context.SalesInvoices
                    .OrderByDescending(s => s.InvoiceDate)
                    .Take(10)
                    .ToListAsync();

                // Update UI
                string currency = SessionManager.Instance.ActiveCompany?.CurrencySymbol ?? "₹";
                lblTotalSales.Text = $"{currency} {totalSalesAmount:N2}";
                lblSalesCount.Text = $"{totalSalesCount} Invoices This Month";

                lblTotalPurchases.Text = $"{currency} {totalPurchasesAmount:N2}";
                lblPurchaseCount.Text = $"{totalPurchaseCount} Bills Recorded";

                lblReceivables.Text = $"{currency} {totalReceivables:N2}";

                lblLowStockCount.Text = lowStockProducts.Count.ToString();

                dgRecentInvoices.ItemsSource = recentInvoices;
                dgLowStock.ItemsSource = lowStockProducts;
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to load dashboard metrics.");
            }
        }
    }
}
