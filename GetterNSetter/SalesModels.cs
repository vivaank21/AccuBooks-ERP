using System;
using System.Collections.Generic;

namespace AccuBooks_ERP.GetterNSetter
{
    public class SalesInvoice : BaseEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty; // e.g., INV-2026-0001
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }

        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }
        public string CustomerName { get; set; } = "Cash Customer";
        public string? CustomerPhone { get; set; }
        public string? CustomerGSTIN { get; set; }
        public string? BillingAddress { get; set; }
        public string? ShippingAddress { get; set; }
        public string? PlaceOfSupply { get; set; } // State Code or Name

        public SupplyType SupplyType { get; set; } = SupplyType.IntraState;
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Confirmed;
        public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;

        public decimal SubTotal { get; set; } = 0;
        public decimal TotalDiscount { get; set; } = 0;
        public decimal TotalTaxableAmount { get; set; } = 0;
        public decimal TotalCGST { get; set; } = 0;
        public decimal TotalSGST { get; set; } = 0;
        public decimal TotalIGST { get; set; } = 0;
        public decimal TotalCess { get; set; } = 0;
        public decimal TotalTaxAmount { get; set; } = 0;
        public decimal RoundOff { get; set; } = 0;
        public decimal GrandTotal { get; set; } = 0;
        public decimal PaidAmount { get; set; } = 0;
        public decimal DueAmount { get; set; } = 0;

        public string? Notes { get; set; }
        public string? TermsAndConditions { get; set; }

        public virtual ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
    }

    public class SalesInvoiceItem : BaseEntity
    {
        public int SalesInvoiceId { get; set; }
        public virtual SalesInvoice SalesInvoice { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public string ProductName { get; set; } = string.Empty;
        public string? HsnCode { get; set; }
        public string? UnitName { get; set; }

        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; } = 0;
        public decimal DiscountPercent { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TaxableAmount { get; set; } = 0;

        public decimal TaxRate { get; set; } = 0;
        public decimal CGSTRate { get; set; } = 0;
        public decimal CGSTAmount { get; set; } = 0;
        public decimal SGSTRate { get; set; } = 0;
        public decimal SGSTAmount { get; set; } = 0;
        public decimal IGSTRate { get; set; } = 0;
        public decimal IGSTAmount { get; set; } = 0;
        public decimal CessAmount { get; set; } = 0;
        public decimal TotalTax { get; set; } = 0;

        public decimal TotalAmount { get; set; } = 0;
    }

    public class SalesReturn : BaseEntity
    {
        public string ReturnNumber { get; set; } = string.Empty; // e.g., SR-2026-0001
        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

        public int? SalesInvoiceId { get; set; }
        public virtual SalesInvoice? SalesInvoice { get; set; }

        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        public decimal TotalRefundAmount { get; set; } = 0;
        public string? Reason { get; set; }

        public virtual ICollection<SalesReturnItem> Items { get; set; } = new List<SalesReturnItem>();
    }

    public class SalesReturnItem : BaseEntity
    {
        public int SalesReturnId { get; set; }
        public virtual SalesReturn SalesReturn { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class Quotation : BaseEntity
    {
        public string QuotationNumber { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; } = DateTime.UtcNow;
        public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddDays(30);

        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        public decimal SubTotal { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Notes { get; set; }
        public bool IsConvertedToInvoice { get; set; } = false;

        public virtual ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
    }

    public class QuotationItem : BaseEntity
    {
        public int QuotationId { get; set; }
        public virtual Quotation Quotation { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
