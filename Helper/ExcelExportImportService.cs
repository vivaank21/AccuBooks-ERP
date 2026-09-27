using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ClosedXML.Excel;

namespace AccuBooks_ERP.Helper
{
    public static class ExcelExportImportService
    {
        public static byte[] ExportListToExcel<T>(IEnumerable<T> data, string sheetTitle = "Data")
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetTitle);

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsPrimitive || 
                            p.PropertyType == typeof(string) || 
                            p.PropertyType == typeof(decimal) || 
                            p.PropertyType == typeof(DateTime) || 
                            p.PropertyType == typeof(DateTime?) ||
                            p.PropertyType == typeof(bool) ||
                            p.PropertyType.IsEnum)
                .ToList();

            // Write Headers
            for (int i = 0; i < properties.Count; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = properties[i].Name;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A8A");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Write Data
            int row = 2;
            foreach (var item in data)
            {
                for (int col = 0; col < properties.Count; col++)
                {
                    var val = properties[col].GetValue(item);
                    var cell = worksheet.Cell(row, col + 1);

                    if (val == null)
                    {
                        cell.Value = string.Empty;
                    }
                    else if (val is DateTime dt)
                    {
                        cell.Value = dt.ToString("yyyy-MM-dd HH:mm");
                    }
                    else if (val is decimal dec)
                    {
                        cell.Value = (double)dec;
                        cell.Style.NumberFormat.Format = "#,##0.00";
                    }
                    else if (val is double d)
                    {
                        cell.Value = d;
                        cell.Style.NumberFormat.Format = "#,##0.00";
                    }
                    else if (val is int n)
                    {
                        cell.Value = n;
                    }
                    else
                    {
                        cell.Value = val.ToString();
                    }
                }
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        public static void SaveExportToPath<T>(IEnumerable<T> data, string filePath, string sheetTitle = "Data")
        {
            var bytes = ExportListToExcel(data, sheetTitle);
            File.WriteAllBytes(filePath, bytes);
        }
    }
}
