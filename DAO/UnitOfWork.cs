using System;
using System.Threading.Tasks;
using AccuBooks_ERP.Database;
using AccuBooks_ERP.GetterNSetter;
using Microsoft.EntityFrameworkCore.Storage;

namespace AccuBooks_ERP.DAO
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        // Lazy-instantiated repositories
        public IRepository<User> Users { get; }
        public IRepository<Role> Roles { get; }
        public IRepository<UserRole> UserRoles { get; }
        public IRepository<Permission> Permissions { get; }
        public IRepository<AuditLog> AuditLogs { get; }
        public IRepository<CompanyProfile> CompanyProfiles { get; }
        public IRepository<FinancialYear> FinancialYears { get; }

        public IRepository<Customer> Customers { get; }
        public IRepository<Supplier> Suppliers { get; }
        public IRepository<Category> Categories { get; }
        public IRepository<Brand> Brands { get; }
        public IRepository<Unit> Units { get; }
        public IRepository<TaxRate> TaxRates { get; }
        public IRepository<HsnCode> HsnCodes { get; }
        public IRepository<Product> Products { get; }

        public IRepository<SalesInvoice> SalesInvoices { get; }
        public IRepository<SalesInvoiceItem> SalesInvoiceItems { get; }
        public IRepository<SalesReturn> SalesReturns { get; }
        public IRepository<Quotation> Quotations { get; }

        public IRepository<PurchaseInvoice> PurchaseInvoices { get; }
        public IRepository<PurchaseInvoiceItem> PurchaseInvoiceItems { get; }
        public IRepository<PurchaseReturn> PurchaseReturns { get; }
        public IRepository<PurchaseOrder> PurchaseOrders { get; }

        public IRepository<StockTransaction> StockTransactions { get; }
        public IRepository<StockAdjustment> StockAdjustments { get; }

        public IRepository<AccountGroup> AccountGroups { get; }
        public IRepository<AccountHead> AccountHeads { get; }
        public IRepository<JournalEntry> JournalEntries { get; }
        public IRepository<JournalEntryLine> JournalEntryLines { get; }
        public IRepository<BankAccount> BankAccounts { get; }
        public IRepository<BankTransaction> BankTransactions { get; }

        public IRepository<EInvoiceRecord> EInvoiceRecords { get; }
        public IRepository<EWayBillRecord> EWayBillRecords { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Users = new Repository<User>(_context);
            Roles = new Repository<Role>(_context);
            UserRoles = new Repository<UserRole>(_context);
            Permissions = new Repository<Permission>(_context);
            AuditLogs = new Repository<AuditLog>(_context);
            CompanyProfiles = new Repository<CompanyProfile>(_context);
            FinancialYears = new Repository<FinancialYear>(_context);

            Customers = new Repository<Customer>(_context);
            Suppliers = new Repository<Supplier>(_context);
            Categories = new Repository<Category>(_context);
            Brands = new Repository<Brand>(_context);
            Units = new Repository<Unit>(_context);
            TaxRates = new Repository<TaxRate>(_context);
            HsnCodes = new Repository<HsnCode>(_context);
            Products = new Repository<Product>(_context);

            SalesInvoices = new Repository<SalesInvoice>(_context);
            SalesInvoiceItems = new Repository<SalesInvoiceItem>(_context);
            SalesReturns = new Repository<SalesReturn>(_context);
            Quotations = new Repository<Quotation>(_context);

            PurchaseInvoices = new Repository<PurchaseInvoice>(_context);
            PurchaseInvoiceItems = new Repository<PurchaseInvoiceItem>(_context);
            PurchaseReturns = new Repository<PurchaseReturn>(_context);
            PurchaseOrders = new Repository<PurchaseOrder>(_context);

            StockTransactions = new Repository<StockTransaction>(_context);
            StockAdjustments = new Repository<StockAdjustment>(_context);

            AccountGroups = new Repository<AccountGroup>(_context);
            AccountHeads = new Repository<AccountHead>(_context);
            JournalEntries = new Repository<JournalEntry>(_context);
            JournalEntryLines = new Repository<JournalEntryLine>(_context);
            BankAccounts = new Repository<BankAccount>(_context);
            BankTransactions = new Repository<BankTransaction>(_context);

            EInvoiceRecords = new Repository<EInvoiceRecord>(_context);
            EWayBillRecords = new Repository<EWayBillRecord>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
