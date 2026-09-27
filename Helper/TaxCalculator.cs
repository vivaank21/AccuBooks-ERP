using System;
using AccuBooks_ERP.GetterNSetter;

namespace AccuBooks_ERP.Helper
{
    public class TaxCalculationResult
    {
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal IGSTRate { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public static class TaxCalculator
    {
        /// <summary>
        /// Calculates item line amounts, discounts, and GST breakdown based on supply type (intra-state vs inter-state).
        /// </summary>
        public static TaxCalculationResult CalculateLineTax(
            decimal quantity,
            decimal unitPrice,
            decimal discountPercent,
            decimal taxRate,
            SupplyType supplyType,
            decimal cessRate = 0,
            bool isTaxInclusive = false)
        {
            var result = new TaxCalculationResult
            {
                Quantity = quantity,
                UnitPrice = unitPrice,
                DiscountPercent = discountPercent,
                TaxRate = taxRate,
                CessRate = cessRate
            };

            decimal rawGross = quantity * unitPrice;

            if (isTaxInclusive && taxRate > 0)
            {
                // If price is tax-inclusive: Taxable = Gross / (1 + Rate/100)
                decimal totalTaxPercentage = taxRate + cessRate;
                decimal discountAmt = rawGross * (discountPercent / 100m);
                decimal netGross = rawGross - discountAmt;

                decimal taxable = Math.Round(netGross / (1m + (totalTaxPercentage / 100m)), 2, MidpointRounding.AwayFromZero);
                result.GrossAmount = rawGross;
                result.DiscountAmount = Math.Round(discountAmt, 2, MidpointRounding.AwayFromZero);
                result.TaxableAmount = taxable;
            }
            else
            {
                result.GrossAmount = Math.Round(rawGross, 2, MidpointRounding.AwayFromZero);
                result.DiscountAmount = Math.Round(rawGross * (discountPercent / 100m), 2, MidpointRounding.AwayFromZero);
                result.TaxableAmount = result.GrossAmount - result.DiscountAmount;
            }

            if (supplyType == SupplyType.IntraState)
            {
                // CGST + SGST (50% each)
                result.CGSTRate = taxRate / 2m;
                result.SGSTRate = taxRate / 2m;
                result.IGSTRate = 0;

                result.CGSTAmount = Math.Round(result.TaxableAmount * (result.CGSTRate / 100m), 2, MidpointRounding.AwayFromZero);
                result.SGSTAmount = Math.Round(result.TaxableAmount * (result.SGSTRate / 100m), 2, MidpointRounding.AwayFromZero);
                result.IGSTAmount = 0;
            }
            else
            {
                // Inter-State / Export (IGST 100%)
                result.CGSTRate = 0;
                result.SGSTRate = 0;
                result.IGSTRate = taxRate;

                result.CGSTAmount = 0;
                result.SGSTAmount = 0;
                result.IGSTAmount = Math.Round(result.TaxableAmount * (result.IGSTRate / 100m), 2, MidpointRounding.AwayFromZero);
            }

            if (cessRate > 0)
            {
                result.CessAmount = Math.Round(result.TaxableAmount * (cessRate / 100m), 2, MidpointRounding.AwayFromZero);
            }

            result.TotalTax = result.CGSTAmount + result.SGSTAmount + result.IGSTAmount + result.CessAmount;
            result.TotalAmount = result.TaxableAmount + result.TotalTax;

            return result;
        }

        public static (decimal roundedTotal, decimal roundOff) CalculateRoundOff(decimal amount)
        {
            decimal rounded = Math.Round(amount, MidpointRounding.AwayFromZero);
            decimal diff = rounded - amount;
            return (rounded, diff);
        }

        public static SupplyType DetermineSupplyType(string? companyStateCode, string? customerStateCode)
        {
            if (string.IsNullOrWhiteSpace(companyStateCode) || string.IsNullOrWhiteSpace(customerStateCode))
                return SupplyType.IntraState;

            return string.Equals(companyStateCode.Trim(), customerStateCode.Trim(), StringComparison.OrdinalIgnoreCase)
                ? SupplyType.IntraState
                : SupplyType.InterState;
        }
    }
}
