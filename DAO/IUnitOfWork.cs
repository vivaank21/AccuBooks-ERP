using System;
using System.Threading.Tasks;
using AccuBooks_ERP.GetterNSetter;

namespace AccuBooks_ERP.DAO
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Role> Roles { get; }
        IRepository<UserRole> UserRoles { get; }
        IRepository<Permission> Permissions { get; }
        IRepository<AuditLog> AuditLogs { get; }
        IRepository<CompanyProfile> CompanyProfiles { get; }
        IRepository<FinancialYear> FinancialYears { get; }

        IRepository<Customer> Customers { get; }
        IRepository<Supplier> Suppliers { get; }
        IRepository<Category> Categories { get; }
        IRepository<Brand> Brands { get; }
        IRepository<Unit> Units { get; }
        IRepository<TaxRate> TaxRates { get; }
        IRepository<HsnCode> HsnCodes { get; }
        IRepository<Product> Products { get; }

        IRepository<SalesInvoice> SalesInvoices { get; }
        IRepository<SalesInvoiceItem> SalesInvoiceItems { get; }
        IRepository<SalesReturn> SalesReturns { get; }
        IRepository<Quotation> Quotations { get; }

        IRepository<PurchaseInvoice> PurchaseInvoices { get; }
        IRepository<PurchaseInvoiceItem> PurchaseInvoiceItems { get; }
        IRepository<PurchaseReturn> PurchaseReturns { get; }
        IRepository<PurchaseOrder> PurchaseOrders { get; }

        IRepository<StockTransaction> StockTransactions { get; }
        IRepository<StockAdjustment> StockAdjustments { get; }

        IRepository<AccountGroup> AccountGroups { get; }
        IRepository<AccountHead> AccountHeads { get; }
        IRepository<JournalEntry> JournalEntries { get; }
        IRepository<JournalEntryLine> JournalEntryLines { get; }
        IRepository<BankAccount> BankAccounts { get; }
        IRepository<BankTransaction> BankTransactions { get; }

        IRepository<EInvoiceRecord> EInvoiceRecords { get; }
        IRepository<EWayBillRecord> EWayBillRecords { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
