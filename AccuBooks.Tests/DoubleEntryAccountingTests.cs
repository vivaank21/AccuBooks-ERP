using System;
using System.Linq;
using AccuBooks_ERP.GetterNSetter;
using Xunit;

namespace AccuBooks.Tests
{
    public class DoubleEntryAccountingTests
    {
        [Fact]
        public void BalancedJournalVoucher_DebitEqualsCredit_ShouldBeValid()
        {
            // Arrange
            var voucher = new JournalEntry
            {
                VoucherNumber = "JV-TEST-001",
                VoucherDate = DateTime.UtcNow,
                VoucherType = VoucherType.Journal
            };

            // Cash Dr. 5000, Capital Cr. 5000
            voucher.Lines.Add(new JournalEntryLine { AccountHeadId = 1, Type = EntryType.Debit, Amount = 5000m });
            voucher.Lines.Add(new JournalEntryLine { AccountHeadId = 2, Type = EntryType.Credit, Amount = 5000m });

            // Act
            decimal totalDebit = voucher.Lines.Where(l => l.Type == EntryType.Debit).Sum(l => l.Amount);
            decimal totalCredit = voucher.Lines.Where(l => l.Type == EntryType.Credit).Sum(l => l.Amount);

            // Assert
            Assert.Equal(5000m, totalDebit);
            Assert.Equal(5000m, totalCredit);
            Assert.Equal(totalDebit, totalCredit);
        }

        [Fact]
        public void MultiLineJournalVoucher_SplitTaxes_ShouldBalanceAccurately()
        {
            // Arrange: Sales Transaction (Debtor Dr. 1180, Sales Cr. 1000, CGST Cr. 90, SGST Cr. 90)
            var voucher = new JournalEntry
            {
                VoucherNumber = "JV-SALES-001",
                VoucherType = VoucherType.Sales
            };

            voucher.Lines.Add(new JournalEntryLine { AccountHeadId = 1003, Type = EntryType.Debit, Amount = 1180m });
            voucher.Lines.Add(new JournalEntryLine { AccountHeadId = 4001, Type = EntryType.Credit, Amount = 1000m });
            voucher.Lines.Add(new JournalEntryLine { AccountHeadId = 2002, Type = EntryType.Credit, Amount = 90m });
            voucher.Lines.Add(new JournalEntryLine { AccountHeadId = 2003, Type = EntryType.Credit, Amount = 90m });

            // Act
            decimal totalDebit = voucher.Lines.Where(l => l.Type == EntryType.Debit).Sum(l => l.Amount);
            decimal totalCredit = voucher.Lines.Where(l => l.Type == EntryType.Credit).Sum(l => l.Amount);

            // Assert
            Assert.Equal(1180m, totalDebit);
            Assert.Equal(1180m, totalCredit);
            Assert.Equal(0, totalDebit - totalCredit);
        }
    }
}
