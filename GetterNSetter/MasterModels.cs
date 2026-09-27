using System;
using System.Collections.Generic;

namespace AccuBooks_ERP.GetterNSetter
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public class Brand : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public class Unit : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // e.g. "Piece", "Kilogram"
        public string ShortName { get; set; } = string.Empty; // e.g. "PCS", "KGS"
        public bool AllowDecimals { get; set; } = false;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public class TaxRate : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // e.g., "GST 18%", "GST 5%"
        public decimal Rate { get; set; } // e.g. 18.00
        public decimal CGSTRate { get; set; } // e.g. 9.00
        public decimal SGSTRate { get; set; } // e.g. 9.00
        public decimal IGSTRate { get; set; } // e.g. 18.00
        public decimal CessRate { get; set; } = 0; // e.g. 0.00
        public bool IsDefault { get; set; } = false;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public class HsnCode : BaseEntity
    {
        public string Code { get; set; } = string.Empty; // e.g. "8471"
        public string? Description { get; set; }
        public decimal DefaultGstRate { get; set; } = 18.00m;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public class Customer : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? GSTIN { get; set; }
        public string? PAN { get; set; }
        public string? BillingAddress { get; set; }
        public string? ShippingAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? StateCode { get; set; }
        public string? PinCode { get; set; }
        public decimal OpeningBalance { get; set; } = 0;
        public decimal CurrentBalance { get; set; } = 0;
        public decimal CreditLimit { get; set; } = 0;
        public int CreditDays { get; set; } = 0;
        public string? Notes { get; set; }

        public virtual ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
    }

    public class Supplier : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? GSTIN { get; set; }
        public string? PAN { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? StateCode { get; set; }
        public string? PinCode { get; set; }
        public decimal OpeningBalance { get; set; } = 0;
        public decimal CurrentBalance { get; set; } = 0;
        public string? BankDetails { get; set; }
        public string? Notes { get; set; }

        public virtual ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();
    }

    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty; // SKU / Code
        public string? Barcode { get; set; }
        public string? Description { get; set; }

        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public int? BrandId { get; set; }
        public virtual Brand? Brand { get; set; }

        public int? UnitId { get; set; }
        public virtual Unit? Unit { get; set; }

        public int? HsnCodeId { get; set; }
        public virtual HsnCode? HsnCode { get; set; }

        public int? TaxRateId { get; set; }
        public virtual TaxRate? TaxRate { get; set; }

        public decimal PurchasePrice { get; set; } = 0;
        public decimal SellingPrice { get; set; } = 0;
        public decimal MRP { get; set; } = 0;
        public decimal WholesalePrice { get; set; } = 0;
        public decimal DiscountPercent { get; set; } = 0;

        public decimal OpeningStock { get; set; } = 0;
        public decimal CurrentStock { get; set; } = 0;
        public decimal MinStockAlert { get; set; } = 5;
        public decimal MaxStockLimit { get; set; } = 1000;

        public bool IsTaxInclusive { get; set; } = false;
        public bool TrackInventory { get; set; } = true;
        public string? ImagePath { get; set; }
    }
}
