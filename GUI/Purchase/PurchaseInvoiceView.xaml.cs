using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AccuBooks_ERP.Database;
using AccuBooks_ERP.GetterNSetter;
using AccuBooks_ERP.Helper;
using Microsoft.EntityFrameworkCore;

namespace AccuBooks_ERP.GUI.Purchase
{
    public partial class PurchaseInvoiceView : UserControl
    {
        private readonly ObservableCollection<PurchaseInvoiceItem> _purchaseItems = new();
        private AccuBooks_ERP.GetterNSetter.Product? _selectedProduct;

        public PurchaseInvoiceView()
        {
            InitializeComponent();
            dgPurchaseItems.ItemsSource = _purchaseItems;
            dpInvoiceDate.SelectedDate = DateTime.Today;
            cmbPaymentMode.ItemsSource = Enum.GetValues(typeof(PaymentMode));
            cmbPaymentMode.SelectedItem = PaymentMode.Cash;

            Loaded += async (s, e) => await InitializeFormAsync();
        }

        private async Task InitializeFormAsync()
        {
            try
            {
                using var context = new AppDbContext();

                var suppliers = await context.Suppliers.OrderBy(s => s.Name).ToListAsync();
                cmbSupplier.ItemsSource = suppliers;
                if (suppliers.Count > 0)
                    cmbSupplier.SelectedIndex = 0;

                var products = await context.Products
                    .Include(p => p.TaxRate)
                    .Include(p => p.Unit)
                    .Include(p => p.HsnCode)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                cmbProductSelect.ItemsSource = products;

                int count = await context.PurchaseInvoices.CountAsync();
                lblPurchaseInvoiceNumber.Text = $"PUR-{DateTime.Today:yyyyMM}-{(count + 1):D4}";
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to initialize purchase invoice form.");
            }
        }

        private void CmbSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecalculateTotals();
        }

        private void CmbProductSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProductSelect.SelectedItem is AccuBooks_ERP.GetterNSetter.Product product)
            {
                _selectedProduct = product;
                txtAddRate.Text = product.PurchasePrice.ToString("0.00");
                txtAddQty.Text = "1";
                txtAddDisc.Text = "0";
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
                MessageBox.Show("Please select an inward product first.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal.TryParse(txtAddQty.Text, out decimal qty);
            decimal.TryParse(txtAddRate.Text, out decimal rate);
            decimal.TryParse(txtAddDisc.Text, out decimal disc);

            if (qty <= 0)
                qty = 1;
            if (rate <= 0)
                rate = _selectedProduct.PurchasePrice;

            var supplier = cmbSupplier.SelectedItem as AccuBooks_ERP.GetterNSetter.Supplier;
            var companyState = SessionManager.Instance.ActiveCompany?.State ?? "Maharashtra";
            var supplyType = TaxCalculator.DetermineSupplyType(companyState, supplier?.State);
            decimal taxRate = _selectedProduct.TaxRate?.Rate ?? 18;

            var taxResult = TaxCalculator.CalculateLineTax(qty, rate, disc, taxRate, supplyType);

            var lineItem = new PurchaseInvoiceItem
            {
                ProductId = _selectedProduct.Id,
                ProductName = _selectedProduct.Name,
                HsnCode = _selectedProduct.HsnCode?.Code ?? "8471",
                UnitName = _selectedProduct.Unit?.ShortName ?? "PCS",
                Quantity = qty,
                PurchasePrice = rate,
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

            _purchaseItems.Add(lineItem);
            RecalculateTotals();

            cmbProductSelect.SelectedIndex = -1;
            _selectedProduct = null;
            txtAddQty.Text = "1";
            txtAddRate.Text = "0.00";
            txtAddDisc.Text = "0";
        }

        private void BtnRemoveLine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is PurchaseInvoiceItem item)
            {
                _purchaseItems.Remove(item);
                RecalculateTotals();
            }
        }

        private void RecalculateTotals()
        {
            decimal taxableTotal = _purchaseItems.Sum(i => i.TaxableAmount);
            decimal taxTotal = _purchaseItems.Sum(i => i.TotalTax);
            decimal subTotal = taxableTotal + taxTotal;

            var (grandTotal, roundOff) = TaxCalculator.CalculateRoundOff(subTotal);

            lblTaxableSubTotal.Text = $"₹ {taxableTotal:N2}";
            lblTotalTax.Text = $"₹ {taxTotal:N2}";
            lblRoundOff.Text = $"₹ {roundOff:+0.00;-0.00}";
            lblGrandTotal.Text = $"₹ {grandTotal:N2}";
        }

        private async void BtnSavePurchase_Click(object sender, RoutedEventArgs e)
        {
            if (_purchaseItems.Count == 0)
            {
                MessageBox.Show("Please add at least one line item.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var supplier = cmbSupplier.SelectedItem as AccuBooks_ERP.GetterNSetter.Supplier;
            if (supplier == null)
            {
                MessageBox.Show("Please select a supplier.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new AppDbContext();

                decimal taxableTotal = _purchaseItems.Sum(i => i.TaxableAmount);
                decimal cgstTotal = _purchaseItems.Sum(i => i.CGSTAmount);
                decimal sgstTotal = _purchaseItems.Sum(i => i.SGSTAmount);
                decimal igstTotal = _purchaseItems.Sum(i => i.IGSTAmount);
                decimal taxTotal = cgstTotal + sgstTotal + igstTotal;
                var (grandTotal, roundOff) = TaxCalculator.CalculateRoundOff(taxableTotal + taxTotal);

                var paymentMode = (PaymentMode)(cmbPaymentMode.SelectedItem ?? PaymentMode.Cash);

                var purchaseInvoice = new PurchaseInvoice
                {
                    InvoiceNumber = lblPurchaseInvoiceNumber.Text,
                    BillNumber = txtVendorBillNo.Text.Trim(),
                    InvoiceDate = dpInvoiceDate.SelectedDate?.ToUniversalTime() ?? DateTime.UtcNow,
                    SupplierId = supplier.Id,
                    SupplierName = supplier.Name,
                    SupplierGSTIN = supplier.GSTIN,
                    PlaceOfSupply = supplier.State ?? "Maharashtra",
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

                foreach (var item in _purchaseItems)
                {
                    purchaseInvoice.Items.Add(new PurchaseInvoiceItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        HsnCode = item.HsnCode,
                        UnitName = item.UnitName,
                        Quantity = item.Quantity,
                        PurchasePrice = item.PurchasePrice,
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

                    // Add Stock Inward
                    var dbProduct = await context.Products.FindAsync(item.ProductId);
                    if (dbProduct != null)
                    {
                        var prevStock = dbProduct.CurrentStock;
                        dbProduct.CurrentStock += item.Quantity;
                        dbProduct.PurchasePrice = item.PurchasePrice; // Update latest purchase cost

                        await context.StockTransactions.AddAsync(new StockTransaction
                        {
                            ProductId = dbProduct.Id,
                            MovementType = StockMovementType.PurchaseIn,
                            Quantity = item.Quantity,
                            PreviousStock = prevStock,
                            NewStock = dbProduct.CurrentStock,
                            UnitCost = item.PurchasePrice,
                            ReferenceType = "PurchaseInvoice",
                            ReferenceNumber = purchaseInvoice.InvoiceNumber,
                            TransactionDate = DateTime.UtcNow,
                            Remarks = $"Purchased on {purchaseInvoice.InvoiceNumber}"
                        });
                    }
                }

                // Update Supplier Balance if Credit
                if (paymentMode == PaymentMode.Credit)
                {
                    var dbSup = await context.Suppliers.FindAsync(supplier.Id);
                    if (dbSup != null)
                        dbSup.CurrentBalance += grandTotal;
                }

                await context.PurchaseInvoices.AddAsync(purchaseInvoice);
                await context.SaveChangesAsync();

                MessageBox.Show($"Purchase Bill '{purchaseInvoice.InvoiceNumber}' recorded and stock updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                _purchaseItems.Clear();
                RecalculateTotals();
                await InitializeFormAsync();
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to save purchase bill.");
                MessageBox.Show($"Error recording purchase: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
