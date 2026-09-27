using System;
using System.Windows;
using System.Windows.Threading;
using AccuBooks_ERP.GUI.Accounting;
using AccuBooks_ERP.GUI.Authentication;
using AccuBooks_ERP.GUI.Dashboard;
using AccuBooks_ERP.GUI.GST;
using AccuBooks_ERP.GUI.Inventory;
using AccuBooks_ERP.GUI.Masters.Customer;
using AccuBooks_ERP.GUI.Masters.Product;
using AccuBooks_ERP.GUI.Masters.Supplier;
using AccuBooks_ERP.GUI.Purchase;
using AccuBooks_ERP.GUI.Sales;
using AccuBooks_ERP.GUI.Settings;
using AccuBooks_ERP.GUI.Utilities;
using AccuBooks_ERP.Helper;

namespace AccuBooks_ERP
{
    public partial class MainWindow : Window
    {
        private readonly DashboardView _dashboardView = new();
        private readonly SalesInvoiceView _salesInvoiceView = new();
        private readonly PurchaseInvoiceView _purchaseInvoiceView = new();
        private readonly CustomerMasterView _customerMasterView = new();
        private readonly SupplierMasterView _supplierMasterView = new();
        private readonly ProductMasterView _productMasterView = new();
        private readonly StockView _stockView = new();
        private readonly JournalEntryView _journalEntryView = new();
        private readonly LedgerView _ledgerView = new();
        private readonly GstReportView _gstReportView = new();
        private readonly BackupRestoreView _backupRestoreView = new();
        private readonly CompanySettingsView _companySettingsView = new();

        private DispatcherTimer? _clockTimer;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Populate Session user details
            var session = SessionManager.Instance;
            if (session.CurrentUser != null)
            {
                lblUserName.Text = session.CurrentUser.FullName;
                lblUserRole.Text = string.Join(", ", session.CurrentRoles);
            }

            if (session.ActiveCompany != null)
            {
                lblActiveCompany.Text = session.ActiveCompany.CompanyName;
            }

            if (session.ActiveFinancialYear != null)
            {
                lblFinancialYear.Text = session.ActiveFinancialYear.Name;
            }

            // Set initial view
            MainContentHost.Content = _dashboardView;

            // Start system clock
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, args) =>
            {
                lblSystemClock.Text = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");
            };
            _clockTimer.Start();
        }

        private void NavDashboard_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentHost != null)
            {
                MainContentHost.Content = _dashboardView;
                _ = _dashboardView.LoadDashboardDataAsync();
            }
        }

        private void NavSales_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentHost != null)
                MainContentHost.Content = _salesInvoiceView;
        }

        private void NavPurchase_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentHost != null)
                MainContentHost.Content = _purchaseInvoiceView;
        }

        private void NavCustomers_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentHost != null)
            {
                MainContentHost.Content = _customerMasterView;
                _ = _customerMasterView.LoadCustomersAsync();
            }
        }

        private void NavSuppliers_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentHost != null)
            {
                MainContentHost.Content = _supplierMasterView;
                _ = _supplierMasterView.LoadSuppliersAsync();
            }
        }

        private void NavProducts_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentHost != null)
            {
                MainContentHost.Content = _productMasterView;
                _ = _productMasterView.LoadProductsAsync();
            }
        }

        private void NavStock_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentHost != null)
            {
                MainContentHost.Content = _stockView;
                _ = _stockView.LoadStockDataAsync();
            }
        }

        private void NavJournal_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentHost != null)
                MainContentHost.Content = _journalEntryView;
        }

        private void NavLedger_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentHost != null)
                MainContentHost.Content = _ledgerView;
        }

        private void NavGST_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentHost != null)
            {
                MainContentHost.Content = _gstReportView;
                _ = _gstReportView.LoadGstDataAsync();
            }
        }

        private void NavBackup_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentHost != null)
                MainContentHost.Content = _backupRestoreView;
        }

        private void NavSettings_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentHost != null)
                MainContentHost.Content = _companySettingsView;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show("Are you sure you want to sign out?", "Confirm Sign Out", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                SessionManager.Instance.ClearSession();
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }
    }
}