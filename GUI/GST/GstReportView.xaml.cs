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

namespace AccuBooks_ERP.GUI.GST
{
    public partial class GstReportView : UserControl
    {
        private List<SalesInvoice> _invoices = new();

        public GstReportView()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadGstDataAsync();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadGstDataAsync();
        }

        public async Task LoadGstDataAsync()
        {
            try
            {
                using var context = new AppDbContext();

                _invoices = await context.SalesInvoices
                    .OrderByDescending(s => s.InvoiceDate)
                    .ToListAsync();

                decimal totalTaxable = _invoices.Sum(i => i.TotalTaxableAmount);
                decimal totalCgst = _invoices.Sum(i => i.TotalCGST);
                decimal totalSgst = _invoices.Sum(i => i.TotalSGST);
                decimal totalIgst = _invoices.Sum(i => i.TotalIGST);

                lblTaxableTotal.Text = $"₹ {totalTaxable:N2}";
                lblOutputCgst.Text = $"₹ {totalCgst:N2}";
                lblOutputSgst.Text = $"₹ {totalSgst:N2}";
                lblOutputIgst.Text = $"₹ {totalIgst:N2}";

                dgGstInvoices.ItemsSource = _invoices;
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to load GST reports.");
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    FileName = $"GSTR1_Report_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    ExcelExportImportService.SaveExportToPath(_invoices, sfd.FileName, "GSTR1_Summary");
                    MessageBox.Show("GSTR report exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
