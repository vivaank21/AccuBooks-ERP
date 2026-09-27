using System;
using System.Collections.Generic;

namespace AccuBooks_ERP.GetterNSetter
{
    public class StockTransaction : BaseEntity
    {
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public StockMovementType MovementType { get; set; }
        public decimal Quantity { get; set; } // Positive for addition, negative for deduction
        public decimal PreviousStock { get; set; }
        public decimal NewStock { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public string? ReferenceType { get; set; } // e.g. "SalesInvoice", "PurchaseInvoice", "StockAdjustment"
        public string? ReferenceNumber { get; set; } // e.g. "INV-2026-0001"
        public string? Remarks { get; set; }
    }

    public class StockAdjustment : BaseEntity
    {
        public string AdjustmentNumber { get; set; } = string.Empty;
        public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;
        public string Reason { get; set; } = "Inventory Count / Correction";
        public string? Remarks { get; set; }

        public virtual ICollection<StockAdjustmentItem> Items { get; set; } = new List<StockAdjustmentItem>();
    }

    public class StockAdjustmentItem : BaseEntity
    {
        public int StockAdjustmentId { get; set; }
        public virtual StockAdjustment StockAdjustment { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public decimal PhysicalStock { get; set; }
        public decimal SystemStock { get; set; }
        public decimal DifferenceQuantity { get; set; } // Physical - System
        public decimal UnitCost { get; set; }
    }
}
