using System;
using System.Windows;
using AccuBooks_ERP.Database;
using AccuBooks_ERP.GetterNSetter;

namespace AccuBooks_ERP.GUI.CustomDialogs
{
    public partial class CustomerEditDialog : Window
    {
        private readonly int? _customerId;
        public bool IsSaved { get; private set; }

        public CustomerEditDialog(int? customerId = null)
        {
            InitializeComponent();
            _customerId = customerId;
            Loaded += async (s, e) => await LoadCustomerDataAsync();
        }

        private async System.Threading.Tasks.Task LoadCustomerDataAsync()
        {
            if (!_customerId.HasValue)
                return;

            lblTitle.Text = "Edit Customer";

            using var context = new AppDbContext();
            var customer = await context.Customers.FindAsync(_customerId.Value);
            if (customer != null)
            {
                txtName.Text = customer.Name;
                txtCompany.Text = customer.CompanyName;
                txtPhone.Text = customer.Phone;
                txtEmail.Text = customer.Email;
                txtGSTIN.Text = customer.GSTIN;
                txtPAN.Text = customer.PAN;
                txtAddress.Text = customer.BillingAddress;
                txtCity.Text = customer.City;
                txtState.Text = customer.State;
                txtPinCode.Text = customer.PinCode;
                txtOpeningBalance.Text = customer.OpeningBalance.ToString("0.00");
                txtCreditLimit.Text = customer.CreditLimit.ToString("0.00");
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
                MessageBox.Show("Customer Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal.TryParse(txtOpeningBalance.Text, out decimal openingBal);
            decimal.TryParse(txtCreditLimit.Text, out decimal creditLim);

            using var context = new AppDbContext();
            Customer customer;

            if (_customerId.HasValue)
            {
                customer = await context.Customers.FindAsync(_customerId.Value) ?? new Customer();
            }
            else
            {
                customer = new Customer();
                await context.Customers.AddAsync(customer);
            }

            customer.Name = name;
            customer.CompanyName = txtCompany.Text.Trim();
            customer.Phone = txtPhone.Text.Trim();
            customer.Email = txtEmail.Text.Trim();
            customer.GSTIN = txtGSTIN.Text.Trim().ToUpperInvariant();
            customer.PAN = txtPAN.Text.Trim().ToUpperInvariant();
            customer.BillingAddress = txtAddress.Text.Trim();
            customer.City = txtCity.Text.Trim();
            customer.State = txtState.Text.Trim();
            customer.PinCode = txtPinCode.Text.Trim();
            customer.OpeningBalance = openingBal;
            if (!_customerId.HasValue)
                customer.CurrentBalance = openingBal;
            customer.CreditLimit = creditLim;

            await context.SaveChangesAsync();

            IsSaved = true;
            DialogResult = true;
            Close();
        }
    }
}
