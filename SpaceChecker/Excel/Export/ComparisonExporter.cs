using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using SpaceChecker.Revit.Models;
using SpaceChecker.UI.ViewModels;


namespace SpaceChecker.Excel.Export
{
    internal static class ComparisonExporter
    {
        public static void Export(List<ComparisonResult> results, string filePath)
        {
            using (var pkg = new ExcelPackage())
            {
                var sheet = pkg.Workbook.Worksheets.Add("Space Program Check");
                WriteHeaders(sheet);
                WriteRows(sheet, results);
                WriteSummary(sheet, results);
                ApplyFormatting(sheet, results.Count);
                pkg.SaveAs(new FileInfo(filePath));
                Process.Start(filePath);
            }
        }

        private static void WriteHeaders(ExcelWorksheet sheet)
        {
            string[] headers = { "Room", "Number", "Level", "Dept",
                              "Required m2", "Actual m2", "Deviation m2", "Deviation %", "Status" };
            for (int i = 0; i < headers.Length; i++)
                sheet.Cells[1, i + 1].Value = headers[i];
        }

        private static void WriteRows(ExcelWorksheet sheet, List<ComparisonResult> results)
        {
            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                int row = i + 2;
                sheet.Cells[row, 1].Value = r.Room?.Name;
                sheet.Cells[row, 5].Value = r.ProgramEntry?.RequiredArea;
                sheet.Cells[row, 6].Value = r.Room?.ActualArea;
                sheet.Cells[row, 7].Value = Math.Round(r.Deviation, 2);
                sheet.Cells[row, 8].Value = Math.Round(r.DeviationPercent, 1);
                sheet.Cells[row, 9].Value = r.Status.ToString();
            }
        }
    }

}
