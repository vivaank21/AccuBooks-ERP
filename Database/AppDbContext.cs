using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AccuBooks_ERP.GetterNSetter;
using Microsoft.EntityFrameworkCore;

namespace AccuBooks_ERP.Database
{
    public class AppDbContext : DbContext
    {
        public static string DbPath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AccuBooksERP",
            "accubooks.db"
        );

        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Security & Company
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
        public DbSet<FinancialYear> FinancialYears => Set<FinancialYear>();

        // Masters
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<TaxRate> TaxRates => Set<TaxRate>();
        public DbSet<HsnCode> HsnCodes => Set<HsnCode>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Product> Products => Set<Product>();

        // Sales
        public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
        public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
        public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();
        public DbSet<SalesReturnItem> SalesReturnItems => Set<SalesReturnItem>();
        public DbSet<Quotation> Quotations => Set<Quotation>();
        public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();

        // Purchase
        public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
        public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
        public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();
        public DbSet<PurchaseReturnItem> PurchaseReturnItems => Set<PurchaseReturnItem>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

        // Inventory
        public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
        public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
        public DbSet<StockAdjustmentItem> StockAdjustmentItems => Set<StockAdjustmentItem>();

        // Accounting & Banking
        public DbSet<AccountGroup> AccountGroups => Set<AccountGroup>();
        public DbSet<AccountHead> AccountHeads => Set<AccountHead>();
        public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
        public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
        public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
        public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();

        // Compliance
        public DbSet<EInvoiceRecord> EInvoiceRecords => Set<EInvoiceRecord>();
        public DbSet<EWayBillRecord> EWayBillRecords => Set<EWayBillRecord>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var directory = Path.GetDirectoryName(DbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                optionsBuilder.UseSqlite($"Data Source={DbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global soft delete query filter for entities deriving from BaseEntity
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Supplier>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Brand>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Unit>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<TaxRate>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<HsnCode>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<SalesInvoice>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<PurchaseInvoice>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<AccountHead>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<AccountGroup>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<BankAccount>().HasQueryFilter(e => !e.IsDeleted);

            // Indexes for speed & uniqueness
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.ItemCode)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Barcode);

            modelBuilder.Entity<SalesInvoice>()
                .HasIndex(s => s.InvoiceNumber)
                .IsUnique();

            modelBuilder.Entity<PurchaseInvoice>()
                .HasIndex(p => p.InvoiceNumber);

            modelBuilder.Entity<JournalEntry>()
                .HasIndex(j => j.VoucherNumber)
                .IsUnique();

            modelBuilder.Entity<AccountHead>()
                .HasIndex(a => a.AccountCode)
                .IsUnique();

            // Set delete behavior to Restrict on critical relationships to preserve audit trail
            modelBuilder.Entity<SalesInvoice>()
                .HasMany(s => s.Items)
                .WithOne(i => i.SalesInvoice)
                .HasForeignKey(i => i.SalesInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseInvoice>()
                .HasMany(p => p.Items)
                .WithOne(i => i.PurchaseInvoice)
                .HasForeignKey(i => i.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JournalEntry>()
                .HasMany(j => j.Lines)
                .WithOne(l => l.JournalEntry)
                .HasForeignKey(l => l.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is BaseEntity entity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedAt = DateTime.UtcNow;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entity.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}
