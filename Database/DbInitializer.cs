using System;
using System.Linq;
using System.Threading.Tasks;
using AccuBooks_ERP.GetterNSetter;
using Microsoft.EntityFrameworkCore;

namespace AccuBooks_ERP.Database
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Roles
            if (!await context.Roles.AnyAsync())
            {
                var adminRole = new Role { Name = "Admin", Description = "Full system administration access" };
                var managerRole = new Role { Name = "Manager", Description = "Operations and reports manager" };
                var accountantRole = new Role { Name = "Accountant", Description = "Financial accounting, vouchers and tax" };
                var cashierRole = new Role { Name = "SalesCashier", Description = "POS and sales billing access" };
                var clerkRole = new Role { Name = "InventoryClerk", Description = "Stock receipts, dispatch and adjustments" };

                await context.Roles.AddRangeAsync(adminRole, managerRole, accountantRole, cashierRole, clerkRole);
                await context.SaveChangesAsync();
            }

            // 2. Seed Default Admin User (admin / admin123)
            if (!await context.Users.AnyAsync())
            {
                var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 11);

                var adminUser = new User
                {
                    Username = "admin",
                    PasswordHash = passwordHash,
                    FullName = "System Administrator",
                    Email = "admin@accubooks.local",
                    IsActive = true
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();

                var userRole = new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                };
                await context.UserRoles.AddAsync(userRole);
                await context.SaveChangesAsync();
            }

            // 3. Seed Company Profile
            if (!await context.CompanyProfiles.AnyAsync())
            {
                var company = new CompanyProfile
                {
                    CompanyName = "AccuBooks Enterprise Pvt Ltd",
                    LegalName = "AccuBooks Enterprise Private Limited",
                    GSTIN = "27AABCA1234F1Z5",
                    PAN = "AABCA1234F",
                    Email = "contact@accubooks.local",
                    Phone = "+91 9876543210",
                    AddressLine1 = "Suite 100, Tech Park",
                    City = "Mumbai",
                    State = "Maharashtra",
                    StateCode = "27",
                    PinCode = "400001",
                    Country = "India",
                    CurrencySymbol = "₹",
                    CurrencyCode = "INR",
                    TermsAndConditions = "1. Goods once sold will not be taken back without valid invoice.\n2. Payment due within specified credit terms."
                };
                await context.CompanyProfiles.AddAsync(company);
            }

            // 4. Seed Current Financial Year
            if (!await context.FinancialYears.AnyAsync())
            {
                var currentYear = DateTime.UtcNow.Year;
                var fy = new FinancialYear
                {
                    Name = $"FY {currentYear}-{currentYear + 1}",
                    StartDate = new DateTime(currentYear, 4, 1),
                    EndDate = new DateTime(currentYear + 1, 3, 31),
                    IsCurrent = true,
                    IsLocked = false
                };
                await context.FinancialYears.AddAsync(fy);
            }

            // 5. Seed Standard Tax Rates (GST)
            if (!await context.TaxRates.AnyAsync())
            {
                await context.TaxRates.AddRangeAsync(
                    new TaxRate { Name = "GST 0% (Exempt)", Rate = 0, CGSTRate = 0, SGSTRate = 0, IGSTRate = 0, IsDefault = false },
                    new TaxRate { Name = "GST 5%", Rate = 5, CGSTRate = 2.5m, SGSTRate = 2.5m, IGSTRate = 5, IsDefault = false },
                    new TaxRate { Name = "GST 12%", Rate = 12, CGSTRate = 6, SGSTRate = 6, IGSTRate = 12, IsDefault = false },
                    new TaxRate { Name = "GST 18%", Rate = 18, CGSTRate = 9, SGSTRate = 9, IGSTRate = 18, IsDefault = true },
                    new TaxRate { Name = "GST 28%", Rate = 28, CGSTRate = 14, SGSTRate = 14, IGSTRate = 28, IsDefault = false }
                );
            }

            // 6. Seed Standard Measurement Units
            if (!await context.Units.AnyAsync())
            {
                await context.Units.AddRangeAsync(
                    new Unit { Name = "Pieces", ShortName = "PCS", AllowDecimals = false },
                    new Unit { Name = "Kilograms", ShortName = "KGS", AllowDecimals = true },
                    new Unit { Name = "Numbers", ShortName = "NOS", AllowDecimals = false },
                    new Unit { Name = "Boxes", ShortName = "BOX", AllowDecimals = false },
                    new Unit { Name = "Meters", ShortName = "MTR", AllowDecimals = true },
                    new Unit { Name = "Liters", ShortName = "LTR", AllowDecimals = true },
                    new Unit { Name = "Packets", ShortName = "PKT", AllowDecimals = false }
                );
            }

            // 7. Seed Default Categories & Sample Brand
            if (!await context.Categories.AnyAsync())
            {
                await context.Categories.AddRangeAsync(
                    new Category { Name = "Electronics", Code = "CAT-ELEC", Description = "Electronic devices & components" },
                    new Category { Name = "Office Supplies", Code = "CAT-OFF", Description = "Stationery and desk equipment" },
                    new Category { Name = "Services", Code = "CAT-SRV", Description = "Consulting and Maintenance services" }
                );
            }

            if (!await context.Brands.AnyAsync())
            {
                await context.Brands.AddRangeAsync(
                    new Brand { Name = "Generic", Description = "Standard unbranded" },
                    new Brand { Name = "AccuPro", Description = "AccuBooks OEM Brand" }
                );
            }

            // 8. Seed Chart of Accounts (Double-Entry Foundations)
            if (!await context.AccountGroups.AnyAsync())
            {
                // Root Account Groups
                var assets = new AccountGroup { Name = "Assets", Nature = AccountNature.Asset, IsSystemGroup = true };
                var liabilities = new AccountGroup { Name = "Liabilities", Nature = AccountNature.Liability, IsSystemGroup = true };
                var equity = new AccountGroup { Name = "Equity", Nature = AccountNature.Equity, IsSystemGroup = true };
                var income = new AccountGroup { Name = "Income", Nature = AccountNature.Income, IsSystemGroup = true };
                var expenses = new AccountGroup { Name = "Expenses", Nature = AccountNature.Expense, IsSystemGroup = true };

                await context.AccountGroups.AddRangeAsync(assets, liabilities, equity, income, expenses);
                await context.SaveChangesAsync();

                // Subgroups
                var currentAssets = new AccountGroup { Name = "Current Assets", Nature = AccountNature.Asset, ParentGroupId = assets.Id, IsSystemGroup = true };
                var bankAccounts = new AccountGroup { Name = "Bank Accounts", Nature = AccountNature.Asset, ParentGroupId = assets.Id, IsSystemGroup = true };
                var sundryDebtors = new AccountGroup { Name = "Sundry Debtors (Receivables)", Nature = AccountNature.Asset, ParentGroupId = assets.Id, IsSystemGroup = true };

                var currentLiab = new AccountGroup { Name = "Current Liabilities", Nature = AccountNature.Liability, ParentGroupId = liabilities.Id, IsSystemGroup = true };
                var dutiesAndTaxes = new AccountGroup { Name = "Duties & Taxes", Nature = AccountNature.Liability, ParentGroupId = liabilities.Id, IsSystemGroup = true };
                var sundryCreditors = new AccountGroup { Name = "Sundry Creditors (Payables)", Nature = AccountNature.Liability, ParentGroupId = liabilities.Id, IsSystemGroup = true };

                var salesGroup = new AccountGroup { Name = "Sales Accounts", Nature = AccountNature.Income, ParentGroupId = income.Id, IsSystemGroup = true };
                var purchaseGroup = new AccountGroup { Name = "Purchase Accounts", Nature = AccountNature.Expense, ParentGroupId = expenses.Id, IsSystemGroup = true };
                var directExpenses = new AccountGroup { Name = "Direct Expenses", Nature = AccountNature.Expense, ParentGroupId = expenses.Id, IsSystemGroup = true };
                var indirectExpenses = new AccountGroup { Name = "Indirect Expenses", Nature = AccountNature.Expense, ParentGroupId = expenses.Id, IsSystemGroup = true };

                await context.AccountGroups.AddRangeAsync(
                    currentAssets, bankAccounts, sundryDebtors,
                    currentLiab, dutiesAndTaxes, sundryCreditors,
                    salesGroup, purchaseGroup, directExpenses, indirectExpenses
                );
                await context.SaveChangesAsync();

                // Standard Account Heads
                await context.AccountHeads.AddRangeAsync(
                    new AccountHead { AccountCode = "1001", AccountName = "Cash in Hand", AccountGroupId = currentAssets.Id, IsSystemAccount = true },
                    new AccountHead { AccountCode = "1002", AccountName = "HDFC Bank A/c", AccountGroupId = bankAccounts.Id, IsSystemAccount = false },
                    new AccountHead { AccountCode = "1003", AccountName = "Accounts Receivable", AccountGroupId = sundryDebtors.Id, IsSystemAccount = true },
                    new AccountHead { AccountCode = "2001", AccountName = "Accounts Payable", AccountGroupId = sundryCreditors.Id, IsSystemAccount = true },
                    new AccountHead { AccountCode = "2002", AccountName = "Output CGST", AccountGroupId = dutiesAndTaxes.Id, IsSystemAccount = true },
                    new AccountHead { AccountCode = "2003", AccountName = "Output SGST", AccountGroupId = dutiesAndTaxes.Id, IsSystemAccount = true },
                    new AccountHead { AccountCode = "2004", AccountName = "Output IGST", AccountGroupId = dutiesAndTaxes.Id, IsSystemAccount = true },
                    new AccountHead { AccountCode = "2005", AccountName = "Input CGST", AccountGroupId = dutiesAndTaxes.Id, IsSystemAccount = true },
                    new AccountHead { AccountCode = "2006", AccountName = "Input SGST", AccountGroupId = dutiesAndTaxes.Id, IsSystemAccount = true },
                    new AccountHead { AccountCode = "2007", AccountName = "Input IGST", AccountGroupId = dutiesAndTaxes.Id, IsSystemAccount = true },
                    new AccountHead { AccountCode = "3001", AccountName = "Owner Capital", AccountGroupId = equity.Id, IsSystemAccount = true },
                    new AccountHead { AccountCode = "4001", AccountName = "Sales Revenue", AccountGroupId = salesGroup.Id, IsSystemAccount = true },
                    new AccountHead { AccountCode = "4002", AccountName = "Discount Allowed", AccountGroupId = indirectExpenses.Id, IsSystemAccount = true },
                    new AccountHead { AccountCode = "5001", AccountName = "Purchase Cost", AccountGroupId = purchaseGroup.Id, IsSystemAccount = true },
                    new AccountHead { AccountCode = "5002", AccountName = "Discount Received", AccountGroupId = income.Id, IsSystemAccount = true },
                    new AccountHead { AccountCode = "5003", AccountName = "Rent Expense", AccountGroupId = indirectExpenses.Id, IsSystemAccount = false },
                    new AccountHead { AccountCode = "5004", AccountName = "Electricity & Utilities", AccountGroupId = indirectExpenses.Id, IsSystemAccount = false }
                );
            }

            // 9. Seed Sample Master Products & Customers if empty
            if (!await context.Products.AnyAsync())
            {
                var defaultUnit = await context.Units.FirstAsync(u => u.ShortName == "PCS");
                var defaultTax = await context.TaxRates.FirstAsync(t => t.Rate == 18);
                var defaultCat = await context.Categories.FirstAsync();

                await context.Products.AddRangeAsync(
                    new Product
                    {
                        Name = "Wireless Laser Barcode Scanner",
                        ItemCode = "PRD-SCAN-01",
                        Barcode = "8901234567890",
                        Description = "High speed 2.4G wireless barcode scanner with stand",
                        CategoryId = defaultCat.Id,
                        UnitId = defaultUnit.Id,
                        TaxRateId = defaultTax.Id,
                        PurchasePrice = 1800,
                        SellingPrice = 2800,
                        MRP = 3200,
                        OpeningStock = 25,
                        CurrentStock = 25,
                        MinStockAlert = 5,
                        TrackInventory = true
                    },
                    new Product
                    {
                        Name = "Thermal Receipt Printer 80mm",
                        ItemCode = "PRD-PRN-80",
                        Barcode = "8909876543210",
                        Description = "Direct thermal POS printer with auto cutter USB+LAN",
                        CategoryId = defaultCat.Id,
                        UnitId = defaultUnit.Id,
                        TaxRateId = defaultTax.Id,
                        PurchasePrice = 3500,
                        SellingPrice = 5200,
                        MRP = 5999,
                        OpeningStock = 15,
                        CurrentStock = 15,
                        MinStockAlert = 3,
                        TrackInventory = true
                    }
                );
            }

            if (!await context.Customers.AnyAsync())
            {
                await context.Customers.AddAsync(new Customer
                {
                    Name = "Walk-in Retail Customer",
                    CompanyName = "Retail",
                    Phone = "+91 9000000000",
                    City = "Mumbai",
                    State = "Maharashtra",
                    StateCode = "27",
                    OpeningBalance = 0,
                    CurrentBalance = 0
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
