using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AccuBooks_ERP.Database;
using AccuBooks_ERP.GetterNSetter;
using Microsoft.EntityFrameworkCore;

namespace AccuBooks_ERP.GUI.CustomDialogs
{
    public partial class ProductEditDialog : Window
    {
        private readonly int? _productId;
        public bool IsSaved { get; private set; }

        public ProductEditDialog(int? productId = null)
        {
            InitializeComponent();
            _productId = productId;
            Loaded += async (s, e) => await InitializeDropdownsAndDataAsync();
        }

        private async Task InitializeDropdownsAndDataAsync()
        {
            using var context = new AppDbContext();

            var categories = await context.Categories.OrderBy(c => c.Name).ToListAsync();
            cmbCategory.ItemsSource = categories;

            var units = await context.Units.OrderBy(u => u.Name).ToListAsync();
            cmbUnit.ItemsSource = units;
            if (units.Count > 0)
                cmbUnit.SelectedIndex = 0;

            var taxRates = await context.TaxRates.OrderBy(t => t.Rate).ToListAsync();
            cmbTaxRate.ItemsSource = taxRates;
            var defaultTax = taxRates.FirstOrDefault(t => t.IsDefault) ?? taxRates.FirstOrDefault();
            if (defaultTax != null)
                cmbTaxRate.SelectedValue = defaultTax.Id;

            if (_productId.HasValue)
            {
                lblTitle.Text = "Edit Product";
                var product = await context.Products.FindAsync(_productId.Value);
                if (product != null)
                {
                    txtName.Text = product.Name;
                    txtItemCode.Text = product.ItemCode;
                    txtBarcode.Text = product.Barcode;
                    cmbCategory.SelectedValue = product.CategoryId;
                    cmbUnit.SelectedValue = product.UnitId;
                    cmbTaxRate.SelectedValue = product.TaxRateId;
                    txtPurchasePrice.Text = product.PurchasePrice.ToString("0.00");
                    txtSellingPrice.Text = product.SellingPrice.ToString("0.00");
                    txtMRP.Text = product.MRP.ToString("0.00");
                    txtOpeningStock.Text = product.OpeningStock.ToString("0");
                    txtMinAlert.Text = product.MinStockAlert.ToString("0");
                }
            }
            else
            {
                // Auto-generate code
                txtItemCode.Text = $"PRD-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            string code = txtItemCode.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Product Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Item Code / SKU is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal.TryParse(txtPurchasePrice.Text, out decimal purchasePrice);
            decimal.TryParse(txtSellingPrice.Text, out decimal sellingPrice);
            decimal.TryParse(txtMRP.Text, out decimal mrp);
            decimal.TryParse(txtOpeningStock.Text, out decimal openingStock);
            decimal.TryParse(txtMinAlert.Text, out decimal minAlert);

            using var context = new AppDbContext();
            Product product;

            if (_productId.HasValue)
            {
                product = await context.Products.FindAsync(_productId.Value) ?? new Product();
            }
            else
            {
                product = new Product();
                await context.Products.AddAsync(product);
            }

            product.Name = name;
            product.ItemCode = code;
            product.Barcode = txtBarcode.Text.Trim();
            product.CategoryId = (int?)cmbCategory.SelectedValue;
            product.UnitId = (int?)cmbUnit.SelectedValue;
            product.TaxRateId = (int?)cmbTaxRate.SelectedValue;
            product.PurchasePrice = purchasePrice;
            product.SellingPrice = sellingPrice;
            product.MRP = mrp > 0 ? mrp : sellingPrice;
            product.OpeningStock = openingStock;
            if (!_productId.HasValue)
                product.CurrentStock = openingStock;
            product.MinStockAlert = minAlert;
            product.TrackInventory = true;

            await context.SaveChangesAsync();

            IsSaved = true;
            DialogResult = true;
            Close();
        }
    }
}
