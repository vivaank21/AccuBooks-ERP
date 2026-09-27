using System;
using System.Collections.Generic;

namespace AccuBooks_ERP.GetterNSetter
{
    public class PurchaseInvoice : BaseEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty; // Supplier's invoice or auto generated
        public string? BillNumber { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }

        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; } = null!;
        public string SupplierName { get; set; } = string.Empty;
        public string? SupplierGSTIN { get; set; }
        public string? PlaceOfSupply { get; set; }

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

        public virtual ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
    }

    public class PurchaseInvoiceItem : BaseEntity
    {
        public int PurchaseInvoiceId { get; set; }
        public virtual PurchaseInvoice PurchaseInvoice { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public string ProductName { get; set; } = string.Empty;
        public string? HsnCode { get; set; }
        public string? UnitName { get; set; }

        public decimal Quantity { get; set; } = 1;
        public decimal PurchasePrice { get; set; } = 0;
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

    public class PurchaseReturn : BaseEntity
    {
        public string ReturnNumber { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

        public int? PurchaseInvoiceId { get; set; }
        public virtual PurchaseInvoice? PurchaseInvoice { get; set; }

        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; } = null!;

        public decimal TotalRefundAmount { get; set; } = 0;
        public string? Reason { get; set; }

        public virtual ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();
    }

    public class PurchaseReturnItem : BaseEntity
    {
        public int PurchaseReturnId { get; set; }
        public virtual PurchaseReturn PurchaseReturn { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public decimal Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PurchaseOrder : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedDeliveryDate { get; set; }

        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; } = null!;

        public decimal SubTotal { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Status { get; set; } = "Pending"; // Pending, Received, Cancelled
        public string? Notes { get; set; }

        public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }

    public class PurchaseOrderItem : BaseEntity
    {
        public int PurchaseOrderId { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
