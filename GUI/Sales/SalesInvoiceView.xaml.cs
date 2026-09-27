using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AccuBooks_ERP.Database;
using AccuBooks_ERP.GetterNSetter;
using AccuBooks_ERP.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace AccuBooks_ERP.GUI.Sales
{
    public partial class SalesInvoiceView : UserControl
    {
        private readonly ObservableCollection<SalesInvoiceItem> _invoiceItems = new();
        private AccuBooks_ERP.GetterNSetter.Product? _selectedProduct;

        public SalesInvoiceView()
        {
            InitializeComponent();
            dgInvoiceItems.ItemsSource = _invoiceItems;
            dpInvoiceDate.SelectedDate = DateTime.Today;
            cmbPaymentMode.ItemsSource = Enum.GetValues(typeof(PaymentMode));
            cmbPaymentMode.SelectedItem = PaymentMode.Cash;
            // Attach event handlers after controls are initialized to avoid early invocations
            txtPlaceOfSupply.TextChanged += TxtPlaceOfSupply_TextChanged;
            Loaded += async (s, e) => await InitializeFormAsync();
        }

        private async Task InitializeFormAsync()
        {
            try
            {
                using var context = new AppDbContext();

                // Load Customers
                var customers = await context.Customers.OrderBy(c => c.Name).ToListAsync();
                cmbCustomer.ItemsSource = customers;
                if (customers.Count > 0)
                    cmbCustomer.SelectedIndex = 0;

                // Load Products
                var products = await context.Products
                    .Include(p => p.TaxRate)
                    .Include(p => p.Unit)
                    .Include(p => p.HsnCode)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                cmbProductSelect.ItemsSource = products;

                // Generate Next Invoice Number
                int count = await context.SalesInvoices.CountAsync();
                lblInvoiceNumber.Text = $"INV-{DateTime.Today:yyyyMM}-{(count + 1):D4}";
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to initialize sales invoice form.");
            }
        }

        private void CmbCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCustomer.SelectedItem is AccuBooks_ERP.GetterNSetter.Customer customer)
            {
                txtPlaceOfSupply.Text = !string.IsNullOrWhiteSpace(customer.State) ? customer.State : "Maharashtra";
                RecalculateTotals();
            }
        }

        private void CmbProductSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProductSelect.SelectedItem is AccuBooks_ERP.GetterNSetter.Product product)
            {
                _selectedProduct = product;
                txtAddRate.Text = product.SellingPrice.ToString("0.00");
                txtAddQty.Text = "1";
                txtAddDisc.Text = product.DiscountPercent.ToString("0");
            }
        }

        private async void TxtScanBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string barcode = txtScanBarcode.Text.Trim();
                if (string.IsNullOrEmpty(barcode))
                    return;

                using var context = new AppDbContext();
                var product = await context.Products
                    .Include(p => p.TaxRate)
                    .Include(p => p.Unit)
                    .Include(p => p.HsnCode)
                    .FirstOrDefaultAsync(p => p.Barcode == barcode || p.ItemCode == barcode);

                if (product != null)
                {
                    _selectedProduct = product;
                    cmbProductSelect.SelectedValue = product.Id;
                    txtAddRate.Text = product.SellingPrice.ToString("0.00");
                    txtAddQty.Text = "1";
                    txtAddDisc.Text = product.DiscountPercent.ToString("0");

                    BtnAddLine_Click(sender, e);
                    txtScanBarcode.Clear();
                }
                else
                {
                    MessageBox.Show($"No product found matching barcode '{barcode}'", "Item Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnAddLine_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProduct == null && cmbProductSelect.SelectedItem is AccuBooks_ERP.GetterNSetter.Product p)
            {
                _selectedProduct = p;
            }

            if (_selectedProduct == null)
            {
                MessageBox.Show("Please select a product first.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal.TryParse(txtAddQty.Text, out decimal qty);
            decimal.TryParse(txtAddRate.Text, out decimal rate);
            decimal.TryParse(txtAddDisc.Text, out decimal disc);

            if (qty <= 0)
                qty = 1;
            if (rate <= 0)
                rate = _selectedProduct.SellingPrice;

            var companyState = SessionManager.Instance.ActiveCompany?.State ?? "Maharashtra";
            var supplyType = TaxCalculator.DetermineSupplyType(companyState, txtPlaceOfSupply.Text);
            decimal taxRate = _selectedProduct.TaxRate?.Rate ?? 18;

            var taxResult = TaxCalculator.CalculateLineTax(qty, rate, disc, taxRate, supplyType);

            var lineItem = new SalesInvoiceItem
            {
                ProductId = _selectedProduct.Id,
                ProductName = _selectedProduct.Name,
                HsnCode = _selectedProduct.HsnCode?.Code ?? "8471",
                UnitName = _selectedProduct.Unit?.ShortName ?? "PCS",
                Quantity = qty,
                UnitPrice = rate,
                DiscountPercent = disc,
                DiscountAmount = taxResult.DiscountAmount,
                TaxableAmount = taxResult.TaxableAmount,
                TaxRate = taxRate,
                CGSTRate = taxResult.CGSTRate,
                CGSTAmount = taxResult.CGSTAmount,
                SGSTRate = taxResult.SGSTRate,
                SGSTAmount = taxResult.SGSTAmount,
                IGSTRate = taxResult.IGSTRate,
                IGSTAmount = taxResult.IGSTAmount,
                TotalTax = taxResult.TotalTax,
                TotalAmount = taxResult.TotalAmount
            };

            _invoiceItems.Add(lineItem);
            RecalculateTotals();

            // Reset selection
            cmbProductSelect.SelectedIndex = -1;
            _selectedProduct = null;
            txtAddQty.Text = "1";
            txtAddRate.Text = "0.00";
            txtAddDisc.Text = "0";
            txtScanBarcode.Focus();
        }

        private void BtnRemoveLine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is SalesInvoiceItem item)
            {
                _invoiceItems.Remove(item);
                RecalculateTotals();
            }
        }

        private void TxtPlaceOfSupply_TextChanged(object sender, TextChangedEventArgs e)
        {
            RecalculateTotals();
        }

        private void RecalculateTotals()
        {
            // Ensure UI controls are initialized before updating them
            if (lblTaxableSubTotal == null || lblTotalTax == null || lblRoundOff == null || lblGrandTotal == null)
            {
                // UI not ready yet (method may be called from events during construction)
                return;
            }

            decimal taxableTotal = _invoiceItems.Sum(i => i.TaxableAmount);
            decimal taxTotal = _invoiceItems.Sum(i => i.TotalTax);
            decimal subTotal = taxableTotal + taxTotal;

            var (grandTotal, roundOff) = TaxCalculator.CalculateRoundOff(subTotal);

            lblTaxableSubTotal.Text = $"₹ {taxableTotal:N2}";
            lblTotalTax.Text = $"₹ {taxTotal:N2}";
            lblRoundOff.Text = $"₹ {roundOff:+0.00;-0.00}";
            lblGrandTotal.Text = $"₹ {grandTotal:N2}";
        }

        private async void BtnSaveOnly_Click(object sender, RoutedEventArgs e)
        {
            await SaveInvoiceAsync(openPdf: false);
        }

        private async void BtnSaveAndPdf_Click(object sender, RoutedEventArgs e)
        {
            await SaveInvoiceAsync(openPdf: true);
        }

        private async Task SaveInvoiceAsync(bool openPdf)
        {
            if (_invoiceItems.Count == 0)
            {
                MessageBox.Show("Please add at least one line item.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var customer = cmbCustomer.SelectedItem as AccuBooks_ERP.GetterNSetter.Customer;
            if (customer == null)
            {
                MessageBox.Show("Please select a customer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new AppDbContext();

                decimal taxableTotal = _invoiceItems.Sum(i => i.TaxableAmount);
                decimal cgstTotal = _invoiceItems.Sum(i => i.CGSTAmount);
                decimal sgstTotal = _invoiceItems.Sum(i => i.SGSTAmount);
                decimal igstTotal = _invoiceItems.Sum(i => i.IGSTAmount);
                decimal taxTotal = cgstTotal + sgstTotal + igstTotal;
                var (grandTotal, roundOff) = TaxCalculator.CalculateRoundOff(taxableTotal + taxTotal);

                var paymentMode = (PaymentMode)(cmbPaymentMode.SelectedItem ?? PaymentMode.Cash);

                var invoice = new SalesInvoice
                {
                    InvoiceNumber = lblInvoiceNumber.Text,
                    InvoiceDate = dpInvoiceDate.SelectedDate?.ToUniversalTime() ?? DateTime.UtcNow,
                    CustomerId = customer.Id,
                    CustomerName = customer.Name,
                    CustomerPhone = customer.Phone,
                    CustomerGSTIN = customer.GSTIN,
                    BillingAddress = customer.BillingAddress,
                    PlaceOfSupply = txtPlaceOfSupply.Text.Trim(),
                    PaymentMode = paymentMode,
                    PaymentStatus = paymentMode == PaymentMode.Credit ? PaymentStatus.Unpaid : PaymentStatus.Paid,
                    TotalTaxableAmount = taxableTotal,
                    TotalCGST = cgstTotal,
                    TotalSGST = sgstTotal,
                    TotalIGST = igstTotal,
                    TotalTaxAmount = taxTotal,
                    RoundOff = roundOff,
                    GrandTotal = grandTotal,
                    PaidAmount = paymentMode == PaymentMode.Credit ? 0 : grandTotal,
                    DueAmount = paymentMode == PaymentMode.Credit ? grandTotal : 0,
                    Notes = txtNotes.Text.Trim(),
                    CreatedBy = SessionManager.Instance.CurrentUser?.Username ?? "admin"
                };

                foreach (var item in _invoiceItems)
                {
                    invoice.Items.Add(new SalesInvoiceItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        HsnCode = item.HsnCode,
                        UnitName = item.UnitName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountPercent = item.DiscountPercent,
                        DiscountAmount = item.DiscountAmount,
                        TaxableAmount = item.TaxableAmount,
                        TaxRate = item.TaxRate,
                        CGSTRate = item.CGSTRate,
                        CGSTAmount = item.CGSTAmount,
                        SGSTRate = item.SGSTRate,
                        SGSTAmount = item.SGSTAmount,
                        IGSTRate = item.IGSTRate,
                        IGSTAmount = item.IGSTAmount,
                        TotalTax = item.TotalTax,
                        TotalAmount = item.TotalAmount
                    });

                    // Update Stock
                    var dbProduct = await context.Products.FindAsync(item.ProductId);
                    if (dbProduct != null && dbProduct.TrackInventory)
                    {
                        var prevStock = dbProduct.CurrentStock;
                        dbProduct.CurrentStock -= item.Quantity;

                        await context.StockTransactions.AddAsync(new StockTransaction
                        {
                            ProductId = dbProduct.Id,
                            MovementType = StockMovementType.SalesOut,
                            Quantity = -item.Quantity,
                            PreviousStock = prevStock,
                            NewStock = dbProduct.CurrentStock,
                            UnitCost = item.UnitPrice,
                            ReferenceType = "SalesInvoice",
                            ReferenceNumber = invoice.InvoiceNumber,
                            TransactionDate = DateTime.UtcNow,
                            Remarks = $"Sold on {invoice.InvoiceNumber}"
                        });
                    }
                }

                // Update Customer Balance if Credit Sale
                if (paymentMode == PaymentMode.Credit)
                {
                    var dbCust = await context.Customers.FindAsync(customer.Id);
                    if (dbCust != null)
                        dbCust.CurrentBalance += grandTotal;
                }

                await context.SalesInvoices.AddAsync(invoice);
                await context.SaveChangesAsync();

                MessageBox.Show($"Invoice '{invoice.InvoiceNumber}' created and saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                if (openPdf)
                {
                    var company = await context.CompanyProfiles.FirstOrDefaultAsync();
                    var tempPdf = Path.Combine(Path.GetTempPath(), $"{invoice.InvoiceNumber}.pdf");
                    PdfReportService.SaveInvoicePdf(invoice, company, tempPdf);

                    Process.Start(new ProcessStartInfo(tempPdf) { UseShellExecute = true });
                }

                // Reset Form
                _invoiceItems.Clear();
                RecalculateTotals();
                await InitializeFormAsync();
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to save invoice.");
                MessageBox.Show($"Error saving invoice: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
