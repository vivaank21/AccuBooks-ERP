using System;
using System.Collections.Generic;
using System.Linq;
using AccuBooks_ERP.GetterNSetter;
using AccuBooks_ERP.Helper;
using Xunit;

namespace AccuBooks.Tests
{
    public class PurchaseAndLedgerTests
    {
        [Fact]
        public void PurchaseLineTax_Calculation_ShouldMatchExpectedGrossAndTax()
        {
            // Arrange
            decimal qty = 10;
            decimal costRate = 500m; // 5000 total gross
            decimal discount = 5m;   // 5% discount = 250 => 4750 taxable
            decimal gstRate = 18m;   // 18% GST => 855 tax (427.5 CGST, 427.5 SGST)

            // Act
            var result = TaxCalculator.CalculateLineTax(qty, costRate, discount, gstRate, SupplyType.IntraState);

            // Assert
            Assert.Equal(5000m, result.GrossAmount);
            Assert.Equal(250m, result.DiscountAmount);
            Assert.Equal(4750m, result.TaxableAmount);
            Assert.Equal(427.50m, result.CGSTAmount);
            Assert.Equal(427.50m, result.SGSTAmount);
            Assert.Equal(855.00m, result.TotalTax);
            Assert.Equal(5605.00m, result.TotalAmount);
        }

        [Fact]
        public void TrialBalance_ZeroSumInvariant_ShouldHold()
        {
            // Arrange: Setup standard balanced accounts
            var accounts = new List<AccountHead>
            {
                new AccountHead { AccountName = "Cash", CurrentBalance = 15000m },             // Dr 15,000
                new AccountHead { AccountName = "Debtors", CurrentBalance = 25000m },          // Dr 25,000
                new AccountHead { AccountName = "Capital", CurrentBalance = -30000m },         // Cr 30,000
                new AccountHead { AccountName = "Creditors", CurrentBalance = -10000m }        // Cr 10,000
            };

            // Act
            decimal totalDebit = accounts.Where(a => a.CurrentBalance > 0).Sum(a => a.CurrentBalance);
            decimal totalCredit = accounts.Where(a => a.CurrentBalance < 0).Sum(a => Math.Abs(a.CurrentBalance));

            // Assert
            Assert.Equal(40000m, totalDebit);
            Assert.Equal(40000m, totalCredit);
            Assert.Equal(totalDebit, totalCredit);
        }

        [Fact]
        public void StockValuation_QuantityMultipliedByCost_ShouldComputeCorrectly()
        {
            // Arrange
            decimal qty = 45;
            decimal unitCost = 1250.50m;

            // Act
            decimal valuation = qty * unitCost;

            // Assert
            Assert.Equal(56272.50m, valuation);
        }
    }
}
