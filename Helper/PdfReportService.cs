using System;
using System.IO;
using AccuBooks_ERP.GetterNSetter;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AccuBooks_ERP.Helper
{
    public static class PdfReportService
    {
        static PdfReportService()
        {
            // QuestPDF Community License for open-source & internal business usage
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static byte[] GenerateInvoicePdf(SalesInvoice invoice, CompanyProfile? company)
        {
            company ??= new CompanyProfile();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Segoe UI").FontColor(Colors.Grey.Darken3));

                    // Header
                    page.Header().Element(c => ComposeHeader(c, invoice, company));

                    // Content / Items Table
                    page.Content().Element(c => ComposeContent(c, invoice, company));

                    // Footer
                    page.Footer().Element(c => ComposeFooter(c, invoice, company));
                });
            });

            return document.GeneratePdf();
        }

        public static void SaveInvoicePdf(SalesInvoice invoice, CompanyProfile? company, string filePath)
        {
            var pdfBytes = GenerateInvoicePdf(invoice, company);
            File.WriteAllBytes(filePath, pdfBytes);
        }

        private static void ComposeHeader(IContainer container, SalesInvoice invoice, CompanyProfile company)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    // Company Info Left
                    row.RelativeItem(2).Column(c =>
                    {
                        c.Item().Text(company.CompanyName).FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
                        if (!string.IsNullOrWhiteSpace(company.LegalName))
                            c.Item().Text(company.LegalName).FontSize(9).FontColor(Colors.Grey.Darken1);

                        c.Item().Text($"{company.AddressLine1} {company.City} {company.State} - {company.PinCode}");
                        c.Item().Text($"GSTIN: {company.GSTIN ?? "N/A"} | PAN: {company.PAN ?? "N/A"}");
                        c.Item().Text($"Phone: {company.Phone ?? "N/A"} | Email: {company.Email ?? "N/A"}");
                    });

                    // Invoice Title & Info Right
                    row.RelativeItem(1).Column(c =>
                    {
                        c.Item().AlignRight().Text("TAX INVOICE").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                        c.Item().AlignRight().Text($"Invoice #: {invoice.InvoiceNumber}").Bold();
                        c.Item().AlignRight().Text($"Date: {invoice.InvoiceDate:dd-MMM-yyyy}");
                        if (invoice.DueDate.HasValue)
                            c.Item().AlignRight().Text($"Due Date: {invoice.DueDate.Value:dd-MMM-yyyy}");
                        c.Item().AlignRight().Text($"Place of Supply: {invoice.PlaceOfSupply ?? company.State ?? "Local"}");
                    });
                });

                col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                // Billed To / Shipped To
                col.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Billed To:").Bold().FontColor(Colors.Blue.Darken3);
                        c.Item().Text(invoice.CustomerName).Bold();
                        if (!string.IsNullOrWhiteSpace(invoice.BillingAddress))
                            c.Item().Text(invoice.BillingAddress);
                        if (!string.IsNullOrWhiteSpace(invoice.CustomerGSTIN))
                            c.Item().Text($"GSTIN: {invoice.CustomerGSTIN}");
                        if (!string.IsNullOrWhiteSpace(invoice.CustomerPhone))
                            c.Item().Text($"Phone: {invoice.CustomerPhone}");
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Payment Details:").Bold().FontColor(Colors.Blue.Darken3);
                        c.Item().Text($"Mode: {invoice.PaymentMode}");
                        c.Item().Text($"Status: {invoice.PaymentStatus}").Bold();
                        if (!string.IsNullOrWhiteSpace(company.BankName))
                        {
                            c.Item().Text($"Bank: {company.BankName} A/c: {company.BankAccountNumber}");
                            c.Item().Text($"IFSC: {company.BankIFSC} Branch: {company.BankBranch}");
                        }
                    });
                });

                col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            });
        }

        private static void ComposeContent(IContainer container, SalesInvoice invoice, CompanyProfile company)
        {
            container.PaddingTop(10).Column(col =>
            {
                // Table
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25); // S.No
                        columns.RelativeColumn(3); // Item Description
                        columns.RelativeColumn(1); // HSN
                        columns.ConstantColumn(40); // Qty
                        columns.RelativeColumn(1); // Rate
                        columns.ConstantColumn(35); // Disc%
                        columns.RelativeColumn(1); // Taxable
                        columns.ConstantColumn(35); // GST%
                        columns.RelativeColumn(1); // Tax Amt
                        columns.RelativeColumn(1.2f); // Total
                    });

                    // Table Header
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCellStyle).Text("#");
                        header.Cell().Element(HeaderCellStyle).Text("Item Description");
                        header.Cell().Element(HeaderCellStyle).Text("HSN/SAC");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("Qty");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("Rate");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("Disc%");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("Taxable");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("GST%");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("Tax");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("Amount");
                    });

                    // Table Rows
                    int index = 1;
                    foreach (var item in invoice.Items)
                    {
                        table.Cell().Element(RowCellStyle).Text(index++.ToString());
                        table.Cell().Element(RowCellStyle).Text(item.ProductName).Bold();
                        table.Cell().Element(RowCellStyle).Text(item.HsnCode ?? "-");
                        table.Cell().Element(RowCellStyle).AlignRight().Text($"{item.Quantity} {item.UnitName ?? ""}");
                        table.Cell().Element(RowCellStyle).AlignRight().Text($"{item.UnitPrice:N2}");
                        table.Cell().Element(RowCellStyle).AlignRight().Text($"{item.DiscountPercent:N0}%");
                        table.Cell().Element(RowCellStyle).AlignRight().Text($"{item.TaxableAmount:N2}");
                        table.Cell().Element(RowCellStyle).AlignRight().Text($"{item.TaxRate:N0}%");
                        table.Cell().Element(RowCellStyle).AlignRight().Text($"{item.TotalTax:N2}");
                        table.Cell().Element(RowCellStyle).AlignRight().Text($"{item.TotalAmount:N2}").Bold();
                    }
                });

                // Summary & Tax Breakdown Box
                col.Item().PaddingTop(12).Row(row =>
                {
                    // Terms Left
                    row.RelativeItem(1.5f).Column(c =>
                    {
                        c.Item().Text("Terms & Conditions:").Bold().FontSize(8);
                        c.Item().Text(company.TermsAndConditions ?? "1. Subject to local jurisdiction.\n2. Goods once sold cannot be returned.").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                        
                        if (!string.IsNullOrWhiteSpace(invoice.Notes))
                        {
                            c.Item().PaddingTop(4).Text($"Notes: {invoice.Notes}").FontSize(8).Italic();
                        }
                    });

                    // Totals Table Right
                    row.RelativeItem(1.2f).Column(c =>
                    {
                        c.Item().Row(r => { r.RelativeItem().Text("Taxable SubTotal:"); r.RelativeItem().AlignRight().Text($"{company.CurrencySymbol} {invoice.TotalTaxableAmount:N2}"); });
                        
                        if (invoice.TotalCGST > 0)
                            c.Item().Row(r => { r.RelativeItem().Text("CGST Total:"); r.RelativeItem().AlignRight().Text($"{company.CurrencySymbol} {invoice.TotalCGST:N2}"); });
                        if (invoice.TotalSGST > 0)
                            c.Item().Row(r => { r.RelativeItem().Text("SGST Total:"); r.RelativeItem().AlignRight().Text($"{company.CurrencySymbol} {invoice.TotalSGST:N2}"); });
                        if (invoice.TotalIGST > 0)
                            c.Item().Row(r => { r.RelativeItem().Text("IGST Total:"); r.RelativeItem().AlignRight().Text($"{company.CurrencySymbol} {invoice.TotalIGST:N2}"); });
                        
                        if (invoice.RoundOff != 0)
                            c.Item().Row(r => { r.RelativeItem().Text("Round Off:"); r.RelativeItem().AlignRight().Text($"{invoice.RoundOff:+0.00;-0.00}"); });

                        c.Item().PaddingTop(3).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                        c.Item().PaddingTop(3).Row(r =>
                        {
                            r.RelativeItem().Text("Grand Total:").FontSize(11).Bold().FontColor(Colors.Blue.Darken3);
                            r.RelativeItem().AlignRight().Text($"{company.CurrencySymbol} {invoice.GrandTotal:N2}").FontSize(11).Bold().FontColor(Colors.Blue.Darken3);
                        });

                        c.Item().Row(r => { r.RelativeItem().Text("Paid Amount:"); r.RelativeItem().AlignRight().Text($"{company.CurrencySymbol} {invoice.PaidAmount:N2}"); });
                        if (invoice.DueAmount > 0)
                        {
                            c.Item().Row(r => { r.RelativeItem().Text("Balance Due:").Bold().FontColor(Colors.Red.Darken2); r.RelativeItem().AlignRight().Text($"{company.CurrencySymbol} {invoice.DueAmount:N2}").Bold().FontColor(Colors.Red.Darken2); });
                        }
                    });
                });
            });

            static IContainer HeaderCellStyle(IContainer c) =>
                c.Background(Colors.Blue.Lighten4).PaddingVertical(4).PaddingHorizontal(3).BorderBottom(1).BorderColor(Colors.Blue.Lighten2);

            static IContainer RowCellStyle(IContainer c) =>
                c.PaddingVertical(4).PaddingHorizontal(3).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
        }

        private static void ComposeFooter(IContainer container, SalesInvoice invoice, CompanyProfile company)
        {
            container.Column(col =>
            {
                col.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Customer Signature").FontSize(8).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingTop(25).Text("_____________________").FontSize(8);
                    });

                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().Text($"For {company.CompanyName}").Bold().FontSize(8);
                        c.Item().PaddingTop(25).AlignRight().Text("Authorised Signatory").FontSize(8);
                    });
                });

                col.Item().PaddingTop(8).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingTop(2).Row(r =>
                {
                    r.RelativeItem().Text("Thank you for your business!").FontSize(7.5f).Italic();
                    r.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            });
        }
    }
}
