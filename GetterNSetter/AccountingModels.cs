using System;
using System.Collections.Generic;

namespace AccuBooks_ERP.GetterNSetter
{
    public class AccountGroup : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // e.g., "Current Assets", "Direct Expenses", "Sales Accounts"
        public AccountNature Nature { get; set; } // Asset, Liability, Equity, Income, Expense
        public int? ParentGroupId { get; set; }
        public virtual AccountGroup? ParentGroup { get; set; }
        public bool IsSystemGroup { get; set; } = false;

        public virtual ICollection<AccountGroup> SubGroups { get; set; } = new List<AccountGroup>();
        public virtual ICollection<AccountHead> AccountHeads { get; set; } = new List<AccountHead>();
    }

    public class AccountHead : BaseEntity
    {
        public string AccountCode { get; set; } = string.Empty; // e.g. "1001"
        public string AccountName { get; set; } = string.Empty; // e.g. "Cash in Hand", "HDFC Bank", "Sales Account"

        public int AccountGroupId { get; set; }
        public virtual AccountGroup AccountGroup { get; set; } = null!;

        public decimal OpeningBalance { get; set; } = 0;
        public EntryType OpeningBalanceType { get; set; } = EntryType.Debit;
        public decimal CurrentBalance { get; set; } = 0;
        public bool IsSystemAccount { get; set; } = false;
        public string? Description { get; set; }

        public virtual ICollection<JournalEntryLine> JournalLines { get; set; } = new List<JournalEntryLine>();
    }

    public class JournalEntry : BaseEntity
    {
        public string VoucherNumber { get; set; } = string.Empty; // e.g., JV-2026-0001
        public DateTime VoucherDate { get; set; } = DateTime.UtcNow;
        public VoucherType VoucherType { get; set; } = VoucherType.Journal;

        public string? ReferenceNumber { get; set; } // e.g., invoice number
        public string? ReferenceType { get; set; } // "SalesInvoice", "PurchaseInvoice", etc.

        public decimal TotalDebit { get; set; } = 0;
        public decimal TotalCredit { get; set; } = 0;
        public string? Narration { get; set; }

        public virtual ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    }

    public class JournalEntryLine : BaseEntity
    {
        public int JournalEntryId { get; set; }
        public virtual JournalEntry JournalEntry { get; set; } = null!;

        public int AccountHeadId { get; set; }
        public virtual AccountHead AccountHead { get; set; } = null!;

        public EntryType Type { get; set; } // Debit or Credit
        public decimal Amount { get; set; } = 0;
        public string? Description { get; set; }
    }

    public class BankAccount : BaseEntity
    {
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string? AccountHolderName { get; set; }
        public string? IFSC { get; set; }
        public string? Branch { get; set; }
        public decimal OpeningBalance { get; set; } = 0;
        public decimal CurrentBalance { get; set; } = 0;

        public int? LinkedAccountHeadId { get; set; }
        public virtual AccountHead? LinkedAccountHead { get; set; }

        public virtual ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
    }

    public class BankTransaction : BaseEntity
    {
        public int BankAccountId { get; set; }
        public virtual BankAccount BankAccount { get; set; } = null!;

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public EntryType Type { get; set; } // Debit (Deposit) or Credit (Withdrawal)
        public decimal Amount { get; set; } = 0;
        public string? ReferenceNumber { get; set; } // Cheque/UTR/UPI Ref
        public string? Description { get; set; }
        public bool IsReconciled { get; set; } = false;
        public DateTime? ReconciledDate { get; set; }
    }

    public class EInvoiceRecord : BaseEntity
    {
        public int SalesInvoiceId { get; set; }
        public virtual SalesInvoice SalesInvoice { get; set; } = null!;

        public string? IRN { get; set; } // Invoice Reference Number (64 chars hash)
        public string? AckNo { get; set; }
        public DateTime? AckDate { get; set; }
        public string? SignedQRCode { get; set; }
        public string? SignedInvoice { get; set; }
        public string Status { get; set; } = "Generated"; // Generated, Cancelled, Failed
        public string? ErrorMessage { get; set; }
    }

    public class EWayBillRecord : BaseEntity
    {
        public int SalesInvoiceId { get; set; }
        public virtual SalesInvoice SalesInvoice { get; set; } = null!;

        public string? EWayBillNumber { get; set; }
        public DateTime? EWayBillDate { get; set; }
        public DateTime? ValidUpto { get; set; }
        public string? TransporterId { get; set; }
        public string? TransporterName { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DistanceKm { get; set; }
        public string Status { get; set; } = "Active";
    }
}
