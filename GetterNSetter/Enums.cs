namespace AccuBooks_ERP.GetterNSetter
{
    public enum UserRoleType
    {
        Admin = 1,
        Manager = 2,
        Accountant = 3,
        SalesCashier = 4,
        InventoryClerk = 5
    }

    public enum PaymentMode
    {
        Cash = 1,
        BankTransfer = 2,
        UPI = 3,
        CreditCard = 4,
        DebitCard = 5,
        Cheque = 6,
        Credit = 7,
        Mixed = 8
    }

    public enum PaymentStatus
    {
        Unpaid = 1,
        PartiallyPaid = 2,
        Paid = 3,
        Overdue = 4
    }

    public enum InvoiceStatus
    {
        Draft = 1,
        Confirmed = 2,
        Cancelled = 3,
        Returned = 4
    }

    public enum StockMovementType
    {
        OpeningStock = 1,
        PurchaseIn = 2,
        SalesOut = 3,
        PurchaseReturnOut = 4,
        SalesReturnIn = 5,
        AdjustmentIn = 6,
        AdjustmentOut = 7,
        DamageLoss = 8
    }

    public enum AccountNature
    {
        Asset = 1,
        Liability = 2,
        Equity = 3,
        Income = 4,
        Expense = 5
    }

    public enum EntryType
    {
        Debit = 1,
        Credit = 2
    }

    public enum VoucherType
    {
        Journal = 1,
        Payment = 2,
        Receipt = 3,
        Sales = 4,
        Purchase = 5,
        Contra = 6,
        DebitNote = 7,
        CreditNote = 8
    }

    public enum SupplyType
    {
        IntraState = 1, // Within state (CGST + SGST)
        InterState = 2, // Outside state (IGST)
        Export = 3,
        Exempted = 4
    }
}
