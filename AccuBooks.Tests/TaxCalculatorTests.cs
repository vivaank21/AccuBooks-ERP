using AccuBooks_ERP.GetterNSetter;
using AccuBooks_ERP.Helper;
using Xunit;

namespace AccuBooks.Tests
{
    public class TaxCalculatorTests
    {
        [Fact]
        public void CalculateLineTax_IntraState_ShouldSplitCGSTAndSGSTEqually()
        {
            // Arrange
            decimal quantity = 2;
            decimal unitPrice = 1000m;
            decimal discount = 10m; // 10%
            decimal taxRate = 18m; // 18% GST

            // Act
            var result = TaxCalculator.CalculateLineTax(
                quantity,
                unitPrice,
                discount,
                taxRate,
                SupplyType.IntraState
            );

            // Assert
            // Gross: 2000, Discount 10%: 200 => Taxable: 1800
            // CGST 9%: 162, SGST 9%: 162, IGST: 0 => Total Tax: 324 => Total: 2124
            Assert.Equal(2000m, result.GrossAmount);
            Assert.Equal(200m, result.DiscountAmount);
            Assert.Equal(1800m, result.TaxableAmount);
            Assert.Equal(9m, result.CGSTRate);
            Assert.Equal(9m, result.SGSTRate);
            Assert.Equal(162m, result.CGSTAmount);
            Assert.Equal(162m, result.SGSTAmount);
            Assert.Equal(0m, result.IGSTAmount);
            Assert.Equal(324m, result.TotalTax);
            Assert.Equal(2124m, result.TotalAmount);
        }

        [Fact]
        public void CalculateLineTax_InterState_ShouldApplyFullIGST()
        {
            // Arrange
            decimal quantity = 1;
            decimal unitPrice = 5000m;
            decimal discount = 0m;
            decimal taxRate = 18m;

            // Act
            var result = TaxCalculator.CalculateLineTax(
                quantity,
                unitPrice,
                discount,
                taxRate,
                SupplyType.InterState
            );

            // Assert
            Assert.Equal(5000m, result.TaxableAmount);
            Assert.Equal(0m, result.CGSTAmount);
            Assert.Equal(0m, result.SGSTAmount);
            Assert.Equal(18m, result.IGSTRate);
            Assert.Equal(900m, result.IGSTAmount);
            Assert.Equal(900m, result.TotalTax);
            Assert.Equal(5900m, result.TotalAmount);
        }

        [Fact]
        public void CalculateLineTax_TaxInclusive_ShouldBackCalculateTaxableCorrectly()
        {
            // Arrange: 1 item at 118 inclusive of 18% GST => Taxable should be 100, Tax 18
            decimal quantity = 1;
            decimal unitPrice = 118m;
            decimal discount = 0m;
            decimal taxRate = 18m;

            // Act
            var result = TaxCalculator.CalculateLineTax(
                quantity,
                unitPrice,
                discount,
                taxRate,
                SupplyType.IntraState,
                cessRate: 0,
                isTaxInclusive: true
            );

            // Assert
            Assert.Equal(100m, result.TaxableAmount);
            Assert.Equal(9m, result.CGSTAmount);
            Assert.Equal(9m, result.SGSTAmount);
            Assert.Equal(18m, result.TotalTax);
            Assert.Equal(118m, result.TotalAmount);
        }

        [Fact]
        public void CalculateRoundOff_ShouldRoundToNearestInteger()
        {
            // Act & Assert
            var (rounded1, diff1) = TaxCalculator.CalculateRoundOff(1234.40m);
            Assert.Equal(1234m, rounded1);
            Assert.Equal(-0.40m, diff1);

            var (rounded2, diff2) = TaxCalculator.CalculateRoundOff(1234.60m);
            Assert.Equal(1235m, rounded2);
            Assert.Equal(0.40m, diff2);
        }

        [Fact]
        public void DetermineSupplyType_SameState_ShouldReturnIntraState()
        {
            // Act & Assert
            Assert.Equal(SupplyType.IntraState, TaxCalculator.DetermineSupplyType("27", "27"));
            Assert.Equal(SupplyType.IntraState, TaxCalculator.DetermineSupplyType("Maharashtra", "Maharashtra"));
            Assert.Equal(SupplyType.InterState, TaxCalculator.DetermineSupplyType("27", "29"));
            Assert.Equal(SupplyType.InterState, TaxCalculator.DetermineSupplyType("Maharashtra", "Karnataka"));
        }
    }
}
