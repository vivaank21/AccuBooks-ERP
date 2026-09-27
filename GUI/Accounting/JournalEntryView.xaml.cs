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

namespace AccuBooks_ERP.GUI.Accounting
{
    public class JournalLineDisplayItem
    {
        public int AccountHeadId { get; set; }
        public AccountHead AccountHead { get; set; } = null!;
        public EntryType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal? DebitDisplay => Type == EntryType.Debit ? Amount : null;
        public decimal? CreditDisplay => Type == EntryType.Credit ? Amount : null;
    }

    public partial class JournalEntryView : UserControl
    {
        private readonly ObservableCollection<JournalLineDisplayItem> _lines = new();

        public JournalEntryView()
        {
            InitializeComponent();
            dgJournalLines.ItemsSource = _lines;
            dpVoucherDate.SelectedDate = DateTime.Today;
            cmbVoucherType.ItemsSource = Enum.GetValues(typeof(VoucherType));
            cmbVoucherType.SelectedItem = VoucherType.Journal;

            Loaded += async (s, e) => await InitializeFormAsync();
        }

        private async Task InitializeFormAsync()
        {
            try
            {
                using var context = new AppDbContext();
                var accounts = await context.AccountHeads.OrderBy(a => a.AccountName).ToListAsync();
                cmbAccountHead.ItemsSource = accounts;
                if (accounts.Count > 0)
                    cmbAccountHead.SelectedIndex = 0;

                int count = await context.JournalEntries.CountAsync();
                lblVoucherNumber.Text = $"JV-{DateTime.Today:yyyyMM}-{(count + 1):D4}";
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to initialize journal form.");
            }
        }

        private void BtnAddLine_Click(object sender, RoutedEventArgs e)
        {
            if (cmbAccountHead.SelectedItem is not AccountHead account)
            {
                MessageBox.Show("Please select an Account Head.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal.TryParse(txtLineAmount.Text, out decimal amount);
            if (amount <= 0)
            {
                MessageBox.Show("Please enter a valid positive amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var type = cmbDrCr.SelectedIndex == 0 ? EntryType.Debit : EntryType.Credit;

            _lines.Add(new JournalLineDisplayItem
            {
                AccountHeadId = account.Id,
                AccountHead = account,
                Type = type,
                Amount = amount
            });

            RecalculateTotals();
            txtLineAmount.Text = "0.00";
        }

        private void BtnRemoveLine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is JournalLineDisplayItem item)
            {
                _lines.Remove(item);
                RecalculateTotals();
            }
        }

        private void RecalculateTotals()
        {
            decimal totalDebit = _lines.Where(l => l.Type == EntryType.Debit).Sum(l => l.Amount);
            decimal totalCredit = _lines.Where(l => l.Type == EntryType.Credit).Sum(l => l.Amount);
            decimal diff = Math.Abs(totalDebit - totalCredit);

            lblTotalDebit.Text = $"₹ {totalDebit:N2}";
            lblTotalCredit.Text = $"₹ {totalCredit:N2}";
            lblDifference.Text = diff == 0 ? "Balanced ✓" : $"Diff: ₹ {diff:N2}";
            lblDifference.Foreground = diff == 0 ? (System.Windows.Media.Brush)FindResource("SuccessBrush") : (System.Windows.Media.Brush)FindResource("DangerBrush");
        }

        private async void BtnPostVoucher_Click(object sender, RoutedEventArgs e)
        {
            if (_lines.Count < 2)
            {
                MessageBox.Show("A journal entry requires at least two account lines.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal totalDebit = _lines.Where(l => l.Type == EntryType.Debit).Sum(l => l.Amount);
            decimal totalCredit = _lines.Where(l => l.Type == EntryType.Credit).Sum(l => l.Amount);

            if (totalDebit != totalCredit)
            {
                MessageBox.Show($"Debit and Credit totals must be equal to post.\nDebit: ₹{totalDebit:N2}\nCredit: ₹{totalCredit:N2}\nDifference: ₹{Math.Abs(totalDebit - totalCredit):N2}", "Imbalanced Entry", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using var context = new AppDbContext();

                var voucher = new JournalEntry
                {
                    VoucherNumber = lblVoucherNumber.Text,
                    VoucherDate = dpVoucherDate.SelectedDate?.ToUniversalTime() ?? DateTime.UtcNow,
                    VoucherType = (VoucherType)(cmbVoucherType.SelectedItem ?? VoucherType.Journal),
                    TotalDebit = totalDebit,
                    TotalCredit = totalCredit,
                    Narration = txtNarration.Text.Trim(),
                    CreatedBy = SessionManager.Instance.CurrentUser?.Username ?? "admin"
                };

                foreach (var line in _lines)
                {
                    voucher.Lines.Add(new JournalEntryLine
                    {
                        AccountHeadId = line.AccountHeadId,
                        Type = line.Type,
                        Amount = line.Amount
                    });

                    // Update Account Head balance
                    var account = await context.AccountHeads.FindAsync(line.AccountHeadId);
                    if (account != null)
                    {
                        if (line.Type == EntryType.Debit)
                            account.CurrentBalance += line.Amount;
                        else
                            account.CurrentBalance -= line.Amount;
                    }
                }

                await context.JournalEntries.AddAsync(voucher);
                await context.SaveChangesAsync();

                MessageBox.Show($"Journal Voucher '{voucher.VoucherNumber}' posted successfully!", "Posted", MessageBoxButton.OK, MessageBoxImage.Information);

                _lines.Clear();
                txtNarration.Clear();
                RecalculateTotals();
                await InitializeFormAsync();
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to post journal entry.");
                MessageBox.Show($"Error posting voucher: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
