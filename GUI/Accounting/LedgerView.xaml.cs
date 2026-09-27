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

namespace AccuBooks_ERP.GUI.Accounting
{
    public class LedgerDisplayLine
    {
        public DateTime Date { get; set; }
        public string VoucherNumber { get; set; } = string.Empty;
        public string? Narration { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal RunningBalance { get; set; }
    }

    public class TrialBalanceDisplayItem
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public decimal? DebitBalance { get; set; }
        public decimal? CreditBalance { get; set; }
    }

    public partial class LedgerView : UserControl
    {
        private List<LedgerDisplayLine> _ledgerLines = new();
        private List<TrialBalanceDisplayItem> _trialBalanceItems = new();

        public LedgerView()
        {
            InitializeComponent();
            dpFromDate.SelectedDate = DateTime.Today.AddMonths(-1);
            dpToDate.SelectedDate = DateTime.Today;

            Loaded += async (s, e) => await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                using var context = new AppDbContext();
                var accounts = await context.AccountHeads
                    .Include(a => a.AccountGroup)
                    .OrderBy(a => a.AccountName)
                    .ToListAsync();

                cmbLedgerAccount.ItemsSource = accounts;
                if (accounts.Count > 0)
                    cmbLedgerAccount.SelectedIndex = 0;

                await LoadTrialBalanceAsync(accounts);
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to initialize ledger view.");
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await InitializeAsync();
        }

        private async void CmbLedgerAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadLedgerStatementAsync();
        }

        private async void BtnLoadLedger_Click(object sender, RoutedEventArgs e)
        {
            await LoadLedgerStatementAsync();
        }

        private async Task LoadLedgerStatementAsync()
        {
            if (cmbLedgerAccount.SelectedItem is not AccountHead account)
                return;

            try
            {
                using var context = new AppDbContext();

                var from = dpFromDate.SelectedDate?.ToUniversalTime() ?? DateTime.MinValue;
                var to = (dpToDate.SelectedDate?.Date.AddDays(1).AddTicks(-1))?.ToUniversalTime() ?? DateTime.MaxValue;

                var lines = await context.JournalEntryLines
                    .Include(l => l.JournalEntry)
                    .Where(l => l.AccountHeadId == account.Id && l.JournalEntry.VoucherDate >= from && l.JournalEntry.VoucherDate <= to)
                    .OrderBy(l => l.JournalEntry.VoucherDate)
                    .ToListAsync();

                _ledgerLines = new List<LedgerDisplayLine>();
                decimal running = account.OpeningBalance;

                // Opening Balance Line
                _ledgerLines.Add(new LedgerDisplayLine
                {
                    Date = from,
                    VoucherNumber = "-",
                    Narration = "Opening Balance",
                    Debit = account.OpeningBalanceType == EntryType.Debit ? account.OpeningBalance : null,
                    Credit = account.OpeningBalanceType == EntryType.Credit ? account.OpeningBalance : null,
                    RunningBalance = running
                });

                foreach (var line in lines)
                {
                    if (line.Type == EntryType.Debit)
                        running += line.Amount;
                    else
                        running -= line.Amount;

                    _ledgerLines.Add(new LedgerDisplayLine
                    {
                        Date = line.JournalEntry.VoucherDate,
                        VoucherNumber = line.JournalEntry.VoucherNumber,
                        Narration = line.Description ?? line.JournalEntry.Narration,
                        Debit = line.Type == EntryType.Debit ? line.Amount : null,
                        Credit = line.Type == EntryType.Credit ? line.Amount : null,
                        RunningBalance = running
                    });
                }

                dgLedgerLines.ItemsSource = _ledgerLines;
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to load account ledger.");
            }
        }

        private async Task LoadTrialBalanceAsync(List<AccountHead>? accounts = null)
        {
            try
            {
                using var context = new AppDbContext();
                accounts ??= await context.AccountHeads.Include(a => a.AccountGroup).OrderBy(a => a.AccountName).ToListAsync();

                _trialBalanceItems = new List<TrialBalanceDisplayItem>();
                decimal totalDebit = 0;
                decimal totalCredit = 0;

                foreach (var acc in accounts)
                {
                    decimal balance = acc.CurrentBalance;
                    decimal? dr = balance >= 0 ? balance : null;
                    decimal? cr = balance < 0 ? Math.Abs(balance) : null;

                    if (dr.HasValue) totalDebit += dr.Value;
                    if (cr.HasValue) totalCredit += cr.Value;

                    _trialBalanceItems.Add(new TrialBalanceDisplayItem
                    {
                        AccountCode = acc.AccountCode,
                        AccountName = acc.AccountName,
                        GroupName = acc.AccountGroup.Name,
                        DebitBalance = dr,
                        CreditBalance = cr
                    });
                }

                dgTrialBalance.ItemsSource = _trialBalanceItems;
                lblTrialTotalDebit.Text = $"Dr: ₹ {totalDebit:N2}";
                lblTrialTotalCredit.Text = $"Cr: ₹ {totalCredit:N2}";
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Failed to calculate Trial Balance.");
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    FileName = $"TrialBalance_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    ExcelExportImportService.SaveExportToPath(_trialBalanceItems, sfd.FileName, "Trial_Balance");
                    MessageBox.Show("Trial Balance exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
