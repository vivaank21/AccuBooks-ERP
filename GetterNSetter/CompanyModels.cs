using System;

namespace AccuBooks_ERP.GetterNSetter
{
    public class CompanyProfile : BaseEntity
    {
        public string CompanyName { get; set; } = "AccuBooks Enterprise";
        public string? LegalName { get; set; }
        public string? GSTIN { get; set; }
        public string? PAN { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? StateCode { get; set; } = "27"; // e.g. 27 for Maharashtra
        public string? PinCode { get; set; }
        public string? Country { get; set; } = "India";
        public string CurrencySymbol { get; set; } = "₹";
        public string CurrencyCode { get; set; } = "INR";
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIFSC { get; set; }
        public string? BankBranch { get; set; }
        public string? UPIId { get; set; }
        public string? TermsAndConditions { get; set; }
        public string? LogoPath { get; set; }
    }

    public class FinancialYear : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // e.g., "FY 2026-2027"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCurrent { get; set; } = false;
        public bool IsLocked { get; set; } = false;
    }
}
