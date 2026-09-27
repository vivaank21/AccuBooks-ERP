# AccuBooks ERP - Enterprise Accounting & Inventory Suite

**AccuBooks ERP** is a production-ready, double-entry financial accounting and enterprise resource planning (ERP) system built with **C# / .NET 10 (WPF)**, **Entity Framework Core (SQLite)**, **QuestPDF**, **ClosedXML**, **Serilog**, and **CommunityToolkit.Mvvm**.

---

## 🚀 Quick Start Guide

### 1. Default Login Credentials
When you run the application for the first time, the local SQLite database is automatically created and seeded with default masters, standard tax brackets, and an initial SuperAdmin account:

- **Username**: `admin`
- **Password**: `admin123`

---

### 2. Building and Running the Application

Open a terminal (PowerShell or Command Prompt) in the project directory:

```powershell
# Build the application
dotnet build "AccuBooks ERP.csproj"

# Run the application
dotnet run --project "AccuBooks ERP.csproj"
```

---

### 3. Running Automated Unit Tests

The test suite validates security, cryptographic encryption, GST calculations, purchase tax credits, and double-entry balance invariants:

```powershell
dotnet test "AccuBooks.Tests\AccuBooks.Tests.csproj"
```

---

## 📂 Project Architecture

```
AccuBooks ERP/
├── Database/               # EF Core SQLite DbContext & automatic Seed Initializer
│   ├── AppDbContext.cs     # Database schema, query filters, relationships & indexes
│   └── DbInitializer.cs    # Seeding (SuperAdmin, Tax Brackets, Units, Chart of Accounts)
│
├── GetterNSetter/          # Domain Entities, DTOs & Business Models
│   ├── BaseEntity.cs       # Base entity with audit fields (CreatedAt/UpdatedAt/SoftDelete)
│   ├── Enums.cs            # PaymentMode, VoucherType, SupplyType, InvoiceStatus
│   ├── SecurityModels.cs   # User, Role, UserRole, Permission, AuditLog
│   ├── CompanyModels.cs    # CompanyProfile, FinancialYear
│   ├── MasterModels.cs     # Customer, Supplier, Product, Category, Brand, Unit, TaxRate, HSN
│   ├── SalesModels.cs      # SalesInvoice, SalesInvoiceItem, SalesReturn, Quotation
│   ├── PurchaseModels.cs   # PurchaseInvoice, PurchaseInvoiceItem, PurchaseReturn, PO
│   ├── InventoryModels.cs  # StockTransaction, StockAdjustment, StockAdjustmentItem
│   └── AccountingModels.cs # AccountGroup, AccountHead, JournalEntry, BankAccount
│
├── DAO/                    # Data Access Layer & Unit of Work Pattern
│   ├── IRepository.cs      # Generic async CRUD & paged filtering repository interface
│   ├── Repository.cs       # Generic EF Core repository implementation
│   ├── IUnitOfWork.cs      # Unit of Work interface managing multi-table transactions
│   └── UnitOfWork.cs       # Unit of Work implementation
│
├── Helper/                 # Utility, Security, Calculation & Document Engines
│   ├── SecurityHelper.cs   # BCrypt password hashing & AES-256 string encryption
│   ├── SessionManager.cs   # Current user session, active company, and permission store
│   ├── TaxCalculator.cs    # GST Engine (Intra-state CGST+SGST, Inter-state IGST, Round-offs)
│   ├── PdfReportService.cs # QuestPDF A4 Tax Invoice document generator
│   ├── ExcelExportImportService.cs # ClosedXML Excel export & import service
│   ├── DatabaseBackupHelper.cs    # SQLite .db.zip compressed backup & restore engine
│   └── LoggerService.cs    # Serilog structured daily rolling file logger
│
├── GUI/                    # User Interface Views & Dialogs (WPF / MVVM)
│   ├── Splash/             # Animated Startup Splash Screen with DB migration
│   ├── Authentication/     # Glassmorphic Login Window
│   ├── Dashboard/          # Executive KPI cards, sales metrics & low stock alerts
│   ├── Masters/            # Customer, Supplier & Product Catalogs with live search & Excel export
│   ├── Sales/              # POS Billing Screen with barcode scanner & instant PDF generation
│   ├── Purchase/           # Inward Vendor Bills, Input Tax Credit & Stock addition
│   ├── Inventory/          # Stock Balance Ledger & Stock Movement Audit Trail
│   ├── Accounting/         # Double-Entry Journal Vouchers, General Ledger & Trial Balance
│   ├── GST/                # GSTR-1 & GSTR-3B tax compliance summary & export
│   ├── Utilities/          # Database Backup & Disaster Recovery
│   ├── Settings/           # Company Profile, User Password update & Dynamic Theme switcher
│   └── CustomDialogs/      # Customer, Supplier & Product modal create/edit dialogs
│
├── Resources/              # Themes, Styles & UI Assets
│   ├── Themes/             # DarkTheme.xaml & LightTheme.xaml
│   └── Styles/             # Typography, Buttons, TextBoxes, DataGrid, Dialogs
│
└── AccuBooks.Tests/        # Automated xUnit Test Project
    ├── SecurityHelperTests.cs
    ├── TaxCalculatorTests.cs
    ├── DoubleEntryAccountingTests.cs
    └── PurchaseAndLedgerTests.cs
```

---

## 🧭 Complete Production Modules Guide

### 1. 📊 Executive Dashboard
- **Real-Time KPIs**: Sales totals, purchase volume, outstanding receivables, and low-stock count.
- **Recent Invoices**: Quick visual lookup of the latest 10 sales transactions.
- **Critical Low Stock Warning**: Highlights items with stock below their minimum safety thresholds.

### 2. 🛒 Point of Sale & Sales Invoicing
- **Customer Selection**: Choose from existing accounts or walk-in customers.
- **Barcode Scanner Support**: Hardware barcode gun / keyboard wedge input automatically selects products and adds them to line items.
- **Automated GST Engine**:
  - Intra-State transactions automatically calculate **CGST (50%) + SGST (50%)**.
  - Inter-State transactions compute **IGST (100%)**.
- **Stock Outward Deduction**: Decrements `Product.CurrentStock` and writes a `StockMovementType.SalesOut` audit log.
- **One-Click QuestPDF Invoicing**: Click *Save & Print PDF* to instantly generate and open an A4 Tax Invoice.

### 3. 📥 Purchase Bills & Inward Goods
- **Vendor / Supplier Selection**: Track supplier invoice numbers and payment status.
- **Input Tax Credit (ITC)**: Computes input CGST/SGST/IGST for tax deductions.
- **Stock Inward Addition**: Automatically increments `Product.CurrentStock`, updates the latest purchase price, and logs `StockMovementType.PurchaseIn`.
- **Accounts Payable**: Updates supplier balances on credit purchases.

### 4. 👥 Customer & 🏢 Vendor Masters
- Full CRUD management with instant live search and filtering.
- Track GSTIN, PAN, credit limits, opening balances, and contact addresses.
- **Excel Export**: Export complete lists with styled headers and formatting via ClosedXML.

### 5. 📦 Product & Item Catalog
- Manage SKUs, barcodes, categories, units of measurement, and multi-tier pricing (Selling Price, MRP, Purchase Cost).
- Configure low-stock alert thresholds and GST tax brackets.

### 6. 📋 Inventory & Stock Ledger
- **Stock Balance**: Shows current on-hand quantities, units, and live inventory valuations (`Qty * Cost`).
- **Audit Trail**: Complete movement history tracking purchases, sales, and reference invoice numbers.

### 7. 📑 Double-Entry Journal Vouchers
- Create manual financial adjustment vouchers across any account heads.
- **Balance Invariant Enforcement**: Ensures **Total Debits == Total Credits** before allowing posts.

### 8. 📖 General Ledger & Trial Balance
- **Account Ledger Statement**: Drilldown into any account head with date range filters to view running balances.
- **Trial Balance**: Aggregates all debit and credit balances with zero-sum total validation.

### 9. 🏛️ GST Compliance & Returns
- Outward taxable supplies breakdown for GSTR-1 and monthly GSTR-3B filings.
- One-click export to Excel.

### 10. 💾 Database Backup & Disaster Recovery
- **Create Backup**: Generates compressed, timestamped `.db.zip` archives.
- **Restore Backup**: Restores past archives with automatic safety fallback.

### 11. ⚙️ Company Configuration & Theme Switcher
- Configure display name, legal registered entity, GSTIN, PAN, bank details, and invoice terms.
- Change admin passwords with BCrypt hashing.
- **Dynamic Theme Switcher**: Toggle instantly between Dark Theme and Light Theme at runtime.

---

## 🔒 Enterprise Security Highlights

1. **BCrypt Hashing**: Work factor 11 for brute-force resistant password storage.
2. **AES-256 Symmetric Encryption**: Used for sensitive configuration storage.
3. **Transactional Integrity**: All financial transactions run within atomic EF Core database transactions.
4. **Soft Deletion**: Accidental data loss is prevented via soft-delete entity filters.
5. **Local Database**: Stored in `%LocalAppData%\AccuBooksERP\accubooks.db`.
6. **Logging**: Diagnostic logs stored in `%LocalAppData%\AccuBooksERP\Logs\accubooks-.log`.

---

## 🧪 Test Suite Summary
All 14 automated xUnit tests pass with 0 errors:
- `SecurityHelperTests` (4 tests)
- `TaxCalculatorTests` (5 tests)
- `DoubleEntryAccountingTests` (2 tests)
- `PurchaseAndLedgerTests` (3 tests)
