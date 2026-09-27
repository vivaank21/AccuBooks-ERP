using System;
using System.Windows;
using AccuBooks_ERP.Database;
using AccuBooks_ERP.GetterNSetter;

namespace AccuBooks_ERP.GUI.CustomDialogs
{
    public partial class SupplierEditDialog : Window
    {
        private readonly int? _supplierId;
        public bool IsSaved { get; private set; }

        public SupplierEditDialog(int? supplierId = null)
        {
            InitializeComponent();
            _supplierId = supplierId;
            Loaded += async (s, e) => await LoadSupplierDataAsync();
        }

        private async System.Threading.Tasks.Task LoadSupplierDataAsync()
        {
            if (!_supplierId.HasValue)
                return;

            lblTitle.Text = "Edit Supplier / Vendor";

            using var context = new AppDbContext();
            var supplier = await context.Suppliers.FindAsync(_supplierId.Value);
            if (supplier != null)
            {
                txtName.Text = supplier.Name;
                txtCompany.Text = supplier.CompanyName;
                txtPhone.Text = supplier.Phone;
                txtEmail.Text = supplier.Email;
                txtGSTIN.Text = supplier.GSTIN;
                txtPAN.Text = supplier.PAN;
                txtAddress.Text = supplier.Address;
                txtCity.Text = supplier.City;
                txtState.Text = supplier.State;
                txtPinCode.Text = supplier.PinCode;
                txtOpeningBalance.Text = supplier.OpeningBalance.ToString("0.00");
                txtBankDetails.Text = supplier.BankDetails;
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
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Supplier Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal.TryParse(txtOpeningBalance.Text, out decimal openingBal);

            using var context = new AppDbContext();
            Supplier supplier;

            if (_supplierId.HasValue)
            {
                supplier = await context.Suppliers.FindAsync(_supplierId.Value) ?? new Supplier();
            }
            else
            {
                supplier = new Supplier();
                await context.Suppliers.AddAsync(supplier);
            }

            supplier.Name = name;
            supplier.CompanyName = txtCompany.Text.Trim();
            supplier.Phone = txtPhone.Text.Trim();
            supplier.Email = txtEmail.Text.Trim();
            supplier.GSTIN = txtGSTIN.Text.Trim().ToUpperInvariant();
            supplier.PAN = txtPAN.Text.Trim().ToUpperInvariant();
            supplier.Address = txtAddress.Text.Trim();
            supplier.City = txtCity.Text.Trim();
            supplier.State = txtState.Text.Trim();
            supplier.PinCode = txtPinCode.Text.Trim();
            supplier.OpeningBalance = openingBal;
            if (!_supplierId.HasValue)
                supplier.CurrentBalance = openingBal;
            supplier.BankDetails = txtBankDetails.Text.Trim();

            await context.SaveChangesAsync();

            IsSaved = true;
            DialogResult = true;
            Close();
        }
    }
}
